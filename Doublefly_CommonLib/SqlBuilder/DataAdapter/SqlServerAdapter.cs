using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace SqlBuilder.DataAdapter
{
    public class SqlServerAdapter : BaseSqlAdapter
    {
        protected override string ParamPrefix { get { return "@"; } }

        protected override string GetSelectSql()
        {
            StringBuilder sql = new StringBuilder();
            if (TopNumber > 0)
            {
                sql.AppendLine(string.Format("SELECT TOP {0} ", TopNumber));
            }
            else
            {
                sql.AppendLine("SELECT ");
            }
            sql.AppendLine(string.Format("{0}", base.GetSelectColumnSql()));
            sql.AppendLine(string.Format("FROM {0} ", base.EntityInfo.TableName));

            if (ParamValues.Any())
            {
                if (base.SqlType == Infrastructure.SqlType.Select && base.OrderByColums.Any())
                {
                    return string.Format("{0} {1} {2}", sql, base.WhereSql, base.OrderBySql);
                }

                return string.Format("{0} {1}", sql, base.WhereSql);
            }

            return sql.ToString();
        }

        protected override string GetInsertSql()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(string.Format("INSERT INTO {0} ", base.EntityInfo.TableName));

            sql.AppendLine(string.Format("({0})", string.Join(",", base.ColumnNames)));

            sql.AppendLine(" VALUES");

            sql.AppendLine(string.Format("({0}{1})", this.ParamPrefix, string.Join("," + this.ParamPrefix, base.FieldNames)));
            return sql.ToString();
        }

        protected override string GetUpdateSql()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(string.Format("UPDATE {0} SET ", base.EntityInfo.TableName));

            sql.AppendLine(base.GetUpdateColumnsSql());

            sql.AppendLine(string.Format(" {0}", WhereSql));

            if (ParamValues.Any()) return sql.ToString();

            var p = base.er.GetPrimary(base.EntityInfo);

            sql.AppendFormat(" AND {0}={1} ", p.Column, this.ParamPrefix + p.Field);

            return sql.ToString();
        }

        protected override string GetDeleteSql()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(string.Format("UPDATE {0} SET ", base.EntityInfo.TableName));

            sql.AppendLine(string.Format(" Disabled=1,{0} ", base.GetUpdateColumnsSql()));

            sql.AppendLine(string.Format(" {0} AND Disabled=0 ", WhereSql));

            if (ParamValues.Any()) return sql.ToString();

            var p = base.er.GetPrimary(base.EntityInfo);
            sql.AppendFormat(" AND {0}={1} ", p.Column, this.ParamPrefix + p.Field);

            return sql.ToString();
        }

        protected override string GetCountSql()
        {
            var result = string.Format("SELECT COUNT(1) FROM {0}", base.EntityInfo.TableName);

            if (!ParamValues.Any()) return result;

            return string.Format("{0} {1}", result, base.WhereSql);
        }

        protected override string GetPageSql(PageParam page)
        {
            var sql = string.Format(
                @"WITH    temp_list
          AS ( 
                {0}
             )
    SELECT  *
    FROM    ( SELECT    t.* ,
                        ROW_NUMBER() OVER ( {1} ) rn
              FROM      temp_list t
            ) t
    WHERE   t.rn BETWEEN @BeginRow AND @EndRow", this.GetSelectSql(), base.OrderBySql);

            base.ParamValues.TryAdd(string.Format("{0}BeginRow", this.ParamPrefix), page.BeginRow);
            base.ParamValues.TryAdd(string.Format("{0}EndRow", this.ParamPrefix), page.EndRow);
            return sql;
        }
    }
}
