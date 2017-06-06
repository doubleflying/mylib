using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using Newtonsoft.Json;
using SqlBuilder;
using SqlBuilder.DataAdapter;
using SqlBuilder.Infrastructure;
using Utility.Helper;

namespace Dapper
{
    public static class DapperExtensions
    {
        private static void OnExecuted(string sql, object param, long time = 0)
        {
            var action = Configuration.DbLog;
            if (action != null)
            {
                Task.Factory.StartNew(() => action(sql, param, time));
            }
        }

        /// <summary>
        /// 执行业务日志记录
        /// </summary>
        /// <param name="logs"></param>
        private static void OnBusinessed(IEnumerable<BusinessLog> logs)
        {
            var action = Configuration.BusinessLog;
            if (action == null) return;

            var businessLogs = logs as BusinessLog[] ?? logs.ToArray();

            if (logs != null && businessLogs.Any() && action != null)
            {
                foreach (var businessLog in businessLogs)
                {
                    action(businessLog);
                }
            }
        }

        private static IEnumerable<BusinessLog> GetBusinessLog(string sql, object param, string ip)
        {
            var action = Configuration.BusinessLog;
            if (action == null) return null;

            List<BaseEntity> entities = new List<BaseEntity>();
            if (param is IEnumerable<object>)
            {
                entities = JsonConvert.DeserializeObject<List<BaseEntity>>(JsonConvert.SerializeObject(param));
            }
            else
            {
                entities.Add(JsonConvert.DeserializeObject<BaseEntity>(JsonConvert.SerializeObject(param)));
            }

            return GetBusinessLog<BaseEntity>(sql, entities, ip);
        }

        /// <summary>
        /// 根据sql获取当前业务操作记录对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static IEnumerable<BusinessLog> GetBusinessLog<TEntity>(string sql, IEnumerable<TEntity> param, string ip) where TEntity : BaseEntity
        {
            var action = Configuration.BusinessLog;
            if (action == null) yield return null;

            var entityType = typeof(TEntity);

            foreach (var entity in param)
            {
                EntityInfo entityInfo;

                BusinessLog log = new BusinessLog()
                {
                    BusinessId = entity.Id,
                    BusinessSql = sql,
                    BusinessParameter = JsonConvert.SerializeObject(param),
                    ModifyIp = ip,
                    ModifyBy = entity.ModifyBy
                };

                if (entityType == typeof(BaseEntity))
                {
                    var newSql = sql.Replace("\r\n", "");

                    var matches = Regex.Matches(newSql, @"(?<=UPDATE).*?(?=SET)|(?<=INTO).*?(?=\()");

                    foreach (var v in matches)
                    {
                        log.TableName = v.ToString().Trim();

                        if (!SqlBuilder.Configuration.TableInfos.TryGetValue(log.TableName, out entityInfo)) continue;

                        if (entityInfo.IsLog)
                        {
                            yield return log;
                        }
                    }
                }

                if (!SqlBuilder.Configuration.EntityInfos.TryGetValue(entityType.FullName, out entityInfo))
                {
                    continue;//不需要执行
                }

                if (entityInfo == null || !entityInfo.IsLog)
                {
                    continue;//不需要执行
                }

                log.TableName = entityInfo.TableName;

                yield return log;
            }
        }

        public static ISqlAdapter GetSqlAdapter(this IDbConnection connection)
        {
            //string name = connection.GetType().Name.ToLower();
            ISqlAdapter adapter = new SqlServerAdapter();
            //Dictionary<string, ISqlAdapter> adapterDictionary
            //            = new Dictionary<string, ISqlAdapter>() {
            //    {
            //        "sqlconnection", new SqlServerAdapter()
            //    }
            //};

            //if (!adapterDictionary.TryGetValue(name, out adapter))
            //{
            //    throw new NotImplementedException(string.Format("Unknow sql connection '{0}'", name));
            //}
            return adapter;
        }

        private static int ExecuteExt<TEntity>(this IDbConnection connection, string sql, IEnumerable<TEntity> param, IDbTransaction transaction = null, int? commandTimeout = null)
            where TEntity : BaseEntity
        {
            var result = connection.ExecuteExt(sql, param, transaction, commandTimeout, isExecuteSql: false);

            if (result > 0)
            {
                var ip = HttpContextHelper.GetIpAddress();
                Task.Factory.StartNew(() =>
                {
                    var businessLogs = GetBusinessLog<TEntity>(sql, param, ip);
                    OnBusinessed(businessLogs);
                });
            }

            return result;
        }

        public static int ExecuteExt(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null, int? commandTimeout = null, bool isExecuteSql = false)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var result = connection.Execute(sql, param, transaction, commandTimeout);

