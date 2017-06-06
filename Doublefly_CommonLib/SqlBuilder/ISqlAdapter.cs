using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;

namespace SqlBuilder
{
    public interface ISqlAdapter
    {
        /// <summary>
        /// 参数对象
        /// </summary>
        ConcurrentDictionary<string, object> ParamValues { get; set; }

        /// <summary>
        /// 获得Insert语句，包含整个实体字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        string GetInsert<TEntity>() where TEntity : BaseEntity;

        /// <summary>
        /// 获得指定字段生成获得Update语句
        /// 根据指定的where条件进行修改(默认根据主键ID进行修改)
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">where条件表达式</param>
        /// <param name="keySelector">查询字段表达式</param>
        /// <returns></returns>
        string GetUpdate<TEntity>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null) where TEntity : BaseEntity;

        /// <summary>
        /// 获得指定字段生成获得Delete语句
        /// 根据指定的where条件进行修改(默认根据主键ID进行修改)
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">where条件表达式</param>
        /// <returns></returns>
        string GetDelete<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : BaseEntity;

        /// <summary>
        /// 获取Select 查询语句
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">where条件表达式</param>
        /// <param name="keySelector">查询字段表达式</param>
        /// <param name="topNumber">指定条数</param>
        /// <param name="orderByTypes">请于排序对应数量的排序类型，未对应的将默认降序</param>
        /// <returns></returns>
        string GetSelect<TEntity>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null) where TEntity : BaseEntity;

        /// <summary>
        /// 获取 Count 查询语句
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="predicate">where条件表达式</param>
        /// <returns></returns>
        string GetCount<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : BaseEntity;

        /// <summary>
        /// 创建分页Sql语句
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="page">分页参数</param>
        /// <param name="predicate">where条件表达式</param>
        /// <param name="keySelector">查询字段表达式</param>
        /// <param name="orderByTypes">请于排序对应数量的排序类型，未对应的将默认降序</param>
        /// <returns></returns>
        string GetPage<TEntity>(PageParam page, Expression<Func<TEntity, bool>> predicate = null,
            Expression<Func<TEntity, object>> keySelector = null, Dictionary<string, OrderByTypeEnum> orderByTypes = null) where TEntity : BaseEntity;
    }
}
