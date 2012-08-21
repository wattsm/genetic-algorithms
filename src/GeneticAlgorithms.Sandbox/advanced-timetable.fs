namespace GeneticAlgorithms.Sandbox

open System
open System.Xml
open System.IO
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling.Advanced
open GeneticAlgorithms.Example.Timetabling.Advanced.Xml

module AdvancedTimetable = 

    let writeXml timetableSettings timetable =

        let path = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "advanced-timetable.xml")

        if (File.Exists path) then
            File.Delete path

        let settings = XmlWriterSettings ()
        settings.Indent <- true

        use stream = File.CreateText path
        use writer = XmlWriter.Create (stream, settings)

        Xml.writeTimetable writer timetableSettings timetable

        path   

    let report generationNo timetable fitness = 
        printfn "Generation %A (fitness = %M)" generationNo fitness

    let run () =

        printfn "Generating seed data..."

        let roomTypeCodes = 
            SeedData.generateRandomTypeCodes 3

        let locations = 
            SeedData.generateRandomLocations roomTypeCodes 50

        let lessonGroupCodes = 
            SeedData.generateRandomGroupCodes "L" 50

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
        printfn "   # lessons = %A" (Lessons.countLessons modules)
        printfn "   # locations = %A" (Locations.countLocations locations)
        printfn "   # rooms = %A" (Rooms.countRooms locations)

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

        let calculator = 
            TimetableFitnessCalculator.Create fitnessSettings timetableSettings

        let algorithmSettings = {
            IsElitist = true;
            TimetableSettings = timetableSettings;
            FitnessCalculator = calculator;
            TournamentSize = 3;
        }

        let algorithm = 
            TimetableAlgorithm.Create algorithmSettings

        let runnerSettings = {
            PopulationSize = 15;
            MaxGenerations = 10;
            AcceptableFitness = 80m;
        }

        printfn "Beginning evolution..."

        let runner = 
            Runner<Timetable> (runnerSettings, factory, calculator, algorithm)

        let result = 
            runner.Run report

        printfn "Result = %A" result.Type

        printfn "Fitness = %M" (calculator.CalculateFitness result.Fittest)
        printfn "   # module clashes = %A" (Timetables.moduleClashes timetableSettings result.Fittest)
        printfn "   # room clashes = %A" (Timetables.roomClashes result.Fittest)

        printfn "Writing XML timetable..."
        printfn "XML @ %A" (writeXml timetableSettings result.Fittest)

        Console.ReadLine () |> ignore