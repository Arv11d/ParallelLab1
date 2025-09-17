using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sorting
{
    public class TopNParallelSort<T> : ITopNSort<T>
    {
        public string Name { get { return "TopN Parallel Chunked Sort+Take(N)"; } }

        public T[] TopNSort(T[] inputOutput, int n)
        {
            return TopNSort(inputOutput, n, Comparer<T>.Default);
        }

        public T[] TopNSort(T[] inputOutput, int n, IComparer<T> comparer)
        {
            int processorCount = Environment.ProcessorCount*1024;
            int chunkSize = (int)Math.Ceiling(inputOutput.Length / (double)processorCount);

            // Sort chunks
            var chunks = new List<T[]>(processorCount);
            Parallel.For(0, processorCount, i =>
            {
                int start = i * chunkSize;
                if (start >= inputOutput.Length) return;

                int length = Math.Min(chunkSize, inputOutput.Length - start);
                var chunk = new T[length];
                Array.Copy(inputOutput, start, chunk, 0, length);
                Array.Sort(chunk, comparer);
                lock (chunks)
                {
                    chunks.Add(chunk);
                }
            });

            // Merge chunks and take top N
            var merged = chunks.SelectMany(c => c)
                               .OrderBy(x => x, comparer) // final global sort
                               .Take(n)
                               .ToArray();

            return merged;
        }
    }
}
