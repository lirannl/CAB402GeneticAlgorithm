module CAB402.FSharp.Sorters

// The genes of one individual within the population. Each gene is an integer.
type Individual = int array

// The genes of an individual together with an assessment of the fitness of that individual
type ScoredIndividual = Individual * float

// a collection of scored individuals that make up a generation
type Population = ScoredIndividual array

// This function assumes that all elements from part exist in orderArr
let sortBasedOn (part: int[]) (orderArr: int[]): int[] =
    // Return only the elements from the orderArr which exist in the provided part. 
    // Since they'll come from the orderArr, they'll already be in order.
    Array.filter (fun (elem: int) -> Array.contains elem part) orderArr


// Sorts a population based on its' members fitnesses
let sortPopulation (pop: Population) =
    let IndividualScore (scored: ScoredIndividual) =
        let (_, score) = scored
        score
    // Order the population from the most fit to the least fit
    Array.sortByDescending IndividualScore pop