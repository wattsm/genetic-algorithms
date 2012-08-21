namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine

type MutationStrategy = 
    | ByWeek
    | ByModule

type AlgorithmSettings = {
    IsElitist : Boolean;
    TournamentSize : int;
    TimetableSettings : TimetableSettings;
    FitnessCalculator : IFitnessCalculator<Timetable>;
    Strategy : MutationStrategy;
}

type TimetableAlgorithm (settings : AlgorithmSettings)  =

    let mutateByWeek timetable =

            let weekNo = random timetable.StartWeek timetable.EndWeek
            let week = Scheduling.addLessonEvents settings.TimetableSettings (Week.Empty weekNo settings.TimetableSettings)

            Timetables.replaceWeek timetable week

    let mutateByModule =     
        
        let module' = 
            randomItem settings.TimetableSettings.Modules

        let removeEvents = 
            Scheduling.removeModuleEvents module'.ModuleCode

        let addEvents =
            Scheduling.addModuleEvents settings.TimetableSettings module'

        removeEvents >> addEvents

    static member Create settings = 
        TimetableAlgorithm (settings) :> IAlgorithm<Timetable>

    interface IAlgorithm<Timetable> with

        member this.IsElitist = settings.IsElitist

        member this.Mix timetable1 timetable2 = 

            //Half and half mix of the two timetables by week (the smallest wholly transferrable unit)

            let pick (week1, week2) = 
                if (random 1 2 ) = 1 then
                    week1
                else 
                    week2

            let weeks = 
                timetable1.Weeks
                |> List.zip timetable2.Weeks
                |> List.map pick

            { timetable1 with Weeks = weeks; }

        member this.Mutate timetable = 

            //Random reschedule a week (1/20 chance)
            if (randomChoice 0.05m) then

                let mutator = 
                    match settings.Strategy with
                    | ByWeek ->  mutateByWeek
                    | ByModule -> mutateByModule

                mutator timetable

            else
                timetable

        member this.Select timetables = 
            Tournaments.select settings.TournamentSize settings.FitnessCalculator timetables