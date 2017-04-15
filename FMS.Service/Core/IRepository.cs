using Newtonsoft.Json.Linq;
using FMS.Service.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Service.Core
{
    public interface IRepository<T> : IDisposable where T : class
    {
        /// <summary>
        /// Gets all objects from database
        /// </summary>
        IQueryable<T> All();

        /// <summary>
        /// Gets objects from database by filter.
        /// </summary>
        /// <param name="predicate">Specified a filter</param>
        IQueryable<T> Filter(Expression<Func<T, bool>> predicate);

        //IQueryable<T> Filter<TKey>(Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> orderby, ref Pagination pagination);
        IQueryable<T> Filter(Expression<Func<T, bool>> filter, ref Pagination pagination);

        /// <summary>
        /// Gets objects' count from database by filter.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int Count(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets the object(s) is exists in database by specified filter.
        /// </summary>
        /// <param name="predicate">Specified the filter expression</param>
        bool Contains(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Find object by keys.
        /// </summary>
        /// <param name="keys">Specified the search keys.</param>
        T Find(params object[] keys);

        /// <summary>
        /// Find object by specified expression.
        /// </summary>
        /// <param name="predicate"></param>
        T Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Create a new object to database.
        /// </summary>
        /// <param name="t">Specified a new object to create.</param>
        T Create(T t);

        /// <summary>
        /// Delete the object from database.
        /// </summary>
        /// <param name="t">Specified a existing object to delete.</param>        
        T Delete(T t);

        /// <summary>
        /// Delete objects from database by specified filter expression.
        /// </summary>
        /// <param name="predicate"></param>
        int Delete(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Update object changes and save to database.
        /// </summary>
        /// <param name="t">Specified the object to save.</param>
        int Update(T t);
        T Update(JObject option, params object[] id);


        /// <summary>
        /// update objects from database by specified filter expression.
        /// </summary>
        /// <param name="predicate">所有要改的对象</param>
        /// <returns></returns>
        int Update(Expression<Func<T, bool>> predicate, JObject target);

        /// <summary>
        /// Get the total objects count.
        /// </summary>
        int Count();

        //int Assign()

        #region Async

        Task<T> FindAsync(params object[] keys);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);

        //Task<IQueryable<T>> FilterAsync(Expression<Func<T, bool>> predicate);
        Task<IQueryable<T>> FilterAsync<TKey>(Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> orderby, ref Pagination pagination);
        Task<IQueryable<T>> FilterAsync(Expression<Func<T, bool>> filter, ref Pagination pagination);


        Task<int> UpdateAsync(T t);
        Task<int> UpdateAsync(JObject option, params object[] id);
        //Task<int> UpdateAsync(Expression<Func<T, bool>> predicate, JObject target);


        Task<T> CreateAsync(T t);
        #endregion
        Task<T> DeleteAsync(T t);
    }
}
