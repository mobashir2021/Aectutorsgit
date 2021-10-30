using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Nhibernate.Conventions;
using AECMIS.DAL.Nhibernate.Interceptors;
using AECMIS.DAL.Nhibernate.Mappings;
using AECMIS.DAL.Nhibernate.Strategy;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Utils;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Type;

namespace AECMIS.DAL.Nhibernate
{
    public class SessionManager
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionStrategy _strategy;

        public static ISessionStrategy Strategy
        {
            get
            {
                if (_strategy == null)
                {
                    if (HttpContext.Current == null)
                        _strategy = new SingletonStrategy();
                    else
                        _strategy = new WebStrategy();
                }
                return _strategy;
            }
            set { _strategy = value; }
        }

        /// <summary>
        /// Sessions the factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static ISessionFactory SessionFactory<T>() where T : Entity
        {
            lock (Strategy)
            {
                return _sessionFactory ?? (_sessionFactory = GetFluentConfiguration<T>().
                                                                 BuildSessionFactory());
            }
        }

        private static ISessionFactory SessionFactory()
        {
            lock (Strategy)
            {
                return _sessionFactory ?? (_sessionFactory = GetFluentConfiguration().
                                                                 BuildSessionFactory());
            }
        }

        /// <summary>
        /// Generates the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outputToConsole">if set to <c>true</c> [output to console].</param>
        /// <param name="executeAgainstDatabase">if set to <c>true</c> [execute against database].</param>
        /// <param name="justDrop">if set to <c>true</c> [just drop].</param>
        public static void GenerateSchema<T>(bool outputToConsole, bool executeAgainstDatabase, bool justDrop)
        {
            try
            {
                _sessionFactory = GetFluentConfiguration<T>()
                    .ExposeConfiguration(cfg => new SchemaExport(cfg).SetOutputFile("schemaExport.sql")
                                                    .Execute(outputToConsole, executeAgainstDatabase, justDrop))
                    .BuildSessionFactory();
            }
            catch (Exception e)
            {
                throw e.InnerException ?? e;
            }
        }

        /// <summary>
        /// Gets the fluent configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static FluentConfiguration GetFluentConfiguration<T>()
        {
            FluentConfiguration config = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ShowSql()
                              .ConnectionString(x => x.FromConnectionStringWithKey("AECMISDB")).Dialect<CustomDialect>
                /*.AdoNetBatchSize(10)*/)
                .Mappings(m => m.HbmMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .Mappings(
                    m =>
                    m.FluentMappings.AddFromAssemblyOf<Address>(
                        x => x.Namespace.StartsWith("AECMIS.DAL.Nhibernate.Mappings") && x != typeof(BaseMap<>))
                        //.AddFromAssembly(Assembly.GetExecutingAssembly())
                        .Conventions.AddFromAssemblyOf<EnumConvention>());

            var autoMap = AutoMap.AssemblyOf <Entity>();

            return config;
        }

        private static FluentConfiguration GetFluentConfiguration()
        {
            FluentConfiguration config = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ShowSql()
                              .ConnectionString(x => x.FromConnectionStringWithKey("AECMISDB")).Dialect<CustomDialect>
                /*.AdoNetBatchSize(10)*/)
                .Mappings(m => m.HbmMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .Mappings(
                    m =>
                    m.FluentMappings.AddFromAssemblyOf<Address>(
                        x => x.Namespace.StartsWith("AECMIS.DAL.Nhibernate.Mappings") && x != typeof(BaseMap<>))
                        //.AddFromAssembly(Assembly.GetExecutingAssembly())
                        .Conventions.AddFromAssemblyOf<EnumConvention>());

            var autoMap = AutoMap.AssemblyOf<Entity>();

            return config;
        }


        /// <summary>
        /// Removes the session.
        /// </summary>
        /// <param name="session">The session.</param>
        private static void RemoveSession(ISession session)
        {
            session.Clear();
            session.Close();
            Strategy.RemoveSession();
            session = null;
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISession GetSession<T>() where T : Entity
        {
            ISession currentSession = Strategy.FindSession();
            try
            {
                if (currentSession != null
                    && currentSession.Connection.State.In(ConnectionState.Broken, ConnectionState.Closed))
                {
                    RemoveSession(currentSession);
                }
            }
            catch (Exception)
            {
                currentSession = null;
            }

            if (null == currentSession)
            {
                ISessionFactory sessionFactory = SessionFactory<T>();
                currentSession = sessionFactory.OpenSession(new CustomInterceptor());
                Strategy.AddSession(currentSession);
            }

            //Console.WriteLine("Session has state {0}".Fill(currentSession.Connection.State));
            return currentSession;
        }

        public static ISession GetSession()
        {
            ISession currentSession = Strategy.FindSession();
            try
            {
                if (currentSession != null
                    && currentSession.Connection.State.In(ConnectionState.Broken, ConnectionState.Closed))
                {
                    RemoveSession(currentSession);
                }
            }
            catch (Exception)
            {
                currentSession = null;
            }

            if (null == currentSession)
            {
                ISessionFactory sessionFactory = SessionFactory();
                currentSession = sessionFactory.OpenSession(new CustomInterceptor());
                Strategy.AddSession(currentSession);
            }

            //Console.WriteLine("Session has state {0}".Fill(currentSession.Connection.State));
            return currentSession;
        }


        /// <summary>
        /// Closes the session.
        /// </summary>
        public static void CloseSession()
        {
            ISession currentSession = Strategy.FindSession();
            if (currentSession != null)
            {
                Console.WriteLine("Closing existing session");
                if (currentSession.IsOpen)
                {
                    currentSession.Clear();
                    currentSession.Close();
                    currentSession.Dispose();
                }
                Strategy.RemoveSession();
            }
        }
    }

    /// <summary>
    /// Unused class
    /// </summary>
    public static class FluentNHibernateExtensions
    {
        public static FluentMappingsContainer AddFromAssemblyOf<T>(
            this FluentMappingsContainer mappings,
            Predicate<Type> where)
        {
            if (where == null)
                return mappings.AddFromAssemblyOf<T>();

            var mappingClasses = typeof(T).Assembly.GetExportedTypes()
                .Where(x => typeof(IMappingProvider).IsAssignableFrom(x)
                            && where(x));

            foreach (var type in mappingClasses)
            {
                mappings.Add(type);
            }

            return mappings;
        }
    }
}



