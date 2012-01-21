using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Windows.Forms;
using Owl.DataBase.Domain;
using Owl.Repositories;
using Point = System.Drawing.Point;

namespace Owl.Domain
{
    class VectorRedactor
    {
        public enum RedactorStates
        {
            Default,
            Dragging,
            CreatingLine,
            CreatingWord
        }

        public enum RedactorModes
        {
            Create,
            Move,
            AddPolygon
        }

        private Point LastCursorLocation { get; set; }
        protected RedactorStates RedactorState { get; set; }
        public RedactorModes Mode { get; private set; }
        public Layout Layout;
        private Graphics Canvas { get; set; }
        private Redactor _redactor;
        private List<Marker> ActiveMarkers { get; set; }
        private static Glyph ActiveGlyph { get; set; }
        private CanvasGlyph MainGlyph { get; set; }
        public GlyphConfig WordConfig { get; private set; }
        public GlyphConfig LineConfig { get; private set; }

        private static List<Glyph> Glyphs { get; set; } 

        public class VectorRedactorConfig
        {
            public Brush LineBrush { get; set; }
            public Pen LinePen { get; set; }
            public Brush WordBrush { get; set; }
            public Pen WordPen { get; set; }
        }

        public class GlyphConfig
        {
            public readonly Brush Brush;
            public readonly Pen Pen;
            public readonly Redactor Redactor;
            public VectorRedactor VectorRedactor;

            public GlyphConfig(Brush brush, Pen pen, Redactor redactor, VectorRedactor vectorRedactor)
            {
                Brush = brush;
                Pen = pen;
                Redactor = redactor;
                VectorRedactor = vectorRedactor;

            }
        }

        public VectorRedactor(Graphics canvas, Redactor redactor, VectorRedactorConfig config)
        {
            Canvas = canvas;
            _redactor = redactor;
            canvas.SmoothingMode = SmoothingMode.HighQuality;

            Layout = new Layout(canvas);
            
            WordConfig = new GlyphConfig(config.WordBrush, config.WordPen, redactor, this);
            LineConfig = new GlyphConfig(config.LineBrush, config.LinePen, redactor, this);

            RedactorState = RedactorStates.Default;
            
            MainGlyph = new CanvasGlyph(WordConfig) {Redactor = _redactor, VectorRedactor = this};
            MainGlyph.MainGlyph = MainGlyph;
            
            ActiveGlyph = MainGlyph;
        }

        abstract class Glyph
        {
            public Figure Figure;
            public Redactor Redactor;
            public VectorRedactor VectorRedactor;
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
                glyph.VectorRedactor = VectorRedactor;
                Childs.Add(glyph);
            }

            /// <summary>
            /// Загрузка конфигурации глифа
            /// </summary>
            /// <param name="config">Конфигурация</param>
            protected void LoadConfig (GlyphConfig config)
            {
                Redactor = config.Redactor;
                VectorRedactor = config.VectorRedactor;
            }

            public void RemoveChild(Glyph glyph)
            {
                foreach (var child in Childs.Where(child => child == glyph))
                {
                    child.VectorRedactor.Layout.RemoveFigure(child.Figure);
                    Childs.Remove(child);
                    return;
                }
                throw new Exception("Child not exist");
            }

            public abstract void Move(int dx, int dy);
            public Glyph Parent { get; protected set; }
            protected List<Glyph> Childs { get; set; }

            /// <summary>
            /// Находит глиф в точке, рекурсивно проходя по дочерним элементам.
            /// </summary>
            /// <param name="location"></param>
            /// <returns></returns>
            public Glyph FindGlyphIn (Point location)
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
                    Figure.Draw(Config.Brush,Config.Pen);

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
            public void DrawBounds ()
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

            public override void ProcessSelection ()
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

            public LineGlyph (Line line)
            {
                Line = line;

                var path = new GraphicsPath();

                if (line.Polygon.Points!=null&&line.Polygon.Points.Count>0)
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
                Glyphs.Add(glyph);
                VectorRedactor.Layout.AddFigure(glyph.Figure);
            }
        }

        class CanvasGlyph : Glyph
        {
            public CanvasGlyph (GlyphConfig config)
            {
                Config = config;
                Childs = new List<Glyph>();
            }

