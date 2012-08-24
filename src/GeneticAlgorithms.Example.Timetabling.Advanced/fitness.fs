namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Collections
open GeneticAlgorithms.Example.Timetabling.Advanced.Metrics
open GeneticAlgorithms.Engine

type ClashWeights = {
    RoomCapacity : decimal;
    RoomType : decimal;
    RoomClash : decimal;
    ModuleClash : decimal;
    LessonClash : decimal;
}
with

    member this.IsValid () =

        let total = 
            this.RoomCapacity
                + this.RoomType
                + this.RoomClash
                + this.ModuleClash
                + this.LessonClash

        (total = 1m)

type FitnessSettings = {
    Weights : ClashWeights;
}

type FitnessType = 
    | RoomClash
    | ModuleClash
    | LessonClash
    | RoomCapacity
    | RoomType
    | Location

type FitnessReportItem = {
    ItemType : FitnessType;
    Count : int;
    Fitness : decimal;
}

type FitnessReport = { 
    TimetableId : Guid;
    SlotCount : int;
    EventCount : int;
    Items : FitnessReportItem list;
}

[<AutoOpen>]
module private Fitness = 

    ///Flattens a timetable into a list of its slots
    let flatten timetable = 
        timetable.Weeks
        |> PSeq.collect (fun week -> 
                week.Days
                |> PSeq.collect (fun day -> 
                    day.Slots
                    |> List.filter (fun slot -> not (List.isEmpty slot.Events))
                )
            ) 

    ///Calculate fitness as a percentage of the number of events
    let eventPercentageFitness slot (unfitCount : int) = 

        let eventCount = decimal (List.length slot.Events)
        let unfitCount' = decimal unfitCount

        ((eventCount - unfitCount') / eventCount) * 100m

    ///Calculates the number of events with insufficiently large rooms
    let insufficientCapacityCount = 
        memoisedBySlot (fun settings slot -> 
            slot.Events
            |> PSeq.map (fun event -> 

                    let classSize = 
                        Denormalised.getClassSize settings event.ModuleCode

                    let capacity = 
                        Denormalised.getCapacity settings event.RoomCode

                    (classSize, capacity)
                )
            |> PSeq.filter (fun (classSize, capacity) -> classSize > capacity)
            |> PSeq.length
        )

    //Calculates the number of rooms which are not of the type required by the lesson
    let invalidRoomTypeCount = 
        memoisedBySlot (fun settings slot -> 
            slot.Events
            |> PSeq.map (fun event ->

                    let expected =
                        Denormalised.getLessonRoomType settings event.LessonCode

                    let actual = 
                        Denormalised.getRoomType settings event.RoomCode

                    (expected, actual)
                )
            |> PSeq.filter (fun (expected, actual) -> expected <> actual)
            |> PSeq.length
        ) 

    ///Get a report of the fitness of a given timetable
    let getFitnessReport =
        memoisedByTimetable (fun settings timetable -> 

                let slots = flatten timetable
                let slotCount = PSeq.length slots

                //For each slot
                    //Calculate each metric's count
                    //Calculate metric's percentage fitness
                        //For each item type
                            //Sum counts
                            //Average percentages

                let counters = 
                    [
                        (RoomClash, Clashes.roomClashesBySlot);
                        (ModuleClash, Clashes.moduleClashesBySlot);
                        (LessonClash, Clashes.lessonClashesBySlot);
                        (RoomCapacity, insufficientCapacityCount);
                        (RoomType, invalidRoomTypeCount);
                    ];

                let items = 
                    slots
                    |> PSeq.collect (fun slot ->
                            
                            counters
                            |> PSeq.map (fun (itemType, counter) -> 

                                    let unfitCount = counter settings slot
                                    let fitness = eventPercentageFitness slot unfitCount

                                    { 
                                        ItemType = itemType;
                                        Count = unfitCount;
                                        Fitness = fitness;
                                    }
                                )

                        )
                    |> PSeq.groupBy (fun item -> item.ItemType)
                    |> PSeq.map (fun (itemType, items) ->

                            {
                                ItemType = itemType;
                                Count = (PSeq.sumBy (fun item -> item.Count) items);
                                Fitness = (PSeq.averageBy (fun item -> item.Fitness) items);
                            }

                        )
                    |> PSeq.toList

                {
                    TimetableId = timetable.UniqueId;
                    SlotCount = (PSeq.length slots);
                    EventCount = (PSeq.sumBy (fun slot -> List.length slot.Events) slots);
                    Items = items;
                }
            )       


type TimetableFitnessCalculator (fs, ts) = 

    let settings = 
        Denormalised.getSettings ts

    let calculateFitness =
        memoisedByTimetable (fun settings timetable -> 

                    let report = getFitnessReport settings timetable

                    let weights = 
                        [
                            (RoomCapacity, fs.Weights.RoomCapacity);
                            (RoomType, fs.Weights.RoomType);
                            (ModuleClash, fs.Weights.ModuleClash);
                            (LessonClash, fs.Weights.LessonClash);
                            (RoomClash, fs.Weights.RoomClash);
                        ];

                    let applyWeight item = 

                        let weight = 
                            weights
                            |> List.pick (fun (itemType, weight) -> 
                                    if (itemType = item.ItemType) then
                                        Some weight
                                    else
                                        None
                                )

                        (item.Fitness * weight)


                    report.Items
                    |> PSeq.map applyWeight
                    |> PSeq.sum
                )

    do
        if not (fs.Weights.IsValid ()) then
            raise (ArgumentException ("Room, module and lesson fitness weightings must add up to 1."))

    static member Create fs ts = 
        TimetableFitnessCalculator (fs, ts) :> IFitnessCalculator<Timetable>

    member this.GetReport = 
        getFitnessReport settings 
    
    interface IFitnessCalculator<Timetable> with

        member this.CalculateFitness timetable = 
            calculateFitness settings timetable


                
            