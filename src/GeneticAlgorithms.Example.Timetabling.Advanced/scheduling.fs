namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Example.Timetabling.Advanced.Timetables

module Scheduling = 

    let eventsFor (settings : Denormalised.Settings) (lessons : Lesson list) = 
        lessons
        |> List.map (fun lesson -> 

                let locationCode, roomCode =
                    randomItem settings.RoomsByLocation

                { ModuleCode = lesson.ModuleCode; LessonCode = lesson.LessonCode; RoomCode = roomCode; LocationCode = locationCode }
            )

    let addGroupedEvents (settings : TimetableSettings) settings' week lessons = 

        let events = 
            eventsFor settings' lessons

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

    let addLessonEvents settings settings' week = 
        Lessons.organiseWeekLessons week.WeekNo settings
        |> List.fold (addGroupedEvents settings settings') week

    let removeModuleEvents moduleCode (timetable : Timetable) = 

        let removeSlotEvents slot = 
            { slot with Events = (List.filter (fun e -> e.ModuleCode <> moduleCode) slot.Events); }

        let removeDayEvents day = 
            {  day with Slots = (List.map removeSlotEvents day.Slots); }

        let removeWeekEvents week = 
            { week with Days = (List.map removeDayEvents week.Days); }

        { timetable with Weeks = (List.map removeWeekEvents timetable.Weeks); }

    let addModuleEvents (settings : TimetableSettings) settings' (module' : Module) (timetable : Timetable)  = 

        let addWeekLessons week = 
            module'.Lessons
            |> List.filter (Lessons.isActiveFor week.WeekNo)
            |> Lessons.createLessonGroups
            |> List.fold (addGroupedEvents settings settings') week
        
        { timetable with Weeks = (List.map addWeekLessons timetable.Weeks); }