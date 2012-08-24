namespace GeneticAlgorithms.Example.Timetabling.Advanced

open System
open System.Collections.Generic

[<AutoOpen>]
module Memoisation =

    let private memoisedByIndex (transform : 'externalKey -> 'internalKey) (f : 'settings -> 'externalKey -> 'value) =

        let cache = Dictionary<_, _> ()
        let cacheLock = new Object ()

        fun settings key -> 
            
            let key' = transform key

            if not (cache.ContainsKey key') then
                lock cacheLock (fun () ->
                        if not (cache.ContainsKey key') then

                            cache.Add (key', (f settings key))
                    )

            cache.[key']

    let memoised f =
        memoisedByIndex id f

    let memoisedBySlot f = 
        memoisedByIndex (fun (slot : Slot) -> slot.UniqueId) f

    let memoisedByTimetable f = 
        memoisedByIndex (fun (timetable : Timetable) -> timetable.UniqueId) f