using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection;
using FMS.Service.Models;
using FMS.Service.DAO;
using System.Text.RegularExpressions;

namespace FMS.Service.Core
{

    public partial class Repository<TObject> : IRepository<TObject> where TObject : class
    {

        public OmsContext context;
        private bool shareContext = false;

        public Repository()
            : this(new OmsContext())
        {
        }

        public Repository(OmsContext context)
        {
            this.context = context;
            //shareContext = true;
        }

        protected DbSet<TObject> DbSet
        {
            get
            {
                return context.Set<TObject>();
            }
        }

        public void Dispose()
        {
            if (shareContext && (context != null))
                context.Dispose();
        }

        public virtual IQueryable<TObject> All()
        {
            return DbSet.AsQueryable();
        }

        public virtual IQueryable<TObject> Filter(Expression<Func<TObject, bool>> predicate)
        {
            return DbSet.Where(predicate).AsQueryable<TObject>();
        }
        public int Count(Expression<Func<TObject, bool>> predicate)
        {
            return DbSet.Count(predicate);
        }


        //public virtual IQueryable<TObject> Filter<TKey>(Expression<Func<TObject, bool>> filter, Expression<Func<TObject, TKey>> orderby, ref Pagination pagination)
        //{
        //    var _list = filter != null ? DbSet.Where(filter).AsQueryable() : DbSet.AsQueryable();
        //    if (pagination == null) pagination = new Pagination();
        //    if (pagination.Grep != null && pagination.Grep.Count > 0)
        //    {

        //        _list = _Filter(_list, pagination.Grep);
        //        //_list = _SEARCH(_list, pagination.Grep);
        //    }
        //    if (!pagination.NoPaging)
        //    {
        //        _list = _list.OrderBy(orderby).Skip((pagination.Index - 1) * pagination.Size).Take(pagination.Size);
        //    }
        //    pagination.All = _list.Count();
        //    pagination.Count = pagination.All % pagination.Size == 0 ? (pagination.All / pagination.Size) : ((pagination.All / pagination.Size) + 1);
        //    return _list.AsQueryable();
        //}


