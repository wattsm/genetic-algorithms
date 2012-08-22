namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Collections

module Metrics = 

    let memoisedBySlot (f : Denormalised.Settings -> Slot -> 'a) = 

        let cache = Dictionary<_,_> ()
        let cacheLock = new Object ()

        fun settings (slot : Slot) ->
            if not (cache.ContainsKey slot.UniqueId) then
                lock cacheLock (fun () ->
                        if not (cache.ContainsKey slot.UniqueId) then

                            cache.Add (slot.UniqueId, (f settings slot))

                    )

            cache.[slot.UniqueId]

            

    let private getGroupCode groupCodes itemCode = 
        groupCodes
        |> List.tryPick (fun (itemCode', groupCode) -> 
                if (itemCode = itemCode') then
                    Some groupCode
                else
                    None
            )

    let private getClashCount sums =
        
        let count = (snd sums)

        if (count > 1) then
            (count - 1) //Cannot clash with itself
        else
            0

    [<RequireQualifiedAccess>]
    module Clashes = 

        let clashesByTimetable settings timetable (bySlot : Denormalised.Settings -> Slot -> int) = 
            timetable.Weeks
            |> PSeq.sumBy (fun week -> 
                    week.Days
                    |> PSeq.sumBy (fun day ->
                            day.Slots
                            |> PSeq.sumBy (bySlot settings)                        
                        )
                )

        ///A room clash is a room being used more than once in a slot
        let roomClashesBySlot = 
            memoisedBySlot (fun _ slot ->
                    slot.Events
                    |> PSeq.countBy (fun event -> event.RoomCode)
                    |> PSeq.sumBy getClashCount
                )

        ///A module clash is two or more events from different modules that are in the same group in the same slot
        let moduleClashesBySlot = 
            memoisedBySlot (fun settings slot -> 

                    let getGroupCode' moduleCode = 
                        getGroupCode settings.GroupedModules moduleCode

                    slot.Events
                    |> PSeq.map (fun event -> event.ModuleCode)
                    |> PSeq.distinct
                    |> PSeq.choose getGroupCode'
                    |> PSeq.countBy id
                    |> PSeq.sumBy getClashCount
                )

        ///A lesson clash is when two or more lessons from the same module but in different lesson groups are in the same slot
        let lessonClashesBySlot = 
            memoisedBySlot (fun settings slot -> 

                    let getGroupCode' event = 
                        getGroupCode settings.GroupedLessons event.LessonCode

                    let lessonClashesByModule moduleCode = 

                        let groupCount = 
                            slot.Events
                            |> PSeq.choose (fun event ->
                                    if (event.ModuleCode = moduleCode) then
                                        Some (getGroupCode' event)
                                    else
                                        None
                                )
                            |> PSeq.distinct
                            |> PSeq.length            

                        (groupCount - 1)
         
                    slot.Events
                    |> PSeq.map (fun event -> event.ModuleCode)
                    |> PSeq.distinct
                    |> PSeq.sumBy lessonClashesByModule
                )

    [<RequireQualifiedAccess>]
    module Counts = 

        let countLessons modules = 
            modules
            |> PSeq.sumBy (fun m -> List.length m.Lessons)

        let rec countLocations (locations : Location list) = 
            match locations with
            | [] -> 0
            | l::ls ->
                1
                + (List.length l.Locations)
                + (countLocations ls)

        let rec countRooms locations = 
            match locations with
            | [] -> 0
            | l::ls ->
                (List.length l.Rooms) 
                + (countRooms l.Locations) 
                + (countRooms ls)