namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

type Room = {
    RoomCode : string;
    TypeCode : string;
    Capacity : int;
}

type Location = {
    LocationCode : String;
    Rooms : Room list;
    Locations : Location list;
}

type Lesson = {
    LessonCode : string;
    ModuleCode : string;
    GroupCode : string option;
    RoomTypeCode : string;
    LocationCode : string option;
    Weeks : int list;
}

type Module = {
    ModuleCode : string;
    ClassSize : int;
    Lessons : Lesson list;
    GroupCode : string option;
}

type TimetableSettings = {
    Modules : Module list;
    Locations : Location list;
    StartWeek : int;
    EndWeek : int;
    SlotsPerDay : int;
}

