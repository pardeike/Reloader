using System.Timers;
using Verse;

namespace TestMod
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static RepeatDoStuff repeater;

        static Main()
        {
            Log.Error("Test mod started");
            repeater = new RepeatDoStuff();
        }
    }

    public class RepeatDoStuff
    {
        DoStuff instance = new DoStuff();

        public RepeatDoStuff()
        {
            Timer t1 = new Timer(3000);
            t1.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) =>
            {
                instance.Ping();
            });
            t1.AutoReset = true;
            t1.Start();

            Timer t2 = new Timer(500);
            t2.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) =>
            {
                instance.Counting();
            });
            t2.AutoReset = true;
            t2.Start();
        }
    }

    public class DoStuff
    {
        public int counter = 0;

        public void Ping()
        {
            Log.Warning("Ping! (" + counter + ")");
        }

        public void Counting()
        {
            Log.Warning("Counting: " + ++counter);
        }
    }
}
