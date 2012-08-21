namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module Rooms =     
        

    let rec countRooms (locations : Location list) = 
        locations
        |> List.map (fun l -> 

                (List.length l.Rooms) + (countRooms l.Locations)


            )
        |> List.sum
        

    let ofType typeCode room = 
        room.TypeCode = typeCode

    let withCapacity capacity room = 
        room.Capacity >= capacity

    let roomsAt location = 
        
        let rec getRooms (locations : Location list) = 
            match locations with
            | [] -> []
            | l::ls -> 

                let childRooms = getRooms l.Locations
                let rooms = List.append l.Rooms childRooms

                List.append rooms (getRooms ls)

        getRooms [ location; ] 

    let findRooms requirements location = 
        (roomsAt location)
        |> List.filter (satisfiesAll requirements)

    //TODO Convert timetable data to arrays and parallelise?

    let roomClashes slot = 
        if (List.length slot.Events) <= 1 then
            0
        else
            slot.Events
            |> List.toSeq
            |> Seq.countBy (fun e -> e.RoomCode)
            |> Seq.filter (fun (_, n) -> (n > 1))
            |> Seq.length