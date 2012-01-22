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
        protected GlyphConfig Config;

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
            }

            //Если точка не принадлежит дочерним элементам
            //то она принадлежит данному
            return highterLayerGlyph == null ? this : null;
        }

        /// <summary>
        /// Отрисовывает глиф и его элементы рекурсивно
        /// </summary>
        public virtual void Draw()
        {
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
                (Figure as SolidFigure).DrawBounds();
            else
                throw new ArgumentException("Solid glyph figure is not SolidFigure");
        }

        public override void Move(int dx, int dy)
        {
            if (Figure == null)
                throw new NullReferenceException("Figure is null");

            if (Figure is SolidFigure)
                (Figure as SolidFigure).Move(dx, dy);
            else
                throw new ArgumentException("Figure must be solid");
        }

        public virtual void ProcessSelection()
        {
            throw new NotImplementedException();
        }
    }

    class WordGlyph : SolidGlyph
    {
        public Word Word { get; private set; }

        public WordGlyph(Word word)
        {
            Word = word;

            var path = new GraphicsPath();

            if (Word.Polygons != null && Word.Polygons.Count > 0)
                foreach (var polygon in Word.Polygons)
                    path.AddPath(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints()), false);

            Figure = new SolidFigure(path);
            Childs = new List<Glyph>();
        }

        public override void ProcessMove(Point offset)
        {
            Move(offset.X, offset.Y);
            Redactor.InvalidateInterfaceBox();
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
    }

    class LineGlyph : SolidGlyph
    {
        private Line Line { get; set; }

        public LineGlyph(Line line)
        {
            Line = line;

            var path = new GraphicsPath();

            if (line.Polygon.Points != null && line.Polygon.Points.Count > 0)
                path = Functions.GeneratePathFromPoints(Line.Polygon.ConvertToDrawingPoints());

            Figure = new SolidFigure(path);
            Childs = new List<Glyph>();
        }

        public override void ProcessMove(Point offset)
        {
            Move(offset.X, offset.Y);
            Redactor.InvalidateInterfaceBox();
        }

        protected override bool Intersects(Point point)
        {
            return Figure.Intersects(point);
        }

        public void InsertChild(WordGlyph glyph)
        {
            Childs.Add(glyph);
            Line.AddWord(glyph.Word);
            ParentVectorRedactor.Glyphs.Add(glyph);
            ParentVectorRedactor.Layout.AddFigure(glyph.Figure);
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

            var oldLocation = figure.Points[figure.Points.Count - 1];
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


            var glyph = new LineGlyph(line) { Figure = new SolidFigure(path) };

            Parent.InsertChild(glyph);
            
            ParentVectorRedactor.ActiveGlyph = glyph;

            Invalidate();
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

            var wordGlyph = new WordGlyph(word) { Figure = new SolidFigure(path) };
            
            Parent.InsertChild(wordGlyph);

            ParentVectorRedactor.ActiveGlyph = wordGlyph;


            Invalidate();
        }

        private void CompleteCreating()
        {
            switch (ParentVectorRedactor.RedactorState)
            {
                case RedactorStates.Dragging:
                    break;

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
            Parent = ParentVectorRedactor.ActiveGlyph;
            ParentVectorRedactor.ActiveGlyph.InsertChild(this);
            ParentVectorRedactor.ActiveGlyph = this;
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
    }
}
