using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NHibernate;

namespace AECMIS.DAL.Nhibernate.Strategy
{
    /// <summary>
    /// Web Strategy
    /// </summary>
    public class WebStrategy : ISessionStrategy
    {
        private const string _currentSessionKey = "nhibernate.current_session";

        #region ISessionStrategy Members

        /// <summary>
        /// Find the current session
        /// </summary>
        /// <returns>Current Session</returns>
        public ISession FindSession()
        {
            HttpContext context = HttpContext.Current;
            return context != null ? context.Items[_currentSessionKey] as ISession : null;
        }

        /// <summary>
        /// Add the session to the stategy
        /// </summary>
        /// <param name="session">Session</param>
        public void AddSession(ISession session)
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
                context.Items[_currentSessionKey] = session;
        }

        /// <summary>
        /// Remove the current session
        /// </summary>
        public void RemoveSession()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
                context.Items.Remove(_currentSessionKey);
        }

        #endregion
    }
}
