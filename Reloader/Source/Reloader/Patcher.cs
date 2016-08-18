using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

/* A convenient way to develop RimWorld mods without restarting RimWorld
 *
 * Developed by Andreas Pardeike with tribute to those long nights restarting
 * RimWorld and plowing through endless disassembled .net code
 *
 * This should be a community effort, so please feel free to contribute. It's
 * far from perfect, will eat your memory for breakfast and probably fail horrible
 * on everything that is not public non-static. It's a concept for now.
 *
 * Cheers,
 * Andreas Pardeike
 *
 * Twitter: @pardeike
 * iMessage: +46722120680
 * email: andreas@pardeike.net
 */

namespace Reloader
{
	static class Extensions
	{
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			enumerable.ToList().ForEach(action);
		}
	}

	[StaticConstructorOnStartup]
	static class Patcher
	{
		static string NAMESPACE_DEBUG = null;

		internal class MethodSearchParams
		{
			public Type declaringType { get; set; }
			public bool isPrivate { get; set; }
			public bool isStatic { get; set; }
			public string methodName { get; set; }
			public string namespaceName { get; set; }
			public ParameterInfo[] parameterInfo { get; set; }
			public ParameterInfo returnParamInfo { get; set; }
			public string replacementNamespace { get; set; }
		}

		static BindingFlags[] bindings = new BindingFlags[]
		{
			BindingFlags.Public | BindingFlags.Instance,
			BindingFlags.NonPublic | BindingFlags.Instance,
			BindingFlags.Public | BindingFlags.Static,
			BindingFlags.NonPublic | BindingFlags.Static
		};

		static void ForAllMethodsDo(Assembly assembly, Func<MethodInfo, bool> condition, Action<MethodInfo> action)
		{
			assembly.GetTypes().ForEach(type =>
			{
				bindings.ForEach(binding =>
				{
					BindingFlags b = binding | BindingFlags.DeclaredOnly;
					type.GetMethods(b).Where(condition).ForEach(action);
				});
			});
		}

		static bool HasReloadMethodAttribute(MethodInfo method)
		{
			return method.GetCustomAttributes(false)
				.Where(att => att is ReloadMethod).Any();
		}

		static MethodSearchParams GetSearchParams(MethodInfo method)
		{
			ReloadMethod attribute = method.GetCustomAttributes(false)
				.Cast<ReloadMethod>().First(att => att is ReloadMethod);

			return new MethodSearchParams()
			{
				isStatic = method.IsStatic,
				isPrivate = method.IsPrivate,
				namespaceName = attribute.namespaceName != null ? attribute.namespaceName : method.DeclaringType.Namespace,
				methodName = attribute.methodName != null ? attribute.methodName : method.Name,
				declaringType = attribute.className != null ? Type.GetType(attribute.className, false, false) : method.DeclaringType,
				parameterInfo = method.GetParameters(),
				returnParamInfo = method.ReturnParameter,
				replacementNamespace = method.DeclaringType.Namespace
			};
		}

		static string FixNamespace(Type type, MethodSearchParams p)
		{
			string fullname = type.FullName;
			if (p.replacementNamespace != null)
			{
				if (fullname.StartsWith(p.replacementNamespace + "."))
				{
					fullname = p.namespaceName + "." + fullname.Substring(p.replacementNamespace.Length + 1);
				}
			}
			return fullname;
		}

		static string MatchesSearch(MethodInfo method, MethodSearchParams p)
		{
			if (method.GetCustomAttributes(false).Any(attr => attr.GetType() == typeof(ReloadMethod)))
				return "original";
			if (method.IsStatic != p.isStatic)
				return "static";
			if (method.IsPrivate != p.isPrivate)
				return "private";
			if (method.DeclaringType.Namespace != p.namespaceName)
				return "namespace";
			string declaringTypeName = FixNamespace(p.declaringType, p);
			if (method.DeclaringType.FullName != declaringTypeName)
				return "declaringtype(" + method.DeclaringType.FullName + "<>" + declaringTypeName + ")";
			if (method.Name != p.methodName)
				return "methodname";
			string returnTypeName = FixNamespace(p.returnParamInfo.ParameterType, p);
			if (method.ReturnParameter.ParameterType.FullName != returnTypeName)
				return "returnparam(" + method.ReturnParameter.ParameterType.FullName + "<>" + returnTypeName + ")";
			ParameterInfo[] methodParams = method.GetParameters();
			if (methodParams.Length != p.parameterInfo.Length)
				return "#params";
			for (int i = 0; i < methodParams.Length; i++)
			{
				string paramTypeName = FixNamespace(p.parameterInfo[i].ParameterType, p);
				if (methodParams[i].ParameterType.FullName != paramTypeName)
					return "paramtype#" + (i + 1) + "(" + methodParams[i].ParameterType.FullName + "<>" + paramTypeName + ")";
			}

			// we found a candidate
			return null;
		}

		static void Detour(MethodInfo replacement)
		{
			MethodSearchParams searchParams = GetSearchParams(replacement);

			AppDomain.CurrentDomain.GetAssemblies().ForEach(assembly =>
			{
				ForAllMethodsDo(
					assembly,
					method =>
					{
						string failed = MatchesSearch(method, searchParams);
						if (NAMESPACE_DEBUG != null && method.DeclaringType.Namespace == NAMESPACE_DEBUG)
						{
							Log.Warning("- " + method.DeclaringType.FullName + "." + method.Name + ": " + (failed == null ? "MATCH" : failed));
						}
						return failed == null;
					},
					method => Detours.TryDetourFromTo(method, replacement)
				);
			});
		}

		static void ReplaceMethods(string path)
		{
			Assembly assembly = AppDomain.CurrentDomain.Load(File.ReadAllBytes(path));
			ForAllMethodsDo(assembly, HasReloadMethodAttribute, Detour);
		}

		static void Watch(string folderPath)
		{
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = folderPath;
			watcher.Filter = "*.dll";
			watcher.NotifyFilter = NotifyFilters.CreationTime
				| NotifyFilters.LastWrite
				| NotifyFilters.FileName
				| NotifyFilters.DirectoryName;
			FileSystemEventHandler handler = new FileSystemEventHandler((sender, args) =>
			{
				Log.Warning("Reloading " + args.FullPath);
				ReplaceMethods(args.FullPath);
			});
			watcher.Created += handler;
			watcher.Changed += handler;
			watcher.EnableRaisingEvents = true;
		}

		static Patcher()
		{
			string modsDir = Path.Combine(Directory.GetCurrentDirectory(), "Mods");
			foreach (string modDirectory in Directory.GetDirectories(modsDir))
			{
				string reloadDir = Path.Combine(modDirectory, "Assemblies.reloading");
				if (Directory.Exists(reloadDir))
				{
					Watch(reloadDir);
				}
			}
		}
	}
}
