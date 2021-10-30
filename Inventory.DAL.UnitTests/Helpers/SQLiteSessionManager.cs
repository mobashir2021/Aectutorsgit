using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Nhibernate.Interceptors;
using AECMIS.DAL.Nhibernate.Mappings;
using AECMIS.DAL.Nhibernate.Strategy;
using Iesi.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Utils;
using System.Data;

namespace AECMIS.DAL.UnitTests.Helpers
{
    public class SQLiteSessionManager
    {
        private static ISessionFactory _sqlLiteSessionFactory;
        private static Configuration _configuration;
        private static ISessionStrategy _strategy;

        private static readonly object SyncObject = new object();


        private static ISessionFactory SessionFactorySqlLite<T>() where T : Entity
        {
            lock (SqlLiteStrategy)
            {
                return _sqlLiteSessionFactory ?? (_sqlLiteSessionFactory = GetSqlLiteFluentConfiguration<T>());
            }
        }

        private static ISessionFactory GetSqlLiteFluentConfiguration<T>()
        {

            var config = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Address>(x => x.Namespace.StartsWith("AECMIS.DAL.Nhibernate.Mappings") && x != typeof(BaseMap<>)))
                .ExposeConfiguration(c => _configuration = c).
                ExposeConfiguration(c => c.SetProperty("hbm2ddl.keywords", "none"))
                .BuildConfiguration();
            var classMappings = config.ClassMappings.ToList();
            var collectionMappings = config.CollectionMappings.ToList();
             
            collectionMappings.ForEach(c=>
                                           {
                                               if(c.CollectionTable!=null && c.CollectionTable.Name.Contains("."))
                                                   c.CollectionTable.Name = c.CollectionTable.Name.Replace(".", "_").Replace("[", "").Replace("]", "");
                                           }
                );

            classMappings.ForEach(c =>
            {
                if (c.Table.Name.Contains("."))
                    c.Table.Name = c.Table.Name.Replace(".", "_").Replace("[", "").Replace("]", "");
            });
            var sessionFactory = config.BuildSessionFactory();
            return sessionFactory;
        }

        private static ISessionStrategy _sqlLiteStrategy;
        private static ISessionStrategy SqlLiteStrategy
        {
            get { return _sqlLiteStrategy ?? (_sqlLiteStrategy = new SingleThreadedSingletonStrategy()); }
        }

        public static ISession GetSqlLiteSession<T>() where T : Entity
        {
            ISession currentSession = SqlLiteStrategy.FindSession(); //Strategy.FindSession();
            try
            {
                if (currentSession != null
                    && currentSession.Connection.State.In(ConnectionState.Broken, ConnectionState.Closed))
                {
                    currentSession = null;
                }
            }
            catch (Exception)
            {
                currentSession = null;
            }

            if (null == currentSession)
            {
                ISessionFactory sessionFactory = SessionFactorySqlLite<T>();
                currentSession = sessionFactory.OpenSession(new CustomInterceptor());
                new SchemaExport(_configuration).Execute(true, true, false, currentSession.Connection, null);

                SqlLiteStrategy.AddSession(currentSession);
            }
            return currentSession;
        }


    }
}
