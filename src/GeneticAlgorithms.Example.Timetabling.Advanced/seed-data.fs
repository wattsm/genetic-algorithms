namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module SeedData = 

    module Settings = 

        let MinRooms = 1
        let MaxRooms = 5
        let MinLessons = 1
        let MaxLessons = 10
        let LessonGroupFrequency = 0.1m
        let LessonLocationFrequency = 0.2m
        let ModuleGroupFrequence = 0.2m
        let MinRoomCapacityUnits = 4
        let MaxRoomCapacityUnits = 10
        let RoomCapacityUnit = 5
        let MinClassSize = 15
        let MaxClassSize = 35

    let private randomChoice (freq : decimal) = 
        (random 1 100) >= int (freq * 100m)

    let generateRandomTypeCodes count = 
        List.init count (fun i -> String.Format ("T{0}", (string i)))

    let generateRandomRooms (typeCodes : string list) (locationCode : string) = 

        let generate n = 
            
            let code = String.Format ("{0}/RM-{1}", locationCode, (string n))
            let capacity = (random Settings.MinRoomCapacityUnits Settings.MaxRoomCapacityUnits) * Settings.RoomCapacityUnit
            let typeCode = randomItem typeCodes

            { RoomCode = code; TypeCode = typeCode; Capacity = capacity; }

        List.init (random Settings.MinRooms Settings.MaxRooms) generate

    let rec generateRandomLocations (typeCodes : string list) count = 

        let generate parentLocationCode n =

            let code = 
                match parentLocationCode with
                | None ->  String.Format ("LOC-{0}", (string n))
                | Some str -> String.Format ("{0}-{1}", str, (string n))

            let rooms = generateRandomRooms typeCodes code

            let locations = 
                match (random 1 3) with
                | 3 ->  generateRandomLocations typeCodes (random 1 3)
                | _ -> []

            { LocationCode = code; Rooms = rooms; Locations = locations; }

        List.init count (generate None)

    let generateRandomGroupCodes (prefix : string) count = 
        List.init count (fun i -> String.Format ("{0}-GRP-{1}", prefix, (string i)))

    let generateRandomLessons (moduleCode : string) (roomTypeCodes : string list) (locationCodes : string list) (groupCodes : string list) (weekPatterns : int list list) =

        let generate n = 

            let lessonCode = String.Format ("{0}-L{1}", moduleCode, (string n))
            let weekPattern = randomItem weekPatterns            
            let roomTypeCode = randomItem roomTypeCodes

            let locationCode = 
                if (randomChoice Settings.LessonLocationFrequency) then
                    Some (randomItem locationCodes)
                else
                    None

            let groupCode = 
                if (randomChoice Settings.LessonGroupFrequency) then
                    Some (randomItem groupCodes)
                else
                    None

            { ModuleCode = moduleCode; LessonCode = lessonCode; GroupCode = groupCode; RoomTypeCode = roomTypeCode; LocationCode = locationCode; Weeks = weekPattern; }

        List.init (random Settings.MinLessons Settings.MaxLessons) generate

    let generateRandomModules count (roomTypeCodes : string list) (locations : Location list) (lessonGroupCodes : string list) (moduleGroupCodes : string list) (weekPatterns : int list list) =
        
        let locationCodes = 
            
            let rec flatten (locs : Location list) = 
                match locs with
                | [] -> []
                | l::ls ->
                    List.append (l.LocationCode :: (flatten l.Locations)) (flatten ls)

            flatten locations

        let generate n = 

            let code = String.Format ("MOD-{0}", (string n))
            let classSize = (random Settings.MinClassSize Settings.MaxClassSize)
            
            let groupCode = 
                if (randomChoice Settings.ModuleGroupFrequence) then
                    Some (randomItem moduleGroupCodes)
                else
                    None

            let lessons = generateRandomLessons code roomTypeCodes locationCodes lessonGroupCodes weekPatterns

            { ModuleCode = code; ClassSize = classSize; GroupCode = groupCode; Lessons = lessons; }

        List.init count generate




    
    