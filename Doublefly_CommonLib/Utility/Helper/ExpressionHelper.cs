using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility.Common;

namespace Utility.Helper
{
    public static class ExpressionHelper
    {
        #region 私有方法

        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        #endregion


        /// <summary>
        /// 解析表达式的字段并返回数据库列名
        /// </summary>
        /// <typeparam name="TEntity">实体对象</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public static List<string> GetPropertyByExpress<TEntity>(Expression<Func<TEntity, object>> expression) where TEntity : class
        {
            List<string> propertyList = new List<string>();

            if (expression == null) return propertyList;

            if ((expression != null) && (!(expression.Body is NewExpression) || ((expression.Body as NewExpression).Members.Count == 0)))
            {
                return propertyList;
            }

            propertyList.AddRange((expression.Body as NewExpression).Members.Select(info => info.Name));

            return propertyList;
        }

        /// <summary>
        /// And合并多个表达式
        /// </summary>
        /// <param name="expressions">表达式集合</param>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> AndMerge<TEntity>(params Expression<Func<TEntity, bool>>[] expressions)
        {
            if (expressions.Length <= 1)
            {
                return expressions[0];
            }

            Expression<Func<TEntity, bool>> lambda = expressions[0];

            for (int index = 1; index < expressions.Length; index++)
            {
                lambda = lambda.And(expressions[index]);
            }

            return lambda;
        }

        /// <summary>
        /// Or合并多个表达式
        /// </summary>
        /// <param name="expressions">表达式集合</param>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> OrMerge<TEntity>(params Expression<Func<TEntity, bool>>[] expressions)
        {
            if (expressions.Length <= 1)
            {
                return expressions[0];
            }

            Expression<Func<TEntity, bool>> lambda = expressions[0];

            for (int index = 1; index < expressions.Length; index++)
            {
                lambda = lambda.Or(expressions[index]);
            }

            return lambda;
        }

        /// <summary>
        /// 连接两个And表达式
        /// </summary>
        /// <param name="firstExpression">第一个表达式</param>
        /// <param name="secodExpression">第二个表达式</param>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> And<TEntity>(this Expression<Func<TEntity, bool>> firstExpression, Expression<Func<TEntity, bool>> secodExpression)
        {
            return firstExpression.Compose(secodExpression, Expression.AndAlso);
        }

        /// <summary>
        /// 连接两个Or表达式
        /// </summary>
        /// <param name="firstExpression">第一个表达式</param>
        /// <param name="secodExpression">第二个表达式</param>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public static Expression<Func<TEntity, bool>> Or<TEntity>(this Expression<Func<TEntity, bool>> firstExpression, Expression<Func<TEntity, bool>> secodExpression)
        {
            return firstExpression.Compose(secodExpression, Expression.OrElse);
        }

    }
}
