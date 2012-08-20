namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

type Room = {
    RoomCode : string;
    TypeCode : string;
    Capacity : int;
}
with

    override this.ToString () =
        String.Format ("Room {0} (type: {1}, capacity: {2})", this.RoomCode, this.TypeCode, this.Capacity)

type Location = {
    LocationCode : String;
    Rooms : Room list;
    Locations : Location list;
}
with

    override this.ToString () =
        String.Format ("Location {0} (#rooms: {1}, #locations: {2})", this.LocationCode, (List.length this.Rooms), (List.length this.Locations))

type Lesson = {
    LessonCode : string;
    ModuleCode : string;
    GroupCode : string option;
    RoomTypeCode : string;
    LocationCode : string option;
    Weeks : int list;
}
with

    override this.ToString () =

        let groupCode = 
            match this.GroupCode with
            | Some code -> code
            | _ -> ""

        String.Format ("{0} / {1} (group: {2})", this.ModuleCode, this.LessonCode, groupCode)

type Module = {
    ModuleCode : string;
    ClassSize : int;
    GroupCode : string option;
    Lessons : Lesson list;    
}
with

    override this.ToString () =
        
        let groupCode = 
            match this.GroupCode with
            | Some code -> code
            | _ -> ""

        String.Format ("{0} (class size: {1}, group: {2}, #lessons: {3})", this.ModuleCode, this.ClassSize, groupCode, (List.length this.Lessons))

type TimetableSettings = {
    Modules : Module list;
    Locations : Location list;
    StartWeek : int;
    EndWeek : int;
    SlotsPerDay : int;
}

