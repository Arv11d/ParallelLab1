using System;
using System.Diagnostics;
using Mandelbrot;
using Sorting;

namespace ShowResults
{
    public class Program
    {
        public static void Main()
        {
            //ShowTopNSort();
            //ShowSorting();
            RunSortingSizes();
        }
        public static void ShowMalbrot()
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
        public static void TestMalbrot(double minX, double maxX, double stepX, int repeats = 10)
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

        public static void ShowTopNSort(int arraySize = 2000000000, int topN = 10_000)
        {
            var process = Process.GetCurrentProcess();

            var rand = new Random();

            // Generate random array
            var inputArray = Enumerable.Range(0, arraySize)
                                       .Select(_ => rand.Next())
                                       .ToArray();

            // Single-threaded sort
            var topNSort = new TopNStandardSort<int>();
            var sw = Stopwatch.StartNew();
            var singleResult = topNSort.TopNSort((int[])inputArray.Clone(), topN);
            sw.Stop();
            process.Refresh();

            Console.WriteLine("Single-threaded:");
            Console.WriteLine($"  Time elapsed: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"  CPU time: {process.TotalProcessorTime.TotalMilliseconds} ms");
            Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");

            // Parallel sort
            var topNParallelSort = new TopNParallelSort<int>();
            sw.Restart();
            process.Refresh();
            var parallelResult = topNParallelSort.TopNSort((int[])inputArray.Clone(), topN);
            sw.Stop();
            process.Refresh();

            Console.WriteLine("Parallel:");
            Console.WriteLine($"  Time elapsed: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"  CPU time: {process.TotalProcessorTime.TotalMilliseconds} ms");
            Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");

            // Verify results match
            bool equal = singleResult.SequenceEqual(parallelResult);
            Console.WriteLine($"Results match:   {equal}");
        }

        public static void ShowSorting(int n = 2_000_000) // ~2M elements
        {
            var rand = new Random(42);
            var data = new int[n];
            for (int i = 0; i < n; i++) data[i] = rand.Next();

            var process = Process.GetCurrentProcess();
            var sw = new Stopwatch();

            // --- Sequential (baseline) ---
            var seq = (int[])data.Clone();
            var seqSorter = new StandardSort<int>();   // or SelectionSort<int>()

            process.Refresh();
            var cpuBefore = process.TotalProcessorTime;
            sw.Restart();
            seqSorter.Sort(seq, Comparer<int>.Default);
            sw.Stop();
            process.Refresh();
            var seqCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
            var seqWallMs = sw.ElapsedMilliseconds;

            // --- Parallel ---
            var par = (int[])data.Clone();
            var parSorter = new ParallelPEESort<int>();

            process.Refresh();
            cpuBefore = process.TotalProcessorTime;
            sw.Restart();
            parSorter.Sort(par, Comparer<int>.Default);
            sw.Stop();
            process.Refresh();
            var parCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
            var parWallMs = sw.ElapsedMilliseconds;

            // --- Verify + report ---
            bool same = seq.SequenceEqual(par);
            Console.WriteLine($"Sorted correctly: {same}");
            Console.WriteLine($"Sequential  - Wall: {seqWallMs} ms  | CPU: {seqCpuMs:F0} ms");
            Console.WriteLine($"Parallel    - Wall: {parWallMs} ms  | CPU: {parCpuMs:F0} ms");
            Console.WriteLine($"Speedup (wall): {(double)seqWallMs / parWallMs:F2}×");
        }

        public static void RunSortingSizes(int trials = 5)
        {
            foreach (var n in new[] { 100_000, 500_000, 1_000_000, 2_000_000 })
            {
                long seqWall = 0, parWall = 0;
                double seqCpu = 0, parCpu = 0;
                bool ok = true;

                // One base input reused each trial for fairness
                var rnd = new Random(42);
                var baseData = Enumerable.Range(0, n).Select(_ => rnd.Next()).ToArray();
                var golden = (int[])baseData.Clone();
                Array.Sort(golden);

                for (int t = 0; t < trials; t++)
                {
                    var process = Process.GetCurrentProcess();
                    var sw = new Stopwatch();

                    // Sequential
                    var seq = (int[])baseData.Clone();
                    process.Refresh();
                    var cpuBefore = process.TotalProcessorTime;
                    sw.Restart(); new StandardSort<int>().Sort(seq, Comparer<int>.Default); sw.Stop();
                    process.Refresh();
                    seqWall += sw.ElapsedMilliseconds;
                    seqCpu += (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
                    ok &= seq.SequenceEqual(golden);

                    // Parallel
                    var par = (int[])baseData.Clone();
                    process.Refresh();
                    cpuBefore = process.TotalProcessorTime;
                    sw.Restart(); new ParallelPEESort<int>().Sort(par, Comparer<int>.Default); sw.Stop();
                    process.Refresh();
                    parWall += sw.ElapsedMilliseconds;
                    parCpu += (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
                    ok &= par.SequenceEqual(golden);
                }

                double seqW = seqWall / (double)trials, parW = parWall / (double)trials;
                double seqC = seqCpu / (double)trials, parC = parCpu / (double)trials;

                Console.WriteLine(
                    $"N={n:N0} | ok={ok} | Seq: {seqW:F1} ms (CPU {seqC:F0}) | " +
                    $"Par: {parW:F1} ms (CPU {parC:F0}) | Speedup {seqW / parW:F2}×");
            }
        }


    }

}
