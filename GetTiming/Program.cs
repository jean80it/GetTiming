using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GetTiming
{
    class Program
    {
        public enum ArgsParserState
        { 
            ExpectingParam,
            ParseNExecutions,
            ParseExeAndParams,
            ParsePreClean
        }

        static void Main(string[] args)
        {
            long frequency = Stopwatch.Frequency;
            double nanosecPerTick = (double)1000000000 / (double)frequency;

            if (args.Length < 1)
            {
                Console.WriteLine(@"
usage: getTiming [options] [-e] [exe filename] [exe params]

options:
 -i  prints timer info
 -h  prints header
 -w  forces execution with no console
 -n  sets number of executions
 -p  specifies exe or script to prepare for each execution
 -e  must be last param; specifies excutable and params to execute
");
                return;
            }

            ArgsParserState parseState = ArgsParserState.ExpectingParam;
            
            bool nowin = false;
            int nExec = 10;
            
            string exeName = null;
            string paramsString = "";
            string preCleanExe = null;

            int paramsPos = 0;

            bool done = false;

            foreach(string s in args)
            {
                switch (parseState)
                {
                    case ArgsParserState.ExpectingParam:
                        switch (s)
                        {
                            case "-i":
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

                            case "-h":
                                Console.WriteLine("{0,11:##########}|{1,11:###########}|{2,11:###########}|{3,11:###########}|{4,11:###########}|{5,11:###########}", "exe name", "minimum", "average", "median", "maximum", "proc.");
                                return;

                            case "-n":
                                parseState = ArgsParserState.ParseNExecutions;
                                break;

                            case "-w":
                                nowin = true;
                                break;

                            case "-p":
                                parseState = ArgsParserState.ParsePreClean;
                                break;

                            case "-e":
                                parseState = ArgsParserState.ParseExeAndParams;
                                break;
                        }
                        break;

                    case ArgsParserState.ParseNExecutions:
                        if (!int.TryParse(s, out nExec))
                        {
                            Console.WriteLine("wrong -n param (must be an integer)");
                            return;
                        }
                        if (nExec<3)
                        {
                            Console.WriteLine("wrong -n param (must be >= 3)");
                            return;
                        }
                        parseState = ArgsParserState.ExpectingParam;
                        break;

                    case ArgsParserState.ParsePreClean:
                        preCleanExe = s;
                        parseState = ArgsParserState.ExpectingParam;
                        break;

                    case ArgsParserState.ParseExeAndParams:
                        ++paramsPos;
                        exeName = s;
                        foreach (string p in (new List<string>(args)).GetRange(paramsPos, args.Length - paramsPos))
                        {
                            paramsString += p + " ";
                        }
                        done = true; // we won't return to any other state (anything following is parameters to measured exe)
                        break;
                }

                if (done) break;

                ++paramsPos;
            }

            if (string.IsNullOrEmpty(exeName))
            {
                Console.WriteLine("no exe name specified!");
                return;
            }

            List<long> times = new List<long>(nExec);

            ProcessStartInfo psi = new ProcessStartInfo(exeName, paramsString);
            psi.CreateNoWindow = nowin;

            Stopwatch sw = new Stopwatch();

            TimeSpan totalProcTime = new TimeSpan();

            long totalExecTime = 0;

            long minTime = long.MaxValue;
            long maxTime = long.MinValue;

            try
            {
                for (int i = 0; i < nExec; ++i)
                {
                    if ((!string.IsNullOrEmpty(preCleanExe)) && File.Exists(preCleanExe))
                    {
                        Process.Start(preCleanExe).WaitForExit(); 
                    }

                    sw.Reset();
                    
                    Thread.Sleep(400); // wait for system to settle

                    sw.Start();
                    Process proc = Process.Start(psi);
                    proc.WaitForExit();
                    sw.Stop();

                    times.Add(sw.ElapsedTicks);

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
                Console.WriteLine("Execution failed.");
                return;
            }


            long median = 0; 
            times.Sort();
            if (nExec % 2 == 0)
            {
                median = (times[nExec / 2] + times[(nExec / 2) + 1]) / 2;
            }
            else
            {
                median = times[nExec / 2];
            }
            

            double avgExecTime = (((double)(totalExecTime - minTime) - maxTime) * (double)nanosecPerTick) / (double)(nExec - 2);

            double avgProcTime = totalProcTime.TotalMilliseconds / (double)nExec;

            Console.WriteLine("{0,11:##########}|{1,11:###########}|{2,11:###########}|{3,11:###########}|{4,11:###########}|{5,11:###########}", exeName.Substring(0, Math.Min(10, exeName.Length)), minTime * nanosecPerTick, avgExecTime, median * nanosecPerTick, maxTime * nanosecPerTick, avgProcTime * 1000);
            
        }
    }
}
