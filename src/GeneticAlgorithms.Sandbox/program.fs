namespace GeneticAlgorithms.Sandbox

open System
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Examples.Strings

module Program = 

    let _ = 
        printfn "Begin"

        let report (str : string) = 
            Console.WriteLine str

        let target = "darwin"

        let fitness = StringMatchFitnessCalculator.Create (target)
        let factory = StringFactory.Create (target.Length)
        let algorithm = StringAlgorithm.Create (fitness)
        let settings = { PopulationSize = 100; AcceptableFitness = 80m; MaxGenerations = 100; }

        let runner = Runner<string> (settings, factory, fitness, algorithm)
        let result = runner.Run (report)

        printfn "Complete"
        printfn "%A" result

        Console.ReadLine () |> ignore