using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.FSharp.Core;
using System.Collections;

namespace PerformanceTester
{
    class Program
    {
        const int indivsNum = 100;
        const int steps = 1000;
        const int runs = 3;
        static void PrintAvgRunTime(String lang)
        {
            var times = Enumerable.Range(0, runs).Select(run => TimeAlgorithm(lang)).ToArray();
            var avg = times.Sum() / runs;
            Console.WriteLine(string.Format("Evovled {0} generations in an average of {1} seconds, using {2}!", steps, avg, lang));
        }

        static Func<FSharpFunc<int[], double>, int, int, IEnumerable<Tuple<int[], double>>> Optimize(String lang)
        {
            switch (lang) 
            {
                case "fsharp" : return CAB402.FSharp.GeneticAlgorithm.Optimize;
                case "csharp": return CAB402.CSharp.GeneticAlgorithm.Optimize;
                default: throw new ArgumentOutOfRangeException("No such option.");
            }
        }

        static double TimeAlgorithm(string lang)
        {
            var cities = RandomMonad.evaluateWith(new Random(), TSP.generateRandomCities(indivsNum));
            FSharpFunc<int[], double> fitnessFunc = TSP.TSPCost(cities);
            var startTime = DateTime.Now;
            var results = Optimize(lang)(fitnessFunc, indivsNum, indivsNum).Take(steps).ToArray();
            return (DateTime.Now - startTime).TotalSeconds;
        }

        static void Main()
        {
            PrintAvgRunTime("csharp");
            PrintAvgRunTime("fsharp");
        }
    }
}
