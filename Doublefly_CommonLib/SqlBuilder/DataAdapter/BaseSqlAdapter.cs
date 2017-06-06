using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using Newtonsoft.Json;
using SqlBuilder.Infrastructure;
using SqlBuilder.Resolve;
using Utility.Helper;

namespace SqlBuilder.DataAdapter
{
    public abstract class BaseSqlAdapter : ISqlAdapter
    {
        //TODO 通过其他渠道缓存
        private static ConcurrentDictionary<string, Tuple<string, ConcurrentDictionary<string, object>>> sqlData = new ConcurrentDictionary<string, Tuple<string, ConcurrentDictionary<string, object>>>();//sql数据缓存
        internal EntityInfo EntityInfo;//处理的实体对象描述
        protected List<string> ExcColums;//指定操作列
        protected ConcurrentDictionary<string, string> OrderByColums = new ConcurrentDictionary<string, string>();//排序字段
        public ConcurrentDictionary<string, object> ParamValues { get; set; }//记录参数名和值
        readonly ExpressionResolve expressionResolve = new ExpressionResolve();
        protected string WhereSql = "";//SQL字符串,只表示包括Where部分 用于动态拼接
        protected string OrderBySql = "";//排序部分字段
        protected SqlType SqlType { get; set; }//获取字段类型
        protected int TopNumber { get; set; }//top记录条数
        protected readonly EntityResolve er;
        protected BaseSqlAdapter()
        {
            er = new EntityResolve();
        }

        #region 属性/方法
        private void InitEntityInfo<TEntity>(SqlType sqlType, int topNumber)
        {
            this.EntityInfo = er.GetEntityInfo<TEntity>();
            this.ExcColums = new List<string>();
            this.OrderByColums = new ConcurrentDictionary<string, string>();
            this.ParamValues = new ConcurrentDictionary<string, object>();
            WhereSql = " WHERE 1=1 ";
            SqlType = sqlType;
            TopNumber = topNumber;
            OrderBySql = "";
        }

        /// <summary>
        /// 根据操作类型获取所有字段集合
        /// </summary>
        protected List<ParamColumnModel> GetParamColumnModels()
        {
            var colums = this.EntityInfo.ParamColumns[this.SqlType];

            return colums;
        }

        /// <summary>
        /// 查询指定的字段集合
        /// </summary>
        protected List<ParamColumnModel> SelectColumns
        {
            get
            {
                var colums = this.GetParamColumnModels();

                if (this.ExcColums != null && this.ExcColums.Count > 0)
                {
                    colums = colums.Where(m => this.ExcColums.Contains(m.ColumnName)).ToList();
                }

                return colums;
            }
        }

        /// <summary>
        /// 实体对象属性名集合
        /// </summary>
        protected IEnumerable<string> FieldNames
        {
            get
            {
                var cAll = this.GetParamColumnModels().Select(c => c.FieldName).ToList();

                return cAll;
            }
        }

        /// <summary>
        /// 实体对象属性对应的数据库字段集合
        /// </summary>
        protected IEnumerable<string> ColumnNames
        {
            get
            {
                var fAll = this.GetParamColumnModels().Select(c => c.ColumnName).ToList();
                return fAll;
            }
        }
        #endregion

        #region 生成SQL

        private string GetSqlByCache(string paramKey)
        {
            Tuple<string, ConcurrentDictionary<string, object>> value;

            if (sqlData.TryGetValue(paramKey, out value))
            {
                ParamValues = value.Item2;
                return value.Item1;
            }

            return null;
        }

        private void SetSqlCache(string paramKey, Tuple<string, ConcurrentDictionary<string, object>> value)
        {
            if (sqlData.Count > 10000)
            {
                sqlData = new ConcurrentDictionary<string, Tuple<string, ConcurrentDictionary<string, object>>>();
            }

            sqlData.TryAdd(paramKey, value);
        }

