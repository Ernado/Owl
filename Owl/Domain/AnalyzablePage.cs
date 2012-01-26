using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Owl.Algorythms;
using Owl.DataBase.Domain;
using Owl.GeneticAlgorithm.Repositories;
using Owl.Repositories;

namespace Owl.Domain
{
    /// <summary>
    ///Класс страницы документа. 
    /// </summary>
    public class AnalyzablePage
    {
        private readonly double _ratio;
        private bool[,] _bit;
        private IList<AnalyzableLine> _lines = new List<AnalyzableLine>();
        private Bitmap _original;
        private int _height;
        private int _width;

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
            _bit = Functions.BinarizeImage(_original);
        }

        public AnalyzablePage(Page page)
        {
            if (page == null)
                throw new Exception("Страница не загружена");

            string imageFilePath = page.Book.Directory + "/" + page.FileName;

            if (!File.Exists(imageFilePath))
                throw new Exception("Ошибка чтения изображения");

            _original = new Bitmap(imageFilePath);

            _bit = Functions.BinarizeImage(_original);

            _height = _original.Height;
            _width = _original.Width;
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
                _bit = Functions.BinarizeImage(value, _ratio);
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
                _original = Functions.RasterizeBitMassive(value);
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
        private IList<AnalyzableLine> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public void AddLine(AnalyzableLine analyzableLine)
        {
            analyzableLine.AnalyzablePage = this;
            Lines.Add(analyzableLine);
        }

        public int HeightRange()
        {
            List<int> heights = _lines.Select(line => line.Height).AsParallel().ToList();
            if (heights.Count == 0)
                return Height;
            return heights.Max() - heights.Min();
        }

        public int DistanceRange()
        {
            var distances = new List<int>();
            int lastLineEnd = Lines[0].End;
            for (int i = 1; i < Lines.Count; i++)
            {
                distances.Add(Lines[i].End - lastLineEnd);
                lastLineEnd = Lines[i].End;
            }
            if (distances.Count == 0)
                return Height;
            return distances.Max() - distances.Min();
        }

        public double DensityRange()
        {
            List<double> densities = Lines.Select(line => line.Density()).AsParallel().ToList();

            if (densities.Count == 0)
                return 1;

            return densities.Max() - densities.Min();
        }

        /// <summary>
        /// Рассчитывает разность между максимальным и минимальным расстоянием между крайними точками.
        /// </summary>
        /// <returns>
        /// Массив со средними расстояниями между точками для каждой строчки.
        /// </returns>
        public IEnumerable<double> Ranges()
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
                foreach (AnalyzableLine line in _lines)
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
            AnalyzablePage page = this;
            var factorConfig = new List<GrayCodeConfig> {new GrayCodeConfig(2, 20)};
            var config = new GeneticAlgorithm.Domain.GeneticAlgorithm.Config(0.05, 20, 0.1, factorConfig, 20, 8);
            page.Lines = new List<AnalyzableLine>();
            page = new PageSegmentator(config, page).SegmentedPage();
            Lines = page.Lines;
        }
    }


    /// <summary>
    /// Класс строки.
    /// </summary>
    public class AnalyzableLine
    {
        private readonly List<AnalyzableWord> _words = new List<AnalyzableWord>();
        private int _height;
        private int _start;

        /// <summary>
        /// Инициализирует класс строки Line с данными начала строки и её высоты.
        /// </summary>
        /// <param name="start">Начало строки.</param>
        /// <param name="height">Высота.</param>
        public AnalyzableLine(int start, int height)
        {
            _start = start;
            _height = height;
        }


        public AnalyzablePage AnalyzablePage { get; set; }

        public int End
        {
            get { return _start + _height; }
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

        public void AddWord(AnalyzableWord analyzableWord)
        {
            analyzableWord.AnalyzableLine = this;
            _words.Add(analyzableWord);
        }

        public double Density()
        {

                int blackPixels = 0;
                int end = End;
                for (int y = _start; y < end; y++)
                    for (int x = 0; x < AnalyzablePage.Width; x++)
                        if (AnalyzablePage.Bit[x, y])
                            blackPixels++;
                return blackPixels/(double) (Height*AnalyzablePage.Width + 1);
        }
    }


    /// <summary>
    /// Класс слова. 
    /// </summary> 
    public abstract class AnalyzableWord
    {
        private readonly int _start;
        private readonly int _width;
        private AnalyzableLine _analyzableLine;

        protected AnalyzableWord(int start, int width)
        {
            _start = start;
            _width = width;
        }

        public AnalyzableLine AnalyzableLine
        {
            set { _analyzableLine = value; }
        }
    }
}