using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.UnitTests.Helpers;
using Machine.Specifications;

namespace AECMIS.DAL.UnitTests.Tests
{
    public abstract class TestBase<T> where T : Entity
    {
        protected static SqLiteRepository<T, int> DataRepository;
        
        public Establish context = () =>
                                       {
                                           HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.
                                               Initialize();
                                           DataRepository = new SqLiteRepository<T, int>();

                                       };

        public Cleanup cleanup = () => DataRepository.CloseSession();

        
        


    }
}