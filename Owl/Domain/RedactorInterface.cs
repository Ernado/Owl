using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Owl.Domain
{
    class RedactorInterface
    {
        private Layout _layout;
        private Graphics Canvas { get; set; }

        public RedactorInterface(Graphics canvas)
        {
            Canvas = canvas;
            _layout = new Layout(canvas);
        }

        public 

        abstract class Glyph
        {
            public abstract void Draw();
            public abstract bool Intersects(Point point);
            public abstract void Insert(Glyph glyph);
            public abstract void Remove(Glyph glyph);
            public virtual Glyph Parent { get; protected set; }
            public virtual Glyph Child { get; protected set; }
        }

        class WordGlyph : Glyph
        {
            public Word Word { get; set; }
            public Figure Figure;

            public WordGlyph (Word word)
            {
                Figure = new SolidFigure();
            }

            public override void Draw()
            {
                Figure.Draw();
            }
            public override bool  Intersects(Point point)
            {
                return Figure.Intersects(point);
            }

            public override void Insert(Glyph glyph)
            {
                throw new NotImplementedException();
            }

            public override void Remove(Glyph glyph)
            {
                throw new NotImplementedException();
            }
        }
    }
}
