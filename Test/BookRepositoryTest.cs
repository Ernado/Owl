using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Owl.DataBase.Domain;
using Owl.DataBase.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Owl.Test
{
    [TestClass]
    public class BookTest
    {
        private const string DbFile = "test.db";

        [TestMethod]
        public void CanGenerateSchema()
        {
            var cfg = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .UsingFile(DbFile)
                )
                .Mappings(m =>
                          m.FluentMappings.AddFromAssemblyOf<IBookRepository>())
                .BuildConfiguration();

            new SchemaExport(cfg).Execute(false,true,false);
        }
    }
    
    /// <summary>
    ///Это класс теста для BookRepositoryTest, в котором должны
    ///находиться все модульные тесты BookRepositoryTest
    ///</summary>
    [TestClass]
    public class BookRepositoryTest
    {
        private static ISessionFactory _sessionFactory;
        private const string DbFile = "test.db";
        private static Configuration _configuration;
        private static readonly Polygon Polygon = new Polygon() {Points = {new Point() {X = 5, Y = 5}}};
        private readonly Line _line = new Line() {Number = 1, Polygon = Polygon};
        private readonly Book[] _books = new[]
                                             {
                                                 new Book {Directory = "alpha", Name = "alpha", Pages = new List<Page>{new Page{FileName = "alphapage",Number = 1}}},
                                                 new Book {Directory = "beta", Name = "Beta", Pages = new List<Page>{new Page{FileName = "betapage",Number = 1}}},
                                                 new Book {Directory = "gamma", Name = "Gamma", Pages = new List<Page>{new Page{FileName = "gammapage",Number = 1}}}
                                             };

        /// <summary>
        /// Сгенерировать данные для теста
        /// </summary>
        private void CreateInitialData()
        {
            using (ISession session = _sessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var book in _books)
                {
                    foreach (var page in book.Pages)
                    {
                        page.Book = book;
                        _line.Polygon.Points[0].Polygon = Polygon;
                        page.AddLine(_line);
                    }
                    session.Save(book);   
                }
                   
                transaction.Commit();
            }
        }

        /// <summary>
        /// Изоляция тестов
        /// </summary>
        [TestInitialize]
        public void TestFixtureSetup()
        {
            if (File.Exists(DbFile))
                File.Delete(DbFile);

            new SchemaExport(_configuration).Execute(false,true,false);
            CreateInitialData();
        }

        /// <summary>
        /// Очистка базы данных после теста
        /// </summary>
        [TestCleanup]
        public void TestCleanUp()
        {
            if (File.Exists(DbFile))
                File.Delete(DbFile);
        }

        [ClassInitialize]
        public static void TestSetup (TestContext testContext)
        {
            _configuration = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .UsingFile(DbFile)
                )
                .Mappings(m =>
                          m.FluentMappings.AddFromAssemblyOf<BookRepository>())
                .BuildConfiguration();

            _sessionFactory = _configuration.BuildSessionFactory();
        }

        /// <summary>
        /// Проверка возможности добавления книги
        /// </summary>
        [TestMethod]
        public void CanAddNewBook()
        {
            var page = new Page { FileName = "test_filename", Number = 1};
            page.AddLine(new Line {Number = 1});
            var book = new Book { Directory = "test", Name = "test"};
            book.AddPage(page);
            var repository = new BookRepository(DbFile);
            repository.Add(book);

            using (var session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Book>(book.UID);

                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(book, fromDb);
                Assert.AreEqual(book.Name, fromDb.Name);
                Assert.AreEqual(book.Directory, fromDb.Directory);
                Assert.AreEqual(book.Pages[0].Number, fromDb.Pages[0].Number);
                Assert.AreEqual(book.Pages[0].FileName, fromDb.Pages[0].FileName);
                Assert.AreEqual(book.Pages[0].Lines[0].Number, fromDb.Pages[0].Lines[0].Number);
            }
        }

        /// <summary>
        /// Проверка возможности обновления книги
        /// </summary>
        [TestMethod]
        public void CanUpdateProduct ()
        {
            var book = _books[0];
            book.Name = "Zeta";
            var repository = new BookRepository(DbFile);
            repository.Update(book);

            using (var session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Book>(book.UID);
                Assert.AreEqual(book.Name,fromDb.Name);
            }
        }

        [TestMethod]
        public void CanRemoveProduct ()
        {
            var book = _books[0];
            var repository = new BookRepository(DbFile);
            repository.Remove(book);

            using (var session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Book>(book.UID);
                Assert.IsNull(fromDb);
            }
        }

        [TestMethod]
        public void CanGetAll ()
        {
            var repository = new BookRepository(DbFile);
            var fromDb = repository.GetAll();
            foreach (var book in _books)
            {
                Assert.IsTrue(IsInCollection(book,fromDb));
            }    
        }

        [TestMethod]
        public void CanGetByName ()
        {
            var book = _books[1];
            var repository = new BookRepository(DbFile);
            var fromDb = repository.GetByName(book.Name);
            Assert.AreEqual(fromDb.Name,book.Name);
            Assert.AreEqual(fromDb.Pages[0].FileName, book.Pages[0].FileName);
        }

        private bool IsInCollection(Book product, IEnumerable<Book> fromDb)
        {
            return fromDb.Any(item => product.UID == item.UID);
        }
    }
}
