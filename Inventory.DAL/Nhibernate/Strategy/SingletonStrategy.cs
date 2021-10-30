using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate;


namespace AECMIS.DAL.Nhibernate.Strategy
{
    

    /// <summary>
    /// Single threaded Strategy
    /// </summary>
    public class SingletonStrategy : ISessionStrategy
    {
        /// <summary>
        /// List of Sessions
        /// </summary>
        private readonly Dictionary<int, ISession> sessions = new Dictionary<int, ISession>();

        #region ISessionStrategy Members

        /// <summary>
        /// Find the current session
        /// </summary>
        /// <returns>First Session</returns>
        public ISession FindSession()
        {
            return this.sessions.FirstOrDefault(s => s.Key == Thread.CurrentThread.ManagedThreadId).Value;
        }

        /// <summary>
        /// Add new session to the stategy
        /// </summary>
        /// <param name="session">The session to add</param>
        public void AddSession(ISession session)
        {
            // Currently will always return a new session for multiple thread environment
            this.sessions.Add(Thread.CurrentThread.ManagedThreadId, session);
        }

        /// <summary>
        /// Remove the current session
        /// </summary>
        public void RemoveSession()
        {
            this.sessions.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        #endregion
    }
}
