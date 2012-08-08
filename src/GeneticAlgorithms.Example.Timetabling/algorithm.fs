namespace GeneticAlgorithms.Example.Timetabling

open System
open GeneticAlgorithms.Engine

type AlgorithmSettings = {    
    IsElitist : bool;
    TournamentSize : int;
    TimetableSettings : TimetableSettings;
}

type CourseTimetableAlgorithm (settings, fitness : IFitnessCalculator<Timetable>) = 

    static member Create settings fitness = 
        CourseTimetableAlgorithm (settings, fitness) :> IAlgorithm<Timetable>

    interface IAlgorithm<Timetable> with

        member this.IsElitist = settings.IsElitist

        member this.Mix timetable1 timetable2 = 

            //TODO Pass in the fitness settings or some other way of quantifying "acceptable" levels of clashes?

            let mixDays day1 day2 = 
                if not (Clashes.dayHasClashes day1) then
                    day1
                else if not (Clashes.dayHasClashes day2) then
                    day2                    
                else
                    
                    let chooseCourseLecture day1 day2 courseCode = 

                        let selectedDay = 
                            if ((Common.random 1 2) = 1) then
                               day1
                            else
                                day2

                        Days.getCourseLecture courseCode selectedDay

                        //TODO Optionally reassign room and tutor?
                            
                    settings.TimetableSettings.CourseCodes
                    |> List.map (chooseCourseLecture day1 day2)
                    |> List.fold (fun day (slotNo, lecture) -> Days.addLecture slotNo lecture day) (DaySchedule.Empty day1.DayNo)

            { Days = (List.map2 mixDays timetable1.Days timetable2.Days); }

        member this.Mutate timetable =
            if (Common.random 1 10) = 1 then

                //TODO Selective pressures: don't choose random tutors/rooms that would create a clash if possible

                let dayNo = Common.random 1 (timetable.Length)

                let day = 
                    timetable 
                    |> Timetables.getDay dayNo

                let slots =
                    day.Slots
                    |> List.filter (fun s -> not (List.isEmpty s.Lectures))   

                let slot = 
                    Common.random 0 ((List.length slots) - 1)
                    |> List.nth slots

                let lectureIndex = Common.random 0 ((List.length slot.Lectures) - 1)
                let lecture = List.nth slot.Lectures lectureIndex

                let lecture' = {
                    CourseCode = lecture.CourseCode;
                    TutorCode = (Common.pickRandomItem settings.TimetableSettings.TutorCodes);
                    RoomCode = (Common.pickRandomItem settings.TimetableSettings.RoomCodes);
                }

                timetable
                |> Timetables.replaceLecture dayNo slot.SlotNo lectureIndex lecture' 

            else
                timetable

        member this.Select timetables = 
            Tournaments.select settings.TournamentSize fitness timetables