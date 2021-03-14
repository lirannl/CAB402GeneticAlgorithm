using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    class Population : IEnumerable<ScoredIndividual>
    {
        public ScoredIndividual[] members;
        readonly Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction;

        public Population(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, int geneCount, int memberCount, Random rand)
        {
            this.fitnessFunction = fitnessFunction;
            // Create the members array
            members = new ScoredIndividual[memberCount];
            foreach (var i in Enumerable.Range(0, memberCount))
            {
                var baseGenes = Enumerable.Range(0, geneCount).ToArray();
                members[i] = new ScoredIndividual(fitnessFunction, RandomOps.Shuffle(baseGenes, rand));
            }
        }

        public ScoredIndividual ConductTournament(Random rand)
        {
            int n = 2;
            var competitors = new ScoredIndividual[n];
        }

        public ScoredIndividual Procreate(Random rand)
        {

        }

        public ScoredIndividual this[int index] { get => ((IList<ScoredIndividual>)members)[index]; set => ((IList<ScoredIndividual>)members)[index] = value; }

        public int Count => ((ICollection<ScoredIndividual>)members).Count;

        public bool Contains(ScoredIndividual item)
        {
            return ((ICollection<ScoredIndividual>)members).Contains(item);
        }

        public IEnumerator<ScoredIndividual> GetEnumerator()
        {
            return ((IEnumerable<ScoredIndividual>)members).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return members.GetEnumerator();
        }

    }
}
