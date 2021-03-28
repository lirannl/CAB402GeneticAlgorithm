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

        public Population(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, int geneCount, int memberCount, Random rand)
        {
            this.fitnessFunction = fitnessFunction;
            // Create the members array
            members = new ScoredIndividual[memberCount];
            foreach (var i in Enumerable.Range(0, memberCount))
            {
                // Generate a genomic sequence
                var genes = Enumerable.Range(0, geneCount).ToArray();
                // Shuffle it randomly
                genes.Shuffle(rand);
                members[i] = new ScoredIndividual(fitnessFunction, genes);
            }
        }

        public ScoredIndividual ConductTournament(Random rand)
        {
            int n = 2;
            // Randomly select two members of the population
            var competitors = new ScoredIndividual[n];
            foreach (var i in Enumerable.Range(0, n))
            {
                competitors[i] = members.Pick(rand);
            }
            return competitors.Max();

        }

        public ScoredIndividual[] ElitismSelection(ScoredIndividual[] children)
        {
            var membersList = members.ToList();
            // Sort by fitness
            membersList.Sort();
            // Sort from most fit to least fit
            membersList.Reverse();
            // How many members are needed to get the next generation to be as large as the current one
            var lackingChildren = members.Length - children.Length;
            return children.Concat(membersList.Take(lackingChildren)).ToArray();
        }

        public ScoredIndividual Procreate(Random rand)
        {
            var parent1 = ConductTournament(rand);
            var parent2 = ConductTournament(rand);

            var childGenes = parent1.genes.CrossWith(parent2.genes, rand);
            // Possibly cause a mutation
            childGenes.Mutate(rand);

            return new ScoredIndividual(fitnessFunction, childGenes);
        }

        public void Evolve(int children, Random rand)
        {
            var childPopulation = new ScoredIndividual[children];
            foreach (var i in Enumerable.Range(0, children))
            {
                childPopulation[i] = Procreate(rand);
            }
            var newMembers = ElitismSelection(childPopulation);
            members = newMembers;
        }

        public ScoredIndividual this[int index] { 
            get => ((IList<ScoredIndividual>)members)[index]; 
            set => ((IList<ScoredIndividual>)members)[index] = value; 
        }

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
