using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Text;
using System.IO;
using System.Windows.Forms;
using Owl.Algorythms;
using Owl.Domain;

namespace Owl
{
    public partial class OwlForm : Form
    {
        DrawingAgent _drawingAgent = new DrawingAgent();

        public OwlForm()
        {
            InitializeComponent();
        }


        private void OwlFormLoad(object sender, EventArgs e)
        {
            var bitmap = new Bitmap(100, 100);
            try
            {
                bitmap = new Bitmap(@"Data/IMAGE.BMP");
            }
            catch (ArgumentException)
            {
                Application.Exit();
            }                
            var page = new AnalyzablePage(bitmap);
            var ranges = page.Ranges();
            var points = page.Summ;
            var factors = new List<GrayCode>();
            var str = new StreamWriter("Ranges.dat");
            foreach (double t in ranges)
            {
                str.WriteLine(t);
            }

            str.Close();
            var str1 = new StreamWriter("Points.dat");
            foreach (double t in points)
            {
                str1.WriteLine(t);
            }
            str1.Close();
            ranges = Repositories.Functions.FeautureScaling(ranges);
            points = Repositories.Functions.FeautureScaling(points);
            factors.Add(new GrayCode(0,-1,1));
            factors.Add(new GrayCode(0, 0, 1));
            factors.Add(new GrayCode(0, 0, 1));
            //var geneticAlgorithm = new GeneticAlgorithm(points, ranges, factors);
            //page.Lines = geneticAlgorithm.Solve();
            page.RenderLines(imageOutput);
        }

        private void imageOutput_Click(object sender, EventArgs e)
        {

        }
    }

    public class DrawingAgent
    {
        List<DrawingEvent> _drawingEvents = new List<DrawingEvent>();

        public class DrawingEvent
        {
            private int _type;
            private readonly Pen _pen;
            private Rectangle _rectangle;
            private Point _start, _end;
            private Brush _brush;

            /// <summary>
            /// Инициализирует класс события отрисовки отрезка. 
            /// </summary>
            /// <param name="pen">Перо.</param>
            /// <param name="start">Стартовая точка.</param>
            /// <param name="end">Конечная точка</param>
            public DrawingEvent(Pen pen, Point start, Point end)
            {
                _pen = pen;
                _start = start;
                _end = end;
                _type = 1;
            }

            /// <summary>
            /// Инициализирует класс события отрисоки прямоугольника.
            /// </summary>
            /// <param name="brush">Кисть заливки</param>
            /// <param name="pen">Перо.</param>
            /// <param name="start">Начальная точка.</param>
            /// <param name="size">Размеры.</param>
            public DrawingEvent(Pen pen, Brush brush, Point start, Size size)
            {
                var rectangle = new Rectangle(start, size);
                _brush = brush;
                _rectangle = rectangle;
                _start = start;
                _type = 2;
            }

            /// <summary>
            /// Инициализирует класс события отрисовки отрезка.
            /// Фигура - отрезок с кооридатами x1,y1,x2,y2
            /// </summary>
            /// <param name="pen">Перо.</param>
            /// <param name="x1">Абсцисса начальной точи.</param>
            /// <param name="y1">Ордината начальной точи.</param>
            /// <param name="x2">Абсцисса конечной точки.</param>
            /// <param name="y2">Ордината конечной точки.</param>
            public DrawingEvent(Pen pen, int x1, int y1, int x2, int y2)
            {
                _pen = pen;
                _start = new Point(x1, y1);
                _end = new Point(x2, y2);
                _type = 1;
            }

            /// <summary>
            /// Выполняет событие отрисовки, рисуя на элементе Grahpics.
            /// </summary>
            /// <param name="graphics">Холст для отрисовки.</param>
            public void Draw(Graphics graphics)
            {
                switch (_type)
                {
                    case 1:
                        graphics.DrawLine(_pen, _start, _end);
                        break;
                    case 2:
                        graphics.DrawRectangle(_pen, _rectangle);
                        graphics.FillRectangle(_brush, _rectangle);
                        break;
                    default:
                        throw new Exception("Nothing to draw");

                }
            }

            /// <summary>
            /// Выполняет событие отрисовки, рисуя на элементе PictureBox.
            /// </summary>
            /// <param name="box">Холст для отрисовки.</param>
            public void Draw(PictureBox box)
            {
                Graphics grahpics = Graphics.FromImage(box.Image);
                Draw(grahpics);
            }
        }

        /// <summary>
        /// Отрисовывает все события.
        /// </summary>
        /// <param name="graphics"></param>
        public void Draw(Graphics graphics)
        {
            if (_drawingEvents.Count>0)
            {
                foreach (DrawingEvent drawningEvent in _drawingEvents)
                {
                    drawningEvent.Draw(graphics);
                }
                //Очистим список задач
                _drawingEvents.Clear();
            }
        }

        /// <summary>
        /// Отрисовывает все события.
        /// </summary>
        /// <param name="pictureBox"></param>
        public void Draw(PictureBox pictureBox)
        {
            if (_drawingEvents.Count > 0)
            {
                foreach (DrawingEvent drawingEvent in _drawingEvents)
                {
                    drawingEvent.Draw(pictureBox);
                }
                //Очистим список событий
                _drawingEvents.Clear();
            }
        }

        /// <summary>
        /// Добавляет новое событие отрисовки.
        /// </summary>
        /// <param name="drawingEvent"></param>
        public void AddEvent (DrawingEvent drawingEvent)
        {
            _drawingEvents.Add(drawingEvent);
        }
             
    }
}
