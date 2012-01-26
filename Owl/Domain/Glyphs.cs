using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Owl.DataBase.Domain;
using Owl.Repositories;
using Point = System.Drawing.Point;

namespace Owl.Domain
{
    enum RedactorStates
    {
        Default,
        Dragging,
        CreatingLine,
        CreatingWord
    }

    enum RedactorModes
    {
        Create,
        Move,
        AddPolygon
    }

    class GlyphConfig
    {
        public readonly Brush Brush;
        public readonly Pen Pen;

        public GlyphConfig(Brush brush, Pen pen)
        {
            Brush = brush;
            Pen = pen;
        }
    }

    abstract class Glyph
    {
        public Figure Figure;
        public Redactor Redactor;
        public VectorRedactorRepository ParentVectorRedactor;
        public Glyph MainGlyph;
        public GlyphConfig Config;

        public abstract void ProcessMove(Point location);

        protected abstract bool Intersects(Point point);

        protected void Invalidate()
        {
            if (Redactor == null)
                throw new ArgumentNullException();

            Redactor.InvalidateInterfaceBox();
        }

        public void InsertChild(Glyph glyph)
        {
            glyph.Parent = this;
            glyph.MainGlyph = MainGlyph;
            glyph.Redactor = Redactor;
            glyph.ParentVectorRedactor = ParentVectorRedactor;
            ParentVectorRedactor.Layout.AddFigure(glyph.Figure);
            Childs.Add(glyph);
        }

/*
        /// <summary>
        /// Загрузка конфигурации глифа
        /// </summary>
        /// <param name="config">Конфигурация</param>
        protected void LoadConfig(GlyphConfig config)
        {
            Redactor = config.Redactor;
            ParentVectorRedactor = config.VectorRedactor;
        }
*/

        public void RemoveChild(Glyph glyph)
        {
            foreach (var child in Childs.Where(child => child == glyph))
            {
                child.ParentVectorRedactor.Layout.RemoveFigure(child.Figure);
                Childs.Remove(child);
                return;
            }
            throw new Exception("Child not exist");
        }

        public abstract void Move(int dx, int dy);
        protected Glyph Parent { get; set; }
        protected List<Glyph> Childs { get; set; }

        /// <summary>
        /// Находит глиф в точке, рекурсивно проходя по дочерним элементам.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Glyph FindGlyphIn(Point location)
        {
            Glyph highterLayerGlyph = null;

            if (!Intersects(location))
                return null;

            //проитерируем по дочерним элементам в инвертированном списке
            for (var index = Childs.Count - 1; index >= 0; index--)
            {
                var child = Childs[index];
                highterLayerGlyph = child.FindGlyphIn(location);
                if (highterLayerGlyph != null)
                    break;
            }

            //Если точка не принадлежит дочерним элементам
            //то она принадлежит данному
            return highterLayerGlyph ?? this;
        }

        /// <summary>
        /// Отрисовывает глиф и его элементы рекурсивно
        /// </summary>
        public virtual void Draw()
        {
            if (Config == null)
                throw new ArgumentNullException();

            if (Figure != null)
                Figure.Draw(Config.Brush, Config.Pen);

            if (Childs != null)
                for (var index = Childs.Count - 1; index >= 0; index--)
                {
                    var child = Childs[index];
                    child.Draw();
                }
        }
    }

    abstract class SolidGlyph : Glyph
    {
        public void DrawBounds()
        {
            if (Figure == null)
                throw new ArgumentNullException();

            if (Figure is SolidFigure)
                (Figure as SolidFigure).DrawBounds(Config.Pen);
            else
                throw new ArgumentException("Solid glyph figure is not SolidFigure");
        }

        public override void Move(int dx, int dy)
        {
            if (ParentVectorRedactor.Mode != RedactorModes.Move)
                return;

            if (Figure == null)
                throw new NullReferenceException("Figure is null");

            if (!(Figure is SolidFigure))
                throw new ArgumentException("Figure must be solid");

            (Figure as SolidFigure).Move(dx, dy);
        }

        public virtual void ProcessSelection()
        {
            throw new NotImplementedException();
        }

        public override void ProcessMove(Point offset)
        {
            if (ParentVectorRedactor.RedactorState != RedactorStates.Dragging) return;

            Move(offset.X, offset.Y);
            UpdateData();
            Redactor.InvalidateInterfaceBox();
        }

