namespace GeneticAlgorithms.Example.Timetabling

module Clashes = 

    let countSlotClashesBy (getCode : Lecture -> string) (slot : SlotSchedule) = 
        slot.Lectures
        |> Seq.countBy getCode
        |> Seq.filter (fun (key, count) -> count > 1)
        |> Seq.length        

    let countDayClashesBy (getCode : Lecture -> string) (day : DaySchedule)  = 
        day.Slots
        |> List.map (countSlotClashesBy getCode)
        |> List.sum

    let countTimetableClashesBy (getCode : Lecture -> string) (timetable : Timetable) = 
        timetable.Days
        |> List.map (countDayClashesBy getCode)
        |> List.sum

    let countSlotClashes (slot : SlotSchedule) = 
        (countSlotClashesBy (fun l -> l.RoomCode) slot)
            + (countSlotClashesBy (fun l -> l.TutorCode) slot)
            + (countSlotClashesBy (fun l -> l.CourseCode) slot)

    let countTimetableClashes (timetable : Timetable) = 
        (countTimetableClashesBy (fun l -> l.RoomCode) timetable)
            + (countTimetableClashesBy (fun l -> l.TutorCode) timetable)
            + (countTimetableClashesBy (fun l -> l.CourseCode) timetable)
        
    let slotHasClashes (slot : SlotSchedule) = 
        (countSlotClashes slot) > 0

    let dayHasClashes (day : DaySchedule) = 
        day.Slots
        |> List.exists slotHasClashes
        