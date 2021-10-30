using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Nhibernate;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Nhibernate.Repositories;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System.Linq.Expressions;

namespace AECMIS.DAL.UnitTests.Helpers
{
    public class SqLiteRepository<TEntity, TId> : IRepository<TEntity, TId>, IDisposable where TEntity : Entity
    {
        #region fields & properties
        protected ISession Session
        {
            get
            {
                return  SQLiteSessionManager.GetSqlLiteSession<TEntity>();
            }
        }

        

        public void CloseSession()
        {
            //SQLiteSessionManager.CloseSession();
        }
        #endregion

        /// <summary>
        /// Gets the entity by its primary key value
        /// </summary>
        /// <returns>null if doesn't exist</returns>
        public TEntity Get(TId id)
        {
            using (var tx = Session.BeginTransaction())
            {
                var item = Session.Get<TEntity>(id);
                tx.Commit();
                return item;
            }
        }

        /// <summary>
        ///  This is a test
        /// </summary>
        /// <param name="namedQuery"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public object Get(string namedQuery, TId id)
        {
            using (var tx = Session.BeginTransaction())
            {
                var item = Session.GetNamedQuery(namedQuery).SetString("OppId", id.ToString());
                tx.Commit();
                return item.List();
            }
        }

        /// <summary>
        /// Gets the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public IList<TEntity> Get(List<long> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the entity by its primary key value
        /// will return proxy instead of null
        /// use this to avoid db call when you KNOW that the entity exists
        /// </summary>
        /// <returns>proxy if doesn't exist</returns>
        public TEntity Load(TId id)
        {
            using (var tx = Session.BeginTransaction())
            {
                var item = Session.Load<TEntity>(id);
                tx.Commit();
                return item;
            }
        }

        /// <summary>
        /// Find all entities of the current type
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<TEntity> FindAll()
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = Session.Query<TEntity>();
                // NOTE: the Take(100) limits a potentially unbounded resultset
                tx.Commit();
                return items;
            }
        }

        /// <summary>
        /// Finds First or Default
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public TEntity FindSingle(Expression<Func<TEntity, bool>> where)
        {
            using (var tx = Session.BeginTransaction())
            {
                var item = QueryList(where).FirstOrDefault();
                //tx.Commit();
                return item;
            }
        }


        public bool Exists(Expression<Func<TEntity, bool>> where)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = Session.Query<TEntity>()
                    .Where(where)
                    .Any();

                tx.Commit();

                return items;
            }
        }

        public IEnumerable<TEntity> CreateQuery(string hql)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = Session.CreateQuery(hql).List<TEntity>().Take(100); // NOTE: the Take(100) limits a potentially unbounded resultset

                tx.Commit();

                return items;
            }
        }


        public IEnumerable<TEntity> CreateCriteria(QueryOver criterion)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = criterion.DetachedCriteria.GetExecutableCriteria(Session).List<TEntity>();
                tx.Commit();

                return items;
            }
        }

        public IEnumerable<TEntity> CreateCriteria(DetachedCriteria criterion)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = criterion.GetExecutableCriteria(Session).List<TEntity>();
                tx.Commit();

                return items;
            }
        }

        public IEnumerable<TEntity> CreateCriteria(ICriterion criterion)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = Session.CreateCriteria<TEntity>().Add(criterion).List<TEntity>();

                tx.Commit();

                return items;
            }
        }

        public IEnumerable<TEntity> CreateCriteria(List<ICriterion> criteria)
        {
            using (var tx = Session.BeginTransaction())
            {
                ICriteria criteriaQuery = Session.CreateCriteria<TEntity>();
                foreach (var criterion in criteria)
                {
                    criteriaQuery.Add(criterion);
                }
                var items = criteriaQuery.List<TEntity>();
                tx.Commit();

                return items;
            }
        }


        public List<TEntity> QueryList(Expression<Func<TEntity, bool>> where)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = Session.Query<TEntity>()
                    //.Take(100) // NOTE: the Take(100) limits a potentially unbounded resultset
                    .Where(where).ToList();

                tx.Commit();

                return items;
            }
        }


        public TEntity SaveEntity(TEntity entity)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.Update(entity);
                tx.Commit();
            }

            return entity;
 
        }

        /// <summary>
        /// Save or Update
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="evict"> </param>
        /// <returns></returns>
        public TEntity Save(TEntity entity, bool evict)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.SaveOrUpdate(entity);
                tx.Commit();
                if (evict)
                    Session.Evict(entity);
            }

            return entity;
        }

        public TEntity Save(TEntity entity)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.SaveOrUpdate(entity);
                tx.Commit();
            }

            return entity;
        }


        public TEntity Merge(TEntity entity)
        {
            using (var tx = Session.BeginTransaction())
            {
                entity = (TEntity)Session.Merge(entity);
                tx.Commit();
            }
            return entity;
        }

        public void Delete(TEntity entity)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.Delete(entity);
                tx.Commit();
            }
        }


        public void ClearSession()
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.Flush();
              // Session.Clear();
                tx.Commit();
            }
        }

        public void FlushSession()
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.Flush();
                // Session.Clear();
                tx.Commit();
            }
        }

        /// <summary>
        /// Evicts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Evict(TEntity entity)
        {
            this.Session.Evict(entity);
        }


        #region Dispose

        public void Dispose()
        {
            Session.Close();
        }

        #endregion
    }
}