        protected virtual void UpdateData()
        {
            throw new NotImplementedException();
        }
    }

    class WordGlyph : SolidGlyph
    {
        public Word Word { get; private set; }

        private List<Figure> Figures { get; set; }

        public override void Move(int dx, int dy)
        {
            base.Move(dx, dy);
            foreach (var figure in Figures)
            {
                //TODO
            }

        }

        public WordGlyph(Word word)
        {
            Word = word;

            var path = new GraphicsPath();

            if (Word.Polygons != null && Word.Polygons.Count > 0)
                foreach (var polygon in Word.Polygons)
                    path.AddPath(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints()), false);
            Figures = new List<Figure>();

            if (Word.Polygons != null && Word.Polygons.Count > 0)
            {
                foreach (var polygon in Word.Polygons)
                    Figures.Add(new SolidFigure(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints())));

                foreach (var figure in Figures)
                    Figure.Path.AddPath(figure.Path, false);
            }


            Figure = new SolidFigure(path);
            Childs = new List<Glyph>();
        }

        public override void ProcessSelection()
        {
            if (Word == null)
                throw new NullReferenceException("Word can't be null");

            Redactor.LoadWord(Word);
        }

        protected override bool Intersects(Point point)
        {
            return Figure.Intersects(point);
        }

        protected override void UpdateData()
        {
            Word.Polygons = new List<Polygon>();
            var solidFigure = Figure as SolidFigure;
            if (solidFigure == null)
                throw new ArgumentNullException();

            foreach (var figure in Figures)
            {
                var points = figure.Path.PathPoints;
                var polygon = new Polygon();
                
                foreach (var point in points)
                    polygon.AddPoint(new DataBase.Domain.Point {X = point.X, Y = point.Y});

                Word.AddPolygon(polygon);
            }
        }
    }

    class LineGlyph : SolidGlyph
    {
        private Line Line { get; set; }

        public WordGlyph InsertNewWordGlyph (Word word)
        {
            var wordGlyph = new WordGlyph(word) {Config = ParentVectorRedactor.WordConfig};
            InsertChild(wordGlyph);
            return wordGlyph;
        } 

        public LineGlyph(Line line)
        {
            Line = line;

            var path = new GraphicsPath();

            if (line.Polygon != null && (line.Polygon.Points != null && line.Polygon.Points.Count > 0))
                path = Functions.GeneratePathFromPoints(Line.Polygon.ConvertToDrawingPoints());

            Figure = new SolidFigure(path);
            Childs = new List<Glyph>();
        }

        protected override bool Intersects(Point point)
        {
            return Figure.Intersects(point);
        }

        public void InsertChild(WordGlyph glyph)
        {
            Childs.Add(glyph);
            Line.AddWord(glyph.Word);
            ParentVectorRedactor.Layout.AddFigure(glyph.Figure);
        }

        public override void  ProcessSelection()
        {
            if (Line == null)
                throw new NullReferenceException("Line can't be null");

            Redactor.LoadLine(Line);
        }

        protected override void UpdateData()
        {
            Line.Polygon = new Polygon();
            var solidFigure = Figure as SolidFigure;
            if (solidFigure == null)
                throw new ArgumentNullException();

            var points = solidFigure.Path.PathPoints;

            foreach (var pathPoint in points)
                Line.Polygon.AddPoint(new DataBase.Domain.Point {X = pathPoint.X, Y = pathPoint.Y});
        }
    }

    class CanvasGlyph : Glyph
    {
        public CanvasGlyph(GlyphConfig config)
        {
            Config = config;
            Childs = new List<Glyph>();
        }

        public override void ProcessMove(Point location)
        {
            return; 
        }

        public LineGlyph InsertNewLineGlyph(Line line)
        {
            var lineGlyph = new LineGlyph(line) { Config = ParentVectorRedactor.LineConfig };
            InsertChild(lineGlyph);
            return lineGlyph;
        }

        protected override bool Intersects(Point point)
        {
            return true;
        }

        public override void Move(int dx, int dy)
        {
            throw new Exception("Can't move canvas");
        }
    }

    class PathGlyph : Glyph
    {
        public PathGlyph(GlyphConfig config)
        {
            Config = config;
            Figure = new UnclosedPathFigure();
        }

        protected override bool Intersects(Point point)
        {
            return false;
        }

        public override void Move(int dx, int dy)
        {
            return;
        }

        public override void Draw()
        {
            Figure.Draw(Config.Brush, Config.Pen);
        }

        public override void ProcessMove(Point offset)
        {
            var figure = Figure as UnclosedPathFigure;

            if (figure == null)
                throw new ArgumentNullException();

            var oldLocation = ParentVectorRedactor.LastCursorLocation;
            var newLocation = new Point {X = oldLocation.X + offset.X, Y = oldLocation.Y + offset.Y};

            if (figure.IsNearStart(newLocation))
                newLocation = figure.FirstPoint;

            figure.ReplaceLastPoint(newLocation);
            figure.UpdatePath();

            Invalidate();
        }

        /// <summary>
        /// Завершает добавления новой строки
        /// </summary>
        private void CompleteCreatingLine()
        {
            var figure = Figure as UnclosedPathFigure;

            if (figure == null)
                throw new ArgumentNullException();

            var path = figure.GeneratePath();

            var line = new Line();
            line.Polygon.LoadPointList(figure.Points);
            Redactor.Page.AddLine(line);

            var glyph = new LineGlyph(line) { Figure = new SolidFigure(path), Config = ParentVectorRedactor.LineConfig};

            Parent.InsertChild(glyph);

            ParentVectorRedactor.ActiveGlyph = glyph;
        }

        /// <summary>
        /// Завершает добавление нового слова
        /// </summary>
        private void CompleteCreatingWord()
        {
            var figure = Figure as UnclosedPathFigure;

            if (figure == null)
                throw new ArgumentNullException();

            var path = figure.GeneratePath();

            var word = new Word();
            if (ParentVectorRedactor.Mode == RedactorModes.AddPolygon)
                word = Redactor.Word;

            var polygon = new Polygon();
            polygon.LoadPointList(figure.Points);
            word.AddPolygon(polygon);

            if (ParentVectorRedactor.Mode == RedactorModes.AddPolygon)
                Redactor.Line.AddWord(word);

            var wordGlyph = new WordGlyph(word) { Figure = new SolidFigure(path), Config = ParentVectorRedactor.WordConfig};
            
            Parent.InsertChild(wordGlyph);

            ParentVectorRedactor.ActiveGlyph = wordGlyph;
        }

        private void CompleteCreating()
        {
            switch (ParentVectorRedactor.RedactorState)
            {
                case RedactorStates.Dragging:
                    throw new ArgumentOutOfRangeException();

                case RedactorStates.CreatingLine:
                    CompleteCreatingLine();
                    break;

                case RedactorStates.CreatingWord:
                    CompleteCreatingWord();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            ParentVectorRedactor.RedactorState = RedactorStates.Default;
            var solidGlyph = ParentVectorRedactor.ActiveGlyph as SolidGlyph;

            if (solidGlyph == null)
                throw new ArgumentNullException();

            solidGlyph.ProcessSelection();
            
            Parent.RemoveChild(this);

            Invalidate();
        }

        public void ProcessLeftClick(Point location)
        {
            var unclosedPathFigure = Figure as UnclosedPathFigure;

            if (unclosedPathFigure == null)
                throw new ArgumentNullException();

            if (unclosedPathFigure.IsNearStart(location))
            {
                CompleteCreating();
                return;
            }

            unclosedPathFigure.AddPoint(location);

            Invalidate();
        }

        public void StartCreatingIn(Point point)
        {
            Figure = new UnclosedPathFigure { Layout = ParentVectorRedactor.Layout };
            ParentVectorRedactor.Layout.AddFigure(Figure);
            (Figure as UnclosedPathFigure).AddPoint(point);
            (Figure as UnclosedPathFigure).AddPoint(point);
            (Figure as UnclosedPathFigure).UpdatePath();

            Invalidate();
        }
    }

    public interface IVectorRedactorInterface
    {
        void ProcessMove(MouseEventArgs e);
        void ProcessClick(MouseEventArgs e);
        void Draw();
        bool ProcessModeChangeToMove();
        bool ProcessModeChangeToCreate();
        bool ProcessModeChangeToAdd();
        void ProcessMouseDown(MouseEventArgs e);
        void LoadPage(Page page);
    }
}
