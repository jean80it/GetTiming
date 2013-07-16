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

            long frequency = Stopwatch.Frequency;
            double nanosecPerTick = (double)1000000000 / (double)frequency;

            if (args.Length < 1)
            {
                Console.WriteLine("usage: getTiming [exe filename] [exe params]");
                Console.WriteLine();
                // Display the timer frequency and resolution. 
                if (Stopwatch.IsHighResolution)
                {
                    Console.WriteLine("Operations timed using the system's high-resolution performance counter.");
                }
                else
                {
                    Console.WriteLine("Operations timed using the DateTime class.");
                }
                Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
                Console.WriteLine("  Timer is accurate within {0:0} nanoseconds", nanosecPerTick);
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

            TimeSpan totalProcTime = new TimeSpan();

            long totalExecTime = 0;

            long minTime = long.MaxValue;
            long maxTime = long.MinValue;

            try
            {
                for (int i = 0; i < times; ++i)
                {
                    if (File.Exists("clean.cmd"))
                    {
                        Process.Start("clean.cmd").WaitForExit(); 
                    }

                    sw.Reset();
                    sw.Start();
                    var proc = Process.Start(psi);
                    proc.WaitForExit();
                    sw.Stop();
                    
                    totalProcTime += proc.TotalProcessorTime;
                    totalExecTime += sw.ElapsedTicks;

                    if (sw.ElapsedTicks > maxTime)
                    {
                        maxTime = sw.ElapsedTicks;
                    }
                    else
                    {
                        if (sw.ElapsedTicks < minTime)
                        {
                            minTime = sw.ElapsedTicks;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return;
            }

            var avgExecTime = (((double)(totalExecTime - minTime) - maxTime) * (double)nanosecPerTick) / (double)(times - 2);

            var avgProcTime = totalProcTime.TotalMilliseconds / (double)times;

            Console.WriteLine("v: {0,11:###########} / -: {1,11:###########} / ^: {2,11:###########} / p: {3,11:###########} \n\n", minTime * (double)nanosecPerTick, avgExecTime, maxTime * (double)nanosecPerTick, avgProcTime * 1000);
            
        }
    }
}
