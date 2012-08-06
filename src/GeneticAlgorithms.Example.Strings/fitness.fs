namespace GeneticAlgorithms.Examples.Strings

open System
open GeneticAlgorithms.Engine

type StringMatchFitnessCalculator (target : string) = 

    let length = target.Length

    let toCharList (str : string) = 
        str.ToCharArray ()
        |> Array.toList

    static member Create target = 
        StringMatchFitnessCalculator (target) :> IFitnessCalculator<string>
    
    interface IFitnessCalculator<string> with

        member this.CalculateFitness str = 
            
            let count = 
                str
                |> toCharList
                |> List.zip (toCharList target)
                |> List.map (fun (actual, expected) -> actual = expected)
                |> List.filter (fun r -> r)
                |> List.length

            ((decimal count) / (decimal length)) * 100m

            