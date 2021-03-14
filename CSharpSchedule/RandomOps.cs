using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    class RandomOps
    {
        // Randomly pick an item from a list-like object
        public static T Pick<T>(IList<T> items, Random rand)
        {
            int index = rand.Next(0, items.Count);
            return items[index];
        }

        // Randomly rearrange the items in an array
        public static T[] Shuffle<T>(T[] items, Random rand)
        {
            List<T> itemsList = items.ToList();

            int outputIdx = 0;
            foreach (int i in Enumerable.Range(0, items.Length))
            {
                // Take a random item from the list
                var item = Pick(itemsList, rand);
                itemsList.Remove(item);
                // Write the item to the array
                items[outputIdx] = item;
                // Increment the position
                outputIdx++;
            }
            return items;
        }

        // Return true with probability 'chance', otherwise false.
        public static bool Maybe(double chance, Random rand)
        {
            var decision = rand.NextDouble();
            return decision <= chance;
        }
    }
}
