namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Example.Timetabling.Advanced.Timetables

module Scheduling = 

    let randomRoomFor settings (lesson : Lesson) = 

        let classSize = Modules.getClassSize settings lesson.ModuleCode

        let locations = 
            match lesson.LocationCode with
            | Some locationCode -> [ (Locations.findLocation settings locationCode); ]
            | _ -> settings.Locations

        let requirements =
            [
                (Rooms.ofType lesson.RoomTypeCode);
                (Rooms.withCapacity classSize);
            ]

        locations
        |> List.collect (fun l -> 

                Rooms.findRooms requirements l
                |> List.map (fun r -> (l.LocationCode, r.RoomCode))

            )
        |> randomItemOrNone

    let eventsFor settings (lessons : Lesson list) = 
        lessons
        |> List.map (fun lesson -> 

                let locationCode, roomCode = 
                    match (randomRoomFor settings lesson) with
                    | None -> ("", "")
                    | Some (lc, rc) -> (lc, rc)

                { ModuleCode = lesson.ModuleCode; LessonCode = lesson.LessonCode; RoomCode = roomCode; LocationCode = locationCode }
            )

    let addEvent settings week event =

        let dayOfWeek = enum<DayOfWeek> (random 1 5)
        let slotNo = random 1 settings.SlotsPerDay

        let day =
            week
            |> dayOn dayOfWeek
        
        let slot =
            day
            |> slotAt slotNo
            |> addEvents [ event; ]

        week
        |> replaceDay (day |> replaceSlot slot)

    let addEventsFor settings week lessons = 
        eventsFor settings lessons
        |> List.fold (addEvent settings) week

    let addEventsTo settings week = 
        Lessons.organiseLessons week.WeekNo settings
        |> List.fold (addEventsFor settings) week