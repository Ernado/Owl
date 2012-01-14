using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;

namespace Owl.GeneticAlgorithm.Domain
{
    class GeneticAlgorithm
    {
        protected Config GeneticConfiguration;
        private IEnumerable<Organism> _population;
        protected Organism BestOrganism;
        private readonly Random _random = new Random();

        public class Config
        {
            public double MutationRate { get; private set; }
            public double MaximumIterations { get; private set; }
            public double ConvergenceRate { get; private set; }
            public List<GrayCode> Factors { get; set; }
            public int PopulationSize { get; private set; }
            public int Accuracy { get; private set; }

            public Config(double mutationRate, double maximumIterations, double convergenceRate, List<GrayCode> factors, int populationSize, int accuracy)
            {
                MutationRate = mutationRate;
                MaximumIterations = maximumIterations;
                ConvergenceRate = convergenceRate;
                Factors = factors;
                PopulationSize = populationSize;
                Accuracy = accuracy;
                if (factors.Any(grayCode => grayCode.BitAccuracy != Accuracy))
                    throw new Exception("Invalid factor bit accuracy");
            }
        }

        /// <summary>
        /// Скрещивание организмов
        /// </summary>
        /// <param name="alpha">Первая особь</param>
        /// <param name="beta">Вторая особь</param>
        /// <returns>Потомок</returns>
        private Organism Breed(Organism alpha, Organism beta)
        {
            //Проверка на одинаковую длину хромосом у организмов
            if (alpha.GenesCount != beta.GenesCount)
            {
                throw new Exception("Can't breed organisms with different chromosome length");
            }

            var length = alpha.GenesCount; //длина хромосомы 
            var offspring = new Organism(); //потомок

            //С вероятностью 0.5 инициирующей особью будет A
            if (_random.NextDouble() <= 0.5)
            {
                var t = alpha;
                alpha = beta;
                beta = t;
            }

            //Сгенерируем точки кроссовера
            var crossoverPoints = new[] { _random.Next(length), _random.Next(length) };
            var crossoverStart = crossoverPoints.Min(); //Начало отрезка - меньшая точка
            var crossoverEnd = crossoverPoints.Max(); //Конец - большая

            //Получаем новую хромосому, заменяя гены инициируемого организма в [start; end] генами из инициирующего. 
            // A {Ai..As..Ae..An} + B {Bi..Bs..Be..Bn} = Descendant {Bi..Bs-1,As..Ae,Be+1..Bn}
            for (var i = 0; i < length; i++)
                if (i >= crossoverStart && i <= crossoverEnd)
                    offspring.AddGene(alpha.GetGene(i));
                else
                    offspring.AddGene(beta.GetGene(i));

            //Мутируем один ген с вероятностью MutatuonRate
            for (var index = 0; index < offspring.GenesCount; index++)
                if (_random.NextDouble() <= GeneticConfiguration.MutationRate)
                    offspring.Alleles[index] = !offspring.Alleles[index];

            return offspring;
        }

        private IEnumerable<Organism> InitialPopulation ()
        {
            var population = new List<Organism>();
            while (population.Count < GeneticConfiguration.PopulationSize)
            {
                var organism = new Organism();
                foreach (var factor in GeneticConfiguration.Factors.AsParallel())
                {
                    var decimalNumber = (uint) _random.Next(0, (int) (Math.Pow(2, factor.BitAccuracy) - 1));
                    factor.DecimalCode = decimalNumber;
                    organism.Alleles.AddRange(factor.BooleanCode);
                }
                organism.Factors = GeneticConfiguration.Factors;
                population.Add(organism);
            }
            return population;
        } 

        /// <summary>
        /// Возвращает организм с вероятностью, зависящей от живучести
        /// </summary>
        /// <returns>Организм</returns>
        private Organism Roullete()
        {
            var score = _random.NextDouble();
            double position = 0;

            foreach (var organism in _population)
            {
                position += organism.Likelihood;

                if (score <= position)
                {
                    return organism;
                }
            }

            throw new Exception("Roullete choice failed");
        }

        private void GenerateLikalihoods ()
        {
            var fitnessSum = _population.Sum(organism => organism.Fitness);
            foreach (var organism in _population.AsParallel())
            {
                organism.GenerateLikelihood(fitnessSum);
            }
        }

        protected virtual double Fitness (Organism organism)
        {
            throw new NotImplementedException("Fitness function not set");
        }

        private void GenerateFitnesses ()
        {
            foreach (var organism in _population.AsParallel())
            {
                organism.Fitness = Fitness(organism);
            }
        }

        private IEnumerable<Organism> NewPopulation ()
        {
            var newPopulation = new List<Organism>();
            newPopulation.AddRange(_population.AsParallel().Select(organism => CreateOffspring()));
            return newPopulation;
        }
 
        /// <summary>
        /// Скрещивает два организма с помощью взвешенного выбора
        /// </summary>
        /// <returns>Потомок</returns>
        private Organism CreateOffspring ()
        {
            var alpha = new Organism();
            var beta = new Organism();

            while (alpha == beta)
            {
                alpha = Roullete();
                beta = Roullete();
            }

            return Breed(alpha, beta);
        }

        /// <summary>
        /// Находит решение
        /// </summary>
        protected void Solve()
        {
            int iterations = 0;
            double previousMaxFitness = 0;
            double maxFitness = 0;
            _population = InitialPopulation();

            while ((Math.Abs(previousMaxFitness - maxFitness) > maxFitness*GeneticConfiguration.ConvergenceRate || iterations < 2) & iterations < GeneticConfiguration.MaximumIterations)
            {
                previousMaxFitness = maxFitness;
                GenerateFitnesses();
                GenerateLikalihoods();
                _population = NewPopulation();
                BestOrganism =
                    (from organism in _population orderby organism.Fitness descending select organism).ToList()[0];
                maxFitness = BestOrganism.Fitness;
                iterations++;
            }
        }
    }
}
