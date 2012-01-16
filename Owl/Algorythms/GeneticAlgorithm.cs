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
        /// Инициализирует объект класса GeneticAlgorithm, создавая популяцию из случайных хромосом.
        /// </summary>
        /// <param name="points">Массив масштабированных сумм черных точек в строчках</param>
        /// <param name="ranges">Массив масштабированных амплитуд расстояний между точками</param>
        /// <param name="factors">Список факторов с заданными границами</param>
        /// <param name="populationSize">Размер популяции</param>
        /// <param name="mutationRate">Вероятность мутаций</param>
        /// <param name="accuracy">Точность вычислений</param>
        /// <param name="convergenceRate">Процент разности между максимальными приспособленностями популяций для определения сходимости</param>
        /// <param name="maximumIterations">Максимальное количество генерируемых популяций</param>
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

            //Сгенерируем начальную популяцию
            GenerateRandomPopulation();
        }

        /// <summary>
        /// Сбрасывает хромосомы популяции на случайные значения.
        /// </summary>
        private void GenerateRandomPopulation()
        {
            var population = new List<string>();
            //Сгенерируем популяцию.
            var random = new Random();
            for (int i = 0; i < _populationSize; i++)
            {
                //Сгенерируем случайную хромосому
                string chromosome = "";
                foreach (GrayCode grayCode in _factors)
                {
                    var grayUint = (uint) random.Next(0, (int) (Math.Pow(2, _accuracy) - 1));
                    grayCode.DecimalCode = grayUint;
                    chromosome += grayCode.StringCode;
                }

                //Добавим в популяцию
                population.Add(chromosome);
            }
            //Заменим популяцию алгоритма полученной
            _population = population;
        }

        /// <summary>
        /// Генерирует живучести для каждого организма на основе данных
        /// о приспособленности (фитнесс-функции)
        /// </summary>
        private void GenerateLikelihoods()
        {
            //Посчитаем сумму фитнесс-функций для всех особей
            double fitnesSum = _fitnesses.Sum();

            //Посчитаем живучесть для каждой особи.
            for (int i = 0; i < _populationSize; i++)
                _likelihoods[i] = _fitnesses[i]/fitnesSum;
        }

        /// <summary>
        /// Возврашает индекс организма, выбираемый на основе данных
        /// о живучести (чем больше живучесть, тем больше вероятность
        /// выбора)
        /// </summary>
        /// <returns>Выбранный индекс организма</returns>
        private int Roullete(Random randomR)
        {
            double random = randomR.NextDouble(); //Случайное число
            double position = 0.0; //Позиция на рулетке

            //Итерируем по секторам
            for (int i = 0; i < _populationSize; i++)
            {
                //Переходим на конец сектора, соотв. живучести хромосомы.
                position += _likelihoods[i];

                //Если случайное число находится в пределах этого сектора, возвращаем
                //индекс сектора (хромосомы).
                if (random <= position)
                {
                    return i;
                }
            }

            //Если не можем выбрать
            return (new Random()).Next(_population.Count);
        }

        /// <summary>
        /// Возвращает потомка хромосомы A и B (порядок не важен)
        /// Так же мутирует один ген потомка с определенной вероятностью.
        /// </summary>
        /// <param name="chromosomeA">Хромосома A</param>
        /// <param name="chromosomeB">Хромосома B</param>
        /// <param name="random">ГПСЧ</param>
        /// <returns>Хромосома A+B</returns>
        private string CrossOver(string chromosomeA, string chromosomeB, Random random)
        {
            int length = chromosomeA.Length; //длина хромосомы 
            string offspring = ""; //потомок

            //С вероятностью 0.5 инициирующей особью будет A
            if (random.NextDouble() <= 0.5)
            {
                // A <-> B
                string t = chromosomeA;
                chromosomeA = chromosomeB;
                chromosomeB = t;
            }

            //Сгенерируем точки кроссовера:
            var points = new[] {random.Next(length), random.Next(length)};
            int start = points.Min(); //Начало отрезка - меньшая точка
            int end = points.Max(); //Конец - большая

            //Получаем новую хромосому, заменяя гены инициируемой хромосомы в [start; end] генами из инициирующей. 
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

            //Мутируем один ген с вероятностью MutatuonRate
            
            return offspring;
        }

        /// <summary>
        /// Выбирает две хромосомы, скрещивает их, возвращает потомка.
        /// </summary>
        /// <returns>Хромосома потомка</returns>
        private string ChooseAndBreed(Random random)
        {
            int indexA = 0;
            int indexB = 0;
            int iterations = 0;

            //Нужно выбрать две разных хромосомы.
            while (_population[indexA] == _population[indexB])
            {
                indexA = Roullete(random);
                indexB = Roullete(random);
                if (++iterations > _populationSize) break;
            }
            return CrossOver(_population[indexA], _population[indexB], random);
        }

        /// <summary>
        /// Генерирует живучесть для популяции.
        /// </summary>
        private void GenerateFitnesses()
        {
            //Проитерируем по всем хромосомам.
            var results = new List<AnalyzableLine>[_populationSize];
            for (int i = 0; i < _populationSize; i++)
            {
                //bool isString = false;
                int summHeight = 0;
                var lines = new List<AnalyzableLine>();
                int y = 0;
                while (y < _points.Length)
                {
                    //начнем строку текста
                    if (Result(_population[i], _ranges[y]))
                    {
                        //Начало строки  - текущая строчка массива.
                        int start = y;

                        var line = new AnalyzableLine(start, 0);

                        //Произведем итерации по строчкам до тех пор, пока функция
                        //не перестанет быть 'True'
                        while (Result(_population[i], _ranges[y]))
                        {
                            y++;
                            //Проверим на конец массива
                            if (y == _points.Length)
                                break;
                        }
                        int height = y - start;

                        //Вычислим высоту
                        line.Height = height;
                        summHeight += height;

                        //Добавим строку в список строк страницы.
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
                    //Вычислим однородность (фитнесс-функцию)
                    //Вычислим диапазон высот 
                    int heightsRange = lines.Max(line => line.Height) - lines.Min(line => line.Height);

                    //Вычислим диапазон расстояний между строками
                    int last = lines[0].Height + lines[0].Start;
                    var ranges = new int[lines.Count];
                    for (int k = 1; k < lines.Count; k++)
                    {
                        ranges[k - 1] = lines[k].Start - last;
                        last = lines[k].Start + lines[k].Height;
                    }
                    int distancesRange = ranges.Max() - ranges.Min();
                    //summHeight = lines.Sum(line => line.Height);


                    //значение фитнесс-функции для данной хромосомы.
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
        /// Возвращает результат распознавания строки (принадлежит или нет тексту)
        /// </summary>
        /// <param name="chromosome">Хромосома</param>
        /// <param name="range">Амплитуда высоты строк (масшт.)</param>
        /// <returns></returns>
        private bool Result(string chromosome, double range)
        {
            int j = 0;
            int factorIndex = 0;
            string bitValue = "";

            //Выделим из хромосомы гены, принадлежащие определенным факторам
            //Т.е. декодируем информацию, содержащуюся в генах
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