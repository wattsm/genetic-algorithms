namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Xml

module Xml = 

    let writeEvent (writer : XmlWriter) settings (event : Event)  = 

        writer.WriteStartElement "Event"
        writer.WriteElementString ("Module", event.ModuleCode)

        writer.WriteElementString ("Lesson", event.LessonCode)
        writer.WriteElementString ("Room", event.RoomCode)
        writer.WriteElementString ("Location", event.LocationCode)
        writer.WriteEndElement ()

    let writeSlot (writer : XmlWriter) settings (slot : Slot) = 


        writer.WriteStartElement "Slot"
        writer.WriteAttributeString ("Number", (slot.SlotNo.ToString ()))

        writer.WriteStartElement "Events"

        slot.Events
        |> List.iter (writeEvent writer settings)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

    let writeDay (writer : XmlWriter) settings (day : WeekDay) = 
        
        writer.WriteStartElement "WeekDay"
        writer.WriteAttributeString ("Day", (day.Day.ToString ()))

        writer.WriteStartElement "Slots"

        day.Slots
        |> List.iter (writeSlot writer settings)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

    let writeWeek (writer : XmlWriter) settings (week : Week) =

        writer.WriteStartElement "Week"
        writer.WriteAttributeString ("Number", (week.WeekNo.ToString ()))

        writer.WriteStartElement "Days"

        week.Days
        |> List.iter (writeDay writer settings)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

    let writeTimetable (writer : XmlWriter) settings (timetable : Timetable) = 

        writer.WriteStartElement "Timetable"
        writer.WriteStartElement "Weeks"

        timetable.Weeks
        |> List.iter (writeWeek writer settings)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