            public override void ProcessMove(Point location)
            {
                throw new Exception("Cant move canvas");
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
                Figure.Draw(Config.Brush,Config.Pen);
            }

            public override void ProcessMove (Point offset)
            {
                var figure = Figure as UnclosedPathFigure;
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
            private void CompleteCreatingLine ()
            {
                var figure = Figure as UnclosedPathFigure;
                var path = figure.GeneratePath();

                var line = new Line();
                line.Polygon.LoadPointList(figure.Points);
                Redactor.Page.AddLine(line);

                var newfigure = new SolidFigure(path);
                VectorRedactor.Layout.AddFigure(figure);

                var glyph = new LineGlyph(line) { Figure = newfigure };
                Parent.InsertChild(glyph);

                Redactor.LoadLine(line);
                ActiveGlyph = glyph;

                Parent.RemoveChild(this);

                Invalidate();
            }

            /// <summary>
            /// Завершает добавление нового слова
            /// </summary>
            private void CompleteCreatingWord ()
            {
                var figure = Figure as UnclosedPathFigure;
                var path = figure.GeneratePath();

                var word = new Word();
                if (VectorRedactor.Mode == RedactorModes.AddPolygon)
                    word = Redactor.Word;

                var polygon = new Polygon();
                polygon.LoadPointList(figure.Points);
                word.AddPolygon(polygon);

                if (VectorRedactor.Mode == RedactorModes.AddPolygon)
                    Redactor.Line.AddWord(word);

                var newfigure = new SolidFigure(path);
                VectorRedactor.Layout.AddFigure(figure);

                var wordGlyph = new WordGlyph(word) { Figure = newfigure };
                Parent.InsertChild(wordGlyph);

                Redactor.LoadWord(word);

                ActiveGlyph = wordGlyph;

                Parent.RemoveChild(this);

                Invalidate();
            } 

            private void CompleteCreating ()
            {
                switch (VectorRedactor.RedactorState)
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
                VectorRedactor.RedactorState = RedactorStates.Default; 
                
            }

            public void ProcessLeftClick (Point location)
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

                Redactor.InvalidateInterfaceBox();
            }

            public void StartCreatingIn (Point point)
            {
                Parent = ActiveGlyph;
                ActiveGlyph.InsertChild(this);
                ActiveGlyph = this;
                Figure = new UnclosedPathFigure{Layout = VectorRedactor.Layout};
                VectorRedactor.Layout.AddFigure(Figure);
                (Figure as UnclosedPathFigure).AddPoint(point);
                (Figure as UnclosedPathFigure).AddPoint(point);
                (Figure as UnclosedPathFigure).UpdatePath();
               
                Redactor.InvalidateInterfaceBox();
            }
        }

        /// <summary>
        /// Возвращает глиф наиболее высокого уровня, к которому принадлежит точка.
        /// </summary>
        /// <param name="location">Точка для поиска</param>
        /// <returns></returns>
        private Glyph FindGlyphIn (Point location)
        {
            return MainGlyph.FindGlyphIn(location);
        }

        private void StartCreatingNewWordIn (Point location)
        {
            var pathGlyph = new PathGlyph(WordConfig);
            MainGlyph.InsertChild(pathGlyph);

            pathGlyph.StartCreatingIn(location);
            RedactorState = RedactorStates.CreatingWord;
        }

        private void StartCreatingNewLineIn (Point location)
        {
            var pathGlyph = new PathGlyph(LineConfig);
            MainGlyph.InsertChild(pathGlyph);
            pathGlyph.StartCreatingIn(location);

            RedactorState = RedactorStates.CreatingLine;
        }
        
        private void TryStartDraggingGlyphIn (Point location)
        {
            var underCursorGlyph = FindGlyphIn(location);
            if (underCursorGlyph is SolidGlyph && Mode == RedactorModes.Move)
            {
                RedactorState = RedactorStates.Dragging;
                ActiveGlyph = underCursorGlyph;
            }
        }

        public void ProcessMouseUp()
        {
            if (RedactorState != RedactorStates.Dragging) return;

            RedactorState = RedactorStates.Default;
        }

