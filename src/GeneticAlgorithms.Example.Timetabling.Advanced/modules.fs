namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic

module Modules = 

    //TODO Add ModuleGroupCode to Event
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
            
            slot.Events
            |> List.choose (fun event -> getGroupCode settings event.ModuleCode)
            |> List.toSeq   
            |> Seq.countBy id
            |> Seq.filter (fun (_, count) -> (count > 1))
            |> Seq.length

    let getClassSize settings moduleCode = 

        let m = 
            settings.Modules
            |> List.find (fun m' -> m'.ModuleCode = moduleCode)

        m.ClassSize
        

        