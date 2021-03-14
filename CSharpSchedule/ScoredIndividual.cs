using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    class ScoredIndividual : IComparable<ScoredIndividual>
    {
        int[] genes;
        double score;

        public ScoredIndividual(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, int[] genes)
        {
            this.genes = genes;
            score = fitnessFunction.Invoke(genes);
        }

        public int CompareTo(ScoredIndividual other)
        {
            return score.CompareTo(other);
        }
    }
}
