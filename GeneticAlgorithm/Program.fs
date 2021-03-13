module CAB402.FSharp.GeneticAlgorithm

open RandomMonad
open Sorters

// The genes of one individual within the population. Each gene is an integer.
type Individual = int array

// The genes of an individual together with an assessment of the fitness of that individual
type ScoredIndividual = Individual * float

// a collection of scored individuals that make up a generation
type Population = ScoredIndividual array

// Find an individual within the population that has the highest fitness
let fitest (population: Population) : ScoredIndividual =
    // Destructure the parameters
    let reducer ((maxIndiv, maxFlt): ScoredIndividual) ((currIndiv, currFlt): ScoredIndividual) =
        // Restructure the parameters for the return value
        if currFlt > maxFlt then
            (currIndiv, currFlt)
        else
            (maxIndiv, maxFlt)

    Array.reduce reducer population

// Given a set of competeting individuals, return the winning individual (i.e. one with best fitness)
let tournamentWinner (competitors: Population) : Individual =
    // Destructure the Individual from the ScoredIndividual that had the best score
    let (indiv, _) = fitest competitors
    indiv

// Randomly select an individual from a population by conducting a tournament.
// A field of n = 2 competitors is first randomly generated and then the best individual within tht field is selected.
// Each of individuals selected for the competition is selected independently,
// so it is possible that the same individual may be selected more than once.
let tournamentSelect (population: Population) : Rand<Individual> =
    let n = 2

    rand {
        // Destructure a random individual from the population
        let! competitors = randArrayInit n (fun _ -> chooseRandom population)

        return tournamentWinner competitors
    }

// Combine the genes of parent1 and parent2 based on the given splitPoint
// The splitpoint will always be between 1 and length-1, where length is the length of both parent genes)
// The genes in position 0..splitPoint will come directly from the corresponding genes of parent1
// It is important the genes of the generated child is a legal permutation
// i.e. it should include each of the integers between 0 and length-1 precisely once.
// The order of the remaining genes of the child (those that were not inherited from parent1) are
// determined by the order that they occurred in parent2.
// For example if parent1 = [0,3,5,4,2,1,6] and parent2 = [6,4,2,1,0,3,5] and the splitpoint is 4
// then the first 4 genes come from parent1 [0,3,5,4] and the remaining genes [2,1,6] are ordered
// according to parent2 i.e. [6,2,1] because 6 comes before 2 and 2 comes before 1 in parent2.
// So the child in this example will be [0,3,5,4,6,2,1]
let crossAt (parent1: Individual) (parent2: Individual) (splitPoint: int) : Individual =
    let (parent1Genes, remaining) = Array.splitAt splitPoint parent1
    let sortedRemaining = sortBasedOn remaining parent2
    Array.append parent1Genes sortedRemaining

// Combine the genes of parent1 and parent2 at a randonly choosen splitpoint as per the above crossAt algorithm
// The splitpoint is chosen so that both parents provide at least one gene to the child
let cross (parent1: Individual) (parent2: Individual) : Rand<Individual> =
    // All individuals should have the same number of genes, therefore the length of parent1 is also the length of parent2.
    let parentLength = Array.length parent1

    rand {
        // Random valid split point that guarantees at least 1 gene from each parent
        let! splitPoint = intRange 1 (parentLength - 1)
        return crossAt parent1 parent2 splitPoint
    }

// Return a mutated version of the original genes
// the sequence of genes is split into 3 sections, a start, middle and end, based on the 2 provided indexes
// (where 0 <= firstIndex < secondIndex < genes.length)
// The start and end sections of the genes are left intact, while the genes in the middle section are reversed in order.
// For example reverseMutateAt [0,3,5,4,2,1,6] 2 4 = [0,3,2,4,5,1,6]
let reverseMutateAt (genes: Individual) (firstIndex: int) (secondIndex: int) : Individual =
    let (part1, rest) = Array.splitAt firstIndex genes

    let (part2, part3) =
        Array.splitAt ((secondIndex - firstIndex) + 1) rest

    Array.concat (
        seq {
            yield part1
            yield Array.rev part2
            yield part3
        }
    )

