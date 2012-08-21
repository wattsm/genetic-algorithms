namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic

module Modules = 

    let getGroupCode =
        
        let cache = Dictionary<_,_> (StringComparer.OrdinalIgnoreCase)

        fun settings  moduleCode -> 
            if (cache.ContainsKey moduleCode) then
                cache.[moduleCode]
            else
                
                let code = 
                    settings.Modules
                    |> List.find (fun m -> m.ModuleCode = moduleCode)
                    |> (fun m -> m.GroupCode)

                cache.Add (moduleCode, code)

                code  

    let moduleClashes settings slot = 
        if (List.length slot.Events) <= 1 then
            0
        else

            (**
                A module clash is two modules from the same group scheduled
                at the same time.
            **)
            slot.Events
            |> List.toSeq
            |> Seq.map (fun event -> event.ModuleCode)
            |> Seq.distinct
            |> Seq.choose (getGroupCode settings)
            |> Seq.countBy id
            |> Seq.sumBy (fun (_, count) -> (count - 1)) //Events can't clash with themselves

    let getClassSize settings moduleCode = 

        let m = 
            settings.Modules
            |> List.find (fun m' -> m'.ModuleCode = moduleCode)

        m.ClassSize
        

        