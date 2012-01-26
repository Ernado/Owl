using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Owl.DataBase.Domain;
using Owl.Repositories;
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

        public Pen SelectPen;

        public Graphics Canvas { get; set; }

        public Layout(Page page, Graphics canvas)
        {
            Canvas = canvas;
            //throw new NotImplementedException();
        }

        public Layout(Graphics graphics)
        {
            Canvas = graphics;
        }

        public void RemoveFigure (Figure figure)
        {
            foreach (var child in Figures.Where(child => child == figure))
                {
                    Figures.Remove(child);
                    return;
                }
        }

        public void AddFigure (Figure figure)
        {
            figure.Layout = this;
            Figures.Add(figure);
        }
    }

    /// <summary>
    /// Класс фигуры.
    /// </summary>
    public abstract class Figure
    {
        public GraphicsPath Path { get; protected set; }

        public Layout Layout { get; set; }

        /// <summary>
        /// Проверка принадлежности точки к фигуре.
        /// </summary>
        /// <param name="p">точка</param>
        /// <returns></returns>
        public abstract bool Intersects(Point p);

        /// <summary>
        /// Отрисовка фигуры.
        /// </summary>
        /// <param name="brush">Заливка</param>
        /// <param name="pen">Контур</param>
        public abstract void Draw(Brush brush, Pen pen);

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

        public SolidFigure(GraphicsPath path)
        {
            Path = path;
        }

        public Point Location;

        public SolidFigure()
        {
        }

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

        /// <summary>
        /// Рисует границу
        /// </summary>
        public void DrawBounds (Pen pen)
        {
            var bounds = Bounds;
            Layout.Canvas.DrawRectangle(pen, bounds.Left - 2, 
                bounds.Top - 2, bounds.Width + 4, bounds.Height + 4);
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
        public override void Draw(Brush brush, Pen pen)
        {
            //gr.TranslateTransform(location.X, location.Y);
            Layout.Canvas.FillPath(brush, Path);
            Layout.Canvas.DrawPath(pen, Path);
            //if (!string.IsNullOrEmpty(text))
            //gr.DrawString(text, SystemFonts.DefaultFont, Brushes.Black, textRect, StringFormat);
            Layout.Canvas.ResetTransform();
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

        public override void Draw(Brush brush1, Pen pen1)
        {
            Layout.Canvas.DrawRectangle(Pens.Black, Location.X - DefaultSize, Location.Y - DefaultSize, DefaultSize * 2,
                             DefaultSize*2);
            Layout.Canvas.FillRectangle(Brushes.Red, Location.X - DefaultSize, Location.Y - DefaultSize, DefaultSize * 2,
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

        public override bool Intersects(Point p)
        {
            if (From == null || To == null)
                return false;

            RecalcPath();
            return Path.IsOutlineVisible(p, ClickPen);
        }

        public override void Draw(Brush brush, Pen pen)
        {
            if (From == null || To == null)
                return;

            RecalcPath();
            Layout.Canvas.DrawPath(pen, Path);
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

    public class UnclosedPathFigure : Figure
    {
        public UnclosedPathFigure()
        {
            Points = new List<Point>();
        }

        public List<Point> Points { get; private set; }

        public Point FirstPoint { get { return Points[0]; } }

        public void AddPoint (Point point)
        {
            Points.Add(point);
        }

        public void ReplaceLastPoint(Point point)
        {
            Points[Points.Count - 1] = point;
        }

        public void DeleteLastPoint ()
        {
            Points.RemoveAt(Points.Count-1);
        }

        public  bool IsNearStart (Point p)
        {
            var startPoint = Points[0];
            return (Math.Abs(startPoint.X - p.X) < 5) && (Math.Abs(startPoint.Y - p.Y) < 5);
        }

        public override bool Intersects(Point p)
        {
            return false;
        }

        public void UpdatePath()
        {
            Path = Functions.GeneratePathFromPoints(Points, false);
        }

        public GraphicsPath GeneratePath()
        {
            return Functions.GeneratePathFromPoints(Points, false);
        }

        public override void Draw(Brush brush, Pen pen)
        {
            Layout.Canvas.DrawPath(pen, Path);
        }

        public override List<Marker> CreateMarkers(Layout diagram)
        {
            throw new NotImplementedException();
        }
    }
}