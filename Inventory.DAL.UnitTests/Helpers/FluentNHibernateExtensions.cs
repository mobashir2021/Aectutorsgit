using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using NHibernate;
using AECMIS.DAL.Nhibernate.Conventions;
using FluentNHibernate.Cfg;
namespace AECMIS.DAL.UnitTests.Helpers
{
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
