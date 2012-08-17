﻿namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System

module Locations =

    let findLocation settings locationCode = 
        
        let rec find locationCode (locations : Location list) =
            match locations with
            | [] -> None
            | l::ls ->
                if (l.LocationCode = locationCode) then
                    Some l
                else
                    match (find locationCode l.Locations) with
                    | Some location -> Some location
                    | _ -> (find locationCode ls)

        let location = 
            settings.Locations
            |> find locationCode

        match location with
        | None -> raise (ArgumentException ())
        | Some location' -> location'