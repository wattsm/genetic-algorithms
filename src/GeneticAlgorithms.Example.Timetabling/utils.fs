namespace GeneticAlgorithms.Example.Timetabling

open System
open System.Xml

module Common = 

    let private _random = Random (Environment.TickCount)

    let pickRandomItem items = 
        List.nth items (_random.Next (0, (List.length items)))

    let random min max = 
        _random.Next (min, (max + 1))

module Lectures =

    let shareResource lecture1 lecture2 = 
        lecture1.CourseCode = lecture2.CourseCode
            || lecture1.RoomCode = lecture2.RoomCode
            || lecture1.TutorCode = lecture2.TutorCode

    let print (lecture : Lecture) = 
        printfn "(Tutor = %A, Room = %A, Course = %A)" lecture.TutorCode lecture.RoomCode lecture.CourseCode

    let writeXml (writer : XmlWriter) (lecture : Lecture) = 
        writer.WriteStartElement "lecture"

        writer.WriteElementString ("tutor", lecture.TutorCode)
        writer.WriteElementString ("room", lecture.RoomCode)
        writer.WriteElementString ("course", lecture.CourseCode)

        writer.WriteEndElement ()

module Slots = 

    let addLecture lecture slot = 
        { slot with Lectures = (lecture :: slot.Lectures); }

    let replaceLecture index lecture slot = 
        { slot with Lectures = (List.replaceAt index lecture slot.Lectures); }

    let print (slot : SlotSchedule) = 

        printfn "Slot %A (clashes = %A)" slot.SlotNo (Clashes.slotHasClashes slot)
        printfn "-------------------------"

        slot.Lectures
        |> List.iter Lectures.print     
        
    let writeXml (writer : XmlWriter) (slot : SlotSchedule) = 

        writer.WriteStartElement "slot"
        writer.WriteAttributeString ("no", slot.SlotNo.ToString ())

        if (Clashes.slotHasClashes slot) then
            writer.WriteAttributeString ("clashes", "Y")

        slot.Lectures
        |> List.iter (Lectures.writeXml writer)

        writer.WriteEndElement ()

module Days = 

    let getCourseLecture courseCode day = 

        let slot = 
            day.Slots
            |> List.find (fun s -> List.exists (fun l -> l.CourseCode = courseCode) s.Lectures)

        let lecture = 
            slot.Lectures
            |> List.find (fun l -> l.CourseCode = courseCode)

        (slot.SlotNo, lecture)

    let indexOf slotNo day = 
        List.findIndex (fun s -> s.SlotNo = slotNo) day.Slots

    let getSlot slotNo day = 
        List.nth day.Slots (indexOf slotNo day)

    let replaceLecture slotNo index lecture day = 

        let slotIndex = 
            indexOf slotNo day

        let slot' =
            getSlot slotNo day
            |> Slots.replaceLecture index lecture

        { day with Slots = (List.replaceAt slotIndex slot' day.Slots); }

    let addLecture slotNo lecture day = 
        
        let index  =
            day.Slots
            |> List.findIndex (fun s -> s.SlotNo = slotNo)

        let slot' = 
            getSlot slotNo day
            |> Slots.addLecture lecture

        let slots' =
            List.replaceAt index slot' day.Slots

        { day with Slots = slots'; }    

    let print (day : DaySchedule) = 
        
        printfn "Day %A" day.DayNo
        printfn "========================"

        day.Slots
        |> List.iter Slots.print

    let writeXml (writer : XmlWriter) (day : DaySchedule) = 
        writer.WriteStartElement "day"
        writer.WriteAttributeString ("no", day.DayNo.ToString ())

        day.Slots
        |> List.iter (Slots.writeXml writer)

        writer.WriteEndElement ()


module Timetables = 

    let indexOf dayNo timetable =  
        List.findIndex (fun d -> d.DayNo = dayNo) timetable.Days

    let getDay dayNo timetable =         
        List.nth timetable.Days (indexOf dayNo timetable)

    let replaceLecture dayNo slotNo index lecture timetable = 

        let dayIndex =
            indexOf dayNo timetable

        let day' =
            getDay dayNo timetable
            |> Days.replaceLecture slotNo index lecture

        { timetable with Days = (List.replaceAt dayIndex day' timetable.Days); }
        

    let addLecture dayNo slotNo lecture timetable = 

        let index =
            timetable.Days
            |> List.findIndex (fun d -> d.DayNo = dayNo)

        let day' = 
            getDay dayNo timetable
            |> Days.addLecture slotNo lecture

        let days' = 
            List.replaceAt index day' timetable.Days

        { timetable with Days = days'; }

    let print (timetable : Timetable) = 
        timetable.Days
        |> List.iter Days.print

    let writeXml (writer : XmlWriter) (timetable : Timetable) = 
        writer.WriteStartElement "timetable"

        timetable.Days
        |> List.iter (Days.writeXml writer)

        writer.WriteEndElement ()
        
