namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Collections
open GeneticAlgorithms.Example.Timetabling.Advanced.Metrics
open GeneticAlgorithms.Engine

type ClashWeights = {
    Rooms : decimal;
    Modules : decimal;
    Lessons : decimal;
}

type FitnessSettings = {
    Weights : ClashWeights;
}

[<AutoOpen>]
module private Fitness = 

    ///Calculates the fitness rating given to a slot based on the room capacity
    let capacityFitness = 
        memoisedBySlot (fun settings slot -> 
            slot.Events
            |> PSeq.map (fun event -> 

                    let classSize = 
                        Denormalised.getClassSize settings event.ModuleCode

                    let capacity = 
                        Denormalised.getCapacity settings event.RoomCode

                    (classSize, capacity)
                )
            |> PSeq.averageBy (fun (classSize, capacity) -> 
                    if (classSize <= capacity) then
                        1m
                    else
                        0m
                )
        )

    //Calculates the room type fitness based on what room type a lesson requires and what it has been assigned
    let roomTypeFitness = 
        memoisedBySlot (fun settings slot -> 
            slot.Events
            |> PSeq.map (fun event ->

                    let expected =
                        Denormalised.getLessonRoomType settings event.LessonCode

                    let actual = 
                        Denormalised.getRoomType settings event.RoomCode

                    (expected, actual)
                )
            |> PSeq.averageBy (fun (expected, actual) ->
                    if (expected = actual) then
                        1m
                    else
                        0m
                )
        )

    ///Calculates the fitness rating given the number of clashes
    let clashFitness (numClashes : int)  = 
        if (numClashes = 0) then
            1m
        else
            1m / (decimal numClashes)

type TimetableFitnessCalculator (fs, ts) = 

    let settings = 
        Denormalised.getSettings ts

    //TODO Weights for room type and location (if a location is specified, being in a child of that location is also acceptable)

    let flatten timetable = 
        timetable.Weeks
        |> PSeq.collect (fun week -> 
                week.Days
                |> PSeq.collect (fun day -> 
                    day.Slots
                    |> List.filter (fun slot -> not (List.isEmpty slot.Events))
                )
            ) 

    let roomFitness slot = //TODO Weight these factors
        [
            ((Clashes.roomClashesBySlot settings) >> clashFitness);
            (capacityFitness settings);
            (roomTypeFitness settings);
        ]
        |> List.map (fun f -> f slot)
        |> List.average

    let moduleFitness = 
        (Clashes.moduleClashesBySlot settings) >> clashFitness

    let lessonFitness = 
        (Clashes.lessonClashesBySlot settings) >> clashFitness

    let slotFitness slot =
        [
            (fs.Weights.Rooms * (roomFitness slot));
            (fs.Weights.Modules * (moduleFitness slot));
            (fs.Weights.Lessons * (lessonFitness slot));
        ]
        |> List.sum

    let cache = Dictionary<_,_> ()
    let cacheLock = new Object ()

    do
        if (fs.Weights.Modules + fs.Weights.Rooms + fs.Weights.Lessons) <> 1m then
            raise (ArgumentException ("Room, module and lesson fitness weightings must add up to 1."))

    static member Create fs ts = 
        TimetableFitnessCalculator (fs, ts) :> IFitnessCalculator<Timetable>
    
    interface IFitnessCalculator<Timetable> with

        member this.CalculateFitness timetable = 
            if not (cache.ContainsKey timetable.UniqueId) then
                lock cacheLock (fun () -> 
                    if not (cache.ContainsKey timetable.UniqueId) then

                        let slots = 
                            flatten timetable

                        let fitness = 
                            slots
                            |> PSeq.map slotFitness
                            |> PSeq.average

                        cache.Add (timetable.UniqueId, fitness)
                )

            cache.[timetable.UniqueId]


                
            