        /// <summary>
        /// Обрабатывает нажание левой кнопки мыши
        /// </summary>
        /// <param name="e"></param>
        public void ProcessMouseDown(MouseEventArgs e)
        {
            var location = e.Location;

            if (RedactorState == RedactorStates.CreatingLine || RedactorState == RedactorStates.CreatingWord)
                return;

            if (e.Button == MouseButtons.Left && Mode == RedactorModes.Move &&
                RedactorState == RedactorStates.Default)
                TryStartDraggingGlyphIn(location);
        }

        /// <summary>
        /// Начинает создание нового глифа в зависимости от активного.
        /// </summary>
        /// <param name="location">Первая точка нового глифа</param>
        private void StartCreatingNewGlyphIn (Point location)
        {
            _redactor.Cursor = Cursors.Default;
            var parent = ActiveGlyph;

            if (parent is CanvasGlyph)
                StartCreatingNewLineIn(location);

            if (parent is LineGlyph)
                StartCreatingNewWordIn(location);
        }

        /// <summary>
        /// Обрабатывает правый клик
        /// </summary>
        /// <param name="location">Т</param>
        private void ProcessRightClick(Point location)
        {
            throw new NotImplementedException();
        }

        private void StartMoving()
        {
            RedactorState = RedactorStates.Dragging;
        }


        public bool ProcessModeChangeToMove ()
        {
            if (ActiveGlyph is PathGlyph)
                return false;

            Mode = RedactorModes.Move;
            return true;
        }

        public bool ProcessModeChangeToCreate ()
        {
            if (ActiveGlyph is PathGlyph)
                return false;

            Mode = RedactorModes.Create;
            return true;
        }

        public bool ProcessModeChangeToAdd ()
        {
            if (ActiveGlyph is PathGlyph)
                return false;
            return true;
        }

        /// <summary>
        /// Обрабатывает левый клик
        /// </summary>
        /// <param name="location"></param>
        private void ProcessLeftClick(Point location)
        {
            if (RedactorState == RedactorStates.Default)
            {
                var newActiveGlyph = FindGlyphIn(location);
                if (newActiveGlyph == ActiveGlyph)
                {
                    switch (Mode)
                    {
                        case RedactorModes.Create:
                            StartCreatingNewGlyphIn(location);
                            return;
                        case RedactorModes.Move:
                            StartMoving();
                            return;
                        case RedactorModes.AddPolygon:
                            throw new NotImplementedException();
                    }
                }

                else
                {
                    ActiveGlyph = newActiveGlyph;
                    if (ActiveGlyph is SolidGlyph)
                    {
                        (ActiveGlyph as SolidGlyph).ProcessSelection();
                    }
                }
            }

            if ((RedactorState == RedactorStates.CreatingLine || RedactorState == RedactorStates.CreatingWord) && ActiveGlyph is PathGlyph)
                (ActiveGlyph as PathGlyph).ProcessLeftClick(location);

        }

        public void ProcessMove(MouseEventArgs e)
        {
            var cursor = Cursors.Default;
            var location = e.Location;

            if (RedactorState == RedactorStates.Default && !(FindGlyphIn(location) is PathGlyph))
                cursor = Cursors.Hand;

            if (ActiveGlyph is PathGlyph)
                cursor = Cursors.Cross;

            
                ActiveGlyph.ProcessMove(new Point(location.X - LastCursorLocation.X, location.Y - LastCursorLocation.Y));
  

            _redactor.Cursor = cursor;
            LastCursorLocation = location;
        }
        /// <summary>
        /// Обработка кликов мыши
        /// </summary>
        /// <param name="e"></param>
        public void ProcessClick(MouseEventArgs e)
        {
            var location = e.Location;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    ProcessLeftClick(location);
                    break;
                case MouseButtons.Right:
                    ProcessRightClick(location);
                    break;
            }
        }

        /// <summary>
        /// Отрисовка интерфейса
        /// </summary>
        public void Draw()
        {
            MainGlyph.Draw();

            if (ActiveGlyph is SolidGlyph)
                (ActiveGlyph as SolidGlyph).DrawBounds();
            //_redactor.Invalidate();
        }
    }
}
