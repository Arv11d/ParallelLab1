using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sorting
{
    // Range-based Parallel Merge Sort with a single reusable buffer.
    // Avoids allocating left/right arrays at each recursion.
    public sealed class ParallelSort<T> : SortBase<T>
    {
        private readonly int _threshold;
        private readonly int _maxDepth;

        public ParallelSort(int? threshold = null, int? maxDepth = null)
        {
            _threshold = threshold ?? 32_768; // tune: 16k–128k
            _maxDepth = maxDepth ?? (int)Math.Log2(Math.Max(1, Environment.ProcessorCount));
        }

        public override string Name => "ParallelMergeSort (range+buffer)";

        public override void Sort(T[] inputOutput, IComparer<T> comparer)
        {
            if (inputOutput == null) throw new ArgumentNullException(nameof(inputOutput));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (inputOutput.Length <= 1) return;

            // One auxiliary buffer reused for all merges
            var buffer = new T[inputOutput.Length];
            SortRange(inputOutput, buffer, 0, inputOutput.Length, comparer, depth: 0);
        }

        private void SortRange(T[] src, T[] buf, int start, int length, IComparer<T> cmp, int depth)
        {
            if (length <= 1) return;

            // Small or too deep -> sequential to avoid overhead
            if (length <= _threshold || depth >= _maxDepth)
            {
                Array.Sort(src, start, length, cmp);
                return;
            }

            int mid = start + (length / 2);
            int leftLen = mid - start;
            int rightLen = length - leftLen;

            Parallel.Invoke(
                () => SortRange(src, buf, start, leftLen, cmp, depth + 1),
                () => SortRange(src, buf, mid, rightLen, cmp, depth + 1)
            );

            // Merge src[start..mid) and src[mid..start+length) into buf[start..)
            int i = start, j = mid, k = start, end = start + length;
            while (i < mid && j < end)
                buf[k++] = (cmp.Compare(src[i], src[j]) <= 0) ? src[i++] : src[j++];
            while (i < mid) buf[k++] = src[i++];
            while (j < end) buf[k++] = src[j++];

            // Copy merged range back to source
            Array.Copy(buf, start, src, start, length);
        }
    }
}
