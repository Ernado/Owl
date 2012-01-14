using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Owl.Algorythms;
using Owl.GeneticAlgorithm.Repositories;
using Owl.Repositories;
using Owl.DataBase.Domain;

namespace Owl.Domain
{
    /// <summary>
    ///Класс страницы документа. 
    /// </summary>
    public class AnalyzablePage
    {
        private readonly double _ratio;
        private bool[,] _bit;
        private IList<Line> _lines = new List<Line>();
        private Bitmap _original;

        /// <summary>
        /// Инициализирует новый экземпляр класса AnalyzablePage с указанным изображением
        /// и порогом бинаризации.
        /// При инициализации произойдет бинаризация. 
        /// </summary>
        /// <param name="image">
        /// Изображение
        /// </param>
        /// <param name="ratio">Коэффициент яркости.</param>
        public AnalyzablePage(Bitmap image, double ratio = 0.8)
        {
            _original = image;
            _ratio = ratio;
            _bit = Repositories.Functions.BinarizeImage(_original);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса APage с указанным битовым
        /// представлением. При инициализации произойдет преобразование
        /// битового представления в изображение.
        /// </summary>
        /// <param name="bit">
        /// Бинирное представление
        /// </param>
        public AnalyzablePage(bool[,] bit)
        {
            _original = Functions.RasterizeBitMassive(bit);
            _bit = bit;
        }

        protected AnalyzablePage()
        {
            _original = new Bitmap(1, 1);
            _bit = new bool[1,1];
        }

        public AnalyzablePage(Page page)
        {
            if (page == null)
                throw new Exception("Страница не загружена");

            var imageFilePath = page.Book.Directory + "/" + page.FileName;

            if (!File.Exists(imageFilePath))
                throw new Exception("Ошибка чтения изображения");

            _original = new Bitmap(imageFilePath);
            
            _bit = Functions.BinarizeImage(_original);
        }

/*
        /// <summary>
        /// Передает изображение, эквивалентное битовому представлению страницы 
        /// (не оригинал)
        /// </summary>
        public Bitmap BinarizedImage
        {
            get { return Repositories.Functions.RasterizeBitMassive(_bit); }
        }
*/

        /// <summary>
        /// Передает или задает изображение страницы. 
        /// При задании нового изображения происходит перерасчет
        /// битового представления. 
        /// </summary>
        public Bitmap Original
        {
            get { return _original; }
            set
            {
                _bit = Repositories.Functions.BinarizeImage(value, _ratio);
                _original = value;
            }
        }

        /// <summary>
        /// Передает или задает битовое представление страницы.
        /// При задании нового битового представления изображение 
        /// восстанавливается из данного массива.
        /// </summary>
        public bool[,] Bit
        {
            get { return _bit; }
            set
            {
                _original = Repositories.Functions.RasterizeBitMassive(value);
                _bit = value;
            }
        }

        /// <summary>
        /// Считает сумму черных точек для каждой строчки массива и возвращает её.
        /// </summary>
        /// <returns>Массив из сумм черных точек для каждой строчки.</returns>
        public double[] Summ
        {
            get
            {
                var summ = new double[_original.Height];
                for (int y = 0; y < _original.Height; y++)
                {
                    summ[y] = 0;
                    for (int x = 0; x < _original.Width; x++)
                        if (_bit[x, y]) summ[y]++;
                }
                return summ;
            }
        }

        /// <summary>
        /// Возвращает линии.
        /// </summary>
        public IList<Line> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }

        public void AddLine(Line line)
        {
            line.AnalyzablePage = this;
            Lines.Add(line);
        }

        public int HeightRange ()
        {
            var heights = _lines.Select(line => line.Height).ToList();
            return heights.Max() - heights.Min();
        }

        public int DistanceRange ()
        {
            var distances = new List<int>();
            var lastLineEnd = Lines[0].End;
            for (int i = 1; i < Lines.Count; i++)
            {
                distances.Add(Lines[i].End - lastLineEnd);
                lastLineEnd = Lines[i].End;
            }
            return distances.Max() - distances.Min();
        }

        public double DensityRange ()
        {
            var densities = Lines.Select(line => line.Density()).ToList();
            return densities.Max() - densities.Min();
        }

