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
        public static int[] Cross(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, 
            int[] parent1, int[] parent2, Random rand)
        {
            var splitPoint = rand.Next(1, parent1.Length - 1);
            var (parent1Genes, parent2Genes) = SplitArr(splitPoint, parent2);

            return parent1Genes.Concat(sortAccordingTo(parent2Genes, parent2)).ToArray();
        }
    }
}
