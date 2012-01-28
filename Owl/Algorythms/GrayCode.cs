using System;
using System.Collections.Generic;
using System.Linq;

namespace Owl.Algorythms
{
    public class GrayCode
    {
        private string _grayCode;

        /// <summary>
        /// Инициализирует объект класса GrayCode (Число с плавающей точкой с кодом Грея)
        /// </summary>
        /// <param name="value">Число с плавающей точкой в промежутке [min;max]</param>
        /// <param name="config">Конфигурация</param>
        public GrayCode(double value, GrayCodeConfig config)
        {
            Config = config;
            Value = value;
        }

        public GrayCodeConfig Config { get; private set; }

        public uint BitAccuracy
        {
            get { return Config.Accuracy; }
        }

        /// <summary>
        /// Возвращает или задает число, закодированное в код Грея
        /// </summary>
        public double Value
        {
            get
            {
                if (Config == null)
                    throw new ArgumentNullException();

                uint intGray = Convert.ToUInt32(_grayCode, 2);
                uint bin;
                double min = Config.Min;
                double max = Config.Max;
                uint accuracy = Config.Accuracy;

                // последовательный сдвиг вправо и суммирование исходного двоичного числа, 
                //до тех пор, пока очередной сдвиг не обнулит слагаемое.
                for (bin = 0; intGray > 0; intGray >>= 1)
                {
                    bin ^= intGray;
                }

                //Обратная операция. Из порядкового номера сектора получаем представление
                //с плавающей точкой (с определенной точностью) 
                var maxInt = Math.Pow(2, accuracy);
                //double val = bin*(max - min)/(Math.Pow(2, accuracy) - 1);
                double val = min + (bin * (max - min) / maxInt);
                //val -= (Math.Abs(min) - min)/2;
                return val;
            }
            private set
            {
                if (Config == null)
                    throw new ArgumentNullException();

                double min = Config.Min;
                double max = Config.Max;
                uint accuracy = Config.Accuracy;

                //Код Грея получим, проXOR'ив битовое представление с его сдвинутым вправо на один бит значением.
                //REF: http://ru.wikipedia.org/wiki/Код_Грея

                var maximumInt = (int) (Math.Pow(2, accuracy) - 1);

                var intValue = (int) (maximumInt*(value - min)/(max - min));
                var intGray = intValue ^ (intValue >> 1);

                //Переведем в двоичную, получив двоичное представление числа Грея.
                var bitString = Convert.ToString(intGray, 2);

                //Добавим незначащие нули в начало числа.
                while (accuracy != bitString.Length)
                    bitString = "0" + bitString;

                _grayCode = bitString;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}", Value);
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
            get { return StringCode.Select(c => c == '1').ToList(); }

            set { _grayCode = value.Aggregate("", (current, b) => current + (b ? "1" : "0")); }
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
                while (Config.Accuracy != _grayCode.Length)
                    _grayCode = "0" + _grayCode;
            }
        }
    }

    public class GrayCodeConfig
    {
        public readonly uint Accuracy;
        public readonly double Max;
        public readonly double Min;

        public GrayCodeConfig(double min, double max, uint bitAccuracy = 8)
        {
            Max = max;
            Min = min;
            Accuracy = bitAccuracy;
        }
    }
}