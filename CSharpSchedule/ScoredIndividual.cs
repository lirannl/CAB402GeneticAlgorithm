using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    // A set of genes and a score,
    // which is comparable to other ScoredIndividuals based on their scores.
    public class ScoredIndividual : IComparable<ScoredIndividual>
    {
        public int[] genes;
        public double score;

        // Takes a set of genes, assigns them with a score, and creates a ScoredIndividual
        public ScoredIndividual(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, int[] genes)
        {
            this.genes = genes;
            score = fitnessFunction.Invoke(genes);
        }

        public int CompareTo(ScoredIndividual other)
        {
            // Compare based on the scores of both
            return score.CompareTo(other.score);
        }
    }
}
