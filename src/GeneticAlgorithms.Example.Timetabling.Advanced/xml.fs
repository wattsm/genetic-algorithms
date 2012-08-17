namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Xml

module Xml = 

    let writeEvent (writer : XmlWriter) (event : Event)  = 
        
        writer.WriteStartElement "Event"
        writer.WriteElementString ("Module", event.ModuleCode)
        writer.WriteElementString ("Lesson", event.LessonCode)
        writer.WriteElementString ("Room", event.RoomCode)
        writer.WriteElementString ("Location", event.LocationCode)
        writer.WriteEndElement ()

    let writeSlot (writer : XmlWriter) (slot : Slot) = 

        writer.WriteStartElement "Slot"
        writer.WriteAttributeString ("Number", (slot.SlotNo.ToString ()))

        writer.WriteStartElement "Events"

        slot.Events
        |> List.iter (writeEvent writer)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

    let writeDay (writer : XmlWriter) (day : WeekDay) = 
        
        writer.WriteStartElement "WeekDay"
        writer.WriteAttributeString ("Day", (day.Day.ToString ()))

        writer.WriteStartElement "Slots"

        day.Slots
        |> List.iter (writeSlot writer)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

    let writeWeek (writer : XmlWriter) (week : Week) =

        writer.WriteStartElement "Week"
        writer.WriteAttributeString ("Number", (week.WeekNo.ToString ()))

        writer.WriteStartElement "Days"

        week.Days
        |> List.iter (writeDay writer)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

    let writeTimetable (writer : XmlWriter) (timetable : Timetable) = 

        writer.WriteStartElement "Timetable"
        writer.WriteStartElement "Weeks"

        timetable.Weeks
        |> List.iter (writeWeek writer)

        writer.WriteEndElement ()
        writer.WriteEndElement ()

