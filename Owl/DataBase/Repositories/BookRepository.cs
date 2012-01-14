using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Owl.DataBase.Domain;

namespace Owl.DataBase.Repositories
{
    /// <summary>
    /// Класс работы с базой данных
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly ISession _session;
        public BookRepository(string dbFile)
        {
            _session = NHibertaneHelper.OpenSession(dbFile);
        }

        /// <summary>
        /// Добавить документ в базу данных
        /// </summary>
        /// <param name="book">Документ класса Book</param>
        public void Add(Book book)
        {
            
            using (var transaction = _session.BeginTransaction())
            {
                _session.Save(book);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Обновить книгу в базе данных
        /// </summary>
        /// <param name="book">Документ класса Book</param>
        public void Update(Book book)
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Update(book);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Удалить книгу из базы данных
        /// </summary>
        /// <param name="book">Документ класса Book</param>
        public void Remove(Book book)
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Delete(book);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Возвращает книгу с идентификатором uid
        /// </summary>
        /// <param name="uid">Идентификатор</param>
        /// <returns></returns>
        public Book GetById(Guid uid)
        {
                return _session.Get<Book>(uid);
        }

        /// <summary>
        /// Возвращает книгу с заданным именем
        /// </summary>
        /// <param name="name">имя книги</param>
        /// <returns></returns>
        public Book GetByName(string name)
        {
            var book = _session
                .CreateCriteria(typeof (Book))
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<Book>();
            return book;
        }

        public ICollection<Book> GetAll ()
        {
            var books = _session.CreateCriteria(typeof (Book))
                    .List<Book>();
                return books;
        }
    }
}
