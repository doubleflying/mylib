using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain
{
    public interface IRepository
    {
        string ConnectionString { get; set; }
    }

    public interface IBaseRepository<TEntity> : IRepository
        where TEntity : BaseEntity
    {
        #region Query
        /// <summary>
        /// 获取单个实体对象
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns></returns>
        TEntity GetSingle(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 获取集合列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="keySelector">查询字段</param>
        /// <param name="topNumber">查询条数</param>
        /// <param name="orderByTypes">排序条件</param>
        /// <returns></returns>
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null);

        /// <summary>
        /// 获取分页信息(默认按创建时间降序）
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <param name="orderByTypes">排序字段</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="keySelector">查询字段</param>
        /// <returns></returns>
        PagingEntity<TEntity> GetPaging(PageParam page, Expression<Func<TEntity, bool>> predicate = null,
            Expression<Func<TEntity, object>> keySelector = null, Dictionary<string, OrderByTypeEnum> orderByTypes = null);

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="predicate">查询条件表达式</param>
        /// <returns></returns>
        int Count(Expression<Func<TEntity, bool>> predicate);
        #endregion
        #region Command
        /// <summary>
        /// 添加信息
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        int Insert(params TEntity[] entitys);

        /// <summary>
        /// 修改（默认根据主键修改）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="keySelector">指定修改字段（默认为全部字段）</param>
        /// <param name="predicate">修改条件（值必须包含在TEntity里面）</param>
        /// <returns></returns>
        int Update(TEntity entity, Expression<Func<TEntity, object>> keySelector = null, Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// 修改（默认根据主键修改）
        /// </summary>
        /// <param name="entitys">需要修改的实体对象集合</param>
        /// <param name="keySelector">指定修改字段（默认为全部字段）</param>
        /// <param name="predicate">修改条件（值必须包含在TEntity里面）</param>
        /// <returns></returns>
        int Update(IEnumerable<TEntity> entitys, Expression<Func<TEntity, object>> keySelector = null, Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// 删除（根据主键逻辑删除）
        /// </summary>
        /// <param name="entitys">实体对象</param>
        /// <returns></returns>
        int Delete(params TEntity[] entitys);

        /// <summary>
        ///  删除（根据指定条件逻辑删除）
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns></returns>
        int Delete(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        ///  删除（根据指定条件逻辑删除）
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns></returns>
        int Delete(TEntity entitie, Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        ///  删除（根据指定条件逻辑删除）
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns></returns>
        int Delete(IEnumerable<TEntity> entities, Expression<Func<TEntity, bool>> predicate);
        #endregion

        /// <summary>
        /// 执行指定的sql，返回受影响行数
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        int Execute(string sql, object param);

        /// <summary>
        /// 返回执行结果第一行第一列的值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        int ExecuteScalar(string sql, object param);

        /// <summary>
        /// 返回执行结果第一行第一列的值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        T ExecuteScalar<T>(string sql, object param);

        /// <summary>
        /// 根据指定的sql获取实体对象集合
        /// </summary>
        /// <param name="sql">查询sql</param>
        /// <param name="param">查询参数</param>
        /// <returns></returns>
        IEnumerable<TEntity> Query(string sql, object param);

        /// <summary>
        /// 根据指定的sql获取实体对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object param);
    }
}
