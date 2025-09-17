using System;
using System.Collections.Generic;
using System.Threading.Algorithms;
using System.Threading.Tasks;

namespace Sorting
{
    public class ParallelPEESort<T> : ISort<T>
    {
        public string Name { get { return "ParallelPEESort"; } }

        public void Sort(T[] inputOutput)
        {
            ParallelAlgorithms.Sort(inputOutput);
        }

        public void Sort(T[] inputOutput, IComparer<T> comparer)
        {
            if (inputOutput == null) throw new ArgumentNullException(nameof(inputOutput));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            var buffer = new T[inputOutput.Length];
            ParallelMergeSort(inputOutput, buffer, 0, inputOutput.Length, comparer, depth: 0);
        }

        private void ParallelMergeSort(T[] src, T[] buf, int start, int length, IComparer<T> comparer, int depth)
        {
            if (length <= 1) return;

            int threshold = 16_384;
            int mid = start + (length / 2);
            int leftLen = mid - start;
            int rightLen = (start + length) - mid;

            if (length <= threshold)
            {
                Array.Sort(src, start, length, comparer);
                return;
            }
            Parallel.Invoke(
                () => ParallelMergeSort(src, buf, start, leftLen, comparer, depth + 1),
                () => ParallelMergeSort(src, buf, mid, rightLen, comparer, depth + 1)
            );

            Merge(src, buf, start, mid, start + length, comparer);
            Array.Copy(buf, start, src, start, length);
        }

        private void Merge(T[] src, T[] dst, int left, int mid, int right, IComparer<T> comparer)
        {
            int i = left, j = mid, k = left;
            while (i < mid && j < right)
            {
                if (comparer.Compare(src[i], src[j]) <= 0)
                    dst[k++] = src[i++];
                else
                    dst[k++] = src[j++];
            }
            while (i < mid) dst[k++] = src[i++];
            while (j < right) dst[k++] = src[j++];
        }
    }
}
