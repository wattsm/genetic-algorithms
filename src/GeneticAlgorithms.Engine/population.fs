namespace GeneticAlgorithms.Engine

open System
open Microsoft.FSharp.Collections
open System.Diagnostics

[<Measure>] type ms

type Population<'i> private (generationNo, generationTime : int64<ms>, individuals, fitnessCalculator : IFitnessCalculator<'i>, algorithm : IAlgorithm<'i>) = 
    
    let fittest = 
        individuals
        |> PSeq.map (fun i -> (fitnessCalculator.CalculateFitness i, i))
        |> PSeq.maxBy fst

    let size = 
        individuals
        |> List.length

    static member Create individuals fitnessCalculator algorithm =
        Population (0, 0L<ms>, individuals, fitnessCalculator, algorithm)

    member this.Fitness = 
        fst fittest

    member this.Fittest = 
        snd fittest

    member this.GenerationNo = 
        generationNo

    member this.GenerationTime = 
        generationTime

    member this.Evolve () = 

        let stopwatch = Stopwatch.StartNew ()

        //In an elitist model we keep the fittest individual in every generation
        let conserved  = 
            match algorithm.IsElitist with
            | true -> Some this.Fittest
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

        stopwatch.Stop ()

        let generationTime = (stopwatch.ElapsedMilliseconds * 1L<ms>)

        Population ((generationNo + 1), generationTime, individuals', fitnessCalculator, algorithm)


    