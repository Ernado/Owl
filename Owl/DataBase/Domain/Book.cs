using System;
using System.Collections.Generic;
using System.Linq;

namespace Owl.DataBase.Domain
{
    /// <summary>
    /// Класс страницы
    /// </summary>
    public class Page
    {
        public virtual Guid UID { get; protected set; }
        public virtual string FileName { get; set; } //Имя файла (в папке, соотв. документа)
        public virtual int Number { get; set; } //Номер страницы
        public virtual Book Book { get; set; } //Документ, в котором размещена страница
        public virtual IList<Line> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }
        private IList<Line> _lines = new List<Line>(); //Список строчек страницы

        /// <summary>
        /// Добавить строку к строанице
        /// </summary>
        /// <param name="line">Строка</param>
        public virtual void AddLine (Line line)
        {
            line.Page = this;
            Lines.Add(line);
        }
    }

    /// <summary>
    /// Класс документа
    /// </summary>
    public class Book
    {
        public virtual Guid UID  { get; protected set; }
        private IList<Page> _pages = new List<Page>(); //Список страниц документа
        public virtual IList<Page> Pages
        {
            get { return _pages; } 
            set { _pages = value; }
        } 
        public virtual string Directory { get; set; } //Имя папки документа
        public virtual string Name { get; set; } //Название документа
        
        /// <summary>
        /// Добавить страницу к документу
        /// </summary>
        /// <param name="page">Страница</param>
        public virtual void AddPage (Page page)
        {
            page.Book = this; //указываем документ как родительский для страницы
            Pages.Add(page); //добавляем страницу в список страниц документа
        }
    }

    /// <summary>
    /// Класс точки
    /// </summary>
    public class Point
    {
        public virtual Guid UID { get; protected set; }
        public virtual float X { get; set; } //Абсцисса точки
        public virtual float Y { get; set; } //Ордината точки
        public virtual Polygon Polygon { get; set; } //Многоугольник, в которую входит точка
    }

    /// <summary>
    /// Класс многоугольника
    /// </summary>
    public class Polygon
    {

        public virtual Guid UID { get; protected set; }
        public virtual IList<Point> Points
        {
            get { return _points; } 
            set { _points = value; }
        } 
        private IList<Point> _points = new List<Point>();  
        
        /// <summary>
        /// Добавить точку к многоугольнику
        /// </summary>
        /// <param name="point">Точка</param>
        public virtual void AddPoint (Point point)
        {
            point.Polygon = this;
            Points.Add(point);
        }

        public List<System.Drawing.Point> ConvertToDrawingPoints ()
        {
            return Points.Select(point => new System.Drawing.Point
                                              {
                                                  X = (int) Math.Round(point.X), Y = (int) Math.Round(point.Y)
                                              }).ToList();
        }

        /// <summary>
        /// Добавляет список точек к полигону. (Предварительно обнуляя исходный)
        /// </summary>
        /// <param name="points">Точки для добавления к полигону.</param>
        public void LoadPointList (IEnumerable<System.Drawing.Point> points)
        {
            _points.Clear();
            foreach (var point in points)
            {
                var newpoint = new Point {X = point.X, Y = point.Y};
                AddPoint(newpoint);
            }
        }
    }
    
    /// <summary>
    /// Класс строчки
    /// </summary>
    public class Line
    {
        public Line ()
        {
            Polygon = new Polygon();
        }

        public virtual Guid UID { get; protected set; }
        public virtual Polygon Polygon { get; set; } //Многоугольник, описывающий строку
        public virtual IList<Word> Words
        {
            get { return _words; }
            set { _words = value; }
        }  
        private IList<Word> _words = new List<Word>(); 
        public virtual int Number { get; set; } //Номер строки
        public virtual Page Page { get; set; } //Страница, к которой принадлежит строка

        /// <summary>
        /// Добавить слово в строчку
        /// </summary>
        /// <param name="word">Слово</param>
        public virtual void AddWord(Word word)
        {
            word.Line = this;
            Words.Add(word);
        }
    }

    /// <summary>
    /// Класс слова
    /// </summary>
    public class Word
    {
        public virtual Guid UID { get; protected set; }
        public virtual int Number { get;  set; } //Номер слова
        public virtual IList<Polygon> Polygons
        {
            get { return _polygons; }
            set { _polygons = value; }
        } //Многоугольники, задающие маску слова
        private IList<Polygon> _polygons = new List<Polygon>(); 
        public virtual string Name { get; set; } //Само слово
        public virtual Line Line { get; set; } //Строка, к которой принадлежит слово
 
        
        /// <summary>
        /// Добавить многоугольник к маске слова
        /// </summary>
        /// <param name="polygon">Многоугольник</param>
        public virtual void AddPolygon(Polygon polygon)
        {
            Polygons.Add(polygon);
        }
    }

    public interface IBookRepository
    {
        void Add(Book book);
        void Update(Book book);
        void Remove(Book book);
        Book GetById(Guid uid );
        Book GetByName(string name);
        ICollection<Book> GetAll();
    }
}
