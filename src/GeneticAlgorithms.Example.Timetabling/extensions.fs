namespace GeneticAlgorithms.Example.Timetabling

open System

[<AutoOpen>]
module List =

    let replaceAt index item list = 

        let selectItem i =
            if (index = i) then
                item
            else
                List.nth list i 

        List.init (List.length list) selectItem