using CSharpGeneticAlgorithm;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CAB402.CSharp
{
    public class GeneticAlgorithm
    {
        static Tuple<int[], double> GetFitestMember(Population population) 
        {
            var fitest = population.members.Max();
            return new Tuple<int[], double>(fitest.genes, fitest.score);
        }
        public static IEnumerable<Tuple<int[],double>> Optimize(Microsoft.FSharp.Core.FSharpFunc<int[], double> fitnessFunction, int numberOfGenes, int numerOfIndividuals)
        {
            var rand = new Random();
            var currentPopulation = new Population(fitnessFunction, numberOfGenes, numerOfIndividuals, rand);
            
            while (true)
            {
                var fitest = GetFitestMember(currentPopulation);
                yield return fitest;
                // After yielding, evolve the current population
                currentPopulation.Evolve(numerOfIndividuals, rand);
            }
        }
    }
}
