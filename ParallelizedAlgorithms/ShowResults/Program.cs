using System;
using System.Diagnostics;
using Mandelbrot;

namespace ShowResults
{
    public class Program
    {
        public static void Main()
        {
            Test(9000, 10000, 500);
        }
        public static void Show()
        {
            int x = 500;
            int y = 500;
                // Get process for CPU/memory info
                var process = Process.GetCurrentProcess();
                // --- Single-threaded ---
                var m = new MandelbrotSingleThread(x, y);
                m.LowerX = -2.0;
                m.UpperX = 1.0;
                m.LowerY = -1.5;
                m.UpperY = 1.5;

                var sw = Stopwatch.StartNew();
                m.Compute();
                sw.Stop();

                process.Refresh(); // Update process info

                Console.WriteLine("Single-threaded:");
                Console.WriteLine(x);
                Console.WriteLine($"  Time elapsed: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"  CPU time: {process.TotalProcessorTime.TotalMilliseconds} ms");
                Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");


                // --- Parallel ---
                var mp = new MandelbrotParallel(x, y);
                mp.LowerX = -2.0;
                mp.UpperX = 1.0;
                mp.LowerY = -1.5;
                mp.UpperY = 1.5;

                process.Refresh();
                sw.Restart();
                mp.ParallelCompute();
                sw.Stop();

                process.Refresh();

                Console.WriteLine("\nParallel:");
                Console.WriteLine($"  Time elapsed: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"  CPU time: {process.TotalProcessorTime.TotalMilliseconds} ms");
                Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");
            }
        public static void Test(double minX, double maxX, double stepX, int repeats = 10)
        {
            for (double x = minX; x <= maxX; x += stepX)
            {
                long[] SingleTime = new long[repeats];
                double[] SingleCPUTime = new double[repeats];
                long[] SingleMemory = new long[repeats];

                long[] ParallelTime = new long[repeats];
                double[] ParallelCPUTime = new double[repeats];
                long[] ParallelMemory = new long[repeats];

                for (int i = 0; i < repeats; i++)
                {
                    var process = Process.GetCurrentProcess();
                    // Single-threaded 
                    var m = new MandelbrotSingleThread((int)x, (int)x)
                    {
                        LowerX = -2.0,
                        UpperX = 1.0,
                        LowerY = -1.5,
                        UpperY = 1.5
                    };

                    var sw = Stopwatch.StartNew();
                    m.Compute();
                    sw.Stop();

                    process.Refresh();
                    SingleTime[i] = sw.ElapsedMilliseconds;
                    SingleCPUTime[i] = process.TotalProcessorTime.TotalMilliseconds;
                    SingleMemory[i] = process.WorkingSet64 / 1024 / 1024;

                    // Parallel
                    var mp = new MandelbrotParallel((int)x, (int)x)
                    {
                        LowerX = -2.0,
                        UpperX = 1.0,
                        LowerY = -1.5,
                        UpperY = 1.5
                    };

                    process.Refresh();
                    sw.Restart();
                    mp.ParallelCompute();
                    sw.Stop();

                    process.Refresh();
                    ParallelTime[i] = sw.ElapsedMilliseconds;
                    ParallelCPUTime[i] = process.TotalProcessorTime.TotalMilliseconds;
                    ParallelMemory[i] = process.WorkingSet64 / 1024 / 1024;
                }

                // Compute averages
                double avgSingleTime = SingleTime.Average();
                double avgSingleCPU = SingleCPUTime.Average();
                double avgSingleMemory = SingleMemory.Average();

                double avgParallelTime = ParallelTime.Average();
                double avgParallelCPU = ParallelCPUTime.Average();
                double avgParallelMemory = ParallelMemory.Average();

                Console.WriteLine($"--- Results for Width={x}, Height={x} ---");
                Console.WriteLine($"Single-threaded: Time={avgSingleTime:F2} ms, CPU={avgSingleCPU:F2} ms, Memory={avgSingleMemory} MB");
                Console.WriteLine($"Parallel:        Time={avgParallelTime:F2} ms, CPU={avgParallelCPU:F2} ms, Memory={avgParallelMemory} MB");
                Console.WriteLine();
            }
        }

    }
}
