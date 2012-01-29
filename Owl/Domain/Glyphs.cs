using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Owl.DataBase.Domain;
using Owl.Repositories;
using Point = System.Drawing.Point;

namespace Owl.Domain
{
    internal enum RedactorStates
    {
        Default,
        Dragging,
        CreatingLine,
        CreatingWord
    }

    internal enum RedactorModes
    {
        Create,
        Move,
        AddPolygon
    }

    internal class GlyphConfig
    {
        public readonly Brush Brush;
        public readonly Pen Pen;

        public GlyphConfig(Brush brush, Pen pen)
        {
            Brush = brush;
            Pen = pen;
        }
    }

    internal abstract class Glyph
    {
        public GlyphConfig Config;
        public Figure Figure;
        public Glyph MainGlyph;
        public VectorRedactorRepository ParentVectorRedactor;
        public Redactor Redactor;
        public Glyph Parent { get; set; }
        public List<Glyph> Childs { get; protected set; }

        public abstract void ProcessMove(Point location);

        protected abstract bool Intersects(Point point);

        public void Remove()
        {
            if (Figure!=null)
                ParentVectorRedactor.Layout.RemoveFigure(Figure);

            if (Parent != null)
                Parent.Childs.Remove(this);
        }

        protected void Invalidate()
        {
            if (ParentVectorRedactor == null)
                throw new ArgumentNullException();

            ParentVectorRedactor.Invalidate();
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
            foreach (Glyph child in Childs.Where(child => child == glyph))
            {
                child.ParentVectorRedactor.Layout.RemoveFigure(child.Figure);
                Childs.Remove(child);
                return;
            }
            throw new Exception("Child not exist");
        }

        public abstract void Move(int dx, int dy);

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
            for (int index = Childs.Count - 1; index >= 0; index--)
            {
                Glyph child = Childs[index];
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
                for (int index = Childs.Count - 1; index >= 0; index--)
                {
                    Glyph child = Childs[index];
                    child.Draw();
                }
        }

        public abstract void ProcessSelection();
    }

    internal abstract class SolidGlyph : Glyph
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

        public override void ProcessSelection()
        {
            ParentVectorRedactor.Invalidate();
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

    internal class WordGlyph : SolidGlyph
    {
        public WordGlyph(Word word)
        {
            Word = word;

            var path = new GraphicsPath();
            Figure = new SolidFigure(path);

            if (Word.Polygons != null && Word.Polygons.Count > 0)
                foreach (Polygon polygon in Word.Polygons)
                    path.AddPath(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints()), false);

            Figures = new List<Figure>();

            if (Word.Polygons != null && Word.Polygons.Count > 0)
            {
                foreach (Polygon polygon in Word.Polygons)
                    Figures.Add(new SolidFigure(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints())));
            }

            Figure.Path.CloseAllFigures(); ;
            Childs = new List<Glyph>();
        }

        public Word Word { get; private set; }

        private List<Figure> Figures { get; set; }

        public override void Move(int dx, int dy)
        {
            base.Move(dx, dy);
            foreach (Figure figure in Figures)
            {
                var solidFigure = figure as SolidFigure;

                if (solidFigure == null) throw new NullReferenceException("Figure can't be null");

                solidFigure.Move(dx,dy);
            }

            //TODO: check
        }

        public override void ProcessSelection()
        {
            if (Word == null)
                throw new NullReferenceException("Word can't be null");

            base.ProcessSelection();
            Parent.ProcessSelection();
            Redactor.LoadElement(Word);
        }

        protected override bool Intersects(Point point)
        {
            return Figure.Intersects(point);
        }

