using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using NHibernate.Criterion;
using System.Linq;
using System.Linq.Expressions;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public interface IRepository<TEntity, in TId> where TEntity : Entity
    {

        /// <summary>
        /// Gets the entity by its primary key value
        /// </summary>
        /// <returns>null if doesn't exist</returns>
        TEntity Get(TId id);

        IList<TEntity> Get(List<long> ids);

        /// <summary>
        /// Loads the entity by its primary key value
        /// will return proxy instead of null
        /// use this to avoid db call when you KNOW that the entity exists
        /// </summary>
        /// <returns>proxy if doesn't exist</returns>
        TEntity Load(TId id);

        /// <summary>
        /// Find all entities of the current type
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> FindAll();

        /// <summary>
        /// Finds First or Default
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        TEntity FindSingle(Expression<Func<TEntity, bool>> where);

        bool Exists(Expression<Func<TEntity, bool>> where);
        IEnumerable<TEntity> CreateQuery(string hql);
        IEnumerable<TEntity> CreateCriteria(ICriterion criterion);
        IEnumerable<TEntity> CreateCriteria(List<ICriterion> criteria);
        //IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where);
        List<TEntity> QueryList(Expression<Func<TEntity, bool>> where);

        
        IEnumerable<TEntity> CreateCriteria(DetachedCriteria criterion);
        void ClearSession();
        void Evict(TEntity entity);
        void FlushSession();
        /// <summary>
        /// Save or Update
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        TEntity Save(TEntity entity);

        TEntity Merge(TEntity entity);
        void Delete(TEntity entity);
        void Dispose();
    }
}
