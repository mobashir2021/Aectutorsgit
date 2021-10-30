using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AECMIS.DAL.Domain;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : Entity
    {

        #region fields & properties

        /// <summary>
        /// Gets Session.
        /// </summary>
        protected virtual ISession Session
        {
            get
            {
                return SessionManager.GetSession<TEntity>();
            }
            
        }
        #endregion

        /// <summary>
        /// Gets the entity by its primary key value
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// null if doesn't exist
        /// </returns>
        public TEntity Get(TId id)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var item = this.Session.Get<TEntity>(id);
                tx.Commit();
                return item;
            }
        }


        /// <summary>
        /// Gets the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns>List of Entities</returns>
        public IList<TEntity> Get(List<long> ids)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = this.Session.CreateCriteria<TEntity>().Add(Restrictions.In("Id", ids));
                tx.Commit();
                return items.List<TEntity>();
            }
        }

        /// <summary>
        /// Gets the specified instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <returns>List of Entities</returns>
        private IList<TEntity> Get<TEntity>(TEntity instance)
        {
            return this.Session.CreateCriteria(typeof(TEntity))
                .Add(Example.Create(instance))
                .List<TEntity>();
        }

        /// <summary>
        /// Loads the entity by its primary key value
        /// will return proxy instead of null
        /// use this to avoid db call when you KNOW that the entity exists
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// proxy if doesn't exist
        /// </returns>
        public TEntity Load(TId id)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var item = this.Session.Load<TEntity>(id);
                tx.Commit();
                return item;
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>List of Entities</returns>
        public IList<TEntity> FindAll(DetachedCriteria criteria)
        {
            return criteria.GetExecutableCriteria(this.Session).List<TEntity>();
        }

        /// <summary>
        /// Find Take(100) entities of the current type
        /// </summary>
        /// <returns>Queries</returns>
        public virtual IQueryable<TEntity> FindAll()
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = this.Session.Query<TEntity>();

                // NOTE: the Take(100) limits a potentially unbounded result set
                tx.Commit();
                return items;
            }
        }

        /// <summary>
        /// Gets the by sub query.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="subQuery">The sub query.</param>
        /// <returns>List of Entities</returns>
        public IList<TEntity> GetBySubQuery(ICriterion criteria, ICriterion subQuery)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = this.Session.CreateCriteria<TEntity>()
                    .Add(criteria)
                    .Add(subQuery);

                tx.Commit();

                return items.List<TEntity>();
            }
        }

        /// <summary>
        /// Finds First or Default
        /// </summary>
        /// <param name="where">Condition</param>
        /// <returns>The entity</returns>
        public TEntity FindSingle(Expression<Func<TEntity, bool>> where)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var item = this.QueryList(where).FirstOrDefault();

                return item;
            }
        }

        /// <summary>
        /// Existses the specified where.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>Checks if the item exists</returns>
        public bool Exists(Expression<Func<TEntity, bool>> where)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = this.Session.Query<TEntity>()
                    .Where(where)
                    .Any();

                tx.Commit();

                return items;
            }
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <param name="hql">The HQL.</param>
        /// <returns>Entity Enumeration</returns>
        public IEnumerable<TEntity> CreateQuery(string hql)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = this.Session.CreateQuery(hql).List<TEntity>().Take(100); // NOTE: the Take(100) limits a potentially unbounded resultset

                tx.Commit();

                return items;
            }
        }

        /// <summary>
        /// Creates the criteria.
        /// </summary>
        /// <param name="criterion">The criterion.</param>
        /// <returns>Entity Enumeration</returns>
        public IEnumerable<TEntity> CreateCriteria(DetachedCriteria criterion)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = criterion.GetExecutableCriteria(this.Session).List<TEntity>();
                tx.Commit();

                return items;
            }
        }

        /// <summary>
        /// Creates the criteria.
        /// </summary>
        /// <param name="criterion">The criterion.</param>
        /// <returns>Enumerable of type Entity</returns>
        public IEnumerable<TEntity> CreateCriteria(ICriterion criterion)
        {
            using (var tx = Session.BeginTransaction())
            {
                var items = Session.CreateCriteria<TEntity>().Add(criterion).List<TEntity>();

                tx.Commit();

                return items;
            }
        }

        /// <summary>
        /// Creates the criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>
        /// Enumerable of type Entity
        /// </returns>
        public IEnumerable<TEntity> CreateCriteria(List<ICriterion> criteria)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                ICriteria criteriaQuery = this.Session.CreateCriteria<TEntity>();
                foreach (var criterion in criteria)
                {
                    criteriaQuery.Add(criterion);
                }
                var items = criteriaQuery.List<TEntity>();
                tx.Commit();

                return items;
            }
        }


        /// <summary>
        /// Queries the list. Only Takes 100
        /// </summary>
        /// <param name="where">The where.</param>
        /// <returns>List of Entities</returns>
        public List<TEntity> QueryList(Expression<Func<TEntity, bool>> where)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var items = this.Session.Query<TEntity>()

                    // NOTE: the Take(100) limits a potentially unbounded resultset
                    .Where(where).ToList();


                tx.Commit();

                return items;
            }
        }

        

        /// <summary>
        /// Save or Update
        /// </summary>
        /// <param name="target">Entity to save</param>
        /// <returns>Saved Entity</returns>
        public TEntity Save(TEntity target)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                this.Session.SaveOrUpdate(target);
                tx.Commit();
            }

            return target;
        }

        /// <summary>
        /// Merges the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>Merged Entity</returns>
        public TEntity Merge(TEntity entity)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                entity = (TEntity)this.Session.Merge(entity);
                tx.Commit();
            }
            return entity;
        }

        /// <summary>
        /// Deletes the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        public void Delete(DetachedCriteria criteria)
        {
            foreach (TEntity entity in this.FindAll(criteria))
            {
                this.Delete(entity);
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Delete(TEntity entity)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                this.Session.Delete(entity);
                tx.Commit();
            }
        }

        public void ClearSession()
        {
            using (var tx = Session.BeginTransaction())
            {
                //Session.Flush();
                 Session.Clear();
                tx.Commit();
            }
        }

        public void FlushSession()
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.Flush();
                //Session.Clear();
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
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (this.Session != null && (this.Session.IsOpen))
            {
                this.Session.Close();
            }
        }
        #endregion
    }
}
