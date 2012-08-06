namespace GeneticAlgorithms.Engine

type IFactory<'i> =

    abstract member Create : unit -> 'i