        public int Width
        {
            get { return Original.Width; }
        }

        public int Height
        {
            get { return Original.Height; }
        }

        /// <summary>
        /// Рассчитывает разность между максимальным и минимальным расстоянием между крайними точками.
        /// </summary>
        /// <returns>
        /// Массив со средними расстояниями между точками для каждой строчки.
        /// </returns>
        public double[] Ranges()
        {
            var ranges = new double[_original.Height];

            //Пройдемся по всем строчкам
            for (int y = 0; y < _original.Height; y++)
            {
                ranges[y] = 0;
                var dx = new List<int>();
                int last = 0;
                int x = 0; //Текущая координата

                //Найдем первую черную точку
                while (last == 0 && x < _original.Width)
                {
                    if (_bit[x, y])
                        last = x;
                    x++;
                }

                if (last == 0)
                {
                    ranges[y] = 0;
                    continue;
                }
                //Рассчитаем расстояния, начиная от первой точки
                for (x = last; x < _original.Width; x++)
                {
                    if (!_bit[x, y]) continue; //Пропускаем белые точки

                    //Если это  - точка
                    if (last != x - 1)
                    {
                        dx.Add((x - last) + 1);
                    }
                    last = x;
                }
                if (dx.Count > 2)
                {
                    dx.Sort();
                    ranges[y] = dx[dx.Count/2];
                    //ranges[y] = dx.Average();
                }
                else
                    ranges[y] = 0;
            }

            double maximum = ranges.Max();
            for (int i = 0; i < ranges.Length; i++)
            {
                if (Math.Abs(ranges[i] - 0) < 0.9)
                    ranges[i] = maximum;
            }
            return ranges;
        }

        /// <summary>
        /// Анализирует изображение и автоматически делит его на строки. DEPRECATED.
        /// </summary>
        /// <param name="likelihood">Допустимая разность между текстом и пустотами.</param>
        public void FindLines(float likelihood = 0.01f)
        {
            //Посчитаем количество точек в строках
            double[] summ = Summ;

            //Найдем максимальное и минимальное количество точек в строках
            var max = (int) summ.Max();
            var min = (int) summ.Min();

            //Наименьшее значение не должно быть нулевым
            min = (min == 0) ? 1 : min;

            /* Инициируем начальный параметр минимального количества точек как целую часть 
                 * корня квадратного из произведения минимального и максимального количества точек   */
            double minimalDots = Math.Sqrt(min*max);

            int summHeight = 0;

            //Инициируем разность между выделенными (принадлежащими тексту) и невыделенными строками
            //как максимально возможную
            float delta = _original.Height;

            //Пытаемся подобрать нужный минимальный порог.
            //Пока разность между выделенными и невыделенными строками не будет минимальна
            while (Math.Abs(delta) > likelihood*(_original.Height))
            {
                int l = 0;
                summHeight = 0;

                //Очищаем список строк текста.
                _lines = new List<Line>();

                //Совершим итерации по всем строкам бинарного массива.
                while (l < (_original.Height - 1))
                {
                    //Если количество точек в строке массива больше минимальной, 
                    //начнем строку текста
                    if (summ[l] >= minimalDots)
                    {
                        //Начало строки  - текущая строчка массива.
                        int start = l;

                        //Создадим строку в месте, где количество точек больше минимального
                        var line = new Line(start, 0);

                        //Произведем итерации по строчкам до тех пор, пока количество точек
                        //не перестанет быть минимальным и не будет достигнут конец изображения.
                        while (summ[l] >= minimalDots)
                        {
                            l++;
                            //Проверим на конец массива
                            if (l == _original.Height)
                                break;
                        }
                        int height = l - start;

                        //Вычислим высоту
                        line.Height = height;
                        summHeight += height;

                        //Добавим строку в список строк страницы.
                        _lines.Add(line);
                    }

                    l++;
                }

                //Вычислим разность между межстрочным пространством и суммарной высотой строк.
                delta = summHeight*2 - _original.Height;

                //Изменим параметр минимального количества точек.
                float k = delta/_original.Height;
                minimalDots = Math.Abs(minimalDots + minimalDots*k);
            }

            //Посчитаем среднюю высоту строчки
            int medianHeight;
            if (_lines.Count > 0)
                medianHeight = summHeight/_lines.Count;
            else
                //Если нет строчек, выдаем исключение.
                throw new Exception("No lines found");

            //Удалим все строчки, высота которых меньше средней
            List<Line> newlines = _lines.Where(line => line.Height > medianHeight).ToList();

            _lines = newlines;
        }

