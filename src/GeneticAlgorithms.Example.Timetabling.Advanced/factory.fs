namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling.Advanced.Timetables

type TimetableFactory (settings) =

    (* Helper functions *)
    let randomRoomFor (lesson : Lesson) = 

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
        |> List.collect (Rooms.findRooms requirements)
        |> randomItem

    let eventsFor (lessons : Lesson list) = 
        lessons
        |> List.map (fun lesson -> 

                let room = 
                    randomRoomFor lesson

                { ModuleCode = lesson.ModuleCode; LessonCode = lesson.LessonCode; RoomCode = room.RoomCode; }
            )

    let addEventsFor week lessons = 

        let events = 
            eventsFor lessons

        let dayOfWeek = enum<DayOfWeek> (random 1 5)
        let slotNo = random 1 settings.SlotsPerDay

        let day =
            week
            |> dayOn dayOfWeek
        
        let slot =
            day
            |> slotAt slotNo
            |> addEvents events

        week
        |> replaceDay (day |> replaceSlot slot)

    let addEventsTo week = 
        Lessons.organiseLessons week.WeekNo settings
        |> List.fold addEventsFor week

    let empty = 
        { StartWeek = settings.StartWeek; EndWeek = settings.EndWeek; Weeks = []; }

    (* Static factory method *)
    static member Create settings = 
        TimetableFactory (settings) :> IFactory<Timetable>
    
    (* Interface implementation *)
    interface IFactory<Timetable> with

        member this.Create () = 
            [for weekNo in settings.StartWeek .. settings.EndWeek -> Week.Empty weekNo settings]
            |> List.map addEventsTo
            |> List.fold Timetables.replaceWeek empty