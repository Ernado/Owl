using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;

namespace Owl.GeneticAlgorithm.Domain
{
    public abstract class GeneticAlgorithm
    {
        private readonly Random _random = new Random();
        protected Organism BestOrganism;
        public Config GeneticConfiguration;
        private IList<Organism> _population;

        /// <summary>
        /// Мутирует каждый ген организма с определенной вероятностью
        /// </summary>
        /// <param name="organism">Организм для мутации</param>
        private void MutateOrganism(Organism organism)
        {
            double mutationRate = GeneticConfiguration.MutationRate;
            if ((mutationRate < 0) || (mutationRate > 1))
            {
                throw new ArgumentOutOfRangeException();
            }

            try
            {
                for (int index = 0; index < organism.GenesCount; index++)
                    if (_random.NextDouble() <= GeneticConfiguration.MutationRate)
                        organism.Alleles[index] = !organism.Alleles[index];
            }
            catch (Exception e)
            {
                throw new Exception("Cant mutate organism", e);
            }
        }

        /// <summary>
        /// Выполняет операцию кроссовера к генам двух особей
        /// </summary>
        /// <param name="alpha">Инициирующий</param>
        /// <param name="beta">Инициируемый</param>
        /// <returns>Потомок</returns>
        private Organism Crossover(Organism alpha, Organism beta)
        {
            try
            {
                if (alpha.GenesCount != beta.GenesCount)
                {
                    throw new ArgumentOutOfRangeException("alpha", @"Chromosome lengths must be different");
                }

                int length = alpha.GenesCount;

                var offspring = new Organism {Factors = alpha.Factors};

                //Сгенерируем точки кроссовера
                var crossoverPoints = new[] {_random.Next(length), _random.Next(length)};
                int crossoverStart = crossoverPoints.Min(); //Начало отрезка - меньшая точка
                int crossoverEnd = crossoverPoints.Max(); //Конец - большая

                //Получаем новую хромосому, заменяя гены инициируемого организма в [start; end] генами из инициирующего. 
                // A {Ai..As..Ae..An} + B {Bi..Bs..Be..Bn} = Descendant {Bi..Bs-1,As..Ae,Be+1..Bn}
                for (int i = 0; i < length; i++)
                    if ((i >= crossoverStart) && (i <= crossoverEnd))
                        offspring.AddGene(alpha.GetGene(i));
                    else
                        offspring.AddGene(beta.GetGene(i));
                offspring.Factors = offspring.GenerateFactors();
                return offspring;
            }
            catch (Exception e)
            {
                throw new Exception("Cant crossover", e);
            }
        }
        
        /// <summary>
        /// Добавляет организм в популяцию. Проверяет на валидность организм.
        /// </summary>
        /// <param name="organism">Организм</param>
        /// <param name="population">Популяция</param>
        private void AddToPopulation(Organism organism, List<Organism> population)
        {
            if (organism.GenesCount == 0)
                throw new ArgumentOutOfRangeException("organism");

            if (organism.Factors.Count == 0)
                throw new ArgumentOutOfRangeException("organism");

            population.Add(organism);
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
                    throw new ArgumentOutOfRangeException("alpha", @"Chromosome lengths must be same");
                }

                //С вероятностью 0.5 инициирующей особью будет A
                if (_random.NextDouble() <= 0.5)
                {
                    Organism t = alpha;
                    alpha = beta;
                    beta = t;
                }

                Organism offspring = Crossover(alpha, beta);

                MutateOrganism(offspring);

