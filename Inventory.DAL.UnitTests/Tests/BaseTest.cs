using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain.Automapper;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Domain;
using AECMIS.DAL.UnitTests.Helpers;
using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate;
using NUnit.Framework;
using System.Threading;
using System.Globalization;

namespace AECMIS.DAL.UnitTests.Tests
{
    public abstract class BaseTest<T> where T : Entity
    {
        protected ISession Session
        {
            get
            {
                return SQLiteSessionManager.GetSqlLiteSession<T>();
                //return Nhibernate.SessionManager.GetSession<T>();
               
            }
        }

        [OneTimeSetUp]
        public virtual void SetUp()
        {            
            CultureInfo ci = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
           HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.
               Initialize();
            AutoMapper.Mapper.Reset();
            AutomapperBootStrap.InitializeMap();

        }

        [TearDown]
        public void CleanUp()
        {
            Session.Close();
        }

        public abstract void VerifyMapping();

        
    }
}