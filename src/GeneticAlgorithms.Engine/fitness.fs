namespace GeneticAlgorithms.Engine

open System

type IFitnessCalculator<'i> = 

    abstract member CalculateFitness : 'i -> decimal

