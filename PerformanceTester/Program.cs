using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.FSharp.Core;

namespace PerformanceTester
{
    class Program
    {
        static void Main()
        {
            var indivsNum = 100;
            var steps = 1000;
            var cities = RandomMonad.evaluateWith(new Random(), TSP.generateRandomCities(indivsNum));
            FSharpFunc<int[], double> fitnessFunc = TSP.TSPCost(cities);
            Func<FSharpFunc<int[], double>, int, int, IEnumerable<Tuple<int[], double>>> optimise = CAB402.CSharp.GeneticAlgorithm.Optimize;
            var results = optimise(fitnessFunc, indivsNum, indivsNum).Take(steps).ToArray();
            Console.WriteLine("Hello World!");
        }
    }
}
