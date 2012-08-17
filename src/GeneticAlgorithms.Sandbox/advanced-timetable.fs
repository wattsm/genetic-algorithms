namespace GeneticAlgorithms.Sandbox

open System
open System.Xml
open System.IO
open GeneticAlgorithms.Example.Timetabling.Advanced
open GeneticAlgorithms.Example.Timetabling.Advanced.Xml

module AdvancedTimetable = 

    let writeXml timetable =

        let path = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "advanced-timetable.xml")

        if (File.Exists path) then
            File.Delete path

        let settings = XmlWriterSettings ()
        settings.Indent <- true

        use stream = File.CreateText path
        use writer = XmlWriter.Create (stream, settings)

        Xml.writeTimetable writer timetable

        path        

    let rec createRandomLocations count depth parent = 

        let createRandomChildren parent = 
            if depth > 3 then
                []
            else            
                createRandomLocations (random 1 10) (depth + 1) parent

        [ 1 .. count ]
        |> List.map (fun n -> 

                let code = 
                    match parent with
                    | Some n' -> String.Format ("LOC-{0}/{1}", n', n)
                    | _ -> String.Format ("LOC-{0}", n)

                { 
                    LocationCode = code;
                    Rooms = [];
                    Locations = createRandomChildren (Some n);
                }

            )

    let run () =

        let rooms = 
            [
                {
                    RoomCode = "RM001";
                    TypeCode = "T001";
                    Capacity = 100;
                };
                {
                    RoomCode = "RM002";
                    TypeCode = "T002";
                    Capacity = 50;
                }
            ]

        let locations = 
            [
                {
                    LocationCode = "LC001";
                    Rooms = rooms;
                    Locations = [];
                }
            ]

        let locations' = 
            createRandomLocations 50 0 None

        let modules = 
            [
                {
                    ModuleCode = "M001";
                    ClassSize = 75;
                    GroupCode = Some "GRP001";
                    Lessons = 
                        [
                            {
                                LessonCode = "L001";
                                ModuleCode = "M001";
                                GroupCode  = None;
                                RoomTypeCode = "T001";
                                LocationCode = Some "LC001";
                                Weeks = [ 1; 2; 3; 4; 5; ];
                            };
                        ];
                };
            ]
        
        let timetableSettings = {
            Modules = modules;
            Locations = locations;
            StartWeek = 1;
            EndWeek = 10;
            SlotsPerDay = 6;
        }

        let fitnessSettings = {
            Weights = 
                {
                    Rooms = 0.5m;
                    Modules = 0.5m;
                };
            MaxClashes = 2;
        }

        let factory =
            TimetableFactory.Create timetableSettings

        let timetable = 
            factory.Create ()

        let calculator = 
            TimetableFitnessCalculator.Create fitnessSettings timetableSettings

        let fitness =
            calculator.CalculateFitness timetable

        printfn "Fitness = %A" fitness
        printfn "XML @ %A" (writeXml timetable)

        Console.ReadLine () |> ignore