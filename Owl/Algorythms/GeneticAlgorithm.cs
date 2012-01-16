using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Domain;

namespace Owl.Algorythms
{
    public class GeneticAlgorithm
    {
        private readonly int _accuracy;
        private readonly double _convergenceRate;
        private readonly List<GrayCode> _factors;
        private readonly double[] _fitnesses;
        private readonly double[] _likelihoods;
        private double _maximumFitness;
        private readonly int _maximumIterations;
        private readonly double _mutationRate;
        private readonly double[] _points;
        private List<string> _population;
        private readonly int _populationSize;
        private readonly double[] _ranges;
        private List<AnalyzableLine> _bestResult;

        /// <summary>
        /// �������������� ������ ������ GeneticAlgorithm, �������� ��������� �� ��������� ��������.
        /// </summary>
        /// <param name="points">������ ���������������� ���� ������ ����� � ��������</param>
        /// <param name="ranges">������ ���������������� �������� ���������� ����� �������</param>
        /// <param name="factors">������ �������� � ��������� ���������</param>
        /// <param name="populationSize">������ ���������</param>
        /// <param name="mutationRate">����������� �������</param>
        /// <param name="accuracy">�������� ����������</param>
        /// <param name="convergenceRate">������� �������� ����� ������������� ������������������� ��������� ��� ����������� ����������</param>
        /// <param name="maximumIterations">������������ ���������� ������������ ���������</param>
        public GeneticAlgorithm(double[] points, double[] ranges, List<GrayCode> factors,
                                int populationSize = 100, double mutationRate = 0.05, int accuracy = 8,
                                double convergenceRate = 0.05, int maximumIterations = 50)
        {
            _points = points;
            _ranges = ranges;
            _populationSize = populationSize;
            _mutationRate = mutationRate;
            _accuracy = accuracy;
            _convergenceRate = convergenceRate;
            _maximumIterations = maximumIterations;
            _factors = factors;

            _fitnesses = new double[populationSize];
            _likelihoods = new double[populationSize];

            //����������� ��������� ���������
            GenerateRandomPopulation();
        }

        /// <summary>
        /// ���������� ��������� ��������� �� ��������� ��������.
        /// </summary>
        private void GenerateRandomPopulation()
        {
            var population = new List<string>();
            //����������� ���������.
            var random = new Random();
            for (int i = 0; i < _populationSize; i++)
            {
                //����������� ��������� ���������
                string chromosome = "";
                foreach (GrayCode grayCode in _factors)
                {
                    var grayUint = (uint) random.Next(0, (int) (Math.Pow(2, _accuracy) - 1));
                    grayCode.DecimalCode = grayUint;
                    chromosome += grayCode.StringCode;
                }

                //������� � ���������
                population.Add(chromosome);
            }
            //������� ��������� ��������� ����������
            _population = population;
        }

        /// <summary>
        /// ���������� ��������� ��� ������� ��������� �� ������ ������
        /// � ����������������� (�������-�������)
        /// </summary>
        private void GenerateLikelihoods()
        {
            //��������� ����� �������-������� ��� ���� ������
            double fitnesSum = _fitnesses.Sum();

            //��������� ��������� ��� ������ �����.
            for (int i = 0; i < _populationSize; i++)
                _likelihoods[i] = _fitnesses[i]/fitnesSum;
        }

        /// <summary>
        /// ���������� ������ ���������, ���������� �� ������ ������
        /// � ��������� (��� ������ ���������, ��� ������ �����������
        /// ������)
        /// </summary>
        /// <returns>��������� ������ ���������</returns>
        private int Roullete(Random randomR)
        {
            double random = randomR.NextDouble(); //��������� �����
            double position = 0.0; //������� �� �������

            //��������� �� ��������
            for (int i = 0; i < _populationSize; i++)
            {
                //��������� �� ����� �������, �����. ��������� ���������.
                position += _likelihoods[i];

                //���� ��������� ����� ��������� � �������� ����� �������, ����������
                //������ ������� (���������).
                if (random <= position)
                {
                    return i;
                }
            }

            //���� �� ����� �������
            return (new Random()).Next(_population.Count);
        }

        /// <summary>
        /// ���������� ������� ��������� A � B (������� �� �����)
        /// ��� �� �������� ���� ��� ������� � ������������ ������������.
        /// </summary>
        /// <param name="chromosomeA">��������� A</param>
        /// <param name="chromosomeB">��������� B</param>
        /// <param name="random">����</param>
        /// <returns>��������� A+B</returns>
        private string CrossOver(string chromosomeA, string chromosomeB, Random random)
        {
            int length = chromosomeA.Length; //����� ��������� 
            string offspring = ""; //�������

            //� ������������ 0.5 ������������ ������ ����� A
            if (random.NextDouble() <= 0.5)
            {
                // A <-> B
                string t = chromosomeA;
                chromosomeA = chromosomeB;
                chromosomeB = t;
            }

            //����������� ����� ����������:
            var points = new[] {random.Next(length), random.Next(length)};
            int start = points.Min(); //������ ������� - ������� �����
            int end = points.Max(); //����� - �������

            //�������� ����� ���������, ������� ���� ������������ ��������� � [start; end] ������ �� ������������. 
            // A {Ai..As..Ae..An} + B {Bi..Bs..Be..Bn} = Descendant {Bi..Bs-1,As..Ae,Be+1..Bn}
            for (int i = 0; i < length; i++)
            {
                if (i >= start && i <= end)
                    offspring += chromosomeA[i];
                else
                    offspring += chromosomeB[i];
            }
            char[] chars = offspring.ToCharArray();
            for (int index = 0; index < chars.Length; index++)
            {
                if (random.NextDouble() <= _mutationRate)
                {
                    chars[index] = (chars[index] == '0') ? '1' : '0';
                }
            }

            //�������� ���� ��� � ������������ MutatuonRate
            
            return offspring;
        }

