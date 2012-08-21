namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module Timetables = 

    let addEvents events slot = 
        { slot with Events = (List.append slot.Events events); }

    let slotAt slotNo day = 
        day.Slots
        |> List.find (fun slot -> slot.SlotNo = slotNo)

    let replaceSlot slot day = 

        let slots' =
            day.Slots
            |> replaceItem slot (fun slot' -> slot'.SlotNo = slot.SlotNo)

        { day with Slots = slots'; }

    let dayOn day week = 
        week.Days
        |> List.find (fun day' -> day'.Day = day)

    let replaceDay day week = 

        let days' = 
            week.Days
            |> replaceItem day (fun day' -> day'.Day = day.Day)

        { week with Days = days'; }

    let weekAt timetable weekNo = 
        timetable.Weeks
        |> List.find (fun week -> week.WeekNo = weekNo)

    let replaceWeek timetable week = 

        let weeks' = 
            timetable.Weeks
            |> replaceItem week (fun week' -> week'.WeekNo = week.WeekNo)

        { timetable with Weeks = weeks'; }

    let flatten timetable = 
        timetable.Weeks
        |> List.collect (fun week -> week.Days)
        |> List.collect (fun day -> day.Slots)

    let roomClashes = 
        flatten >> (List.map Rooms.roomClashes) >> List.sum

    let moduleClashes settings =
        flatten >> (List.map (Modules.moduleClashes settings)) >> List.sum

    let lessonClashes settings = 
        flatten >> (List.map (Lessons.lessonClashes settings)) >> List.sum  