namespace GeneticAlgorithms.Example.Timetabling

open System
open GeneticAlgorithms.Engine

type CourseTimetableFactory (settings) =

    static member Create settings = 
        CourseTimetableFactory (settings) :> IFactory<Timetable>
    
    interface IFactory<Timetable> with

        member this.Create () = 
        
            let appendLecture courseCode day = 

                let slotNo = Common.random 1 6

                let lecture = { 
                    TutorCode = (Common.pickRandomItem settings.TutorCodes); 
                    RoomCode = (Common.pickRandomItem settings.RoomCodes); 
                    CourseCode = courseCode; 
                }

                day
                |> Days.addLecture slotNo lecture
            
            let appendCourse timetable courseCode = 
                { 
                    timetable with 
                        Days = List.map (appendLecture courseCode) timetable.Days; 
                }                
                
            settings.CourseCodes
            |> List.fold appendCourse (Timetable.Empty settings.Size)