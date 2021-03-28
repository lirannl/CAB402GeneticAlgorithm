using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    static class Extensions
    {
        // Randomly pick an item from a list-like object
        public static T Pick<T>(this IList<T> list, Random rand)
        {
            int index = rand.Next(0, list.Count);
            return list[index];
        }

        // Randomly rearrange the items in an array
        public static void Shuffle<T>(this T[] items, Random rand)
        {
            List<T> itemsList = items.ToList();

            int outputIdx = 0;
            foreach (int i in Enumerable.Range(0, items.Length))
            {
                // Take a random item from the list
                var item = itemsList.Pick(rand);
                itemsList.Remove(item);
                // Write the item to the array
                items[outputIdx] = item;
                // Increment the position
                outputIdx++;
            }
        }

        // Return true with probability 'chance', otherwise false.
        public static bool Maybe(double chance, Random rand)
        {
            var decision = rand.NextDouble();
            return decision <= chance;
        }
        public static T[] SortAccordingTo<T>(this T[] segment, T[] orderArr)
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
        public static (T[], T[]) Split<T>(this T[] source, int splitPoint)
        {
            var part0 = new T[splitPoint];
            var part1 = new T[source.Length - part0.Length];

            Array.Copy(source, part0, part0.Length);
            Array.Copy(source, part0.Length, part1, 0, part1.Length);

            return (part0, part1);
        }

        readonly static double MutateProbability = 0.15;

        // (possibly) mutate the given genes
        public static void Mutate<T>(this T[] genes, Random rand)
        {
            // Probably don't mutate.
            if (!Maybe(MutateProbability, rand))
                return;

            var firstIndex = rand.Next(0, genes.Length - 2);
            var secondIndex = rand.Next(firstIndex + 1, genes.Length);

            var (beginning, rest) = genes.Split(firstIndex);
            var (middle, end) = rest.Split(secondIndex - firstIndex);

            var newGenes = beginning.Concat(middle.Reverse()).Concat(end).ToArray();
            Array.Copy(newGenes, genes, genes.Length);

            return;
        }

        // Cross two arrays into a child array where some of the members are ordered based on parent 1, and the rest are ordered by parent 2
        public static T[] CrossWith<T>(this T[] parent1, T[] parent2, Random rand)
        {
            var splitPoint = rand.Next(1, parent1.Length - 1);
            var (parent1Genes, parent2Genes) = parent2.Split(splitPoint);

            parent2Genes.SortAccordingTo(parent2);

            return parent1Genes.Concat(parent2Genes).ToArray();
        }
    }
}
