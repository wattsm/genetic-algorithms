namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open GeneticAlgorithms.Engine

type AlgorithmSettings = {
    IsElitist : Boolean;
    TournamentSize : int;
    TimetableSettings : TimetableSettings;
    FitnessCalculator : IFitnessCalculator<Timetable>;
}

type TimetableAlgorithm (settings : AlgorithmSettings)  =

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

            //Random reschedule a week
            if (random 1 15) = 1 then

                let weekNo = random timetable.StartWeek timetable.EndWeek
                let week = Scheduling.addEventsTo settings.TimetableSettings (Week.Empty weekNo settings.TimetableSettings)

                Timetables.replaceWeek timetable week

            else
                timetable

        member this.Select timetables = 
            Tournaments.select settings.TournamentSize settings.FitnessCalculator timetables