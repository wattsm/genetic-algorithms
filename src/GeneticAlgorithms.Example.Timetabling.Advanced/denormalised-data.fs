namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic

[<RequireQualifiedAccess>]
module Denormalised = 

    type LessonGroup = {
        GroupCode : string option;
        ModuleCode : string;
        Lessons : Lesson list;
    }

    type Settings = {
        RoomsByLocation : (string * string) list;
        RoomCapacities : (string * int) list;
        RoomTypes : (string * string) list;
        LessonGroupsByWeek : (int * LessonGroup list) list;
        LessonRoomTypes : (string * string) list;
        SlotsPerDay : int;
        GroupedModules : (string * string) list;
        GroupedLessons : (string * string) list;
        ClassSizes : (string * int) list;
    }    
            
    let getClassSize =
        memoised (fun settings moduleCode -> 

                settings.ClassSizes
                |> List.find (fun (moduleCode', _) -> moduleCode' = moduleCode)
                |> snd
                
            )

    let getCapacity = 
        memoised (fun settings roomCode -> 

                settings.RoomCapacities
                |> List.find (fun (roomCode', _) -> roomCode' = roomCode)
                |> snd

            )

    let getRoomType =
        memoised (fun settings roomCode -> 
                
                settings.RoomTypes
                |> List.find (fun (roomCode', _) -> roomCode' = roomCode)
                |> snd
            )

    let getLessonRoomType = 
        memoised (fun settings lessonCode ->

                settings.LessonRoomTypes
                |> List.find (fun (lessonCode', _) -> lessonCode' = lessonCode)
                |> snd

            )

    let rec getLessonRoomTypes (modules : Module list) = 
        match modules with
        | [] -> []
        | m::ms ->

            let roomTypes = 
                m.Lessons
                |> List.map (fun lesson -> (lesson.LessonCode, lesson.RoomTypeCode))

            roomTypes @ (getLessonRoomTypes ms)

    let rec getClassSizes (modules : Module list) = 
        match modules with
        | [] -> []
        | m::ms ->

            (m.ModuleCode, m.ClassSize) :: (getClassSizes ms)

    let rec getGroupedLessons (modules : Module list) =
        match modules with
        | [] -> []
        | m::ms ->

            let grouped = 
                m.Lessons
                |> List.choose (fun lesson -> 
                        match lesson.GroupCode with
                        | Some groupCode -> Some (lesson.LessonCode, groupCode)
                        | _ -> None
                    )

            grouped @ (getGroupedLessons ms)

    let rec getGroupedModules (modules : Module list) = 
        match modules with
        | [] -> []
        | m::ms ->

            match m.GroupCode with
            | None -> (getGroupedModules ms)
            | Some groupCode -> (m.ModuleCode, groupCode) :: (getGroupedModules ms)

    let rec getLessonGroupsByWeek (modules : Module list) weekNos = 

        let isActiveFor weekNo (lesson : Lesson) = 
            lesson.Weeks
            |> List.exists ((=) weekNo)

        let getGroupCodes (lessons : Lesson list) = 
            lessons
            |> Seq.choose (fun lesson -> lesson.GroupCode)            
            |> Seq.distinct
            |> Seq.toList

        let getGroupLessons (lessons : Lesson list) groupCode = 
            lessons
            |> List.filter (fun lesson -> lesson.GroupCode = groupCode)

        let lessonModuleCodes = 
            modules
            |> List.collect (fun m ->
                    m.Lessons
                    |> List.map (fun lesson -> (lesson.LessonCode, m.ModuleCode))
                )

        let getModuleCode (lesson : Lesson) = 
            lessonModuleCodes
            |> List.pick (fun (lessonCode, moduleCode) -> 
                    if (lessonCode = lesson.LessonCode) then
                        Some moduleCode
                    else
                        None
                )

        match weekNos with
        | [] -> [] 
        | w::ws ->

            let lessons = 
                modules
                |> List.collect (fun m -> 
                        m.Lessons
                        |> List.filter (isActiveFor w)
                    )

            let codes = 
                getGroupCodes lessons

            let grouped = 
                codes
                |> List.map (fun code -> 

                        let lessons = getGroupLessons lessons (Some code)                        
                        let moduleCode = getModuleCode (List.head lessons)

                        {
                            GroupCode = Some code;
                            ModuleCode = moduleCode;
                            Lessons = lessons;
                        }
                    )

            let ungrouped = 
                getGroupLessons lessons None
                |> List.map (fun lesson -> 
                        {
                            GroupCode = None;
                            ModuleCode = (getModuleCode lesson);
                            Lessons = [ lesson; ];
                        }
                    )

            (w, grouped @ ungrouped) :: (getLessonGroupsByWeek modules ws)

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
            LessonRoomTypes = (getLessonRoomTypes settings.Modules);
            SlotsPerDay = settings.SlotsPerDay;
            GroupedModules = (getGroupedModules settings.Modules);
            GroupedLessons = (getGroupedLessons settings.Modules);
            ClassSizes = (getClassSizes settings.Modules);
        }