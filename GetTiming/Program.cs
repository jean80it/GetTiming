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

            if (args.Length < 1)
            {
                Console.WriteLine("usage: getTiming [exe filename] [exe params]");
                return;
            }

            List<string> parametersList = new List<string>(args);
            parametersList.RemoveAt(0);
            string paramsString = "";
            foreach (string s in parametersList)
            {
                paramsString += s + " ";
            }

            Console.WriteLine("Executing '{0}' with parameters '{1}'", args[0], paramsString);

            ProcessStartInfo psi = new ProcessStartInfo(args[0]);
            psi.Arguments = paramsString;

            Stopwatch sw = new Stopwatch();

            Thread.Sleep(400); // wait for system to settle

            TimeSpan totalTime = new TimeSpan();

            try
            {
                for (int i = 0; i < times; ++i)
                {
                    if (File.Exists("clean.cmd"))
                    {
                        Process.Start("clean.cmd").WaitForExit(); Process p;
                    }

                    sw.Start();
                    var proc = Process.Start(psi);
                    proc.WaitForExit();
                    sw.Stop();
                    totalTime += proc.TotalProcessorTime;
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
            double nanosecPerTick = (double)1000000000 / (double)frequency;
            Console.WriteLine("  Timer is accurate within {0} nanoseconds",
                nanosecPerTick);

            var avgTime = (sw.ElapsedTicks * nanosecPerTick) / (double)times;
            var procTime = totalTime.Milliseconds / (double)times;

            Console.WriteLine("Process average execution time: {0:0} ns ({1:0.000000} s)", avgTime, avgTime / (double)1000000000);
            Console.WriteLine("Process average processor time: {0:0.000} ms", procTime);
        }
    }
}
