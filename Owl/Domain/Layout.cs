using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Owl.DataBase.Domain;
using Point = System.Drawing.Point;

namespace Owl.Domain
{
    /// <summary>
    /// Класс графического представления разметки.
    /// </summary>
    public class Layout
    {
        /// <summary>
        /// Список фигур разметки.
        /// </summary>
        public readonly List<Figure> Figures = new List<Figure>();

        public Graphics Canvas { get; private set; }

        public Layout(Page page, Graphics canvas)
        {
            Canvas = canvas;
            //throw new NotImplementedException();
        }

        public Layout(Graphics graphics)
        {
            Canvas = graphics;
        }
    }

    /// <summary>
    /// Класс фигуры.
    /// </summary>
    public abstract class Figure
    {
        //readonly SerializableGraphicsPath _serializablePath = new SerializableGraphicsPath();
        //карандаш отрисовки линий
        public static Pen pen = Pens.Black;
        protected GraphicsPath Path { get; set; }

        protected Layout _layout;


        /// <summary>
        /// Проверка принадлежности точки к фигуре.
        /// </summary>
        /// <param name="p">точка</param>
        /// <returns></returns>
        public abstract bool Intersects(Point p);

        /// <summary>
        /// Отрисовка фигуры.
        /// </summary>
        /// <param name="gr">Поверхность рисования.</param>
        public abstract void Draw();

        /// <summary>
        /// Получение маркеров.
        /// </summary>
        /// <param name="diagram"></param>
        /// <returns></returns>
        public abstract List<Marker> CreateMarkers(Layout diagram);
    }

    /// <summary>
    /// Класс замкнутой фигуры.
    /// </summary>
    public class SolidFigure : Figure
    {
        //размер новой фигуры, по умолчанию
        //protected static int DefaultSize = 40;
        //заливка фигуры
        public static Brush brush = Brushes.White;


        public Point Location;

        public Point Center
        {
            get
            {
                return new Point
                           {
                               X = (int) Math.Round((Path.PathPoints.Select(point => point.X)
                                                    ).Sum()/Path.PointCount),
                               Y = (int) Math.Round((Path.PathPoints.Select(point => point.Y)
                                                    ).Sum()/Path.PointCount)
                           };
            }
        }

        /// <summary>
        /// Прямоугольник вокруг фигуры в абсолютных координатах.
        /// </summary>
        public RectangleF Bounds
        {
            get
            {
                RectangleF bounds = Path.GetBounds();
                return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
            }
        }

        //размер прямоугольника вокруг фигуры
        public SizeF Size
        {
            get { return Path.GetBounds().Size; }
            set
            {
                SizeF oldSize = Path.GetBounds().Size;
                var newSize = new SizeF(Math.Max(1, value.Width), Math.Max(1, value.Height));
                //коэффициент шкалировани по x
                float kx = newSize.Width/oldSize.Width;
                //коэффициент шкалировани по y
                float ky = newSize.Height/oldSize.Height;
                Scale(kx, ky);
            }
        }

        /// <summary>
        /// Проверка принадлежности точки к фигуре.
        /// </summary>
        /// <param name="p">точка</param>
        /// <returns></returns>
        public override bool Intersects(Point p)
        {
            return Path.IsVisible(p);
        }

        //изменение масштаба фигуры
        public void Scale(float scaleX, float scaleY)
        {
            //масштабируем линии
            var m = new Matrix();
            m.Scale(scaleX, scaleY);
            Path.Transform(m);
        }

        //сдвиг местоположения фигуры
        public virtual void Move(int dx, int dy)
        {
            var m = new Matrix();
            m.Translate(dx, dy);
            Path.Transform(m);
        }

        //отрисовка фигуры
        public override void Draw()
        {
            //gr.TranslateTransform(location.X, location.Y);
            _layout.Canvas.FillPath(brush, Path);
            _layout.Canvas.DrawPath(pen, Path);
            //if (!string.IsNullOrEmpty(text))
            //gr.DrawString(text, SystemFonts.DefaultFont, Brushes.Black, textRect, StringFormat);
            _layout.Canvas.ResetTransform();
        }

