namespace GeneticAlgorithms.Engine

open System

module Tournaments =

    let private random = Random (Environment.TickCount)

    let select<'i> size (fitness : IFitnessCalculator<'i>) (individuals : 'i list) = 

        let pickIndividual () = 
            
            (random.Next (0, (List.length individuals)))
            |> List.nth individuals            

        (List.init size (fun _ -> pickIndividual ()))
        |> List.maxBy (fun i -> fitness.CalculateFitness i)
             
