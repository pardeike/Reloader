using System;

namespace Reloader
{
	/* Use this attribute to designate which method should be patched
	 * This works like this:
	 * 
	 * Your original (and running code) has for example this class:
	 * 
	 * namespace FooBar
	 * {
	 *		public class FooClass
	 *		{
	 *			public string[] GetStrings(int count)
	 *			{
	 *				...
	 *			}
	 *		}
	 * }
	 * 
	 * and you want to fix a bug while it is running in RimWorld. So you
	 * create a new project, add a new file Class1.cs that has a copy of
	 * the method/class you want to work with and looks like this:
	 * 
	 * namespace Whatever
	 * {
	 *		public class Blah
	 *		{
	 *			[ReloadMethod("FooBar.FooClass", "GetStrings")]
	 *			public string[] GetStrings(int count)
	 *			{
	 *				... fixes ...
	 *			}
	 *		}
	 * }
	 * 
	 * Then you create a new directory at the same level as the Assemblies 
	 * directory in your Mod folder inside RimWorld and name it 
	 * Assemblies.reloading and then you compile your little patch project 
	 * into an dll into that directory.
	 * 
	 * You need to have the Reloader Mod installed before you create that
	 * new directory and you can reference Reloader.dll in your patch 
	 * project to get the ReloadMethod attribute.
	 * 
	 */
	[AttributeUsage(AttributeTargets.Method)]
	public class ReloadMethod : Attribute
	{
		public string className;
		public string methodName;
		public Type[] argumentTypes;

		public ReloadMethod(string className, string methodName, Type[] argumentTypes = null)
		{
			this.className = className;
			this.methodName = methodName;
			this.argumentTypes = argumentTypes;
		}
	}
}