        //создание маркера для изменения размера
        public override List<Marker> CreateMarkers(Layout layout)
        {
            var markers = new List<Marker>();
            Marker m = new SizeMarker();
            m.TargetFigure = this;
            markers.Add(m);

            return markers;
        }
    }

    /// <summary>
    /// Класс маркера.
    /// </summary>
    public abstract class Marker : SolidFigure
    {
        protected const int DefaultSize = 2;
        public Figure TargetFigure;

        public override void Move(int dx, int dy)
        {
            Location.Offset(dx, dy);
        }

        public override bool Intersects(Point p)
        {
            if (p.X < Location.X - DefaultSize || p.X > Location.X + DefaultSize)
                return false;
            if (p.Y < Location.Y - DefaultSize || p.Y > Location.Y + DefaultSize)
                return false;

            return true;
        }

        public override void Draw()
        {
            _layout.Canvas.DrawRectangle(Pens.Black, Location.X - DefaultSize, Location.Y - DefaultSize, DefaultSize * 2,
                             DefaultSize*2);
            _layout.Canvas.FillRectangle(Brushes.Red, Location.X - DefaultSize, Location.Y - DefaultSize, DefaultSize * 2,
                             DefaultSize*2);
        }

        public abstract void UpdateLocation();
    }

    public class FreeMarker : Marker
    {
        public override void UpdateLocation()
        {
        }
    }


    /// <summary>
    /// Маркер изменения размера
    /// </summary>
    public class SizeMarker : Marker
    {
        public override void UpdateLocation()
        {
            var solidFigure = TargetFigure as SolidFigure;
            if (solidFigure != null)
            {
                RectangleF bounds = solidFigure.Bounds;
                Location = new Point((int) Math.Round(bounds.Right) + DefaultSize/2,
                                     (int) Math.Round(bounds.Bottom) + DefaultSize/2);
            }
        }

        public override void Move(int dx, int dy)
        {
            base.Move(dx, dy);
            var solidFigure = TargetFigure as SolidFigure;
            if (solidFigure != null)
                solidFigure.Size =
                    SizeF.Add(solidFigure.Size, new SizeF(dx*2, dy*2));
        }
    }

    /// <summary>
    /// Маркер концов отрезков.
    /// </summary>
    public class EndLineMarker : Marker
    {
        private readonly int _pointIndex;

        public EndLineMarker(int pointIndex)
        {
            _pointIndex = pointIndex;
        }

        public override void UpdateLocation()
        {
            var line = (TargetFigure as LineFigure);
            if (line != null && (line.From == null || line.To == null))
                return; //не обновляем маркеры оторванных концов
            //фигура, с которой связана линия
            if (line != null)
            {
                SolidFigure figure = _pointIndex == 0 ? line.From : line.To;
                Location = figure.Location;
            }
        }
    }

    //соединительная линия
    public class LineFigure : Figure
    {
        private static readonly Pen ClickPen = new Pen(Color.Transparent, 3);
        public SolidFigure From;
        public SolidFigure To;

        public override void Draw()
        {
            if (From == null || To == null)
                return;

            RecalcPath();
            _layout.Canvas.DrawPath(pen, Path);
        }

        public override bool Intersects(Point p)
        {
            if (From == null || To == null)
                return false;

            RecalcPath();
            return Path.IsOutlineVisible(p, ClickPen);
        }

        protected virtual void RecalcPath()
        {
            PointF[] points = null;
            if (Path.PointCount > 0)
                points = Path.PathPoints;
            if (points == null || (Path.PointCount == 2 && points[0] == From.Location && points[1] == To.Location))
                return;
            Path.Reset();
            Path.AddLine(From.Location, To.Location);
        }

        public override List<Marker> CreateMarkers(Layout layout)
        {
            var markers = new List<Marker>();
            var m1 = new EndLineMarker(0);
            m1.TargetFigure = this;
            var m2 = new EndLineMarker(1);
            m2.TargetFigure = this;

            markers.Add(m1);
            markers.Add(m2);

            return markers;
        }
    }
}