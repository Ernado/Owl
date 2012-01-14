using System;
using System.Collections.Generic;
using System.Linq;

namespace Owl.Algorythms
{
    public class GrayCode
    {
        private readonly int _bitAccuracy;
        private readonly double _max;
        private readonly double _min;
        private string _grayCode;

        public int BitAccuracy
        {
            get { return _bitAccuracy; }
        }

        /// <summary>
        /// Инициализирует объект класса GrayCode (Число с плавающей точкой с кодом Грея)
        /// </summary>
        /// <param name="value">Число с плавающей точкой в промежутке [min;max]</param>
        /// <param name="min">Минимальное значение</param>
        /// <param name="max">Максимальное значение</param>
        /// <param name="bitAccuracy">Количество битов под число. [1..32]</param>
        public GrayCode(double value, double min, double max, int bitAccuracy = 8)
        {
            _bitAccuracy = bitAccuracy;
            _min = min;
            _max = max;
            Value = value;
        }

        /// <summary>
        /// Возвращает или задает число, закодированное в код Грея
        /// </summary>
        public double Value
        {
            get
            {
                uint intGray = Convert.ToUInt32(_grayCode, 2);
                uint bin;
                // последовательный сдвиг вправо и суммирование исходного двоичного числа, 
                //до тех пор, пока очередной сдвиг не обнулит слагаемое.
                for (bin = 0; intGray > 0; intGray >>= 1)
                {
                    bin ^= intGray;
                }

                //Обратная операция. Из порядкового номера сектора получаем представление
                //с плавающей точкой (с определенной точностью)
                double val = bin*(_max - _min)/(Math.Pow(2, _bitAccuracy) - 1);
                val -= (Math.Abs(_min) - _min)/2;
                return val;
            }
            private set
            {
                //Разделим промежуток [min;max] на n равных секторов, где n = 2^a - 1, a - точность.
                //И определим, в каком секторе находится value
                //Порядковый номер этого сектора и будет численно равен битовому представлению этого числа.
                //Код Грея получим, проXOR'ив битовое представление с его сдвинутым вправо на один бит значением.
                //REF: http://ru.wikipedia.org/wiki/Код_Грея
                //Приведем значение к положительному.
                double doubleGray = value + (Math.Abs(_min) - _min)/2;

                int intValue = Convert.ToByte(Math.Round(doubleGray*(Math.Pow(2, _bitAccuracy) - 1)/(_max - _min)));
                int intGray = intValue ^ (intValue >> 1);

                //Переведем в двоичную, получив двоичное представление числа Грея.
                string bitString = Convert.ToString(intGray, 2);

                //Добавим незначащие нули в начало числа.
                while (_bitAccuracy != bitString.Length)
                    bitString = "0" + bitString;

                _grayCode = bitString;
            }
        }

        /// <summary>
        /// Возвращает или задает код Грея в строковом варианте.
        /// </summary>
        public string StringCode
        {
            get { return _grayCode; }
            set { _grayCode = value; }
        }

        /// <summary>
        /// Возвращает или задает код Грея в бинарном варианте.
        /// </summary>
        public List<bool> BooleanCode
        {
            get
            {
                return StringCode.Select(c => c == '1').ToList();
            }

            set
            {
                _grayCode = value.Aggregate("", (current, b) => current + (b ? "1" : "0"));
            }
        }

        /// <summary>
        /// Возвращает или задает код Грея в десятичном представлении.
        /// </summary>
        public uint DecimalCode
        {
            get { return Convert.ToUInt32(_grayCode, 2); }
            set
            {
                _grayCode = Convert.ToString(value, 2); //Добавим незначащие нули в начало числа.
                while (_bitAccuracy != _grayCode.Length)
                    _grayCode = "0" + _grayCode;
            }
        }

    }