        /// <summary>
        /// Отображает линии, соответствующие началу и концу строчки на PictureBox.
        /// </summary>
        /// <param name="pictureOutput">PictureBox, на котором надо отобразить линии.</param>
        public void RenderLines(PictureBox pictureOutput)
        {
            var bitmap = new Bitmap(_original);
            Graphics graphicOverlay = Graphics.FromImage(bitmap);

            //Определим цвета.
            Color color = Color.BlueViolet;

            //Получим полупрозрачный цвет.
            Color semiColor = Color.FromArgb(20, color.R, color.G, color.B);
            var pen = new Pen(color);
            var brush = new SolidBrush(semiColor);

            try
            {
                foreach (Line line in _lines)
                {
                    var rectangle = new Rectangle(1, line.Start, bitmap.Width - 2, line.Height);
                    graphicOverlay.DrawRectangle(pen, rectangle);
                    graphicOverlay.FillRectangle(brush, rectangle);
                }
            }
            catch
            {
                MessageBox.Show(@"ERROR: Unable to render lines.");
            }

            //Обновим картинку
            pictureOutput.Image = bitmap;
            pictureOutput.Refresh();
        }

        public void SegmentatePage()
        {
            var page = this;
            var factors = new List<GrayCode> { new GrayCode(0, 2, 20) };
            var config = new GeneticAlgorithm.Domain.GeneticAlgorithm.Config(0.05, 20, 0.1, factors, 20, 8);
            page.Lines = new List<Line>();
            page = new PageSegmentator(config, page).SegmentedPage();
            Lines = page.Lines;
        }
    }


    /// <summary>
    /// Класс строки.
    /// </summary>
    public class Line
    {
        private int _height;
        private int _start;
        private AnalyzablePage _analyzablePage;
        private List<Word> _words = new List<Word>();

        /// <summary>
        /// Инициализирует класс строки Line с данными начала строки и её высоты.
        /// </summary>
        /// <param name="start">Начало строки.</param>
        /// <param name="height">Высота.</param>
        public Line(int start, int height)
        {
            _start = start;
            _height = height;
        }

        public void AddWord (Word word)
        {
            word.Line = this;
            _words.Add(word);
        }


        public AnalyzablePage AnalyzablePage
        {
            get { return _analyzablePage; }
            set { _analyzablePage = value; }
        }


        public Line()
        {
            _start = 0;
            _height = 0;
        }

        public int End
        {
            get{return _start + _height;}
        }

        public double Density ()
        {
            var blackPixels = 0;
            var end = End;
            for (int y = _start; y < end; y++)
                for (int x = 0; x < AnalyzablePage.Height; x++)
                    if (AnalyzablePage.Bit[x, y])
                        blackPixels++;
            return blackPixels/(double)(Height*AnalyzablePage.Width);
        }
        
        /// <summary>
        /// Передает или получает координату начала строки.
        /// </summary>
        public int Start
        {
            get { return _start; }

            set { _start = value; }
        }

        /// <summary>
        /// Передает или получает высоту строки.
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Возвращает изображение строки.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                var original = _analyzablePage.Original;
                var rectangle = new Rectangle(_start, 0, original.Width, _height);
                return original.Clone(rectangle, original.PixelFormat);
            }
        }
    }


    /// <summary>
    /// Класс слова. 
    /// </summary> 
    public class Word
    {
        private readonly int _start;
        private readonly int _width;
        private Line _line;

        public Line Line
        {
            get { return _line; }
            set { _line = value; }
        }

        protected Word(int start, int width)
        {
            _start = start;
            _width = width;
        }

        /// <summary>
        /// Возвращает изображение слова.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                var original = _line.AnalyzablePage.Original;
                var rectangle = new Rectangle(_line.Start, _start, _width, _line.Height);
                return original.Clone(rectangle, original.PixelFormat);
            }
        }
    }
}
    