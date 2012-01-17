using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;

namespace Owl.GeneticAlgorithm.Domain
{
    class GeneticAlgorithm
    {
        protected Config GeneticConfiguration;
        private IList<Organism> _population;
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

            public Config(double mutationRate, int maximumIterations, double convergenceRate, List<GrayCode> factors, int populationSize, int accuracy)
            {
                if ((mutationRate<0) || (mutationRate>1))
                {
                    throw new ArgumentOutOfRangeException("mutationRate");
                }

                if (maximumIterations < 1)
                {
                    throw new ArgumentOutOfRangeException("maximumIterations");
                }

                if ((convergenceRate < 0) || (convergenceRate > 1))
                {
                    throw new ArgumentOutOfRangeException("mutationRate");
                }

                if (factors.Count<1)
                {
                    throw new ArgumentOutOfRangeException("factors");
                }

                if (populationSize < 2)
                {
                    throw new ArgumentOutOfRangeException("populationSize");
                }

                if ((accuracy>20) || (accuracy<1))
                {
                    throw new ArgumentOutOfRangeException("accuracy");
                }

                if (factors.Any(grayCode => grayCode.BitAccuracy != accuracy))
                    throw new Exception("Invalid factor bit accuracy");

                MutationRate = mutationRate;
                MaximumIterations = maximumIterations;
                ConvergenceRate = convergenceRate;
                Factors = factors;
                PopulationSize = populationSize;
                Accuracy = accuracy;
            }
        }

        private void MutateOrganism(Organism organism)
        {
            var mutationRate = GeneticConfiguration.MutationRate;
            if ((mutationRate < 0) || (mutationRate > 1))
            {
                throw new ArgumentOutOfRangeException();
            }

            try
            {
                for (var index = 0; index < organism.GenesCount; index++)
                    if (_random.NextDouble() <= GeneticConfiguration.MutationRate)
                        organism.Alleles[index] = !organism.Alleles[index];
            }
            catch (Exception e)
            {
                throw new Exception("Cant mutate organism",e);
            }

            
        }

        private Organism Crossover (Organism alpha, Organism beta)
        {
            try
            {
                if (alpha.GenesCount != beta.GenesCount)
                {
                    throw new ArgumentOutOfRangeException("alpha", @"Chromosome lengths must be different");
                }

                var length = alpha.GenesCount;

                var offspring = new Organism {Factors = alpha.Factors};

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

                return offspring;
            }
            catch (Exception e)
            {
                throw new Exception("Cant crossover", e);
            }

           
        }

        private void AddToPopulation (Organism organism, List<Organism> population)
        {
            if (organism.GenesCount == 0)
                throw new ArgumentOutOfRangeException("organism");

            if (organism.Factors.Count == 0)
                throw new ArgumentOutOfRangeException("organism");

            population.Add(organism);
        }

        private void AddToPopulation(Organism organism)
        {
            if (organism.GenesCount == 0)
                throw new ArgumentOutOfRangeException("organism");

            if (organism.Factors.Count == 0)
                throw new ArgumentOutOfRangeException("organism");

            _population.Add(organism);
        }

        /// <summary>
        /// Скрещивание организмов
        /// </summary>
        /// <param name="alpha">Первая особь</param>
        /// <param name="beta">Вторая особь</param>
        /// <returns>Потомок</returns>
        private Organism Breed(Organism alpha, Organism beta)
        {
            try
            {
                if (alpha.GenesCount != beta.GenesCount)
                {
                    throw new ArgumentOutOfRangeException("alpha", @"Chromosome lengths must be different");
                }

                //С вероятностью 0.5 инициирующей особью будет A
                if (_random.NextDouble() <= 0.5)
                {
                    var t = alpha;
                    alpha = beta;
                    beta = t;
                }

                var offspring = Crossover(alpha, beta);

                MutateOrganism(offspring);

                return offspring;
            }
            catch (Exception e)
            {
                throw new Exception("Cant breed",e);
            }      
        }

