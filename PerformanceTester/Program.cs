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
            Func<FSharpFunc<int[], double>, int, int, IEnumerable<Tuple<int[], double>>> optimise = CAB402.FSharp.GeneticAlgorithm.Optimize;
            var startTime = DateTime.Now;
            var results = optimise(fitnessFunc, indivsNum, indivsNum).Take(steps).ToArray();
            var duration = DateTime.Now - startTime;
            Console.WriteLine(string.Format("Evovled {0} generations in {1} seconds!", results.Length, duration.TotalSeconds));
        }
    }
}
