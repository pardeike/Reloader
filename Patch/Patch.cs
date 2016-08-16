using Reloader;
using Verse;

namespace Patch
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
	 */

    public class DoStuff
    {
        public int counter = 0;

        [ReloadMethod("TestMod.DoStuff", "Ping")]
        public void HelloThere()
        {
            Log.Warning("Hello World!");
        }

        [ReloadMethod("TestMod.DoStuff", "Counting")]
        public void Silence()
        {
        }
    }
}