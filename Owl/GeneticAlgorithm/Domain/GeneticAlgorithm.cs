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

                //����������� ����� ����������
                var crossoverPoints = new[] {_random.Next(length), _random.Next(length)};
                int crossoverStart = crossoverPoints.Min(); //������ ������� - ������� �����
                int crossoverEnd = crossoverPoints.Max(); //����� - �������

                //�������� ����� ���������, ������� ���� ������������� ��������� � [start; end] ������ �� �������������. 
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

        private void AddToPopulation(Organism organism, List<Organism> population)
        {
            if (organism.GenesCount == 0)
                throw new ArgumentOutOfRangeException("organism");

            if (organism.Factors.Count == 0)
                throw new ArgumentOutOfRangeException("organism");

            population.Add(organism);
        }

        private void AddToPopulation(Organism organism)
        {
            if (organism.GenesCount == 0 || organism.Factors.Count == 0)
                throw new ArgumentOutOfRangeException("organism");

            _population.Add(organism);
        }

        /// <summary>
        /// ����������� ����������
        /// </summary>
        /// <param name="alpha">������ �����</param>
        /// <param name="beta">������ �����</param>
        /// <returns>�������</returns>
        private Organism Breed(Organism alpha, Organism beta)
        {
            try
            {
                if (alpha.GenesCount != beta.GenesCount)
                {
                    throw new ArgumentOutOfRangeException("alpha", @"Chromosome lengths must be same");
                }

                //� ������������ 0.5 ������������ ������ ����� A
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

        private Organism GenerateRandomOrganism()
        {
            try
            {
                var organism = new Organism();
                foreach (
                    GrayCode factor in
                        GeneticConfiguration.FactorConfigs.AsParallel().AsOrdered().Select(GenerateRandomFactor))
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
            while (population.Count < GeneticConfiguration.PopulationSize)
                AddToPopulation(GenerateRandomOrganism(), population);

            return population;
        }

        /// <summary>
        /// ���������� �������� � ������������, ��������� �� ���������
        /// </summary>
        /// <returns>��������</returns>
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
                foreach (Organism organism in _population.AsParallel())
                {
                    organism.GenerateLikelihood(fitnessSum);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate likalihoods", e);
            }
        }

        protected abstract double Fitness(Organism organism);

        private void CalculateFitness(Organism organism)
        {
            var fitness = Fitness(organism);
            organism.Fitness = fitness;
        }

        private void GenerateFitnesses()
        {
            var parallelQuery = _population.AsParallel();
            parallelQuery.ForAll(CalculateFitness);
            /*foreach (var organism in _population)
            {
                organism.Fitness = Fitness(organism);
            }*/
        }

        private IList<Organism> NewPopulation()
        {
            var newPopulation = new List<Organism>();
            //newPopulation.AddRange(_population.AsParallel().Select(organism => CreateOffspring()));

            while (newPopulation.Count < GeneticConfiguration.PopulationSize)
            {
                AddToPopulation(CreateOffspring(), newPopulation);
            }

            return newPopulation;
        }

        /// <summary>
        /// ���������� ��� ��������� � ������� ����������� ������
        /// </summary>
        /// <returns>�������</returns>
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
        /// ������� �������
        /// </summary>
        protected void Solve()
        {
            int iterations = 0;
            int globalMaximumIterations = 0;
            double maxFitness = 0;
            double globalMaximum = 0.0;
            _population = GenerateInitialPopulation();
            var fitnessLog = new List<double>();
            fitnessLog.Add(0);


            while (((globalMaximumIterations < GeneticConfiguration.MaximumIterations*(1-globalMaximum)) || !(Math.Abs(maxFitness - globalMaximum) < 0.01)) &&
                   (iterations < GeneticConfiguration.MaximumIterations))
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
                _population = NewPopulation();
                iterations++;
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

                if ((accuracy > 20) || (accuracy < 1))
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