                if (result > 0 && isExecuteSql)
                {
                    var ip = HttpContextHelper.GetIpAddress();
                    Task.Factory.StartNew(() =>
                    {
                        var businessLog = GetBusinessLog(sql, param, ip);
                        OnBusinessed(businessLog);
                    });
                }

                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }
        public static Task<int> ExecuteAsyncExt(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var result = connection.ExecuteAsync(sql, param, transaction, commandTimeout);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static T ExecuteScalarExt<T>(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var result = connection.ExecuteScalar<T>(sql, param, transaction, commandTimeout);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static Task<T> ExecuteScalarAsyncExt<T>(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var result = connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<T> QueryExt<T>(this IDbConnection connection, string sql, object param,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var result = connection.Query<T>(sql, param, transaction, buffered, commandTimeout);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }
        public static Task<IEnumerable<T>> QueryAsyncExt<T>(this IDbConnection connection, string sql, object param,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var result = connection.QueryAsync<T>(sql, param, transaction, commandTimeout);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<dynamic> QueryExt(this IDbConnection connection, string sql, object param = null,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var result = connection.Query<dynamic>(sql, param, transaction, buffered, commandTimeout);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<TReturn> QueryExt<TFirst, TSecond, TReturn>(this IDbConnection connection, string sql,
            Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null,
            bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var result = connection.Query<TFirst, TSecond, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<TReturn> QueryExt<TFirst, TSecond, TThird, TReturn>(this IDbConnection connection, string sql,
            Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null,
            bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var result = connection.Query<TFirst, TSecond, TThird, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<TReturn> QueryExt<TFirst, TSecond, TThird, TFourth, TReturn>(this IDbConnection connection,
            string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null,
            CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var result = connection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<TReturn> QueryExt<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            this IDbConnection connection, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {

                var result = connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<TReturn> QueryExt<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            this IDbConnection connection, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {

                var result = connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        public static IEnumerable<TReturn> QueryExt<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            this IDbConnection connection, string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null,
            CommandType? commandType = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {

                var result = connection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
                return result;
            }
            finally
            {
                sw.Stop();
                OnExecuted(sql, param, sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<TEntity>(this IDbConnection connection, IEnumerable<TEntity> param, IDbTransaction transaction = null, int? commandTimeout = null)
            where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetInsert<TEntity>();

            var result = ExecuteExt<TEntity>(connection, sql, param, transaction, commandTimeout);

            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="keySelector"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<TEntity>(this IDbConnection connection, IEnumerable<TEntity> param, Expression<Func<TEntity, object>> keySelector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetUpdate(predicate, keySelector);

            var result = connection.ExecuteExt<TEntity>(sql, param, transaction, commandTimeout);

            return result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete<TEntity>(this IDbConnection connection, IEnumerable<TEntity> param,
            Expression<Func<TEntity, bool>> predicate = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetDelete(predicate);

            var result = 0;

            if (param == null && predicate != null)
            {
                if (!adapter.ParamValues.ContainsKey("ModifyTime"))
                {
                    adapter.ParamValues.TryAdd("ModifyTime", DateTime.Now);
                }
                if (!adapter.ParamValues.ContainsKey("ModifyBy"))
                {
                    adapter.ParamValues.TryAdd("ModifyBy", HttpContextHelper.GetOperator());
                }

                result = connection.ExecuteExt(sql, adapter.ParamValues, transaction, commandTimeout, isExecuteSql: true);
            }
            else
            {
                result = connection.ExecuteExt(sql, param, transaction, commandTimeout);
            }

            return result;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="predicate"></param>
        /// <param name="keySelector"></param>
        /// <param name="topNumber"></param>
        /// <param name="orderByTypes"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> Select<TEntity>(this IDbConnection connection, object param,
            Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetSelect(predicate, keySelector, topNumber, orderByTypes);

            if (param == null)
            {
                param = adapter.ParamValues;
            }

            var result = connection.QueryExt<TEntity>(sql, param, transaction, true, commandTimeout);

            return result;
        }

        /// <summary>
        /// 异步查询列表
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="predicate"></param>
        /// <param name="keySelector"></param>
        /// <param name="topNumber"></param>
        /// <param name="orderByTypes"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<TEntity>> SelectAsync<TEntity>(this IDbConnection connection, object param,
            Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetSelect(predicate, keySelector, topNumber, orderByTypes);

            if (param == null)
            {
                param = adapter.ParamValues;
            }

            var result = connection.QueryAsyncExt<TEntity>(sql, param, transaction, commandTimeout);

            return result;
        }

        /// <summary>
        /// 查询记录数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Count<TEntity>(this IDbConnection connection, object param = null,
            Expression<Func<TEntity, bool>> predicate = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetCount(predicate);

            if (param == null)
            {
                param = adapter.ParamValues;
            }

            var result = connection.ExecuteScalarExt<int>(sql, param, transaction, commandTimeout);

            return result;
        }

        /// <summary>
        /// 异步查询记录数
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> CountAsync<TEntity>(this IDbConnection connection, object param = null,
            Expression<Func<TEntity, bool>> predicate = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetCount(predicate);

            if (param == null)
            {
                param = adapter.ParamValues;
            }

            var result = connection.ExecuteScalarAsyncExt<int>(sql, param, transaction, commandTimeout);

            return result;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="page"></param>
        /// <param name="orderByTypes"></param>
        /// <param name="predicate"></param>
        /// <param name="keySelector"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> SelectPage<TEntity>(this IDbConnection connection, object param, PageParam page,
            Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, Dictionary<string, OrderByTypeEnum> orderByTypes = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetPage(page, predicate, keySelector, orderByTypes);

            if (param == null)
            {
                param = adapter.ParamValues;
            }

            var result = connection.QueryExt<TEntity>(sql, param, transaction, true, commandTimeout);

            return result;
        }

        /// <summary>
        /// 异步查询分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="page"></param>
        /// <param name="orderByTypes"></param>
        /// <param name="predicate"></param>
        /// <param name="keySelector"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<IEnumerable<TEntity>> SelectPageAsync<TEntity>(this IDbConnection connection, object param, PageParam page,
            Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, Dictionary<string, OrderByTypeEnum> orderByTypes = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
           where TEntity : BaseEntity
        {
            ISqlAdapter adapter = GetSqlAdapter(connection);

            var sql = adapter.GetPage(page, predicate, keySelector, orderByTypes);

            if (param == null)
            {
                param = adapter.ParamValues;
            }

            var result = connection.QueryAsyncExt<TEntity>(sql, param, transaction, commandTimeout);

            return result;
        }
    }
}
