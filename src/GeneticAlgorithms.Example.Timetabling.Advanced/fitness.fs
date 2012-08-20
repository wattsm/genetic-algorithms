namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling.Advanced.Timetables
open GeneticAlgorithms.Example.Timetabling.Advanced.Rooms
open GeneticAlgorithms.Example.Timetabling.Advanced.Modules

type ClashWeights = {
    Rooms : decimal;
    Modules : decimal;
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

    let roomFitness timetable = 
        (flatten timetable)
        |> List.map (roomClashes >> (clashFitness fs.MaxClashes))
        |> List.average

    let moduleFitness timetable = 
        (flatten timetable)
        |> List.map ((moduleClashes ts) >> (clashFitness fs.MaxClashes))
        |> List.average

    do
        if (fs.Weights.Modules + fs.Weights.Rooms) <> 1m then
            raise (ArgumentException ("Room and module fitness weightings must add up to 1."))

    static member Create fs ts = 
        TimetableFitnessCalculator (fs, ts) :> IFitnessCalculator<Timetable>
    
    interface IFitnessCalculator<Timetable> with

        member this.CalculateFitness timetable = 
            [
                (fs.Weights.Rooms, (roomFitness timetable));
                (fs.Weights.Modules, (moduleFitness timetable));
            ]
            |> List.map (fun (w, f) -> w * f)
            |> List.sum
            