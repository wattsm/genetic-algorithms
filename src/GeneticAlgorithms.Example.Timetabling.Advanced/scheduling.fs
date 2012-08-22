namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module Scheduling = 

    let dayOn dayOfWeek week = 
        week.Days
        |> List.find (fun day -> day.Day = dayOfWeek)

    let slotAt slotNo day = 
        day.Slots
        |> List.find (fun slot -> slot.SlotNo = slotNo)

    let addEvents events slot =
        { slot with UniqueId = Guid.NewGuid (); Events = (events @ slot.Events); }

    let replaceWeek timetable week = 
        
        let weeks = 
            timetable.Weeks
            |> replaceItem week (fun week' -> week'.WeekNo = week.WeekNo)

        { timetable with Weeks = weeks; }

    let replaceDay week day = 

        let days = 
            week.Days
            |> replaceItem day (fun day' -> day'.Day = day.Day)

        { week with Days = days; }

    let replaceSlot day slot = 
        
        let slots = 
            day.Slots
            |> replaceItem slot (fun slot' -> slot'.SlotNo = slot.SlotNo) 

        { day with Slots = slots; }

    let eventsFor (settings : Denormalised.Settings) (lessons : Lesson list) = 
        lessons
        |> List.map (fun lesson -> 

                let locationCode, roomCode =
                    randomItem settings.RoomsByLocation

                { ModuleCode = lesson.ModuleCode; LessonCode = lesson.LessonCode; RoomCode = roomCode; LocationCode = locationCode }
            )

    let addGroupedEvents settings week (group : Denormalised.LessonGroup) = 

        let events = 
            eventsFor settings group.Lessons

        let dayOfWeek = enum<DayOfWeek> (random 1 5)
        let slotNo = random 1 settings.SlotsPerDay

        let day =
            week
            |> dayOn dayOfWeek
        
        let slot =
            day
            |> slotAt slotNo
            |> addEvents events

        slot 
        |> (replaceSlot day) 
        |> (replaceDay week) 

    let getGroupedLessons (settings : Denormalised.Settings) weekNo = 
        settings.LessonGroupsByWeek
        |> List.pick (fun (weekNo', groups) -> 
                if (weekNo' = weekNo) then
                    Some groups
                else
                    None
            )

    let addLessonEvents (settings : Denormalised.Settings) week = 
        getGroupedLessons settings week.WeekNo
        |> List.fold (addGroupedEvents settings) week

    let removeModuleEvents moduleCode (timetable : Timetable) = 

        let removeSlotEvents slot = 
            { slot with UniqueId = Guid.NewGuid (); Events = (List.filter (fun e -> e.ModuleCode <> moduleCode) slot.Events); }

        let removeDayEvents day = 
            {  day with Slots = (List.map removeSlotEvents day.Slots); }

        let removeWeekEvents week = 
            { week with Days = (List.map removeDayEvents week.Days); }

        { timetable with Weeks = (List.map removeWeekEvents timetable.Weeks); }

    let addModuleEvents (settings : Denormalised.Settings) (module' : Module) (timetable : Timetable)  = 

        let addWeekLessons week = 
            getGroupedLessons settings week.WeekNo
            |> List.filter (fun group -> group.ModuleCode = module'.ModuleCode)
            |> List.fold (addGroupedEvents settings) week
        
        { timetable with Weeks = (List.map addWeekLessons timetable.Weeks); }