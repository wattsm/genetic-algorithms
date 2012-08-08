namespace GeneticAlgorithms.Example.Timetabling

open System
open GeneticAlgorithms.Engine

module private Fitness = 

    let getClashFitness maxClashes clashCount = 
        if clashCount >= maxClashes then
            0m
        else
            100m - (((decimal clashCount) / (decimal maxClashes)) * 100m)

type BaseTimetableFitnessCalculator (getCode, maxClashesPerSlot) = 

    interface IFitnessCalculator<Timetable> with

        member this.CalculateFitness timetable = 
            timetable.Days
            |> List.collect (fun d -> d.Slots)
            |> List.map (Clashes.countSlotClashesBy getCode)
            |> List.map (Fitness.getClashFitness maxClashesPerSlot)
            |> List.average

type RoomFitnessCalculator (maxClashesPerSlot) = 
    inherit BaseTimetableFitnessCalculator ((fun lecture -> lecture.RoomCode), maxClashesPerSlot)

    static member Create maxClashesPerSlot = 
        RoomFitnessCalculator (maxClashesPerSlot) :> IFitnessCalculator<Timetable>

type CourseFitnessCalculator (maxClashesPerSlot) = 
    inherit BaseTimetableFitnessCalculator ((fun lecture -> lecture.CourseCode), maxClashesPerSlot)

    static member Create maxClashesPerSlot = 
        CourseFitnessCalculator (maxClashesPerSlot) :> IFitnessCalculator<Timetable>

type TutorFitnessCalculator (maxClashesPerSlot) = 
    inherit BaseTimetableFitnessCalculator ((fun lecture -> lecture.TutorCode), maxClashesPerSlot)

    static member Create maxClashesPerSlot = 
        TutorFitnessCalculator (maxClashesPerSlot) :> IFitnessCalculator<Timetable>

type TimetableFitnessCalculatorSettings = {
    MaxClashesPerSlot : int;
    RoomWeighting : decimal;
    CourseWeighting : decimal;
    TutorWeighting : decimal;
}
with

    member this.Validate () =

        if this.MaxClashesPerSlot < 0 then
            raise (ArgumentException ())

        if this.RoomWeighting < 0m || this.RoomWeighting > 100m then
            raise (ArgumentException ())

        if this.CourseWeighting < 0m || this.CourseWeighting > 100m then
            raise (ArgumentException ())

        if this.TutorWeighting < 0m || this.TutorWeighting > 100m then
            raise (ArgumentException ())

        if (this.TutorWeighting + this.RoomWeighting + this.CourseWeighting) < 1m then
            raise (ArgumentException ())

type TimetableFitnessCalculator (settings) = 

    let contributors = [
        (settings.RoomWeighting, (RoomFitnessCalculator.Create settings.MaxClashesPerSlot));
        (settings.TutorWeighting, (TutorFitnessCalculator.Create settings.MaxClashesPerSlot));
        (settings.CourseWeighting, (CourseFitnessCalculator.Create settings.MaxClashesPerSlot));
    ]

    do settings.Validate ()

    static member Create settings = 
        TimetableFitnessCalculator (settings) :> IFitnessCalculator<Timetable>

    interface IFitnessCalculator<Timetable> with

        member this.CalculateFitness timetable = 
            contributors
            |> List.map (fun (weight, calc) -> (calc.CalculateFitness timetable) * weight)
            |> List.sum
