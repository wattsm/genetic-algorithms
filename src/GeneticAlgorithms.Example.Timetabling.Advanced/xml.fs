namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Xml

module Xml = 

    let writeEvent (writer : XmlWriter) settings (event : Event)  = 

        let groupCode = 
            settings.Modules
            |> List.find (fun m -> m.ModuleCode = event.ModuleCode)
            |> (fun m -> m.GroupCode)
        
        writer.WriteStartElement "Event"
        writer.WriteElementString ("Module", event.ModuleCode)

        match groupCode with
        | Some code -> writer.WriteElementString ("Group", code)
        | _ -> ()

        writer.WriteElementString ("Lesson", event.LessonCode)
        writer.WriteElementString ("Room", event.RoomCode)
        writer.WriteElementString ("Location", event.LocationCode)
        writer.WriteEndElement ()

    let writeSlot (writer : XmlWriter) settings (slot : Slot) = 

        let moduleClashes = Modules.moduleClashes settings slot
        let roomClashes = Rooms.roomClashes slot

        writer.WriteStartElement "Slot"
        writer.WriteAttributeString ("Number", (slot.SlotNo.ToString ()))

        if (moduleClashes > 0) then
            writer.WriteAttributeString ("ModuleClashes", (string moduleClashes))

        if (roomClashes > 0) then
            writer.WriteAttributeString ("RoomClashes", (string roomClashes))

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

