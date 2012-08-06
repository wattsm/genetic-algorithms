namespace GeneticAlgorithms.Engine

open System

type Population<'i> private (generationNo, individuals, fitnessCalculator : IFitnessCalculator<'i>, algorithm : IAlgorithm<'i>) = 
    
    let fittest = lazy (
        
        let pick i1 i2 = 
            if (fst i1) > (fst i2) then
                i1
            else
                i2

        individuals 
        |> List.map (fun i -> (fitnessCalculator.CalculateFitness i, i))
        |> List.reduce pick
    )

    let size = 
        individuals
        |> List.length

    new (individuals, fitnessCalculator, algorithm) = Population (1, individuals, fitnessCalculator, algorithm)

    static member Create individuals fitnessCalculator algorithm =
        Population (0, individuals, fitnessCalculator, algorithm)

    member this.Fitness = 
        fst (fittest.Force ())

    member this.Fittest = 
        snd (fittest.Force ())

    member this.GenerationNo = 
        generationNo

    member this.Evolve () = 

        //In an elitist model we keep the fittest individual in every generation
        let conserved  = 
            match algorithm.IsElitist with
            | true -> [ snd (fittest.Force ()); ]
            | false -> []

        //Cross over the population, choosing individuals via tournament selection
        let rec mix individuals' = 
            match (List.length individuals') = size with
            | true -> individuals'
            | _ -> 

                let newIndividual = 
                    algorithm.Mix (algorithm.Select individuals) (algorithm.Select individuals)

                mix (newIndividual :: individuals')

        //Create new set of individuals and then mutate
        let individuals' = 
            conserved
            |> mix
            |> List.map algorithm.Mutate

        Population ((generationNo + 1), individuals', fitnessCalculator, algorithm)


    