        public string GetInsert<TEntity>() where TEntity : BaseEntity
        {
            return this.Build<TEntity>(SqlType.Insert);
        }

        public string GetUpdate<TEntity>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null) where TEntity : BaseEntity
        {
            return this.Build(SqlType.Update, predicate, keySelector);
        }

        public string GetDelete<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : BaseEntity
        {
            return this.Build(SqlType.Delete, predicate);
        }

        public string GetSelect<TEntity>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null) where TEntity : BaseEntity
        {
            return this.Build(SqlType.Select, predicate, keySelector, topNumber, orderByTypes);
        }

        public string GetCount<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : BaseEntity
        {
            return this.Build(SqlType.Count, predicate);
        }

        public string GetPage<TEntity>(PageParam page, Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, Dictionary<string, OrderByTypeEnum> orderByTypes = null) where TEntity : BaseEntity
        {
            return this.Build(SqlType.Page, predicate, keySelector, orderByTypes: orderByTypes, page: page);
        }

        /// <summary>
        /// 创建SQL语句
        /// </summary>
        public string Build<TEntity>(SqlType type, Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> keySelector = null, int topNumber = 0,
            Dictionary<string, OrderByTypeEnum> orderByTypes = null, PageParam page = null) where TEntity : BaseEntity
        {
            var paramKey = EncryptHelper.MD5(JsonConvert.SerializeObject(new { entity = typeof(TEntity), type, predicate, keySelector, topNumber, orderByTypes, page }));

            var sql = this.GetSqlByCache(paramKey);

            if (!string.IsNullOrEmpty(sql))
            {
                return sql;
            }

            this.InitEntityInfo<TEntity>(type, topNumber);

            this.UpdateWhereSql(predicate);//更新where条件
            this.UpdateSelectSql(keySelector);//更新指定列信息
            this.UpdateOrderBySql(orderByTypes);//排序    

            switch (type)
            {
                case SqlType.Select:
                    sql = GetSelectSql();
                    break;
                case SqlType.Insert:
                    sql = GetInsertSql();
                    break;
                case SqlType.Update:
                    sql = GetUpdateSql();
                    break;
                case SqlType.Delete:
                    sql = GetDeleteSql();
                    break;
                case SqlType.Count:
                    sql = GetCountSql();
                    break;
                case SqlType.Page:
                    if (page == null)
                    {
                        throw new ArgumentNullException("PageParam不能为空");
                    }

                    if (orderByTypes == null || !orderByTypes.Any())
                    {
                        orderByTypes = BaseEntity.GetDefaultOrderBy();
                        this.UpdateOrderBySql(orderByTypes);//排序   
                    }
                    sql = GetPageSql(page);
                    break;
            }
            this.SetSqlCache(paramKey, new Tuple<string, ConcurrentDictionary<string, object>>(sql, ParamValues));

            return sql;
        }

        protected abstract string ParamPrefix { get; } //参数前缀

        /// <summary>
        /// 获取查询语句
        /// </summary>
        /// <returns></returns>
        protected abstract string GetSelectSql();

        /// <summary>
        /// 获取插入Sql语句
        /// </summary>
        /// <returns></returns>
        protected abstract string GetInsertSql();

        /// <summary>
        /// 获取更新Sql语句
        /// </summary>
        /// <returns></returns>
        protected abstract string GetUpdateSql();

        /// <summary>
        /// 获取删除Sql语句
        /// </summary>
        /// <returns></returns>
        protected abstract string GetDeleteSql();

        /// <summary>
        /// 获取计数Sql语句
        /// </summary>
        /// <returns></returns>
        protected abstract string GetCountSql();

        /// <summary>
        /// 获取分页Sql语句
        /// </summary>
        /// <returns></returns>
        protected abstract string GetPageSql(PageParam page);
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取查询字段及别名对应
        /// </summary>
        protected string GetSelectColumnSql()
        {
            var sqlColumn = new StringBuilder();

            for (int i = 0; i < this.SelectColumns.Count(); i++)
            {
                var paramColumnModel = this.SelectColumns[i];

                if (i != 0) sqlColumn.Append(",");

                sqlColumn.Append(paramColumnModel.ColumnName);

                if (paramColumnModel.ColumnName == paramColumnModel.FieldName) continue;

                sqlColumn.Append(" AS ");

                sqlColumn.Append(paramColumnModel.FieldName);
            }

            return sqlColumn.ToString();
        }

        /// <summary>
        /// 获取更新的字段
        /// </summary>
        /// <returns></returns>
        protected string GetUpdateColumnsSql()
        {
            var sql = new StringBuilder();

            var paramColumnModels = this.SelectColumns;

            for (int i = 0; i < paramColumnModels.Count; i++)
            {
                var paramColumnModel = paramColumnModels[i];
                if (i != 0) sql.Append(",");
                sql.Append(" ");
                sql.Append(paramColumnModel.ColumnName);
                sql.Append(" ");
                sql.Append("=");
                sql.Append(" ");
                sql.Append(this.ParamPrefix + paramColumnModel.FieldName);
            }

            return sql.ToString();
        }

        /// <summary>
        /// 根据表达式获取Where条件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private void UpdateWhereSql<TEntity>(Expression<Func<TEntity, bool>> predicate)
        {
            var result = expressionResolve.FormatExpression(this.EntityInfo, this.ParamPrefix, predicate, SqlType);

            if (!string.IsNullOrEmpty(result.Item1))
            {
                this.ParamValues = result.Item2;

                WhereSql = string.Format("{0} AND {1}", WhereSql, result.Item1);
            }
        }

        /// <summary>
        /// 更新选择字段信息 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        private void UpdateSelectSql<TEntity>(Expression<Func<TEntity, object>> keySelector)
        {
            if (keySelector == null && SqlType == Infrastructure.SqlType.Update)
            {
                return;
            }

            var propertyList = expressionResolve.GetPropertyByExpress(this.EntityInfo, keySelector, SqlType);
            var propertyDeses = propertyList as PropertyDes[] ?? propertyList.ToArray();

            if (propertyList == null || !propertyDeses.Any()) return;

            foreach (var result in propertyDeses.Select(p => p.Column).Where(result => result != null && (!this.ExcColums.Contains(result))))
            {
                this.ExcColums.Add(result);
            }
        }

        /// <summary>
        /// 更新排序sql
        /// </summary>
        /// <param name="orderByTypes"></param>
        private void UpdateOrderBySql(Dictionary<string, OrderByTypeEnum> orderByTypes)
        {
            if (orderByTypes == null) return;

            List<string> values = new List<string>();

            foreach (var result in orderByTypes)
            {
                this.OrderByColums.TryAdd(result.Key, result.Value.ToString());

                values.Add(string.Format("{0} {1}", result.Key, result.Value));
            }

            this.OrderBySql = string.Format(" ORDER BY {0}", string.Join(",", values));
        }
        /// <summary>
        /// 升序
        /// </summary>
        /// <typeparam name="TEntity">实体对象</typeparam>
        /// <param name="keySelector">要排序的表达式</param>
        /// <param name="orderByTypes">排序类型</param>
        /// <returns></returns>
        private void OrderBy<TEntity>(Expression<Func<TEntity, object>> keySelector, IEnumerable<OrderByTypeEnum> orderByTypes)
        {
            if (keySelector == null) return;

            var propertyList = expressionResolve.GetPropertyByExpress(this.EntityInfo, keySelector);

            var propertyDeses = propertyList as PropertyDes[] ?? propertyList.ToArray();

            if (propertyList == null || !propertyDeses.Any()) return;

            foreach (var result in propertyDeses)
            {
                if (result != null && !OrderByColums.ContainsKey(result.Column))
                {
                    this.OrderByColums.TryAdd(result.Column, "");
                }
            }
        }

        #endregion
    }
}
