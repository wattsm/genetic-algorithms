namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module Lessons = 

    let isActiveFor weekNo (lesson : Lesson) = 
        lesson.Weeks
        |> List.exists ((=) weekNo)

    let activeLessonsFor weekNo settings = 
        settings.Modules
        |> List.collect (fun m -> m.Lessons)
        |> List.filter (isActiveFor weekNo)

    let getGroupCodes lessons = 

        let rec gather (lessons : Lesson list) codes = 
            match lessons with
            | [] -> codes
            | l::ls ->

                let codes' = 
                    match (l.GroupCode) with
                    | Some code when not (List.exists (fun c -> c = code) codes) -> code :: codes
                    | _ -> codes
                
                gather ls codes'                

        gather lessons []

    let isInGroup (lesson : Lesson) groupCode = 
        lesson.GroupCode = Some groupCode

    let rec getGroupLessons (lessons : Lesson list) groupCode = 
        match lessons with
        | [] -> [] 
        | l::ls ->
            if not (isInGroup l groupCode) then
                getGroupLessons ls groupCode
            else
                l :: (getGroupLessons ls groupCode)
        
    let organiseLessons weekNo settings = 

        let activeLessons = 
            (activeLessonsFor weekNo settings)

        let ungrouped =
            activeLessons
            |> List.filter (fun l -> l.GroupCode = None) 
            |> List.map (fun l -> l :: [])          
            
        let grouped = 
            (getGroupCodes activeLessons)
            |> List.map (getGroupLessons activeLessons)

        List.append ungrouped grouped
        
        