namespace GeneticAlgorithms.Sandbox

open System
open GeneticAlgorithms.Example.Timetabling.Advanced

module AdvancedTimetable = 

    let run () =
        
        let timetableSettings = {
            Modules = [];
            Locations = [];
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

        let timetable = 
            Timetable.Empty timetableSettings

        let calculator = 
            TimetableFitnessCalculator.Create fitnessSettings timetableSettings

        let fitness =
            calculator.CalculateFitness timetable

        printfn "Fitness = %A" fitness

        Console.ReadLine () |> ignore