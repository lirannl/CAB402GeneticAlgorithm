using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharpGeneticAlgorithm.Sorter;

namespace CSharpGeneticAlgorithm
{
    class Individual
    {
        public static int[] Cross(int[] parent1, int[] parent2, Random rand)
        {
            var splitPoint = rand.Next(1, parent1.Length - 1);
            var (parent1Genes, parent2Genes) = SplitArr(splitPoint, parent2);

            return parent1Genes.Concat(sortAccordingTo(parent2Genes, parent2)).ToArray();
        }

        readonly static double MutateProbability = 0.15;

        // Return a (possibly) mutated version of the given genes
        public static int[] Mutate(int[] genes, Random rand)
        {
            // Probably return the provided genes without mutating them.
            if (!RandomOps.Maybe(MutateProbability, rand))
                return genes;

            var firstIndex = rand.Next(0, genes.Length - 2);
            var secondIndex = rand.Next(firstIndex + 1, genes.Length);

            var (beginning, rest) = SplitArr(firstIndex, genes);
            var (middle, end) = SplitArr(secondIndex - firstIndex, rest);

            return beginning.Concat(middle.Reverse()).Concat(end).ToArray();
        }
    }
}
