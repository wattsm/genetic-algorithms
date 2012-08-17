namespace GeneticAlgorithms.Example.Timetabling.Advanced          

open System

[<AutoOpen>]
module Common = 

    let private rnd = Random (Environment.TickCount)

    let random min max = 
        rnd.Next (min, (max + 1))

    let randomItem list = 

        let index = 
            list
            |> List.length 
            |> ((-) 1)
            |> (random 0)

        List.nth list index

    let rec replaceItem item selector list = 
        match list with
        | [] -> []
        | h::t -> 
            
            let h' = 
                if (selector h) then
                    item
                else
                    h

            h' :: (replaceItem item selector t)


    let rec satisfiesAll predicates item = 
        match predicates with
        | [] -> true
        | p::ps ->
            if not (p item) then
                false
            else
                satisfiesAll ps item

