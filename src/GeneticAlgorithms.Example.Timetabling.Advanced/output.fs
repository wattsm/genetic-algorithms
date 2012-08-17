namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

type Event = {
    ModuleCode : string;
    LessonCode : string;
    RoomCode : string;
}

type Slot = {
    SlotNo : int;
    Events : Event list;
}
with

    static member Empty slotNo = 
        { SlotNo = slotNo; Events = []; }

type WeekDay = {
    Day : DayOfWeek;
    Slots : Slot list;
}
with
    
    static member Empty day settings = 

        let slots = 
            List.init settings.SlotsPerDay (fun n -> Slot.Empty (n + 1))

        { Day = day; Slots = slots; }

type Week = {
    WeekNo : int;
    Days : WeekDay list;
}
with

    static member Empty weekNo settings = 
        
        let days = 
            [1..5]
            |> List.map enum<DayOfWeek>
            |> List.map (fun day -> WeekDay.Empty day settings)

        { WeekNo = weekNo; Days = days; }

type Timetable = {
    StartWeek : int;
    EndWeek : int;
    Weeks : Week list;
}
with

    static member Empty (settings : TimetableSettings) = 

        let size =
            (settings.EndWeek - settings.StartWeek) + 1

        let weeks = 
            List.init size (fun offset -> Week.Empty (settings.StartWeek + offset) settings)

        { StartWeek = settings.StartWeek; EndWeek = settings.EndWeek; Weeks = weeks; }