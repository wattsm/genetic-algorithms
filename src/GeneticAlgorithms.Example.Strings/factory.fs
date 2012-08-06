namespace GeneticAlgorithms.Examples.Strings

open System
open GeneticAlgorithms.Engine

type StringFactory (length) = 

    let lowerBound = 97
    let upperBound = 122 + 1
    let random = Random (Environment.TickCount)
    
    static member Create length = 
        StringFactory (length) :> IFactory<string>
    
    interface IFactory<string> with

        member this.Create () =

            Array.zeroCreate<string> length
            |> Array.map (fun _ -> Convert.ToChar (random.Next (lowerBound, upperBound)))
            |> String.Concat