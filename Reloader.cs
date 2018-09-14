using Verse;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reloader
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ReloadMethod : Attribute
	{
		public ReloadMethod() { }
	}

	class Reloader : Mod
	{

		Dictionary<string, MethodInfo> reloadableMethods = new Dictionary<string, MethodInfo>();
		ModContentPack content;

		public Reloader(ModContentPack content) : base(content)
		{
			var modDirName = Path.GetDirectoryName(content.RootDir).ToLower();

			this.content = content;
			LongEventHandler.QueueLongEvent(CacheExstingMethods, "CacheExstingMethods", false, null);

			var folderPath = Path.Combine(content.RootDir, "Assemblies");
			var watcher = new FileSystemWatcher()
			{
				Path = folderPath,
				Filter = "*.dll",
				NotifyFilter = NotifyFilters.CreationTime
				| NotifyFilters.LastWrite
				| NotifyFilters.FileName
				| NotifyFilters.DirectoryName
			};
			var handler = new FileSystemEventHandler((sender, args) =>
			{
				var path = args.FullPath;
				if (ExcludedDLLs(path)) return;
				LoadPath(path);
			});
			watcher.Created += handler;
			watcher.Changed += handler;
			watcher.EnableRaisingEvents = true;
		}

		void CacheExstingMethods()
		{

            AppDomain.CurrentDomain.GetAssemblies()
				.Where(assembly =>
				{
					var name = assembly.FullName;
					name = name.Split(',')[0];
					var dllPath = Path.Combine(Path.Combine(content.RootDir, "Assemblies"), name + ".dll");
					return File.Exists(dllPath) && ExcludedDLLs(dllPath) == false;
				})
				.ToList()
				.ForEach(assembly =>
				{
					Log.Warning("Reloader: analyzing " + assembly.FullName);
					assembly.GetTypes().ToList()
						.ForEach(type => type.GetMethods(allBindings)
							.ToList()
							.ForEach(method =>
							{
								ReloadMethod attr;
								if (method.TryGetAttribute(out attr))
								{

                                    var key = method.DeclaringType.FullName + "." + method.Name;
								    if (method.IsGenericMethodDefinition)
								    {
								        Log.Error($"Reloader: Cannot reload generic method definition {key} - skipping");
								        return;
								    }
                                    reloadableMethods[key] = method;
									var methodType = method.DeclaringType;
									Log.Warning("Reloader: found reloadable method " + key);
								}
							})
						);
				});
		}

		void LoadPath(string path)
		{

            var assembly = Assembly.Load(File.ReadAllBytes(path));
			assembly.GetTypes().ToList()
				.ForEach(type => type.GetMethods(allBindings)
					.ToList()
					.ForEach(newMethod =>
					{
						ReloadMethod attr;
						if (newMethod.TryGetAttribute(out attr) && !newMethod.IsGenericMethodDefinition)
						{
							var key = newMethod.DeclaringType.FullName + "." + newMethod.Name;
							Log.Warning("Reloader: patching " + key);

							var originalMethod = reloadableMethods[key];
							if (originalMethod != null)
							{
								var originalCodeStart = Memory.GetMethodStart(originalMethod, out Exception ex1);
							    if (ex1 != null) {
							        Log.Warning($"Reloader: exception getting original method: {ex1.Message}");
							        return;
							    }

							    var newCodeStart = Memory.GetMethodStart(newMethod, out Exception ex2);
							    if (ex2 != null)
							    {
							        Log.Warning($"Reloader: exception getting new method: {ex2.Message}");
							        return;
							    }
								Memory.WriteJump(originalCodeStart, newCodeStart);
							}
							else
								Log.Warning("Reloader: original missing");
						}
					})
				);
		}

		bool ExcludedDLLs(string path)
		{
			return path.EndsWith("0Harmony.dll") || path.EndsWith("0Reloader.dll");
		}

		public static BindingFlags allBindings =
			BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.GetField
			| BindingFlags.SetField
			| BindingFlags.GetProperty
			| BindingFlags.SetProperty;
	}
}