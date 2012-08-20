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

    let run () =

        printfn "Generating seed data..."

        let roomTypeCodes = 
            SeedData.generateRandomTypeCodes 10

        let locations = 
            SeedData.generateRandomLocations roomTypeCodes 25

        let lessonGroupCodes = 
            SeedData.generateRandomGroupCodes "L" 10

        let moduleGroupCodes = 
            SeedData.generateRandomGroupCodes "M" 10

        let weekPatterns = 
            [
                [1 .. 25];
                [30 .. 52];
            ]

        let modules = 
            SeedData.generateRandomModules 50 roomTypeCodes locations lessonGroupCodes moduleGroupCodes weekPatterns

        printfn "Done."

        let timetableSettings = {
            Modules = modules;
            Locations = locations;
            StartWeek = 1;
            EndWeek = 52;
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

        printfn "Fitness = %M" fitness
        printfn "# module clashes = %A" (Timetables.moduleClashes timetableSettings timetable)
        printfn "# room clashes = %A" (Timetables.roomClashes timetable)
        printfn "XML @ %A" (writeXml timetable)

        Console.ReadLine () |> ignore