    /*
    /// <summary>
    /// Класс генетического алгоритма.
    /// </summary> 
    public class GeneticAlgorithm
    {
        private int _populationSize;
        protected double _fitness;
        protected double _mutateRate;
        protected double[] _points;
        protected double[] _range;
        private List<Organism> _organisms = new List<Organism>();
        /// <summary>
        /// Организм.
        /// </summary>
        public class Organism : GeneticAlgorithm
        {
            private new double _fitness;
            private Chromosome _chromosome;
            private double _likelihood;
            private double _result;

            // TODO: Test

            //<summary>
            //Класс однородной хромосомы с заданным количеством параметров и их длиной.
            //</summary> 
            public class Chromosome
            {
                private byte _sectors;
                private byte _length;
                private string _chromosome;

                public Chromosome(string chromosome, byte sectors, byte length = 8)
                {
                    _sectors = sectors;
                    _length = length;
                    _chromosome = chromosome;
                }

                /// <summary>
                /// Вовзращает или задает хромосому
                /// </summary>
                public string Code
                {
                    get { return _chromosome; }
                    set
                    {
                        if (value.Length != _chromosome.Length)
                            throw new ArgumentException("Lengths of chromosomes must be equal");
                        _chromosome = value;
                    }
                }

                /// <summary>
                /// Возвращает двоичное значение гена.
                /// </summary>
                /// <param name="sector">Номер гена.</param>
                /// <returns></returns>
                public string GetValueCode(byte sector)
                {
                    var value = "";
                    for (int i = (sector - 1) * _length; i < sector * _length; i++)
                    {
                        value += _chromosome[i];
                    }
                    return value;
                }
            }

            /// <summary>
            /// Класс организма.
            /// </summary>
            /// <param name="chromosome">Генотип организма.</param>
            public Organism(Chromosome chromosome)
            {
                _chromosome = chromosome;
            }

            /// <summary>
            /// Рассчитывает и возвращает живучесть организма.
            /// </summary>
            public double Fitness(double[] points, double[] range)
            {
                if (points == null) throw new ArgumentNullException("points");
                if (range == null) throw new ArgumentNullException("range");

                var lines = new List<Owl.APage.Line>();
                double fitness;
                bool isString = false;
                int start = 0;
                int summHeight = 0;

                //Проитерируем по строчкам.
                for (int i = 0; i < points.Length; i++)
                {
                    //Если удовлетворяет подобранным параметрам
                    if (Result(points[i], range[i]))
                    {
                        //Если строчка не начата
                        if (!isString)
                        {
                            isString = true;
                            start = i;
                        }
                    } //Если строчка закончена или конец файла
                    else if (isString || i == (points.Length - 1))
                    {
                        //Вычислим её высоту и добавим в список строчек.
                        int height = i - start;
                        lines.Add(new Owl.APage.Line(start, height));
                        summHeight += height;
                        isString = false;
                    }
                }

                double avgHeight = (double)summHeight / lines.Count;

                //Выберем строки, у которых высота больше средней
                var newlines = lines.Where(line => line.Height < avgHeight).ToList();

                //Вычислим однородность (фитнесс-функцию)
                //Вычислим диапазон высот 
                var heightsRange = newlines.Max(line => line.Height) - newlines.Min(line => line.Height);

                //Вычислим диапазон расстояний между строками
                int last = newlines[0].Height + newlines[0].Start;
                var ranges = new int[newlines.Count];
                for (int i = 1; i < lines.Count; i++)
                {
                    ranges[i - 1] = lines[i].Start - last;
                    last = lines[i].Start + lines[i].Height;
                }
                var distancesRange = ranges.Max() - ranges.Min();

                //Вернем значение фитнесс-функции для данной хромосомы.
                return distancesRange + heightsRange;
            }



            /// <summary>
            /// Возвращает или задает хромосому организма.
            /// </summary>
            public Chromosome OrganismChromosome
            {
                get { return _chromosome; }
                set { _chromosome = value; }
            }

            /// <summary>
            /// Возвращает или задает вероятность размножения.
            /// </summary>
            public double Likelihood
            {
                get { return _likelihood; }
                set { _likelihood = value; }
            }

            /// <summary>
            /// Возвращает результат организма (значение минимизируемой функции с параметрами из генома организма).
            /// </summary>
            private bool Result(double points, double range)
            {
                var vector = new double[3];
                var x = new GrayCode(0, -1, 0) { StringCode = _chromosome.GetValueCode(0) };
                vector[0] = x.Value;
                x = new GrayCode(0, 0, 0.5) { StringCode = _chromosome.GetValueCode(1) };
                vector[1] = x.Value;
                x.StringCode = _chromosome.GetValueCode(2);
                vector[2] = x.Value;
                return (vector[0] + vector[1] * points + vector[2] * range) > 0;
            }
        }

        /// <summary>
        /// Возвращает список инициирующей популяции (со случайным значением хромосомы)
        /// </summary>
        /// <param name="size"></param>
        /// <param name="accuracy"></param>
        /// <param name="points"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private List<Organism> InitiatePopulation(int size, double[] points, double[] range)
        {
            var population = new List<Organism>();
            for (int i = 0; i < size; i++)
            {
                var rand = new Random();
                var randomNumber = Convert.StringCode(rand.Next(255));
                var chromosome = new Organism.Chromosome(randomNumber, 3);
                var organism = new Organism(chromosome);
                population.Add(organism);
            }
            return population;
        }

        public string CrossOver(Organism organism1, Organism organism2)
        {
            var chromosome1 = organism1.OrganismChromosome.Code;
            var chromosome2 = organism2.OrganismChromosome.Code;
            var length = chromosome1.Length;
            string generated = "";
            string master = chromosome1;
            string second = chromosome2;
            var rand = new Random();
            int[] points = { rand.Next(0, length), rand.Next(0, length) };
            if (rand.NextDouble() > 0.5)
            {
                master = chromosome2;
                second = chromosome1;
            }
            var start = points.Min();
            var end = points.Max();
            for (int i = 0; i < length; i++)
            {
                if (i >= start && i <= end)
                {
                    generated += master[i];
                }
                else
                {
                    generated += second[i];
                }
            }

            //Мутируем ген
            if (rand.NextDouble() > _mutateRate)
            {
                var x = rand.Next(0, length);
                char[] chars = generated.ToCharArray();
                chars[x] = (chars[x] == '0') ? '1' : '0';
                generated = chars.ToString();
            }

            return generated;
        }

        public double Evolute()
        {
            var population = new List<Organism>();
            double[] fitnesses = new double[_populationSize];
            //Generate fitnesses
            for (int i = 0; i < _populationSize; i++)
            {
                fitnesses[i] = _organisms[i].Fitness(_points, _range);
            }
            var sumFitness = fitnesses.Sum();
            //
            while (population.Count < _populationSize)
            {
                int index1 = RouletteWheelSelection(fitnesses, sumFitness);
                int index2 = RouletteWheelSelection(fitnesses, sumFitness);
                var chromosome = new Organism.Chromosome(CrossOver(_organisms[index1], _organisms[index2]), 3);
                var organism = new Organism(chromosome);
                population.Add(organism);
            }
            return fitnesses.Max();

        }


        private int RouletteWheelSelection(double[] fitnesses, double sumFitness)
        {

            return 0;
        }

        public void FindOptimal();

    }
    */
}