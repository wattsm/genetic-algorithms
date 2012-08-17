namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module Modules = 

    //TODO Memoise these?

    let getGroupMembers settings moduleCode = 
        
        let m = 
            settings.Modules
            |> List.find (fun m' -> m'.ModuleCode = moduleCode)

        match m.GroupCode with
        | None -> None
        | Some groupCode ->

            let members = 
                settings.Modules
                |> List.choose (fun m -> 
                        match m.GroupCode with
                        | Some groupCode' when groupCode' = groupCode -> Some m.ModuleCode
                        | _ -> None
                    )

            Some members        

    let moduleClashes settings slot = 
        if (List.length slot.Events) <= 1 then
            0
        else
            
            let otherModules moduleCode = 
                slot.Events
                |> List.filter (fun event -> event.ModuleCode <> moduleCode)
                |> List.map (fun event -> event.ModuleCode)

            let isClashing moduleCode = 
                match (getGroupMembers settings moduleCode) with
                | None -> false
                | Some members ->

                    (otherModules moduleCode)
                    |> List.exists (fun code -> List.exists (fun code' -> code' = code) members)

            slot.Events
            |> List.map (fun event -> isClashing event.ModuleCode)
            |> List.filter id
            |> List.length

    let getClassSize settings moduleCode = 

        let m = 
            settings.Modules
            |> List.find (fun m' -> m'.ModuleCode = moduleCode)

        m.ClassSize
        

        