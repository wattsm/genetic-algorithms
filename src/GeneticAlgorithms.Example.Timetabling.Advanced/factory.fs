namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine

type TimetableFactory (settings : TimetableSettings) =

    let settings' = 
        Denormalised.getSettings settings

    (* Static factory method *)
    static member Create settings = 
        TimetableFactory (settings) :> IFactory<Timetable>
    
    (* Interface implementation *)
    interface IFactory<Timetable> with

        member this.Create () = 

            let timetable = 
                [for weekNo in settings.StartWeek .. settings.EndWeek -> Week.Empty weekNo settings]
                |> List.map (Scheduling.addLessonEvents settings')
                |> List.fold Scheduling.replaceWeek (Timetable.Empty settings)

            { timetable with UniqueId = Guid.NewGuid (); }