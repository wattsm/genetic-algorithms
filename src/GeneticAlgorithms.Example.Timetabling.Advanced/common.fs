namespace GeneticAlgorithms.Example.Timetabling.Advanced          

open System

[<AutoOpen>]
module Common = 

    let private rnd = Random (Environment.TickCount)

    let random min max = 
        rnd.Next (min, (max + 1))

    let randomChoice (freq : decimal) = 
        (random 1 100) >= int (freq * 100m)

    let (--) i = 
        i - 1

    let randomItemOrNone list = 
        match list with
        | [] ->  None
        | _ -> 

            let index = 
                list
                |> List.length 
                |> (--)
                |> (random 0)

            Some (List.nth list index)

    let randomItem list = 
        match (randomItemOrNone list) with
        | None -> raise (ArgumentException ())
        | Some item -> item

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
        