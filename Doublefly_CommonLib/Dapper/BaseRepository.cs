using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;

namespace Dapper
{
    public abstract class BaseRepository : WorkUnit, IRepository
    {
    }

    public class BaseRepository<TEntity> : BaseRepository, IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        private IDbConnection GetConnection()
        {
            connection = new SqlConnection(ConnectionString);

            return connection;
        }

        public TEntity GetSingle(Expression<Func<TEntity, bool>> predicate)
        {
            using (connection = GetConnection())
            {
                var result = connection.Select(null, predicate, topNumber: 1);

                return result.SingleOrDefault();
            }
        }

        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null)
        {
            using (connection = GetConnection())
            {
                var result = connection.Select(null, predicate, keySelector, topNumber, orderByTypes);

                return result;
            }
        }

        public PagingEntity<TEntity> GetPaging(PageParam page, Expression<Func<TEntity, bool>> predicate = null,
            Expression<Func<TEntity, object>> keySelector = null, Dictionary<string, OrderByTypeEnum> orderByTypes = null)
        {
            using (connection = GetConnection())
            {
                var data = connection.SelectPage(null, page, predicate, keySelector, orderByTypes);
                var result = connection.Count(null, predicate);
                return new PagingEntity<TEntity>()
                {
                    Data = data.ToList(),
                    Count = result
                };
            }
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            using (connection = GetConnection())
            {
                var result = connection.Count(null, predicate);
                return result;
            }
        }

        /// <summary>
        /// 支持事物的Insert操作
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public int Insert(params TEntity[] entitys)
        {
            foreach (var entity in entitys)
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity不能为空");
                }
                entity.SetInsertProperty();
            }
            if (entitys.Length > Configuration.SingleMaxInsertCount)
            {
                BulkInsert(entitys);
                return entitys.Length;
            }

            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Insert(entitys);
                    return result;
                }
            }
            else
            {
                var result = connection.Insert(entitys, transaction);
                return result;
            }
        }

        public int Update(TEntity entity, Expression<Func<TEntity, object>> keySelector = null, Expression<Func<TEntity, bool>> predicate = null)
        {
            if (!entity.HasValue())
            {
                throw new ArgumentNullException("entity不能为空或者Id不能为空");
            }
            entity.SetModifyProperty();

            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Update(new List<TEntity> { entity }, keySelector, predicate);
                    return result;
                }
            }
            else
            {
                var result = connection.Update(new List<TEntity> { entity }, keySelector, predicate, transaction);
                return result;
            }
        }

        public int Update(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> keySelector = null, Expression<Func<TEntity, bool>> predicate = null)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();

            foreach (var entity in enumerable)
            {
                if (!entity.HasValue())
                {
                    throw new ArgumentNullException("entity不能为空或者Id不能为空");
                }
                entity.SetModifyProperty();
            }

            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Update(enumerable, keySelector, predicate);
                    return result;
                }
            }
            else
            {
                var result = connection.Update(enumerable, keySelector, predicate, transaction);
                return result;
            }
        }

        public int Delete(params TEntity[] entitys)
        {
            if (entitys.Length == 0)
            {
                throw new ArgumentNullException("entitys不能为空或者Id不能为空");
            }
            foreach (var entity in entitys)
            {
                if (!entity.HasValue())
                {
                    throw new ArgumentNullException("entity不能为空或者Id不能为空");
                }
                entity.SetModifyProperty();
            }

            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Delete<TEntity>(entitys, null);
                    return result;
                }
            }
            else
            {
                var result = connection.Delete<TEntity>(entitys, null, transaction: transaction);
                return result;
            }
        }

        public int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Delete<TEntity>(null, predicate);
                    return result;
                }
            }
            else
            {
                var result = connection.Delete<TEntity>(null, predicate, transaction: transaction);
                return result;
            }
        }

        public int Delete(TEntity entitie, Expression<Func<TEntity, bool>> predicate)
        {
            var entities = new List<TEntity>() { entitie };

            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Delete<TEntity>(entities, predicate);
                    return result;
                }
            }
            else
            {
                var result = connection.Delete<TEntity>(entities, predicate, transaction: transaction);
                return result;
            }
        }

        public int Delete(IEnumerable<TEntity> entities, Expression<Func<TEntity, bool>> predicate)
        {
            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.Delete<TEntity>(entities, predicate);
                    return result;
                }
            }
            else
            {
                var result = connection.Delete<TEntity>(entities, predicate, transaction: transaction);
                return result;
            }
        }

        public int Execute(string sql, object param)
        {
            if (!base.IsTransaction)
            {
                using (connection = new SqlConnection(ConnectionString))
                {
                    var result = connection.ExecuteExt(sql, param, isExecuteSql: true);
                    return result;
                }
            }
            else
            {
                var result = connection.ExecuteExt(sql, param, transaction, isExecuteSql: true);
                return result;
            }
        }

        public int ExecuteScalar(string sql, object param)
        {
            using (connection = GetConnection())
            {
                var result = connection.ExecuteScalarExt<int>(sql, param);

                return result;
            }
        }

        public T ExecuteScalar<T>(string sql, object param)
        {
            using (connection = GetConnection())
            {
                var result = connection.ExecuteScalarExt<T>(sql, param);

                return result;
            }
        }

        public IEnumerable<TEntity> Query(string sql, object param)
        {
            using (connection = GetConnection())
            {
                var result = connection.QueryExt<TEntity>(sql, param);
                return result;
            }
        }

        public IEnumerable<T> Query<T>(string sql, object param)
        {
            using (connection = GetConnection())
            {
                var result = connection.QueryExt<T>(sql, param);
                return result;
            }
        }

        private string connectionString;

        public override string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }

                if (!string.IsNullOrEmpty(Configuration.ConnectionString))
                {
                    return Configuration.ConnectionString;
                }
                throw new ArgumentNullException("ConnectionString不能为空");
            }
            set { connectionString = value; }
        }

        /// <summary>
        /// 使用SqlBulkCopy批量插入数据
        /// </summary>
        public void BulkInsert(IEnumerable<TEntity> entities)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();

            if (entities == null || !enumerable.Any())
            {
                return;
            }

            var request = ToDataTable(enumerable);

            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();

                //事务锁
                SqlTransaction bulkTrans = conn.BeginTransaction();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, bulkTrans)
                {
                    BatchSize = 10000,
                    DestinationTableName = request.Item1
                })
                {
                    var dt = request.Item2;

                    if (dt == null)
                    {
                        return;
                    }
                    try
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                        }

                        bulkCopy.WriteToServer(dt);
                        bulkTrans.Commit();
                    }
                    catch (Exception ex)
                    {
                        bulkTrans.Rollback();

                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 将泛类型集合List类转换成DataTable 并返回表名
        /// </summary>
        /// <param name="entitys">泛类型集合</param>
        /// <returns></returns>
        private static Tuple<string, DataTable> ToDataTable<T>(IEnumerable<T> entitys)
        {
            var entityInfos = SqlBuilder.Configuration.EntityInfos;

            //检查实体集合不能为空
            var enumerable = entitys as T[] ?? entitys.ToArray();

            if (entitys == null || !enumerable.Any())
            {
                throw new Exception("需转换的集合为空");
            }

            var entityType = typeof(T);

            var entity2Table = entityInfos[entityType.FullName];

            DataTable dt = new DataTable();

            var propertyDeses = entity2Table.Properties;

            foreach (var p in propertyDeses)
            {
                dt.Columns.Add(p.Column, p.PropertyType);
            }

            foreach (var entity in enumerable)
            {
                object[] entityValues = new object[propertyDeses.Count];

                for (int i = 0; i < entityValues.Length; i++)
                {
                    entityValues[i] = propertyDeses[i].PropertyInfo.GetValue(entity, null);
                }

                dt.Rows.Add(entityValues);
            }

            return new Tuple<string, DataTable>(entity2Table.TableName, dt);
        }
    }
}
