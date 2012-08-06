namespace GeneticAlgorithms.Engine

open System

type RunnerSettings = {
    PopulationSize : int;
    MaxGenerations : int;
    AcceptableFitness : decimal;
}

type ResultType = 
    | Success
    | Acceptable
    | Failure

type Result<'i> = {
    Type : ResultType;
    Fittest : 'i;
}

type Runner<'i> (settings : RunnerSettings, factory : IFactory<'i>, fitnessCalculator : IFitnessCalculator<'i>, algorithm : IAlgorithm<'i>) =

    let (|Complete|_|) (population : Population<'i>) = 
        if (population.GenerationNo = settings.MaxGenerations) then
            if (population.Fitness >= settings.AcceptableFitness) then
                Some (Acceptable)
            else
                Some (Failure)
        else if (population.Fitness >= 100m) then
            Some (Success)
        else
            None

    let rec evolve (population : Population<'i>) (reporter : string -> unit) = 

        reporter (String.Format ("Generation {0}, fittest individual \"{1}\" (fitness = {2}).", population.GenerationNo, population.Fittest, population.Fitness))

        match population with
        | Complete result -> { Type = result; Fittest = population.Fittest; }
        | _ -> evolve (population.Evolve ()) reporter

    member this.Run (reporter : string -> unit) = 

        let individuals = 
            List.init settings.PopulationSize (fun index -> factory.Create ())

        let population = 
            Population.Create individuals fitnessCalculator algorithm

        evolve population reporter
    