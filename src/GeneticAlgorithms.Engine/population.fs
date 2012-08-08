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
            | true -> Some (snd (fittest.Force ()))
            | false -> None

        //Cross over the population, choosing individuals via tournament selection
        let mixPopulation conserved = 

            let mixIndividuals () = 
                async {
                    
                    let i1 = algorithm.Select individuals
                    let i2 = algorithm.Select individuals

                    return algorithm.Mix i1 i2
                }

            let sizeOffset = 
                match conserved with
                | Some _ -> 1
                | None -> 0

            let targetSize = (List.length individuals) - sizeOffset

            let individuals' = 
                Async.Parallel (Seq.init targetSize (fun _ -> mixIndividuals ()))
                |> Async.RunSynchronously
                |> Array.toList

            match conserved with
            | Some individual -> individual :: individuals'
            | _ -> individuals'

        //Create new set of individuals and then mutate
        let individuals' = 
            conserved
            |> mixPopulation
            |> List.toArray
            |> Array.Parallel.map algorithm.Mutate
            |> Array.toList

        Population ((generationNo + 1), individuals', fitnessCalculator, algorithm)


    