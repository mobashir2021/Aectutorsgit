using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace AECMIS.DAL.Nhibernate.Strategy
{
    public interface ISessionStrategy
    {
        /// <summary>
        /// Find the current session
        /// </summary>
        /// <returns></returns>
        ISession FindSession();

        /// <summary>
        /// Add new session to the stategy
        /// </summary>
        /// <param name="session"></param>
        void AddSession(ISession session);

        /// <summary>
        /// Remove the current session
        /// </summary>
        void RemoveSession();
    }
}
