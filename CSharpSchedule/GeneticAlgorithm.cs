using CSharpGeneticAlgorithm;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CAB402.CSharp
{
    public class GeneticAlgorithm
    {
        static IEnumerable<Tuple<int[], double>> ConvertPopulation(Population population)
        {
            return population.members.Select(member => new Tuple<int[], double>(member.genes, member.score));
        }
        public static IEnumerable<Tuple<int[],double>> Optimize(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, int numberOfGenes, int numerOfIndividuals)
        {
            var rand = new Random();
            var currentPopulation = new Population(fitnessFunction, numberOfGenes, numerOfIndividuals, rand);
            
            while (true)
            {
                yield return ConvertPopulation(currentPopulation).Max();
                // After yielding, evolve the current population
                currentPopulation = currentPopulation.Evolve(numerOfIndividuals, rand);
            }
        }
    }
}
