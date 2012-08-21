namespace GeneticAlgorithms.Sandbox

open System
open System.Xml
open System.IO
open System.Diagnostics
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

    let report generationNo generationTime timetable fitness = 
        printfn "Generation %A (time = %A ms, fitness = %5.5M)" generationNo generationTime fitness

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
        printfn "   # lessons = %A" (Lessons.countLessons modules)
        printfn "   # locations = %A" (Locations.countLocations locations)
        printfn "   # rooms = %A" (Rooms.countRooms locations)

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
                    Rooms = 0.3m;
                    Modules = 0.25m;
                    Lessons = 0.45m
                };
        }

        let factory =
            TimetableFactory.Create timetableSettings

        let calculator = 
            TimetableFitnessCalculator.Create fitnessSettings timetableSettings

        let algorithmSettings = {
            IsElitist = true;
            TimetableSettings = timetableSettings;
            FitnessCalculator = calculator;
            TournamentSize = tournamentSize;
            Strategy = ByWeek;
        }

        let algorithm = 
            TimetableAlgorithm.Create algorithmSettings

        let runnerSettings = {
            PopulationSize = populationSize;
            MaxGenerations = 20;
            AcceptableFitness = 0.9m;
        }

        printfn "Beginning evolution..."

        let runner = 
            Runner<Timetable> (runnerSettings, factory, calculator, algorithm)

        let stopwatch = Stopwatch.StartNew ()

        let result = 
            runner.Run report

        stopwatch.Stop ()

        printfn "Result = %A (%A ms)" result.Type (stopwatch.ElapsedMilliseconds * 1L<ms>)

        printfn "Fitness = %M" (calculator.CalculateFitness result.Fittest)
        printfn "   # module clashes = %A" (Timetables.moduleClashes timetableSettings result.Fittest)
        printfn "   # lesson clashes = %A" (Timetables.lessonClashes timetableSettings result.Fittest)
        printfn "   # room clashes = %A" (Timetables.roomClashes result.Fittest)

        if outputXml then
            printfn "Writing XML timetable..."
            printfn "XML @ %A" (writeXml timetableSettings result.Fittest)

        Console.ReadLine () |> ignore