using System;
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
	[StaticConstructorOnStartup]
	static class Patcher
	{
		static void ReplaceMethods(string path)
		{
			AppDomain.CurrentDomain.Load(File.ReadAllBytes(path))
				.GetTypes()
				.SelectMany(type => type.GetMethods())
				.Where(method =>
				{
					return method.GetCustomAttributes(false)
						.Where(att => att is ReloadMethod).Any();
				}).ToList().ForEach(Detour);
		}

		static void Detour(MethodInfo replacement)
		{
			ReloadMethod methodInfo = replacement.GetCustomAttributes(false)
				.Cast<ReloadMethod>().First(att => att is ReloadMethod);

			AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.FullName == methodInfo.className)
				.SelectMany(type => type.GetMethods())
				.Where(method => method.Name == methodInfo.methodName)
				.ToList().ForEach(method =>
				{
					Detours.TryDetourFromTo(method, replacement);
				});
		}

		static void Watch(string folderPath)
		{
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = folderPath;
			watcher.NotifyFilter = NotifyFilters.CreationTime
				| NotifyFilters.LastWrite
				| NotifyFilters.FileName
				| NotifyFilters.DirectoryName;
			watcher.Filter = "*.dll";
			FileSystemEventHandler handler = new FileSystemEventHandler((sender, args) =>
			{
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
