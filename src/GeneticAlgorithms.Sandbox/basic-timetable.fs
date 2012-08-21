namespace GeneticAlgorithms.Sandbox

open System
open System.IO
open System.Xml
open System.Xml.Xsl
open GeneticAlgorithms.Example.Timetabling
open GeneticAlgorithms.Engine

module BasicTimetable =

    let writeTimetableXml timetable = 

        let path = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "basic-timetable.xml")

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

    let writeTimetableHtml xmlPath = 

        let xslPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "timetable.xsl")
        let htmlPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "timetable.html")

        if (File.Exists htmlPath) then
            File.Delete htmlPath

        let stylesheet = XslCompiledTransform ()
        stylesheet.Load (xslPath)

        stylesheet.Transform (xmlPath, htmlPath)

        htmlPath

    let printToConsole generationNo generationTime fittest fitness = 
        Console.WriteLine (String.Format("Generation = {0}, Fittest = {1} clashes, Fitness = {2:0.00}", generationNo, (Clashes.countTimetableClashes fittest), fitness))

    let run () =

        let timetableSettings = {
            Size = 5;
            CourseCodes = List.init 25 (fun n -> String.Format ("CRS{0}", n));
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
        let xmlPath = writeTimetableXml result.Fittest
        let htmlPath = writeTimetableHtml xmlPath

        let msg =
            match result.Type with
            | Failure (fitness) -> String.Format ("No solution found (fittest = {0:0.00})", fitness)
            | Acceptable (fitness) -> String.Format ("Acceptable solution found (fittest = {0:0.00})", fitness)
            | Success -> "Perfect solution found"

        printfn "------------------------"
        printfn "Result = %A" msg
        printfn "Timetable XML = %A" xmlPath
        printfn "Timetable HTML = %A" htmlPath
        
        Console.ReadLine () |> ignore