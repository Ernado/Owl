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

        private RedactorModes _redactorMode;
        private Point LastCursorLocation { get; set; }
        public RedactorStates RedactorState { get; private set; }
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
            WordConfig = new GlyphConfig(config.WordBrush,config.WordPen, redactor,this);
            LineConfig = new GlyphConfig(config.LineBrush, config.LinePen, redactor, this);

            RedactorState = RedactorStates.Default; 
            MainGlyph = new CanvasGlyph(WordConfig);
            MainGlyph.Redactor = _redactor;
            MainGlyph.VectorRedactor = this;
            MainGlyph.MainGlyph = MainGlyph;
            ActiveGlyph = MainGlyph;
        }

        abstract class Glyph
        {
            protected readonly Figure Figure;
            public Redactor Redactor;
            public VectorRedactor VectorRedactor;
            public Glyph MainGlyph;
            protected GlyphConfig Config;

            public abstract void ProcessMove(Point location);

            protected abstract bool Intersects(Point point);

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
            public new SolidFigure Figure;

            public void DrawBounds ()
            {
                Figure.DrawBounds();
            }
        }

        class WordGlyph : SolidGlyph
        {
            public Word Word { get; set; }

            public WordGlyph(Word word)
            {
                //Config = VectorRedactor.WordConfig;
                Word = word;
                var path = new GraphicsPath();
                foreach (var polygon in Word.Polygons)
                {
                    path.AddPath(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints()), false);
                }
                Figure = new SolidFigure(path);
                Childs = new List<Glyph>();
            }

            public override void Move(int dx, int dy)
            {
                Figure.Move(dx,dy);
            }

            public override void ProcessMove(Point offset)
            {
                Move(offset.X, offset.Y);
                Redactor.InvalidateInterfaceBox();
            }

            public void ProcessSelection ()
            {
                Redactor.LoadWord(Word);
            }

            protected override bool Intersects(Point point)
            {
                return Figure.Intersects(point);
            }
        }

        class LineGlyph : SolidGlyph
        {
            public Line Line;

            public LineGlyph (Line line)
            {
                //Config = VectorRedactor.LineConfig;
                Line = line;
                var path = Functions.GeneratePathFromPoints(Line.Polygon.ConvertToDrawingPoints());
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

            public override void Move(int dx, int dy)
            {
                Figure.Move(dx,dy);
            }

            public void InsertChild(WordGlyph glyph)
            {
                Childs.Add(glyph);
                Line.AddWord(glyph.Word);
                Glyphs.Add(glyph);
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


            public new UnclosedPathFigure Figure;

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
                var oldLocation = Figure.Points[Figure.Points.Count-1];
                var newLocation = new Point {X = oldLocation.X + offset.X, Y = oldLocation.Y + offset.Y};

                if (Figure.IsNearStart(newLocation))
                    newLocation = Figure.FirstPoint;

                Figure.ReplaceLastPoint(newLocation);
                Figure.UpdatePath();
                Redactor.InvalidateInterfaceBox();
                Redactor.InvalidateInterfaceBox();
            }

            /// <summary>
            /// Завершает добавления новой строки
            /// </summary>
            private void CompleteCreatingLine ()
            {
                var path = Figure.GeneratePath();

                var line = new Line();
                line.Polygon.LoadPointList(Figure.Points);
                Redactor.Page.AddLine(line);

                var figure = new SolidFigure(path);
                VectorRedactor.Layout.AddFigure(figure);

                var glyph = new LineGlyph(line) { Figure = figure};
                Parent.InsertChild(glyph);

                Redactor.LoadLine(line);
                ActiveGlyph = glyph;

                Parent.RemoveChild(this);

                Redactor.InvalidateInterfaceBox();
            }

            /// <summary>
            /// Завершает добавление нового слова
            /// </summary>
            private void CompleteCreatingWord ()
            {
                var path = Figure.GeneratePath();

                var word = new Word();
                if (VectorRedactor.Mode == RedactorModes.AddPolygon)
                    word = Redactor.Word;

                var polygon = new Polygon();
                polygon.LoadPointList(Figure.Points);
                word.AddPolygon(polygon);

                if (VectorRedactor.Mode == RedactorModes.AddPolygon)
                    Redactor.Line.AddWord(word);

                var figure = new SolidFigure(path);
                VectorRedactor.Layout.AddFigure(figure);

                var wordGlyph = new WordGlyph(word) { Figure = figure };
                Parent.InsertChild(wordGlyph);

                Redactor.LoadWord(word);

                ActiveGlyph = wordGlyph;

                Parent.RemoveChild(this);

                Redactor.InvalidateInterfaceBox();
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
                
            }

            public void ProcessLeftClick (Point location)
            {
                if (Figure.IsNearStart(location))
                {
                    CompleteCreating();
                    return; 
                }

                Figure.AddPoint(location);
                Figure.AddPoint(location);
                Redactor.InvalidateInterfaceBox();
            }

            public void StartCreatingIn (Point point)
            {
                Parent = ActiveGlyph;
                ActiveGlyph.InsertChild(this);
                ActiveGlyph = this;
                Figure = new UnclosedPathFigure{Layout = VectorRedactor.Layout};
                VectorRedactor.Layout.AddFigure(Figure);
                Figure.AddPoint(point);
                Figure.AddPoint(point);
                Figure.UpdatePath();
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
            if (underCursorGlyph is SolidGlyph)
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

            if (e.Button == MouseButtons.Left && _redactorMode == RedactorModes.Move && RedactorState == RedactorStates.Default)
            {
                TryStartDraggingGlyphIn(location);
            }
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
            
        }

        private void StartMoving()
        {
            RedactorState = RedactorStates.Dragging;
        }


        public bool ProcessModeChangeToMove ()
        {
            if (ActiveGlyph is PathGlyph)
                return false;

            _redactorMode = RedactorModes.Move;
            return true;
        }

        public bool ProcessModeChangeToCreate ()
        {
            if (ActiveGlyph is PathGlyph)
                return false;

            _redactorMode = RedactorModes.Create;
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
