using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Context;
namespace AECMIS.DAL.UnitTests.Helpers
{
    public class ContextManager : CurrentSessionContext
    {

        protected override ISession Session
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


    }
}
