using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GetTiming
{
    class Program
    {
        static void Main(string[] args)
        {
            const int times = 10;

            if (args.Length != 1)
            {
                Console.WriteLine("usage: getTiming [execfilename]");
                return;
            }

            if (File.Exists("clean.cmd"))
            {
                Process.Start("clean.cmd").WaitForExit();    
            }

            ProcessStartInfo psi = new ProcessStartInfo(args[0]);
            Stopwatch sw = new Stopwatch();

            Thread.Sleep(400); // wait for system to settle

            try
            {
                for (int i = 0; i < times; ++i)
                {

                    sw.Start();
                    Process.Start(psi).WaitForExit();
                    sw.Stop();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return;
            }

            // Display the timer frequency and resolution. 
            if (Stopwatch.IsHighResolution)
            {
                Console.WriteLine("Operations timed using the system's high-resolution performance counter.");
            }
            else
            {
                Console.WriteLine("Operations timed using the DateTime class.");
            }

            long frequency = Stopwatch.Frequency;
            Console.WriteLine("  Timer frequency in ticks per second = {0}",
                frequency);
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
            Console.WriteLine("  Timer is accurate within {0} nanoseconds",
                nanosecPerTick);

            Console.WriteLine("Process average execution time: {0} ns", (sw.ElapsedTicks * 1000000L / (long)times) / frequency);
        }
    }
}