// Perform a reverse mutation based on two randomly chosen split points
// (such that 0 <= firstIndex < secondIndex < genes.length)
let reverseMutate (chromosome: Individual) : Rand<Individual> =
    let chromosomeLength = Array.length chromosome

    rand {
        let! splitPoint0 = intRange 0 (chromosomeLength - 1)
        let! splitPoint1 = intRange (splitPoint0 + 1) chromosomeLength
        return reverseMutateAt chromosome splitPoint0 splitPoint1
    }

let MutateProbability = 0.15

// Perform a reverse mutation of the given genes with probability 0.15,
// otherwise leave the sequence unaltered.
let possiblyMutate (genes: Individual) : Rand<Individual> =
    rand {
        let! shouldMutate = withProbability MutateProbability

        if shouldMutate then
            return! reverseMutate genes
        else
            return genes
    }

// Create a new population that consists of all of the children, plus the 10 best individuals from the previous generation.
let elitismSelection (parents: Population) (children: Population) : Population =
    let bestParents = Array.sub (sortPopulation parents) 0 10

    Array.concat (
        seq {
            yield children
            yield bestParents
        }
    )

// Create a scored individual by applying the fitness function to assess the fitness of the given genes.
let score (fitnessFunction: Individual -> float) (genes: Individual) : ScoredIndividual = (genes, fitnessFunction genes)

// Randomly generate a population containing the specified number of individuals, each with the specified number of genes.
let randomIndividuals
    (fitnessFunction: Individual -> float)
    (numberGenes: int)
    (numberIndividuals: int)
    : Rand<Population> =
    let scorer = score fitnessFunction
    let indivGen (_: int) : Rand<Individual> = shuffle [| 0 .. (numberGenes - 1) |]

    rand {
        let! pop = randArrayInit numberIndividuals indivGen
        return Array.map scorer pop
    }

// Generate a child by first randomly choosing two parents using tournament selection,
// cross their genes and then optionally mutate the resulting genes.
// Note: individuals have no gender and each parent is chosen independently,
// so there is a small chance that the same individual may be choosen twice.
let procreate fitnessFunction (population: Population) : Rand<ScoredIndividual> =
    rand {
        let! parent1 = tournamentSelect population
        let! parent2 = tournamentSelect population

        let! unMutatedChild = cross parent1 parent2
        let! child = possiblyMutate unMutatedChild
        return score fitnessFunction child
    }

// Create a new generation by creating the specified number of children through procreation and then
// applying elitism selection to create the population of the next generation
let evolveOneGeneration fitnessFunction (parentPopulation: Population) (childPopulationLimit: int) : Rand<Population> =
    rand {
        let! childPop = randArrayInit childPopulationLimit (fun _ -> procreate fitnessFunction parentPopulation)

        return elitismSelection parentPopulation childPop
    }

// Starting with the specified initial population, evolve generation by generation (forever).
// For each population, we determine the fitest individual from that generation and return an infinite sequence of these individuals.
// Due to elitism selection, the fitest individual in each succcessive generation should be at least as good as the previous generation.
let evolveForever
    fitnessFunction
    (initialPopulation: Population)
    (childPopulationLimit: int)
    : Rand<ScoredIndividual seq> =
    let evolver (pop: Population) =
        (fitest pop, evolveOneGeneration fitnessFunction pop childPopulationLimit)

    randSeqUnfold evolver initialPopulation

let Optimize fitnessFunction numberOfGenes numerOfIndividuals : ScoredIndividual seq =
    let solutions =
        rand {
            let! initialPopulation = randomIndividuals fitnessFunction numberOfGenes numerOfIndividuals
            return! evolveForever fitnessFunction initialPopulation numerOfIndividuals
        }

    let random = new System.Random()
    RandomMonad.evaluateWith random solutions
