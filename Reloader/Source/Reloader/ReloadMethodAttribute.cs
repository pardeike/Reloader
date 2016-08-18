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
	 * namespace FooBar
	 * {
	 *		public class FooClass
	 *		{
	 *			[ReloadMethod]
	 *			public string[] GetStrings(int count)
	 *			{
	 *				...
	 *			}
	 *		}
	 * }
	 * 
	 * The attribute supports changes to the namespace, the class and the method
	 * so you can also write:
	 * 
	 * namespace FooBar
	 * {
	 *		public class FooClass
	 *		{
	 *			[ReloadMethod(null, null, "GetStrings")]
	 *			public string[] Whatever(int count)
	 *			{
	 *				...
	 *			}
	 *		}
	 * }
	 * 
	 * or:
	 * 
	 * namespace Whatever1
	 * {
	 *		public class Blah
	 *		{
	 *			[ReloadMethod("FooBar", "FooClass", "GetStrings")]
	 *			public string[] Whatever2(int count)
	 *			{
	 *				...
	 *			}
	 *		}
	 * }
	 * 
	 * Then you create a new directory at the same level as the Assemblies 
	 * directory in your Mod folder inside RimWorld and name it 
	 * Assemblies.reloading and you compile your little patch project 
	 * into an dll into that directory.
	 * 
	 * You need to have the Reloader Mod installed before you create that
	 * new directory and you can reference Reloader.dll in your patch 
	 * project to get the ReloadMethod attribute.
	 */

	[AttributeUsage(AttributeTargets.Method)]
	public class ReloadMethod : Attribute
	{
		public string namespaceName;
		public string className;
		public string methodName;

		public ReloadMethod(string namespaceName = null, string className = null, string methodName = null)
		{
			this.namespaceName = namespaceName;
			this.className = className;
			this.methodName = methodName;
		}
	}
}