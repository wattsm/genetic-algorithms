namespace GeneticAlgorithms.Example.Timetabling.Advanced          

open System
open System.Threading

type RandomWrapper private () = 

    static let seed =
        Environment.TickCount

    static let threadLocalRandom = 
        new ThreadLocal<Random>(fun () -> 

                let seed' = Interlocked.Increment (ref seed)
        
                Random (seed')
            )

    static member GetRandom () =
        threadLocalRandom.Value

[<AutoOpen>]
module Common = 

    let random min max = 
            
        let rnd = RandomWrapper.GetRandom ()

        rnd.Next (min, (max + 1))

    let randomChoice (freq : decimal) = 
        decimal (random 1 100) <= (freq * 100m)

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
        