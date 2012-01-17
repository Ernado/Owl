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
        private Layout _layout;
        private Graphics Canvas { get; set; }
        private Redactor _redactor;
        private List<Marker> ActiveMarkers { get; set; }
        private static Glyph ActiveGlyph { get; set; }
        private CanvasGlyph MainGlyph { get; set; }

        private static List<Glyph> Glyphs { get; set; } 

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

        public VectorRedactor(Graphics canvas, Redactor redactor)
        {
            Canvas = canvas;
            _redactor = redactor;
            _layout = new Layout(canvas);
            canvas.SmoothingMode = SmoothingMode.HighQuality;
        }

        abstract class Glyph
        {
            protected readonly Figure Figure;
            private Brush _brush;
            private Pen _pen;
            protected Redactor Redactor;
            protected VectorRedactor VectorRedactor;
            protected Glyph MainGlyph;
            protected GlyphConfig Config;

            protected Glyph()
            {
                throw new NotImplementedException();
            }

            protected abstract bool Intersects(Point point);

            public virtual void InsertChild(Glyph glyph)
            {
                glyph.Parent = this;
                Childs.Add(glyph);
            }

            /// <summary>
            /// Загрузка конфигурации глифа
            /// </summary>
            /// <param name="config">Конфигурация</param>
            protected void LoadConfig (GlyphConfig config)
            {
                _brush = config.Brush;
                _pen = config.Pen;
                Redactor = config.Redactor;
                VectorRedactor = config.VectorRedactor;
            }

            public void RemoveChild(Glyph glyph)
            {
                foreach (var child in Childs.Where(child => child == glyph))
                {
                    Childs.Remove(child);
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
            /// Отрисовывает глив и его элементы рекурсивно
            /// </summary>
            public void Draw()
            {
                if (Figure != null) 
                    Figure.Draw(_brush,_pen);

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

            public WordGlyph(Word word, GlyphConfig config)
            {
                Config = config;
                Word = word;
                var path = new GraphicsPath();
                foreach (var polygon in Word.Polygons)
                {
                    path.AddPath(Functions.GeneratePathFromPoints(polygon.ConvertToDrawingPoints()), false);
                }
                Figure = new SolidFigure(path);
            }

            public override void Move(int dx, int dy)
            {
                Figure.Move(dx,dy);
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

            public LineGlyph (Line line, GlyphConfig config)
            {
                Config = config;
                Line = line;
                var path = Functions.GeneratePathFromPoints(Line.Polygon.ConvertToDrawingPoints());
                Figure = new SolidFigure(path);
            }

            protected override bool Intersects(Point point)
            {
                return Figure.Intersects(point);
            }

            public override void Move(int dx, int dy)
            {
                throw new NotImplementedException();
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
            }

            protected override bool Intersects(Point point)
            {
                return true;
            }
            
            public override void Move(int dx, int dy)
            {
                throw new NotImplementedException();
            }
        }

        class PathGlyph : Glyph
        {
            public UnclosedPathFigure Figure;

            protected override bool Intersects(Point point)
            {
                throw new NotImplementedException();
            }

            public override void Move(int dx, int dy)
            {
                throw new NotImplementedException();
            }

            public void ProcessMove (Point location)
            {
                if (Figure.IsNearStart(location))
                    location = Figure.FirstPoint;

                Figure.ReplaceLastPoint(location);
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

                var glyph = new LineGlyph(line, Config) { Figure = new SolidFigure(path) };
                MainGlyph.InsertChild(glyph);

                Redactor.LoadLine(line);
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

                var wordGlyph = new WordGlyph(word, Config) { Figure = new SolidFigure(path) };
                MainGlyph.InsertChild(wordGlyph);

                Redactor.LoadWord(word);
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
            }

            public void StartCreatingIn (Point point)
            {
                Parent = ActiveGlyph;
                ActiveGlyph.InsertChild(this);
                ActiveGlyph = this;
                Figure = new UnclosedPathFigure();
                Figure.AddPoint(point);
                Figure.AddPoint(point);
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
            (new PathGlyph()).StartCreatingIn(location);
            RedactorState = RedactorStates.CreatingWord;
        }

        private void StartCreatingNewLineIn (Point location)
        {
            (new PathGlyph()).StartCreatingIn(location);
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

        /// <summary>
        /// Обрабатывает нажание левой кнопки мыши
        /// </summary>
        /// <param name="e"></param>
        public void ProcessMouseDown(MouseEventArgs e)
        {
            var location = e.Location;

            if (RedactorState == RedactorStates.CreatingLine || RedactorState == RedactorStates.CreatingWord)
                return;

            if (e.Button == MouseButtons.Left && _redactorMode == RedactorModes.Move)
            {
                TryStartDraggingGlyphIn(location);
            }
        }

        private void StartCreatingNewGlyphIn (Point location)
        {
            var parent = ActiveGlyph;

            if (parent is CanvasGlyph)
                StartCreatingNewLineIn(location);

            if (parent is LineGlyph)
                StartCreatingNewWordIn(location);
        }

        /// <summary>
        /// Обработка клик левой кнопки мыши
        /// </summary>
        /// <param name="e"></param>
        public void ProcessClick(MouseEventArgs e)
        {
            var location = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                //Если редактор в режиме добавления - не меняем активный глиф
                if (RedactorState == RedactorStates.CreatingLine || RedactorState == RedactorStates.CreatingWord)
                {
                    throw new NotImplementedException();
                }

                ActiveGlyph = FindGlyphIn(location);
                if (_redactorMode == RedactorModes.Create)    
                    StartCreatingNewGlyphIn(location);
            }
            else
            {
                throw new NotImplementedException();
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
        }
    }
}
