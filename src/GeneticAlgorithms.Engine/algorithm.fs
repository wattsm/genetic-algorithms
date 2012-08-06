namespace GeneticAlgorithms.Engine

open System

type IAlgorithm<'i> =

    abstract member IsElitist : bool

    abstract member Mix : 'i -> 'i -> 'i

    abstract member Mutate : 'i -> 'i

    abstract member Select : 'i list -> 'i