        public virtual IQueryable<TObject> Filter(Expression<Func<TObject, bool>> filter, ref Pagination pagination)
        {
            var _list = filter != null ? DbSet.Where(filter).AsQueryable() : DbSet.AsQueryable();
            if (pagination == null) pagination = new Pagination();
            if (pagination.Grep != null && pagination.Grep.Count > 0)
            {
                //_list = _SEARCH(_list, pagination.Grep);
                _list = MYSEARCH(_list, pagination.Grep);
            }
            JObject sorts = new JObject();
            if (string.IsNullOrEmpty(pagination.Sort))
            {
                sorts.Add("id", "asc");
            }
            else
            {
                sorts = JsonConvert.DeserializeObject<JObject>(pagination.Sort);
            }
            //if (pagination.Sort == null) pagination.Sort = new JObject();
            //Expression<Func<TObject, TKey>> orderby;
            //if (pagination.Sort.Count == 0) pagination.Sort.Add("id", "asc");
            //JToken tokens = pagination.Sort;
            //JObject sorts = new JObject();
            //foreach (var item in pagination.Sort.Keys)
            //{
            //    sorts.Add(item, pagination.Sort[item]);
            //}
            JToken tokens = sorts;// pagination.Sort;
            var flag = false;
            IOrderedQueryable<TObject> _orderlist = null;
            foreach (JProperty opt in tokens)
            {
                var property = typeof(TObject).GetProperty(opt.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", opt.Name));

                //            var parameter = Expression.Parameter(property.DeclaringType, property.ToString());
                //            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                //            var orderByExp = Expression.Lambda(propertyAccess, parameter);
                //            MethodCallExpression exp = Expression.Call(
                //typeof(Queryable),
                //opt.Value.ToString() == "desc" ? "OrderByDescending" : "OrderBy",
                //new Type[] { typeof(TObject), property.PropertyType },
                //  _list.Expression,
                //Expression.Quote(orderByExp));
                if (opt.Value.ToString().Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    if (!flag)
                    {
                        //_orderlist = _list.OrderByDescending(x => property.Name);
                        _orderlist = _list.OrderByDescending(property.Name);
                        flag = true;
                    }
                    else
                    {
                        _orderlist = _orderlist.ThenByDescending(property.Name);
                    }
                }
                else
                {
                    if (!flag)
                    {
                        _orderlist = _list.OrderBy(property.Name);// _list.AsQueryable().Provider.CreateQuery<TObject>(exp) as IOrderedQueryable<TObject>; 
                        flag = true;
                    }
                    else
                    {
                        _orderlist = _orderlist.ThenBy(property.Name);
                    }
                }
                //property.SetValue(entity, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(opt.Value), property.PropertyType));
                _list = _orderlist;
            }
            if (!pagination.NoPaging)
            {

                _list = _list.Skip((pagination.Index - 1) * pagination.Size).Take(pagination.Size);
            }
            pagination.All = _list.Count();
            pagination.Count = pagination.All % pagination.Size == 0 ? (pagination.All / pagination.Size) : ((pagination.All / pagination.Size) + 1);
            return _list.AsQueryable();
        }

        public bool Contains(Expression<Func<TObject, bool>> predicate)
        {
            return DbSet.Count(predicate) > 0;
        }

        public virtual TObject Find(params object[] keys)
        {
            return DbSet.Find(keys);
        }

        public virtual TObject Find(Expression<Func<TObject, bool>> predicate)
        {
            return DbSet.FirstOrDefault(predicate);
        }


        public virtual TObject Create(TObject tobject)
        {
            var newEntry = DbSet.Add(tobject);
            if (!shareContext)
                context.SaveChanges();
            return newEntry;
        }

        public virtual int Count()
        {
            return DbSet.Count();
        }


        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="TObject"></param>
        /// <returns></returns>
        public virtual int Update(TObject tobject)
        {
            var entry = context.Entry(tobject);
            //DbSet.Attach(tobject);
            entry.State = EntityState.Modified;
            if (!shareContext)
                return context.SaveChanges();
            return 0;
        }

        public virtual TObject Update(JObject option, params object[] id)
        {
            var entity = Find(id);
            JToken tokens = option;
            foreach (JProperty opt in tokens)
            {
                if (opt.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;
                var property = entity.GetType().GetProperty(opt.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", opt.Name));
                //if (property.PropertyType.IsAbstract || property.PropertyType.IsClass) continue;
                property.SetValue(entity, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(opt.Value), property.PropertyType));
            }

            if (!shareContext)
                context.SaveChanges();
            return entity;

        }

        /// <summary>
        /// 修改数据集的一个或多个属性
        /// </summary>
        /// <param name="predicate">数据集表达式</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual int Update(Expression<Func<TObject, bool>> predicate, JObject target)
        {
            var objects = Filter(predicate);
            foreach (var obj in objects)
            {
                JToken tokens = target;
                foreach (JProperty p in tokens)
                {
                    if (p.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;
                    var property = obj.GetType().GetProperty(p.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", p.Name));
                    property.SetValue(obj, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(p.Value), property.PropertyType));
                }
            }

            if (!shareContext)
                return context.SaveChanges();
            return 0;
        }




        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="tobject"></param>
        /// <returns></returns>
        public virtual TObject Delete(TObject tobject)
        {
            context.Entry(tobject);
            var entry = DbSet.Remove(tobject);
            if (!shareContext)
                context.SaveChanges();
            return entry;
        }
        /// <summary>
        /// 删除数据集
        /// </summary>
        /// <param name="predicate">获取数据集的表达式</param>
        /// <returns></returns>
        public virtual int Delete(Expression<Func<TObject, bool>> predicate)
        {
            var objects = Filter(predicate);
            foreach (var obj in objects)
                DbSet.Remove(obj);
            if (!shareContext)
                return context.SaveChanges();
            return 0;
        }

        #region Async
        public virtual Task<TObject> FindAsync(params object[] keys)
        {
            return DbSet.FindAsync(keys);
        }
        public virtual Task<TObject> FindAsync(Expression<Func<TObject, bool>> predicate)
        {
            return DbSet.FirstOrDefaultAsync(predicate);
        }


        //public virtual Task<IQueryable<TObject>> FilterAsync(Expression<Func<TObject, bool>> predicate)
        //{
        //    return Task.Run<IQueryable<TObject>>(() =>
        //    {
        //        return DbSet.Where<TObject>(predicate).AsQueryable();
        //    });
        //}


        public virtual Task<IQueryable<TObject>> FilterAsync<TKey>(Expression<Func<TObject, bool>> filter, Expression<Func<TObject, TKey>> orderby, ref Pagination pagination)
        {
            var _list = filter != null ? DbSet.Where(filter).OrderBy(orderby).AsQueryable() : DbSet.OrderBy(orderby).AsQueryable();
            if (pagination == null) pagination = new Pagination();
            if (pagination.Grep != null && pagination.Grep.Count > 0)
            {
                //_list = _SEARCH(_list, pagination.Grep);
                _list = MYSEARCH(_list, pagination.Grep);
            }
            JObject sorts = new JObject();
            if (string.IsNullOrEmpty(pagination.Sort))
            {
                sorts.Add("id", "asc");
            }
            else
            {
                sorts = JsonConvert.DeserializeObject<JObject>(pagination.Sort);
            }
            JToken tokens = sorts;// pagination.Sort;
            var flag = false;
            IOrderedQueryable<TObject> _orderlist = null;
            foreach (JProperty opt in tokens)
            {
                var property = typeof(TObject).GetProperty(opt.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", opt.Name));

                //            var parameter = Expression.Parameter(property.DeclaringType, property.ToString());
                //            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                //            var orderByExp = Expression.Lambda(propertyAccess, parameter);
                //            MethodCallExpression exp = Expression.Call(
                //typeof(Queryable),
                //opt.Value.ToString() == "desc" ? "OrderByDescending" : "OrderBy",
                //new Type[] { typeof(TObject), property.PropertyType },
                //  _list.Expression,
                //Expression.Quote(orderByExp));
                if (opt.Value.ToString().Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    if (!flag)
                    {
                        //_orderlist = _list.OrderByDescending(x => property.Name);
                        _orderlist = _list.OrderByDescending(property.Name);
                        flag = true;
                    }
                    else
                    {
                        _orderlist = _orderlist.ThenByDescending(property.Name);
                    }
                }
                else
                {
                    if (!flag)
                    {
                        _orderlist = _list.OrderBy(property.Name);// _list.AsQueryable().Provider.CreateQuery<TObject>(exp) as IOrderedQueryable<TObject>; 
                        flag = true;
                    }
                    else
                    {
                        _orderlist = _orderlist.ThenBy(property.Name);
                    }
                }
                //property.SetValue(entity, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(opt.Value), property.PropertyType));
                _list = _orderlist;
            }
            if (!pagination.NoPaging)
            {
                _list = _list.OrderBy(orderby).Skip((pagination.Index - 1) * pagination.Size).Take(pagination.Size);
            }
            pagination.All = _list.Count();
            pagination.Count = pagination.All % pagination.Size == 0 ? (pagination.All / pagination.Size) : ((pagination.All / pagination.Size) + 1);
            return Task.Run(() => { return _list; });
        }

        public virtual Task<IQueryable<TObject>> FilterAsync(Expression<Func<TObject, bool>> filter, ref Pagination pagination)
        {
            if (pagination == null) pagination = new Pagination();
            var _list = filter != null ? DbSet.Where(filter).AsQueryable() : DbSet.AsQueryable();
            if (pagination.Grep != null && pagination.Grep.Count > 0)
            {
                //_list = _SEARCH(_list, pagination.Grep);
                _list = MYSEARCH(_list, pagination.Grep);
            }
            JObject sorts = new JObject();
            if (string.IsNullOrEmpty(pagination.Sort))
            {
                sorts.Add("id", "asc");
            }
            else
            {
                sorts = JsonConvert.DeserializeObject<JObject>(pagination.Sort);
            }
            JToken tokens = sorts;// pagination.Sort;
            var flag = false;
            IOrderedQueryable<TObject> _orderlist = null;
            foreach (JProperty opt in tokens)
            {
                var property = typeof(TObject).GetProperty(opt.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", opt.Name));

                //            var parameter = Expression.Parameter(property.DeclaringType, property.ToString());
                //            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                //            var orderByExp = Expression.Lambda(propertyAccess, parameter);
                //            MethodCallExpression exp = Expression.Call(
                //typeof(Queryable),
                //opt.Value.ToString() == "desc" ? "OrderByDescending" : "OrderBy",
                //new Type[] { typeof(TObject), property.PropertyType },
                //  _list.Expression,
                //Expression.Quote(orderByExp));
                if (opt.Value.ToString().Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    if (!flag)
                    {
                        //_orderlist = _list.OrderByDescending(x => property.Name);
                        _orderlist = _list.OrderByDescending(property.Name);
                        flag = true;
                    }
                    else
                    {
                        _orderlist = _orderlist.ThenByDescending(property.Name);
                    }
                }
                else
                {
                    if (!flag)
                    {
                        _orderlist = _list.OrderBy(property.Name);// _list.AsQueryable().Provider.CreateQuery<TObject>(exp) as IOrderedQueryable<TObject>; 
                        flag = true;
                    }
                    else
                    {
                        _orderlist = _orderlist.ThenBy(property.Name);
                    }
                }
                //property.SetValue(entity, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(opt.Value), property.PropertyType));
                _list = _orderlist;
            }
            if (!pagination.NoPaging)
            {

                _list = _list.Skip((pagination.Index - 1) * pagination.Size).Take(pagination.Size);
            }
            pagination.All = _list.Count();
            pagination.Count = pagination.All % pagination.Size == 0 ? (pagination.All / pagination.Size) : ((pagination.All / pagination.Size) + 1);
            return Task.Run(() => { return _list; });
        }


        public virtual async Task<int> UpdateAsync(TObject tobject)
        {
            var entry = context.Entry(tobject);
            //DbSet.Attach(tobject);
            entry.State = EntityState.Modified;
            if (!shareContext)
                return await context.SaveChangesAsync();
            return 0;
        }
        public virtual async Task<int> UpdateAsync(JObject option, params object[] id)
        {
            var entity = FindAsync(id);
            JToken tokens = option;
            foreach (JProperty opt in tokens)
            {
                if (opt.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;
                var property = entity.GetType().GetProperty(opt.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", opt.Name));
                if (property.PropertyType.IsAbstract || property.PropertyType.IsClass) continue;
                property.SetValue(entity, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(opt.Value), property.PropertyType));
            }

            if (!shareContext)
                return await context.SaveChangesAsync();
            return 0;

        }
        //public virtual Task<int> UpdateAsync(Expression<Func<TObject, bool>> predicate, JObject target)
        //{
        //    return FilterAsync(predicate).ContinueWith<TObject>(
        //         obj =>
        //         {
        //             JToken tokens = target;
        //             foreach (JProperty p in tokens)
        //             {
        //                 if (p.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;
        //                 var property = obj.GetType().GetProperty(p.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //                 if (property == null) throw new Exception(string.Format("未找到相应的属性名{0}!", p.Name));
        //                 property.SetValue(obj, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(p.Value), property.PropertyType));
        //             }
        //             if (!shareContext)
        //                 return context.SaveChangesAsync(); 
        //         });
        //}


        public virtual async Task<TObject> CreateAsync(TObject tobject)
        {
            var entry = DbSet.Add(tobject);
            if (!shareContext)
                await context.SaveChangesAsync();
            return entry;
        }

        public virtual async Task<IEnumerable<TObject>> CreateAsync(IEnumerable<TObject> tlist)
        {
            var list = DbSet.AddRange(tlist);
            if (!shareContext)
                await context.SaveChangesAsync();
            return list;
        }

        public virtual async Task<TObject> DeleteAsync(TObject tobject)
        {
            context.Entry(tobject);
            var entry = DbSet.Remove(tobject);
            if (!shareContext)
                await context.SaveChangesAsync();
            return entry;
        }
        #endregion
         
    }
}