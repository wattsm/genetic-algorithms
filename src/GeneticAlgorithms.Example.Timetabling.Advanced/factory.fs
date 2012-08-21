namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine
open GeneticAlgorithms.Example.Timetabling.Advanced.Timetables

type TimetableFactory (settings : TimetableSettings) =

    let settings' = 
        Denormalised.getSettings settings

    (* Static factory method *)
    static member Create settings = 
        TimetableFactory (settings) :> IFactory<Timetable>
    
    (* Interface implementation *)
    interface IFactory<Timetable> with

        member this.Create () = 
            [for weekNo in settings.StartWeek .. settings.EndWeek -> Week.Empty weekNo settings]
            |> List.map (Scheduling.addLessonEvents settings settings')
            |> List.fold Timetables.replaceWeek (Timetable.Empty settings)