        protected override void UpdateData()
        {
            Word.Polygons.Clear();
            var solidFigure = Figure as SolidFigure;
            if (solidFigure == null)
                throw new ArgumentNullException();

            foreach (Figure figure in Figures)
            {
                PointF[] points = figure.Path.PathPoints;
                var polygon = new Polygon();

                foreach (PointF point in points)
                    polygon.AddPoint(new DataBase.Domain.Point {X = point.X, Y = point.Y});

                Word.AddPolygon(polygon);
            }
        }
    }

    internal class LineGlyph : SolidGlyph
    {
        public LineGlyph(Line line)
        {
            Line = line;

            var path = new GraphicsPath();

            if (line.Polygon != null && (line.Polygon.Points != null && line.Polygon.Points.Count > 0))
                path = Functions.GeneratePathFromPoints(Line.Polygon.ConvertToDrawingPoints());

            Figure = new SolidFigure(path);
            Childs = new List<Glyph>();
        }

        public Line Line { get; private set; }

        public WordGlyph InsertNewWordGlyph(Word word)
        {
            var wordGlyph = new WordGlyph(word) {Config = ParentVectorRedactor.WordConfig};
            InsertChild(wordGlyph);
            return wordGlyph;
        }

        protected override bool Intersects(Point point)
        {
            return Figure.Intersects(point);
        }

        public void InsertChild(WordGlyph glyph)
        {
            glyph.Parent = this;
            glyph.MainGlyph = MainGlyph;
            glyph.Redactor = Redactor;
            glyph.ParentVectorRedactor = ParentVectorRedactor;
            Childs.Add(glyph);
            if (!Line.Words.Contains(glyph.Word))
                Line.AddWord(glyph.Word);
            ParentVectorRedactor.Layout.AddFigure(glyph.Figure);
        }

        public override void ProcessSelection()
        {
            if (Line == null)
                throw new NullReferenceException("Line can't be null");

            base.ProcessSelection();

            if (Redactor.Line != Line)
                Redactor.LoadElement(Line);
        }

        protected override void UpdateData()
        {
            Line.Polygon = new Polygon();
            var solidFigure = Figure as SolidFigure;
            if (solidFigure == null)
                throw new ArgumentNullException();

            var points = solidFigure.Path.PathPoints;

            foreach (PointF pathPoint in points)
                Line.Polygon.AddPoint(new DataBase.Domain.Point {X = pathPoint.X, Y = pathPoint.Y});
        }
    }

    internal class CanvasGlyph : Glyph
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
            var lineGlyph = new LineGlyph(line)
                                {Config = ParentVectorRedactor.LineConfig, Parent = this, MainGlyph = MainGlyph};
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

        public override void ProcessSelection()
        {
            throw new NotImplementedException();
        }
    }

    internal class PathGlyph : Glyph
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

        public override void ProcessSelection()
        {
            throw new NotImplementedException();
        }

        public override void ProcessMove(Point offset)
        {
            var figure = Figure as UnclosedPathFigure;

            if (figure == null)
                throw new ArgumentNullException();

            Point oldLocation = ParentVectorRedactor.LastCursorLocation;
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

            GraphicsPath path = figure.GeneratePath();

            var line = new Line();
            line.Polygon.LoadPointList(figure.Points);

            var number = 1;
            if (Redactor.Page.Lines.Count > 0)
                number = (from fromDbline in Redactor.Page.Lines orderby fromDbline.Number descending select fromDbline.Number).ToList()[0] + 1;
            line.Number = number;

            Redactor.Page.AddLine(line);

            var glyph = new LineGlyph(line) {Figure = new SolidFigure(path), Config = ParentVectorRedactor.LineConfig};

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

            GraphicsPath path = figure.GeneratePath();

            var word = new Word();
            if (ParentVectorRedactor.Mode == RedactorModes.AddPolygon)
                word = Redactor.Word;

            var polygon = new Polygon();
            polygon.LoadPointList(figure.Points);
            word.AddPolygon(polygon);


            var number = 1;
            if (Redactor.Line.Words.Count > 0)
                number = (from dbWord in Redactor.Line.Words orderby dbWord.Number descending select dbWord.Number).ToList()[0] + 1;
            word.Number = number;

            if (ParentVectorRedactor.Mode == RedactorModes.AddPolygon)
                Redactor.Line.AddWord(word);

            var wordGlyph = new WordGlyph(word)
                                {Figure = new SolidFigure(path), Config = ParentVectorRedactor.WordConfig};

            if (!Redactor.Line.Words.Contains(word))
            {
                Redactor.Line.AddWord(word);
            }

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

            Parent.RemoveChild(this);

            solidGlyph.ProcessSelection();

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
            Figure = new UnclosedPathFigure {Layout = ParentVectorRedactor.Layout};
            ParentVectorRedactor.Layout.AddFigure(Figure);
            (Figure as UnclosedPathFigure).AddPoint(point);
            (Figure as UnclosedPathFigure).AddPoint(point);
            (Figure as UnclosedPathFigure).UpdatePath();

            Invalidate();
        }
    }

    public interface IVectorRedactorInterface
    {
        void ProcessMouseMove(MouseEventArgs e);
        void ProcessClick(MouseEventArgs e);
        void Draw();
        bool ProcessModeChangeToMove();
        bool ProcessModeChangeToCreate();
        bool ProcessModeChangeToAdd();
        void ProcessActivation(Line line);
        void ProcessActivation(Word line);
        void ProcessRemove(Word line);
        void ProcessRemove(Line line);
        void ProcessMouseDown(MouseEventArgs e);
        void LoadPage(Page page);
    }
}