using FluentNHibernate.Mapping;
using Owl.DataBase.Domain;

namespace Owl.DataBase.Mappings
{
    /// <summary>
    /// Классы соответствия сущностей бизнес-модели и её 
    /// отображения в базе данных в виде таблиц
    /// </summary>

    public class BookMap : ClassMap<Book>
    {
        public BookMap()
        {
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.Directory);
            Map(x => x.Name);
            HasMany(x => x.Pages)
                .Cascade.All()
                .Inverse()
                .LazyLoad();
        }
    }

    public class PageMap : ClassMap<Page>
    {
         public PageMap()
         {
             Id(x => x.UID).GeneratedBy.Guid();
             Map(x => x.Number);
             Map(x => x.FileName);
             References(x => x.Book);
             HasMany(x => x.Lines)
                 .Cascade.All()
                 .Inverse();
         }
    }

    public class LineMap : ClassMap<Line> 
    {
        public LineMap()
        {
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.Number);
            HasMany(x => x.Words)
                .Cascade.All()
                .Inverse();
            References(x => x.Page);
            HasOne(x => x.Polygon)
                .Cascade.All()
                .Constrained();
        }
    }

    public class WordMap : ClassMap<Word>
    {
        public WordMap()
        {
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.Name);
            Map(x => x.Number);
            HasMany(x => x.Polygons)
                .Cascade.All()
                .Inverse();
            References(x => x.Line);
        }
    }

    public class PolygonMap : ClassMap<Polygon>
    {
        public PolygonMap()
        {
            Id(x => x.UID).GeneratedBy.Guid();
            HasMany(x => x.Points)
                .Cascade.All()
                .Inverse();
        }
    }

    public class PointMap : ClassMap<Point>
    {
        public PointMap()
        {
            Id(x => x.UID).GeneratedBy.Guid();
            Map(x => x.X);
            Map(x => x.Y);
            References(x => x.Polygon);
        }
         
    }
}
