using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public class UserRepository:Repository<User, int>
    {
        private readonly ISession _session;
        protected override ISession Session
        {
            get { return _session; }
        }

        public UserRepository():this(null)
        {
        }

        public UserRepository(ISession session)
        {
            _session = session ?? SessionManager.GetSession<User>();
        }

        public User GetUserByUserName(string userName, bool doCollate = true)
        {
            var sqlString = doCollate ? "Username like ? COLLATE SQL_Latin1_General_CP1_CS_AS" : "Username like ? ";
           return _session.CreateCriteria(typeof (User))
                .Add(Expression.Sql(sqlString, userName, NHibernateUtil.String))
                .UniqueResult<User>();

        }
    }
}
