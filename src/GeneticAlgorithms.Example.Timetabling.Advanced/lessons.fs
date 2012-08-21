namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic

module Lessons = 

    let getGroupCode = 

        let cache = Dictionary<_,_> ()

        fun settings moduleCode lessonCode ->
            
            let key = String.Format ("{0}-{1}", moduleCode, lessonCode)

            if (cache.ContainsKey key) then
                cache.[key]
            else

                let code = 
                    settings.Modules
                    |> List.find (fun m -> m.ModuleCode = moduleCode)
                    |> (fun m -> m.Lessons)
                    |> List.find (fun l -> l.LessonCode = lessonCode)
                    |> (fun l -> l.GroupCode)

                cache.Add (key, code)

                code

    let lessonClashes settings slot = 

        let events' =
            slot.Events
            |> List.toSeq

        let count moduleCode = 
            events'
            |> Seq.choose (fun event ->
                        
                    if (event.ModuleCode = moduleCode) then
                        Some (getGroupCode settings moduleCode event.LessonCode)
                    else
                        None

                )
            |> Seq.distinct
            |> Seq.length

        events'
        |> Seq.map (fun event -> event.ModuleCode)
        |> Seq.distinct
        |> Seq.sumBy (fun code -> (count code) - 1) //Events can't clash with themselves

    let countLessons (modules : Module list) = 
        modules
        |> List.map (fun m -> List.length m.Lessons)
        |> List.sum        

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

    let createLessonGroups (lessons : Lesson list) = 

        let ungrouped =
            lessons
            |> List.filter (fun l -> l.GroupCode = None) 
            |> List.map (fun l -> l :: [])          
            
        let grouped = 
            (getGroupCodes lessons)
            |> List.map (getGroupLessons lessons)

        List.append ungrouped grouped
        
    let organiseWeekLessons weekNo settings = 
        activeLessonsFor weekNo settings
        |> createLessonGroups

    let organiseLessons weekNo lessons = 
        lessons
        |> List.filter (isActiveFor weekNo)
        |> createLessonGroups

    
        