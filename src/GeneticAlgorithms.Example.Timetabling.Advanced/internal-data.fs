namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

//TODO Any benefit in making these arrays for parallelisation?

[<RequireQualifiedAccess>]
module Denormalised = 

    type LessonGroup = {
        GroupCode : string;
        Lessons : Lesson list;
    }

    type Settings = {

        RoomsByLocation : (string * string) list;
        RoomCapacities : (string * int) list;
        RoomTypes : (string * string) list;
        LessonGroupsByWeek : (int * LessonGroup list) list;

    }

    let rec getLessonGroupsByWeek (modules : Module list) weekNos = 
        match weekNos with
        | [] -> [] 
        | w::ws ->

            let lessons = 
                modules
                |> List.collect (fun m -> 
                        m.Lessons
                        |> List.filter (Lessons.isActiveFor w) 
                    )

            let codes = 
                Lessons.getGroupCodes lessons

            let groupedLessons = 
                codes
                |> List.map (fun code -> 
                        {
                            GroupCode = code;
                            Lessons = (Lessons.getGroupLessons lessons code);
                        }
                    )

            (w, groupedLessons) :: (getLessonGroupsByWeek modules ws)

    let rec getRoomTypes (locations : Location list) = 
        match locations with
        | [] -> []
        | l::ls ->

            let types = 
                l.Rooms
                |> List.map (fun room -> (room.RoomCode, room.TypeCode))

            types @ (getRoomTypes ls)

    let rec getRoomCapacities (locations : Location list) = 
        match locations with
        | [] -> []
        | l::ls ->

            let capacities = 
                l.Rooms
                |> List.map (fun room -> (room.RoomCode, room.Capacity))

            capacities @ (getRoomCapacities ls)
        

    let rec getRoomsByLocation (locations : Location list) = 
        match locations with
        | [] -> []
        | l::ls ->

            let rooms = 
                l.Rooms
                |> List.map (fun room -> (l.LocationCode, room.RoomCode))

            rooms @ (getRoomsByLocation ls)

    let getSettings settings = 
        {
            RoomsByLocation = getRoomsByLocation settings.Locations;
            RoomCapacities = getRoomCapacities settings.Locations;
            RoomTypes = getRoomTypes settings.Locations;
            LessonGroupsByWeek = (getLessonGroupsByWeek settings.Modules [settings.StartWeek .. settings.EndWeek]);
        }