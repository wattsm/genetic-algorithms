namespace GeneticAlgorithms.Engine

open System

type RunnerSettings = {
    PopulationSize : int;
    MaxGenerations : int;
    AcceptableFitness : decimal;
}

type ResultType = 
    | Success
    | Acceptable of decimal
    | Failure of decimal

type Result<'i> = {
    Type : ResultType;
    Fittest : 'i;
}

type Runner<'i> (settings : RunnerSettings, factory : IFactory<'i>, fitnessCalculator : IFitnessCalculator<'i>, algorithm : IAlgorithm<'i>) =

    let (|Complete|_|) (population : Population<'i>) = 
        if (population.GenerationNo = settings.MaxGenerations) then
            if (population.Fitness >= settings.AcceptableFitness) then
                Some (Acceptable (population.Fitness))
            else
                Some (Failure (population.Fitness))
        else if (population.Fitness >= 100m) then
            Some (Success)
        else
            None

    let rec evolve (population : Population<'i>) (reporter : int -> int64<ms> -> 'i -> decimal -> unit) = 

        reporter population.GenerationNo population.GenerationTime population.Fittest population.Fitness

        match population with
        | Complete result -> { Type = result; Fittest = population.Fittest; }
        | _ -> evolve (population.Evolve ()) reporter

    member this.Run reporter = 

        let create () = 
            async {
                return factory.Create ()
            }

        let individuals = 
            Async.Parallel (Seq.init settings.PopulationSize (fun _ -> create ()))
            |> Async.RunSynchronously
            |> Array.toList            

        let population = 
            Population.Create individuals fitnessCalculator algorithm

        evolve population reporter
    