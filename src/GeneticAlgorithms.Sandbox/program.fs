namespace GeneticAlgorithms.Sandbox

open System
open System.IO
open System.Xml
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling

module Program = 

    let writeTimetableXml timetable = 

        let path = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "timetable.xml")

        if (File.Exists path) then
            File.Delete path

        let settings = XmlWriterSettings ()
        settings.Indent <- true

        use stream = File.CreateText path
        use writer = XmlWriter.Create (stream, settings)

        Timetables.writeXml writer timetable

        writer.Close ()
        stream.Close ()

        path

    let printToConsole generationNo fittest fitness = 
        Console.WriteLine (String.Format("Generation = {0}, Fittest = {1} clashes, Fitness = {2:0.00}", generationNo, (Clashes.countTimetableClashes fittest), fitness))

    let _ = 

        let timetableSettings = {
            Size = 5;
            CourseCodes = List.init 30 (fun n -> String.Format ("CRS{0}", n));
            RoomCodes = List.init 20 (fun n -> String.Format ("RM{0}", n)); //Needs to be about # courses / 6
            TutorCodes = List.init 15 (fun n -> String.Format ("T{0}", n)); //Ditto
        }

        let fitnessSettings = {
            MaxClashesPerSlot = 2;
            RoomWeighting = 0.3m;
            TutorWeighting = 0.3m;
            CourseWeighting = 0.4m;
        }

        let algorithmSettings = {
            IsElitist = true;
            TournamentSize = 15;            
            TimetableSettings = timetableSettings;
        }

        let runnerSettings = {
            PopulationSize = 25;
            MaxGenerations = 50;
            AcceptableFitness = 90m;
        }

        let factory = CourseTimetableFactory.Create timetableSettings
        let fitness = TimetableFitnessCalculator.Create fitnessSettings
        let algorithm = CourseTimetableAlgorithm.Create algorithmSettings fitness

        let runner = Runner (runnerSettings, factory, fitness, algorithm)
        let result = runner.Run printToConsole
        let path = writeTimetableXml result.Fittest

        let msg =
            match result.Type with
            | Failure (fitness) -> String.Format ("No solution found (fittest = {0:0.00})", fitness)
            | Acceptable (fitness) -> String.Format ("Acceptable solution found (fittest = {0:0.00})", fitness)
            | Success -> "Perfect solution found"

        printfn "------------------------"
        printfn "Result = %A" msg
        printfn "Timetable XML = %A" path
        
        Console.ReadLine () |> ignore

