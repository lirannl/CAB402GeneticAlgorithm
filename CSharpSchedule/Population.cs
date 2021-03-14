using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGeneticAlgorithm
{
    public class Population : IEnumerable<ScoredIndividual>
    {
        public ScoredIndividual[] members;
        readonly Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction;

        Population(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, ScoredIndividual[] members)
        {
            this.fitnessFunction = fitnessFunction;
            this.members = members;
        }
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
            // Randomly select two members of the population
            var competitors = Enumerable.Range(0, n).Select(_=>RandomOps.Pick(members, rand));
            return competitors.Max();

        }

        public Population ElitismSelection(Population children)
        {
            var membersList = members.ToList();
            // Sort from least fit to most fit
            membersList.Sort();
            // Sort from most fit to least fit
            membersList.Reverse();
            return new Population(fitnessFunction, children.Concat(membersList.Take(10)).ToArray());
        }

        public ScoredIndividual Procreate(Random rand)
        {
            var parent1 = ConductTournament(rand);
            var parent2 = ConductTournament(rand);

            return new ScoredIndividual(
                fitnessFunction,
                Individual.Cross(parent1.genes, parent2.genes, rand)
            );
        }

        public Population Evolve(int children, Random rand)
        {
            var childPopulation = Enumerable.Range(0, children)
                .Select(i => Procreate(rand)).ToArray();
            return ElitismSelection(new Population(fitnessFunction, childPopulation));
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