        private GrayCode GenerateRandomFactor (GrayCode factor)
        {
            try
            {
                var randomFactor = factor;
                var decimalNumber = (uint)_random.Next(0, (int)(Math.Pow(2, factor.BitAccuracy) - 1));
                randomFactor.DecimalCode = decimalNumber;
                return randomFactor;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate random factor",e);
            }
            
        }

        private Organism GenerateRandomOrganism()
        {
            try
            {
                var organism = new Organism();
                foreach (var randomFactor in GeneticConfiguration.Factors.AsParallel().AsOrdered().Select(GenerateRandomFactor))
                {
                    organism.AddFactorAndCode(randomFactor);
                }
                return organism;

            }
            catch (Exception e)
            {
                throw new Exception("Cant generate random organism",e);
            }
            
        }

        private IList<Organism> GenerateInitialPopulation ()
        {
            var population = new List<Organism>();
            try
            {
                while (population.Count < GeneticConfiguration.PopulationSize)
                    AddToPopulation(GenerateRandomOrganism());

                return population;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate initial population",e);
            }
            
        } 

        /// <summary>
        /// Возвращает организм с вероятностью, зависящей от живучести
        /// </summary>
        /// <returns>Организм</returns>
        private Organism Roullete()
        {
            var score = _random.NextDouble();
            double position = 0;

            try
            {
                foreach (var organism in _population)
                {
                    position += organism.Likelihood;

                    if (score <= position)
                    {
                        return organism;
                    }
                }
            }
            catch (Exception e)
            {
                
                throw new Exception("Cant choice organism",e);
            }
            

            throw new Exception("Roullete choice failed");
        }

        private double FitnessSumOfPopulation()
        {
            try
            {
                return _population.Sum(organism => organism.Fitness);
            }
            catch(Exception e)
            {
                throw new Exception("Cant calculate fitness sum",e);
            }
        }

        private void GenerateLikalihoods ()
        {
            try
            {
                var fitnessSum = FitnessSumOfPopulation();
                foreach (var organism in _population.AsParallel())
                {
                    organism.GenerateLikelihood(fitnessSum);
                }
            }
            catch (Exception e)
            {
                
                throw new Exception("Cant generate likalihoods",e);
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
                try {organism.Fitness = Fitness(organism);}
                catch(Exception e)
                {
                    throw new Exception("Cant generate fitness",e);
                }
            }
        }

        private IList<Organism> NewPopulation ()
        {
            try
            {
                var newPopulation = new List<Organism>();
                newPopulation.AddRange(_population.AsParallel().Select(organism => CreateOffspring()));

                while (newPopulation.Count < GeneticConfiguration.PopulationSize)
                {
                    AddToPopulation(CreateOffspring(), newPopulation);
                }

                return newPopulation;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate new population",e);
            }
        }
 
        /// <summary>
        /// Скрещивает два организма с помощью взвешенного выбора
        /// </summary>
        /// <returns>Потомок</returns>
        private Organism CreateOffspring ()
        {
            var alpha = new Organism();
            var beta = new Organism();
            try
            {
                while (alpha == beta)
                {
                    alpha = Roullete();
                    beta = Roullete();
                }

                return Breed(alpha, beta);
            }
            catch (Exception e)
            {

                throw new Exception("Cant create offspring",e);
            }
            
        }

        /// <summary>
        /// Находит решение
        /// </summary>
        protected void Solve()
        {
            int iterations = 0;
            double previousMaxFitness = 0;
            double maxFitness = 0;
            _population = GenerateInitialPopulation();

            while ((Math.Abs(previousMaxFitness - maxFitness) > maxFitness*GeneticConfiguration.ConvergenceRate || iterations < 2) & iterations < GeneticConfiguration.MaximumIterations)
            {
                try
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
                catch(Exception e)
                {
                    throw new Exception("Cant iterate populations",e);
                }
            
        }
        }
    }
}
