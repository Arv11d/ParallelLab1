using System;
using System.Diagnostics;
using Mandelbrot;
using Sorting;
using System.Linq;

namespace ShowResults
{
    public class Program
    {
        public static void Main()
        {
            //ShowTopNSort();
            ShowSortingSizes(10_00, 500_000, 1_000_000, 2_000_000);
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

        public static void ShowSorting(int arraySize = 2_000_000)
        {
            var process = Process.GetCurrentProcess();
            var rand = new Random();

            // Generate random input
            var inputArray = Enumerable.Range(0, arraySize)
                                       .Select(_ => rand.Next())
                                       .ToArray();

            // --- Single-threaded (sequential baseline) ---
            var singleArray = (int[])inputArray.Clone();
            var seqSorter = new MergeSort<int>(); // or new StandardSort<int>();
            var sw = new Stopwatch();

            process.Refresh();
            var cpuBefore = process.TotalProcessorTime;
            sw.Restart();
            seqSorter.Sort(singleArray, Comparer<int>.Default);
            sw.Stop();
            process.Refresh();
            var seqWallMs = sw.ElapsedMilliseconds;
            var seqCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;

            Console.WriteLine("Single-threaded:");
            Console.WriteLine($"  Time elapsed: {seqWallMs} ms");
            Console.WriteLine($"  CPU time: {seqCpuMs:F2} ms");
            Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");

            // --- Parallel (your class) ---
            var parallelArray = (int[])inputArray.Clone();
            var parSorter = new ParallelSort<int>(); // <-- your parallel sorter

            process.Refresh();
            cpuBefore = process.TotalProcessorTime;
            sw.Restart();
            parSorter.Sort(parallelArray, Comparer<int>.Default);
            sw.Stop();
            process.Refresh();
            var parWallMs = sw.ElapsedMilliseconds;
            var parCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;

            Console.WriteLine("Parallel:");
            Console.WriteLine($"  Time elapsed: {parWallMs} ms");
            Console.WriteLine($"  CPU time: {parCpuMs:F2} ms");
            Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");

            // Verify identical results
            bool equal = singleArray.SequenceEqual(parallelArray);
            Console.WriteLine($"Results match:   {equal}");
        }

        public static void ShowSortingVsStandard(int arraySize = 2_000_000)
        {
            var process = Process.GetCurrentProcess();
            var rand = new Random();

            // Generate random input
            var inputArray = Enumerable.Range(0, arraySize)
                                       .Select(_ => rand.Next())
                                       .ToArray();

            // --- Single-threaded (StandardSort baseline) ---
            var singleArray = (int[])inputArray.Clone();
            var seqSorter = new StandardSort<int>(); // <-- baseline changed here
            var sw = new Stopwatch();

            process.Refresh();
            var cpuBefore = process.TotalProcessorTime;
            sw.Restart();
            seqSorter.Sort(singleArray, Comparer<int>.Default);
            sw.Stop();
            process.Refresh();
            var seqWallMs = sw.ElapsedMilliseconds;
            var seqCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;

            Console.WriteLine("Single-threaded (StandardSort):");
            Console.WriteLine($"  Time elapsed: {seqWallMs} ms");
            Console.WriteLine($"  CPU time: {seqCpuMs:F2} ms");
            Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");

            // --- Parallel (your class) ---
            var parallelArray = (int[])inputArray.Clone();
            var parSorter = new ParallelSort<int>(); // <-- your parallel sorter

            process.Refresh();
            cpuBefore = process.TotalProcessorTime;
            sw.Restart();
            parSorter.Sort(parallelArray, Comparer<int>.Default);
            sw.Stop();
            process.Refresh();
            var parWallMs = sw.ElapsedMilliseconds;
            var parCpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;

            Console.WriteLine("Parallel:");
            Console.WriteLine($"  Time elapsed: {parWallMs} ms");
            Console.WriteLine($"  CPU time: {parCpuMs:F2} ms");
            Console.WriteLine($"  Memory usage: {process.WorkingSet64 / 1024 / 1024} MB");

            // Verify identical results
            bool equal = singleArray.SequenceEqual(parallelArray);
            Console.WriteLine($"Results match:   {equal}");
        }


        public static void ShowSortingSizes(params int[] sizes)
        {
            foreach (var n in sizes)
            {
                Console.WriteLine($"\n=== Array size: {n:N0} ===");
                ShowSortingVsStandard(n);   // uses your existing method & output style
            }
        }


    }

}