        /// <summary>
        /// �������� ��� ���������, ���������� ��, ���������� �������.
        /// </summary>
        /// <returns>��������� �������</returns>
        private string ChooseAndBreed(Random random)
        {
            int indexA = 0;
            int indexB = 0;
            int iterations = 0;

            //����� ������� ��� ������ ���������.
            while (_population[indexA] == _population[indexB])
            {
                indexA = Roullete(random);
                indexB = Roullete(random);
                if (++iterations > _populationSize) break;
            }
            return CrossOver(_population[indexA], _population[indexB], random);
        }

        /// <summary>
        /// ���������� ��������� ��� ���������.
        /// </summary>
        private void GenerateFitnesses()
        {
            //������������ �� ���� ����������.
            var results = new List<AnalyzableLine>[_populationSize];
            for (int i = 0; i < _populationSize; i++)
            {
                //bool isString = false;
                int summHeight = 0;
                var lines = new List<AnalyzableLine>();
                int y = 0;
                while (y < _points.Length)
                {
                    //������ ������ ������
                    if (Result(_population[i], _ranges[y]))
                    {
                        //������ ������  - ������� ������� �������.
                        int start = y;

                        var line = new AnalyzableLine(start, 0);

                        //���������� �������� �� �������� �� ��� ���, ���� �������
                        //�� ���������� ���� 'True'
                        while (Result(_population[i], _ranges[y]))
                        {
                            y++;
                            //�������� �� ����� �������
                            if (y == _points.Length)
                                break;
                        }
                        int height = y - start;

                        //�������� ������
                        line.Height = height;
                        summHeight += height;

                        //������� ������ � ������ ����� ��������.
                        lines.Add(line);
                    }
                    y++;
                    
                }

                if (lines.Count < 2)
                {
                    _fitnesses[i] = 0.0;
                }
                else
                {
                    var medHeight = summHeight/lines.Count;
                    var newlines = lines.Where(line => line.Height > medHeight).ToList();
                    if (newlines.Count > 2)
                        lines = newlines;
                    //�������� ������������ (�������-�������)
                    //�������� �������� ����� 
                    int heightsRange = lines.Max(line => line.Height) - lines.Min(line => line.Height);

                    //�������� �������� ���������� ����� ��������
                    int last = lines[0].Height + lines[0].Start;
                    var ranges = new int[lines.Count];
                    for (int k = 1; k < lines.Count; k++)
                    {
                        ranges[k - 1] = lines[k].Start - last;
                        last = lines[k].Start + lines[k].Height;
                    }
                    int distancesRange = ranges.Max() - ranges.Min();
                    //summHeight = lines.Sum(line => line.Height);


                    //�������� �������-������� ��� ������ ���������.
                    //Fitnesses[i] = 1 / ((distancesRange) + heightsRange +(double)Math.Abs(2 * summHeight - Points.Length));
                    _fitnesses[i] = 1 / (double)((distancesRange+1)*(heightsRange+1));
                    results[i] = lines;
                }
            }
            _maximumFitness = _fitnesses.Max();
            var max = 0.0;
            var bestIndex = 0;
            for (int i = 0; i < _populationSize; i++)
            {
                if (_fitnesses[i] > max)
                {
                    max = _fitnesses[i];
                    bestIndex = i;
                }

            }
            _bestResult = results[bestIndex];
        }

        /// <summary>
        /// ���������� ��������� ������������� ������ (����������� ��� ��� ������)
        /// </summary>
        /// <param name="chromosome">���������</param>
        /// <param name="range">��������� ������ ����� (�����.)</param>
        /// <returns></returns>
        private bool Result(string chromosome, double range)
        {
            int j = 0;
            int factorIndex = 0;
            string bitValue = "";

            //������� �� ��������� ����, ������������� ������������ ��������
            //�.�. ���������� ����������, ������������ � �����
            foreach (char t in chromosome)
            {
                bitValue += t;
                j++;
                if (j == _accuracy)
                {
                    _factors[factorIndex].StringCode = bitValue;
                    j = 0;
                    bitValue = "";
                    factorIndex++;
                }
            }
            return (1/_factors[0].Value + (_factors[2].Value)/range) > 0;
        }

        public List<AnalyzableLine> Solve()
        {
            int iterations = 0;
            double previousMaxFitness = 0.0;
            var random = new Random();
            while (Math.Abs(previousMaxFitness - _maximumFitness) > _maximumFitness*_convergenceRate || iterations < 1)
            {
                previousMaxFitness = _maximumFitness;
                GenerateFitnesses();
                GenerateLikelihoods();
                var newPopulation = new List<string>();
                while (newPopulation.Count < _populationSize)
                    newPopulation.Add(ChooseAndBreed(random));
                _population = newPopulation;
                if (++iterations > _maximumIterations) break;
            }
            foreach (var line in _bestResult)
            {
                line.Height = (int)Math.Round(line.Height*1.4);
                line.Start -= (int)Math.Round(line.Height*0.2);
            }
            return _bestResult;
        }
    }



}