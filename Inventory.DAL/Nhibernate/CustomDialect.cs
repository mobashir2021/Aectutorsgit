using NHibernate.Dialect;
using NHibernate.Dialect.Function;

namespace AECMIS.DAL.Nhibernate
{
    public class CustomDialect : MsSql2008Dialect
    {
        public CustomDialect()
        {
           
            RegisterFunction("freetext", new StandardSQLFunction("freetext", null));
            RegisterFunction("contains", new StandardSQLFunction("contains", null));
        }

    }
}