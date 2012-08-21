namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling.Advanced.Timetables
open GeneticAlgorithms.Example.Timetabling.Advanced.Rooms
open GeneticAlgorithms.Example.Timetabling.Advanced.Modules
open GeneticAlgorithms.Example.Timetabling.Advanced.Lessons

type ClashWeights = {
    Rooms : decimal;
    Modules : decimal;
    Lessons : decimal;
}

type FitnessSettings = {
    Weights : ClashWeights;
    MaxClashes : int;
}

[<AutoOpen>]
module private Fitness = 

    let clashFitness (maxClashes : int) (numClashes : int)  = 
        if (numClashes >= maxClashes) then
            0m
        else
            100m - ((decimal numClashes / decimal maxClashes) * 100m)

type TimetableFitnessCalculator (fs, ts) = 

    let roomFitness slots = 
        slots
        |> List.map (roomClashes >> (clashFitness fs.MaxClashes))
        |> List.average

    let moduleFitness slots = 
        slots
        |> List.map ((moduleClashes ts) >> (clashFitness fs.MaxClashes))
        |> List.average

    let lessonFitness slots = 
        slots
        |> List.map ((lessonClashes ts) >> (clashFitness fs.MaxClashes))
        |> List.average

    do
        if (fs.Weights.Modules + fs.Weights.Rooms + fs.Weights.Lessons) <> 1m then
            raise (ArgumentException ("Room, module and lesson fitness weightings must add up to 1."))

    static member Create fs ts = 
        TimetableFitnessCalculator (fs, ts) :> IFitnessCalculator<Timetable>
    
    interface IFitnessCalculator<Timetable> with

        member this.CalculateFitness timetable = 

            let slots = 
                flatten timetable

            [
                (fs.Weights.Rooms, roomFitness);
                (fs.Weights.Modules, moduleFitness);
                (fs.Weights.Lessons, lessonFitness);
            ]
            |> List.map (fun (w, f) -> w * (f slots))
            |> List.sum
            