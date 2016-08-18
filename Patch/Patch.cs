using System.Reflection;
using Reloader;
using Verse;

namespace TestMod
{
	/* This is the patch for fixing TestMod
	* 
	* It is basically a copy of the class DoStuff so that
	* the detour can keep the instance and the counter will
	* continue while we replace methods.
	* 
	* Remember:
	* 
	* - you need to add a reference to Reloader.dll to the
	*   project so you can use the ReloadMethod attribute
	*   
	* - you *NEED* to make sure the ddl version is updated
	*   every time you create a new dll or otherwise .net
	*   will not load it. Use a auto-increment plugin like
	*   "Build Version Increment Add-in by Paul J. Melia
	* 
	* - You can only change existing methods, but you can
	*   add new ones in your patch. If you keep the original
	*   and the patch the same code, you can later copy
	*   everything back to your main project once you're done.
	*   
	*   If you change a method in a class you probably want
	*   to change all methods that instantiate such a class
	*   or the original class (with same name) still exists
	*   and is not used. See example below.
	*/

	public class MsgItem
	{
		private string msg;
		private static int n = 100; // new

		public MsgItem(string msg)
		{
			this.msg = msg;
		}

		[ReloadMethod]
		public string GetMessage()
		{
			n++; // new
			return msg + "-" + n; // changed
		}
	}

	public class DoStuff
	{
		private static class MsgItemFactory
		{
			[ReloadMethod] // without this, the unpatched MsgItem is returned
			public static MsgItem Produce()
			{
				return new MsgItem("Ping");
			}
		}

		public static int counter = 0;

		private static string Test()
		{
			MsgItem bean = MsgItemFactory.Produce();
			string s = bean.GetMessage();
			return s + " (" + counter + ")";
		}

		public void Ping()
		{
			Log.Warning(Test());
		}

		public void Counting()
		{
			++counter;
		}
	}
}