                return offspring;
            }
            catch (Exception e)
            {
                throw new Exception("Cant breed", e);
            }
        }

        /// <summary>
        /// Генерирует фактор со случайным значением
        /// </summary>
        /// <param name="config">Конфигурация фактора</param>
        /// <returns>Случайный фактор</returns>
        private GrayCode GenerateRandomFactor(GrayCodeConfig config)
        {
            try
            {
                var randomFactor = new GrayCode(0, config);
                var decimalNumber = (uint) _random.Next(0, (int) (Math.Pow(2, config.Accuracy) - 1));
                randomFactor.DecimalCode = decimalNumber;
                return randomFactor;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate random factor", e);
            }
        }

        /// <summary>
        /// Генерирует организм с факторами, содержащими случайные 
        /// </summary>
        /// <returns></returns>
        private Organism GenerateRandomOrganism()
        {
            try
            {
                var organism = new Organism();
                foreach (
                    var factor in
                        GeneticConfiguration.FactorConfigs.Select(GenerateRandomFactor))
                {
                    organism.AddFactorAndCode(factor);
                }

                return organism;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate random organism", e);
            }
        }

        private IList<Organism> GenerateInitialPopulation()
        {
            var population = new List<Organism>();
            
            try
            {
                while (population.Count < GeneticConfiguration.PopulationSize)
                    AddToPopulation(GenerateRandomOrganism(), population);
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate initial population");
            }
            
            return population;
        }

        /// <summary>
        /// Возвращает организм с вероятностью, зависящей от живучести
        /// </summary>
        /// <returns>Организм</returns>
        private Organism Roullete()
        {
            double score = _random.NextDouble();
            double position = 0;

            try
            {
                foreach (Organism organism in _population)
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
                throw new Exception("Cant choice organism", e);
            }


            throw new Exception("Roullete choice failed");
        }

        private double FitnessSumOfPopulation()
        {
            try
            {
                return _population.Sum(organism => organism.Fitness);
            }
            catch (Exception e)
            {
                throw new Exception("Cant calculate fitness sum", e);
            }
        }

        private void GenerateLikalihoods()
        {
            try
            {
                double fitnessSum = FitnessSumOfPopulation();
                var parallelQuery = _population.AsParallel();
                parallelQuery.ForAll((e)=>e.GenerateLikelihood(fitnessSum));
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate likalihoods", e);
            }
        }

        protected abstract double Fitness(Organism organism);

        private void CalculateFitness(Organism organism)
        {
            try
            {
                var fitness = Fitness(organism);
                organism.Fitness = fitness;
            }
            catch (Exception e)
            {
                throw new Exception("Cant calculate fitness",e);
            }
        }

        private void GenerateFitnesses()
        {
            try
            {
                var parallelQuery = _population.AsParallel().AsOrdered();
                parallelQuery.ForAll(CalculateFitness);
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate fitnesses",e);
            }
            
        }

        private IList<Organism> NewPopulation()
        {
            var newPopulation = new List<Organism>();
            
            try
            {
                while (newPopulation.Count < GeneticConfiguration.PopulationSize)
                    AddToPopulation(CreateOffspring(), newPopulation);
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate new population",e);
            }

            return newPopulation;
        }

        /// <summary>
        /// Скрещивает два организма с помощью взвешенного выбора
        /// </summary>
        /// <returns>Потомок</returns>
        private Organism CreateOffspring()
        {
            var alpha = new Organism();
            Organism beta = alpha;
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
                throw new Exception("Cant create offspring", e);
            }
        }

        /// <summary>
        /// Находит решение
        /// </summary>
        protected void Solve()
        {
            var iterations = 0;
            var globalMaximumIterations = 0;
            double maxFitness = 0;
            double globalMaximum = 0.0;
            _population = GenerateInitialPopulation();
            var fitnessLog = new List<double>();
            fitnessLog.Add(0);

            try
            {
                while (true)
                {
                    if (maxFitness > globalMaximum)
                    {
                        globalMaximum = maxFitness;
                        globalMaximumIterations = 0;
                    }
                    else
                        globalMaximumIterations++;

                    fitnessLog.Add(maxFitness);
                    GenerateFitnesses();
                    GenerateLikalihoods();
                    BestOrganism =
                        (from organism in _population orderby organism.Fitness descending select organism).ToList()[0];
                    maxFitness = BestOrganism.Fitness;

                    var solved = ((globalMaximumIterations < GeneticConfiguration.MaximumIterations*(1 - globalMaximum)) ||
                                  !(Math.Abs(maxFitness - globalMaximum) < 0.01)) &&
                                 (iterations < GeneticConfiguration.MaximumIterations);

                    if (solved)
                        break;

                    _population = NewPopulation();
                    iterations++;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Cant solve",e);
            }
        }

        #region Nested type: Config

        public class Config
        {
            public IList<GrayCodeConfig> FactorConfigs;

            public Config(double mutationRate, int maximumIterations, double convergenceRate,
                          IList<GrayCodeConfig> configs, uint populationSize, uint accuracy)
            {
                if ((mutationRate < 0) || (mutationRate > 1))
                    throw new ArgumentOutOfRangeException("mutationRate");

                if (maximumIterations < 1)
                    throw new ArgumentOutOfRangeException("maximumIterations");

                if ((convergenceRate < 0) || (convergenceRate > 1))
                    throw new ArgumentOutOfRangeException("mutationRate");

                if (configs.Count < 1)
                    throw new ArgumentOutOfRangeException("configs");

                if (populationSize < 2)
                    throw new ArgumentOutOfRangeException("populationSize");

                if ((accuracy > 20) || (accuracy <= 1))
                    throw new ArgumentOutOfRangeException("accuracy");

                if (configs.Any(grayCode => grayCode.Accuracy != accuracy))
                    throw new Exception("Invalid factor bit accuracy");

                MutationRate = mutationRate;
                MaximumIterations = maximumIterations;
                ConvergenceRate = convergenceRate;
                FactorConfigs = configs;
                PopulationSize = populationSize;
                Accuracy = accuracy;
            }

            public double MutationRate { get; private set; }
            public double MaximumIterations { get; private set; }
            public double ConvergenceRate { get; private set; }
            //public List<GrayCode> Factors { get; set; }
            public uint PopulationSize { get; private set; }
            public uint Accuracy { get; private set; }
        }

        #endregion
    }
}