using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Owl.DataBase.Domain;

namespace Owl.DataBase.Repositories
{
    /// <summary>
    /// �������� ������ � Nhibernate
    /// </summary>
    public static class NHibertaneHelper
    {
        /// <summary>
        /// ������� ����� ������ ���� ��� ��� ��������� � ���������� �
        /// </summary>
        /// <returns>������ ���� ������</returns>
        public static ISession OpenSession(string dbFile)
        {
            return SessionFactory(dbFile).OpenSession();
        }

        private static ISessionFactory _sessionFactory;

        /// <summary>
        /// ������� ������
        /// </summary>
        private static ISessionFactory SessionFactory(string dbFile)
        {
                if (_sessionFactory == null)
                {
                    var cfg = Fluently.Configure()
                        .Database(
                            SQLiteConfiguration.Standard
                                .UsingFile(dbFile)
                        )
                        .Mappings(m =>
                                  m.FluentMappings.AddFromAssemblyOf<Book>())
                        .BuildConfiguration();
                    _sessionFactory = cfg.BuildSessionFactory();
                }
                return _sessionFactory;
        }
    }
}