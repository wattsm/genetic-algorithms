namespace GeneticAlgorithms.Sandbox

open System
open System.Xml
open System.IO
open System.Diagnostics
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling.Advanced
open GeneticAlgorithms.Example.Timetabling.Advanced.Xml
open GeneticAlgorithms.Example.Timetabling.Advanced.Metrics

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

    let report generationNo generationTime timetable fitness = 
        printfn "Generation %A (time = %4A ms, fitness = %5.5M)" generationNo generationTime fitness

    let run () =

        let numberOfModules = 100
        let numberOfLocations = (numberOfModules / 2)
        let numberOfModuleGroups = (numberOfModules / 5)
        let outputXml = false
        let populationSize = max (numberOfModules / 10) 5
        let tournamentSize = max (populationSize / 10) 3

        printfn "Generating seed data..."

        let roomTypeCodes = 
            SeedData.generateRandomTypeCodes 3

        let locations = 
            SeedData.generateRandomLocations roomTypeCodes numberOfLocations

        let moduleGroupCodes = 
            SeedData.generateRandomGroupCodes "M" numberOfModuleGroups

        let weekPatterns = 
            [
                [1 .. 25];
                [30 .. 52];
                [1..10] @ [42..52];
                [11..24] @ [26..41];
            ]

        let modules = 
            SeedData.generateRandomModules numberOfModules roomTypeCodes locations moduleGroupCodes weekPatterns

        printfn "Done."
        printfn "   # modules = %A" numberOfModules
        printfn "   # lessons = %A" (Counts.countLessons modules)
        printfn "   # locations = %A" (Counts.countLocations locations)
        printfn "   # rooms = %A" (Counts.countRooms locations)

        let timetableSettings = {
            Modules = modules;
            Locations = locations;
            StartWeek = 1;
            EndWeek = 52;
            SlotsPerDay = 7;
        }

        let fitnessSettings = {
            Weights = 
                {
                    RoomClash = 0.2m;
                    ModuleClash = 0.3m;
                    LessonClash = 0.3m;
                    RoomCapacity = 0.1m;
                    RoomType = 0.1m;
                };
        }

        let denormalisedSettings = 
            Denormalised.getSettings timetableSettings

        let factory =
            TimetableFactory.Create timetableSettings

        let calculator = 
            TimetableFitnessCalculator (fitnessSettings, timetableSettings)

        let algorithmSettings = {
            IsElitist = true;
            TimetableSettings = timetableSettings;
            FitnessCalculator = calculator;
            TournamentSize = tournamentSize;
            Strategy = ByWeek;
            MutationFrequency = 0.65m;
        }

        let algorithm = 
            TimetableAlgorithm.Create algorithmSettings

        let runnerSettings = {
            PopulationSize = populationSize;
            MaxGenerations = 100;
            AcceptableFitness = 90m;
        }

        printfn "Beginning evolution..."

        let runner = 
            Runner<Timetable> (runnerSettings, factory, calculator, algorithm)

        let stopwatch = Stopwatch.StartNew ()

        let result = 
            runner.Run report

        stopwatch.Stop ()

        printfn "Result = %A (%A ms)" result.Type (stopwatch.ElapsedMilliseconds * 1L<ms>)

        printfn "Fitness = %A" (calculator.GetReport result.Fittest)

        if outputXml then
            printfn "Writing XML timetable..."
            printfn "XML @ %A" (writeXml timetableSettings result.Fittest)

        Console.ReadLine () |> ignore