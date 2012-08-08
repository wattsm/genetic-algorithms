# genetic-algorithms

A (very) basic genetic algorithm engine written in F#. Inspired and based upon [this blog post](http://www.theprojectspot.com/tutorial-post/creating-a-genetic-algorithm-for-beginners/3).

The engine has three possible end-states:
1. Success: a perfect solution (as judged by the fitness calculator) has been found.
2. Acceptable: an acceptable solution (fitness above a user defined minimum) has been found but the maximum number of generations has been reached.
3. Failure: the maximum number of generations has been reached and no acceptable solution was found.
      
Finding a good fitness calculation and crossover/mutation algorithm is critical to effectively solving any given problem.

### Examples

This repository contains two examples, one very simple, one less so.

#### String matching

The first, very simple, example is in the GeneticAlgorithms.Example.Strings project. Given a target word (lower case, alphabetical characters only)
and some settings it will spawn a population of randomly generating strings and attempt to evolve towards the target word.

The algorithm is very simple with no real selective pressures. Mutation rates are high (1/5 chance). For small words (<= 10 characters)
this algorithm will generally find a solution within 20 generations or so.

#### Timetable generation

##### Overview

This example is more complex and it attempts to generate a class timetable for a school/college/university. It is purely illustrative and
makes a great many simplifying assumptions.

A timetable is modelled as a series of days. Each day has a number and 6 slots. Each slot has a number and can contain any number
of lectures. A lecture is a combination of a room, tutor and course - all strings.

The aim of the algorithm is to generate a timetable based on some settings, a selection of rooms, tutors and courses with the minimal
number of clashes (where two resources of the same type are scheduled to be in use at the same time - e.g. tutor T0001 is scheduled for 
two lectures at day 1 slot 1).

Timetables are based on courses, and each course provided must have one lecture per day.

##### Evolution

Crossover of timetables is achieved by comparing a day at a time. If either the first or second day is clash free then it is
carried forward to the next generation. Otherwise for each course the lecture from either day 1 or 2 is selected at random and carried
forward to the next generation.

Mutation of timetables is simply removing a course lecture from a slot and creating a new lecture (with randomly assigned
tutor and room) and assigning it to a slot on the same day.

##### Fitness

A timetable's fitness is the average fitness of its days. The fitness of a day is the average fitness of its slots.

Each slot's fitness is evaluated by course, tutor and room. If a slot has more than the maximum acceptable number of clashes, its fitness is 0. 
If it has less than its fitness is the the inverse of the percentage of maxmium acceptable clashes - e.g. if the number of acceptable clashes is 4 and a slot
has 3 then its fitness is 25 (100 - ((3 / 4) * 100))

These fitness values are then multiplied by their weighting to give the total slot fitness.

### Sandbox

The project GeneticAlgorithms.Sandbox is a console application which can be used to play around with the parameters of the engine. 

Currently it is performing timetable generation. A timetable is generating, with information about each generation
printed to the screen. Once a solution has been found or the maximum number of generations has been reached
the fittest timetable is rendered as XML to /bin/debug|release/timetable.xml.