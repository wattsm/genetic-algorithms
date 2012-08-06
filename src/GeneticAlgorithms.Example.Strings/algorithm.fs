namespace GeneticAlgorithms.Examples.Strings

open System
open GeneticAlgorithms.Engine

type StringAlgorithm (fitnessCalculator : IFitnessCalculator<string>) = 

    [<Literal>]
    let TournamentSize = 5

    [<Literal>]
    let LowerBound = 97

    [<Literal>]
    let UpperBound = 122

    let random = Random (Environment.TickCount)

    static member Create fitnessCalculator =
        StringAlgorithm (fitnessCalculator) :> IAlgorithm<string>

    interface IAlgorithm<string> with

        member this.IsElitist = true

        member this.Mix str1 str2 = 

            let pick strs = 
                if random.Next (1, 3) = 1 then
                    (fst strs)
                else
                    (snd strs)
                
            str1.ToCharArray ()
            |> Array.toList
            |> List.zip (str2.ToCharArray () |> Array.toList)
            |> List.map pick
            |> List.toArray
            |> String.Concat            

        member this.Mutate str = 

            let mutateChar c = 
                if random.Next (1, 6) = 5 then
                    
                    Convert.ToChar (random.Next (LowerBound, UpperBound))

                else
                    c

            str.ToCharArray ()
            |> Array.map mutateChar
            |> String.Concat

        member this.Select strings = 
            
            let rec pick picked =
                match (List.length picked) with
                | TournamentSize -> picked
                | _ ->

                    let index = random.Next (0, (List.length strings))
                    
                    pick ((List.nth strings index) :: picked)

            pick []
            |> List.maxBy (fun str -> fitnessCalculator.CalculateFitness str)
