using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    class Sorter
    {
        public static T[] sortAccordingTo<T>(T[] segment, T[] orderArr)
        {
            List<T> unsortedItems = segment.ToList();

            int writeTo = 0;
            foreach (T item in orderArr)
            {
                if (unsortedItems.Contains(item))
                {
                    segment[writeTo] = item;
                    writeTo++;
                    unsortedItems.Remove(item);
                }

                if (unsortedItems.Count == 0) break;
            }

            return segment;
        }
        public static (T[], T[]) SplitArr<T>(int splitPoint, T[] source)
        {
            var left = new T[splitPoint];
            var right = new T[source.Length - left.Length];

            Array.Copy(source, left, left.Length);
            Array.Copy(source, left.Length, right, 0, right.Length);

            return (left, right);
        }
    }
}
