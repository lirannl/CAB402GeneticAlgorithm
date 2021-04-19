using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.FSharp.Core;

namespace PerformanceTester
{
    class Program
    {
        static Func<FSharpFunc<int[], double>, int, int, IEnumerable<Tuple<int[], double>>> Optimize(String lang)
        {
            switch (lang) 
            {
                case "fsharp" : return CAB402.FSharp.GeneticAlgorithm.Optimize;
                case "csharp": return CAB402.CSharp.GeneticAlgorithm.Optimize;
                default: throw new ArgumentOutOfRangeException("No such option.");
            }
        }

        static void Main(String [] args)
        {
            var lang = args[0];
            var indivsNum = 100;
            var steps = 1000;
            var cities = RandomMonad.evaluateWith(new Random(), TSP.generateRandomCities(indivsNum));
            FSharpFunc<int[], double> fitnessFunc = TSP.TSPCost(cities);
            var startTime = DateTime.Now;
            var results = Optimize(lang)(fitnessFunc, indivsNum, indivsNum).Take(steps).ToArray();
            var duration = DateTime.Now - startTime;
            Console.WriteLine(string.Format("Evovled {0} generations in {1} seconds, using {2}!", results.Length, duration.TotalSeconds, lang));
        }
    }
}
