using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Nhibernate.Strategy;
using NHibernate;

namespace AECMIS.DAL.UnitTests.Helpers
{
    public class SingleThreadedSingletonStrategy : ISessionStrategy
    {
        private ISession _session;

        #region ISessionStrategy Members

        public ISession FindSession()
        {
            return _session;
        }

        public void AddSession(ISession session)
        {
            _session = session;
        }

        public void RemoveSession()
        {
            _session = null;
        }

        #endregion
    }
}
