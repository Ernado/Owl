using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Owl.DataBase.Domain;
using Owl.Domain;
using Point = System.Drawing.Point;

namespace Owl.Repositories
{
    class VectorRedactorRepository : IVectorRedactorInterface
    {
        public Point LastCursorLocation { get; set; }
        public RedactorStates RedactorState { get; set; }
        public RedactorModes Mode { get; private set; }
        public readonly Layout Layout;
        private Graphics Canvas { get; set; }
        private readonly Redactor _redactor;
        public Glyph ActiveGlyph { get; set; }
        private CanvasGlyph MainGlyph { get; set; }
        public GlyphConfig WordConfig { get; set; }
        public GlyphConfig LineConfig { get; set; }

        public class VectorRedactorConfig
        {
            public Brush LineBrush { get; set; }
            public Pen LinePen { get; set; }
            public Brush WordBrush { get; set; }
            public Pen WordPen { get; set; }
        }



        public VectorRedactorRepository(Graphics canvas, Redactor redactor, VectorRedactorConfig config)
        {
            Canvas = canvas;
            _redactor = redactor;
            canvas.SmoothingMode = SmoothingMode.HighQuality;

            Layout = new Layout(canvas);

            WordConfig = new GlyphConfig(config.WordBrush, config.WordPen);
            LineConfig = new GlyphConfig(config.LineBrush, config.LinePen);

            RedactorState = RedactorStates.Default;

            MainGlyph = new CanvasGlyph(WordConfig) { Redactor = _redactor, ParentVectorRedactor = this };
            MainGlyph.MainGlyph = MainGlyph;

            ActiveGlyph = MainGlyph;
        }

        /// <summary>
        /// Возвращает глиф наиболее высокого уровня, к которому принадлежит точка.
        /// </summary>
        /// <param name="location">Точка для поиска</param>
        /// <returns></returns>
        private Glyph FindGlyphIn(Point location)
        {
            return MainGlyph.FindGlyphIn(location);
        }

        private void TryStartDraggingGlyphIn(Point location)
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
            if (RedactorState == RedactorStates.Dragging)
            {
                RedactorState = RedactorStates.Default;
                return;
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

            if (e.Button == MouseButtons.Left && Mode == RedactorModes.Move &&
                RedactorState == RedactorStates.Default)
                TryStartDraggingGlyphIn(location);
        }

        /// <summary>
        /// Начинает создание нового глифа в зависимости от активного.
        /// </summary>
        /// <param name="location">Первая точка нового глифа</param>
        private void StartCreatingNewGlyphIn(Point location)
        {
            _redactor.Cursor = Cursors.Default;
            var parent = ActiveGlyph;

            if (parent is CanvasGlyph)
                RedactorState = RedactorStates.CreatingLine;

            if (parent is LineGlyph)
                RedactorState = RedactorStates.CreatingWord;

            var glyph = new PathGlyph(LineConfig);
            parent.InsertChild(glyph);

            ActiveGlyph = glyph;
            glyph.StartCreatingIn(location);
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


        public bool ProcessModeChangeToMove()
        {
            if (ActiveGlyph is PathGlyph)
                return false;

            Mode = RedactorModes.Move;
            return true;
        }

        public bool ProcessModeChangeToCreate()
        {
            if (ActiveGlyph is PathGlyph)
                return false;

            Mode = RedactorModes.Create;
            return true;
        }

        public bool ProcessModeChangeToAdd()
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
            if (RedactorState == RedactorStates.Dragging)
            {
                RedactorState = RedactorStates.Default;
                return;
            }

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
                        (ActiveGlyph as SolidGlyph).ProcessSelection();
                    _redactor.Invalidate();
                }
            }

            if (ActiveGlyph is PathGlyph)
                (ActiveGlyph as PathGlyph).ProcessLeftClick(location);

        }

        public void ProcessMove(MouseEventArgs e)
        {
            var cursor = Cursors.Default;
            var location = e.Location;

            if (RedactorState == RedactorStates.Default && !(FindGlyphIn(location) is PathGlyph) && !(FindGlyphIn(location) is CanvasGlyph))
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

        public void LoadPage(Page page)
        {
            MainGlyph = new CanvasGlyph(WordConfig) { Redactor = _redactor, ParentVectorRedactor = this };
            foreach (var line in page.Lines)
            {
                var lineGlyph = MainGlyph.InsertNewLineGlyph(line);
                foreach (var word in line.Words)
                    lineGlyph.InsertNewWordGlyph(word);
            }
        }
    }
}
