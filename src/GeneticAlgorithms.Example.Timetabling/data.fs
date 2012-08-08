namespace GeneticAlgorithms.Example.Timetabling

open System

(** ASSUMPTIONS

 - The timetable for a day can be represented as 6 hour long slots (9-10, 10-11, 11-12, 13-14, 14-15 and 15-16).
 - A lecture is comprised of a room, a tutor and a course. 

**)

type Lecture = {
    RoomCode : String;
    TutorCode : String;
    CourseCode : String;
}

type SlotSchedule = {
    SlotNo : int;
    Lectures : Lecture list;
}
with

    static member Empty slotNo = 
        { SlotNo = slotNo; Lectures = []; }

type DaySchedule = {
    DayNo : int
    Slots : SlotSchedule list;
}
with
    
    static member Empty dayNo = 
        { 
            DayNo = dayNo; 
            Slots = (List.init 6 (fun slotNo -> SlotSchedule.Empty (slotNo + 1))); 
        }

type Timetable = {
    Days : DaySchedule list;
}
with

    static member Empty size = 
        {
            Days = List.init size (fun dayNo -> DaySchedule.Empty (dayNo  + 1)); 
        }

    member this.Length = 
        List.length this.Days

type TimetableSettings = {
    CourseCodes : String list;
    TutorCodes : String list;
    RoomCodes : String list;
    Size : int;
}