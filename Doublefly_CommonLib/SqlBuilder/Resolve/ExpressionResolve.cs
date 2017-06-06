using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using SqlBuilder.Infrastructure;

namespace SqlBuilder.Resolve
{
    internal class ExpressionResolve
    {
        private EntityInfo des;
        private string paramPrefix;
        private ConcurrentDictionary<string, object> ParamValues = new ConcurrentDictionary<string, object>();

        public ExpressionResolve()
        {

        }

        public ExpressionResolve(EntityInfo des, string paramPrefix)
        {
            this.des = des;
            this.paramPrefix = paramPrefix;
        }

        /// <summary>
        /// 参数名称集合
        /// </summary>
        private IEnumerable<string> Parameters
        {
            get { return ParamValues.Keys.ToList(); }
        }

        public string BinaryExpressionHandler(Expression left, Expression right, ExpressionType type)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            const string needParKey = "=,>,<,>=,<=,<>";

            string leftPar = this.RouteExpressionHandler(left, false);
            string typeStr = this.ExpressionTypeCast(type);
            bool needPar = needParKey.IndexOf(typeStr, System.StringComparison.Ordinal) > -1;
            string rightPar = this.RouteExpressionHandler(right, needPar);
            bool or = leftPar.IndexOf('&') > -1 || leftPar.IndexOf('|') > -1 || rightPar.IndexOf('&') > -1 || rightPar.IndexOf('|') > -1;
            needPar = (needParKey.IndexOf(typeStr, System.StringComparison.Ordinal) > -1 && !or);
            string appendLeft = leftPar;
            if (left is MemberExpression)
            {
                var filed = des.Properties.FirstOrDefault(m => m.Field == leftPar);

                if (filed != null && !string.IsNullOrEmpty(filed.Column))
                {
                    appendLeft = filed.Column;
                }
            }

            sb.Append(appendLeft);

            var paramCount = Parameters.Count(p => p.StartsWith(paramPrefix + leftPar));
            var paramName = paramPrefix + leftPar + (paramCount > 0 ? paramCount + "" : "");

            if (needPar)
            {
                this.ParamValues.TryAdd(paramName, rightPar);
            }

            if (rightPar.ToUpper() == "NULL")
            {
                switch (typeStr)
                {
                    case "=":
                        rightPar = " IS NULL ";
                        break;
                    case "<>":
                        rightPar = " IS NOT NULL ";
                        break;
                }
            }
            else
            {
                sb.Append(typeStr);
                if (needPar)
                {
                    rightPar = paramName;
                }
            }
            sb.Append(rightPar);
            sb.Append(")");
            return sb.ToString();
        }

        public string RouteExpressionHandler(Expression exp, bool isRight = false)
        {
            string result = "";
            if (exp is BinaryExpression)
            {
                BinaryExpression be = (BinaryExpression)exp;
                result = this.BinaryExpressionHandler(be.Left, be.Right, be.NodeType);
            }
            else if (exp is MemberExpression)
            {
                MemberExpression mExp = (MemberExpression)exp;
                if (isRight)
                {
                    object obj = Expression.Lambda(mExp, new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]);
                    if (obj is Enum)
                    {
                        obj = (int)obj;
                    }
                    result = string.Concat(obj);
                }
                else
                {
                    result = mExp.Member.Name;
                }
            }
            else if (exp is NewArrayExpression)
            {
                NewArrayExpression naExp = (NewArrayExpression)exp;
                StringBuilder sb = new StringBuilder();
                foreach (Expression expression in naExp.Expressions)
                {
                    sb.AppendFormat(",{0}", this.RouteExpressionHandler(expression, false));
                }
                result = ((sb.Length == 0) ? "" : sb.Remove(0, 1).ToString());
            }
            else if (exp is MethodCallExpression)
            {
                if (isRight)
                {
                    result = string.Concat(Expression.Lambda(exp, new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]));
                }
                else
                {
                    MethodAnalyze methodAnalyze = new MethodAnalyze();
                    Dictionary<string, MethodHandler> dic = new Dictionary<string, MethodHandler>();

                    dic.Add("Contains", new MethodHandler(methodAnalyze.Contains));
                    dic.Add("EndsWith", new MethodHandler(methodAnalyze.EndsWith));
                    dic.Add("StartsWith", new MethodHandler(methodAnalyze.StartsWith));
                    MethodCallExpression mcExp = (MethodCallExpression)exp;
                    string methodName = mcExp.Method.Name;
                    if (!dic.ContainsKey(methodName))
                    {
                        throw new Exception("LamadaQuery不支持方法" + mcExp.Method.Name);
                    }
                    List<object> args = new List<object>();
                    string field;
                    if (mcExp.Object == null)
                    {
                        field = this.RouteExpressionHandler(mcExp.Arguments[0], false);
                    }
                    else
                    {
                        field = mcExp.Object.ToString().Split(new char[]
						{
							'.'
						})[1];

                        args.Add(Expression.Lambda(mcExp.Arguments[0], new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]));
                    }
                    if (mcExp.Arguments.Count > 1)
                    {
                        args.Add(Expression.Lambda(mcExp.Arguments[1], new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]));
                    }
                    if (mcExp.Arguments.Count > 2)
                    {
                        args.Add(Expression.Lambda(mcExp.Arguments[2], new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]));
                    }
                    if (mcExp.Arguments.Count > 3)
                    {
                        args.Add(Expression.Lambda(mcExp.Arguments[3], new ParameterExpression[0]).Compile().DynamicInvoke(new object[0]));
                    }

                    var paramCount = Parameters.Count(p => p.StartsWith(paramPrefix + methodName));

                    var paramName = paramPrefix + methodName + (paramCount > 0 ? paramCount + "" : "");

                    result = dic[methodName](field, paramName, new AddParameHandler(this.AddParame), args.ToArray());
                }
            }
            else if (exp is ConstantExpression)
            {
                ConstantExpression cExp = (ConstantExpression)exp;
                if (cExp.Value == null)
                {
                    result = "null";
                }
                else
                {
                    result = cExp.Value.ToString();
                }
            }
            else if (exp is UnaryExpression)
            {
                UnaryExpression ue = (UnaryExpression)exp;
                result = this.RouteExpressionHandler(ue.Operand, isRight);
            }
            else
            {
                result = "";
            }

            return result;
        }

        private void AddParame(string name, object value)
        {
            this.ParamValues.TryAdd(name, value);
        }
        public string ExpressionTypeCast(ExpressionType expType)
        {
            if (expType <= ExpressionType.LessThanOrEqual)
            {
                switch (expType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        {
                            string result = "+";
                            return result;
                        }
                    case ExpressionType.And:
                        {
                            string result = "&";
                            return result;
                        }
                    case ExpressionType.AndAlso:
                        {
                            string result = " AND ";
                            return result;
                        }
                    default:
                        switch (expType)
                        {
                            case ExpressionType.Divide:
                                {
                                    string result = "/";
                                    return result;
                                }
                            case ExpressionType.Equal:
                                {
                                    string result = "=";
                                    return result;
                                }
                            case ExpressionType.GreaterThan:
                                {
                                    string result = ">";
                                    return result;
                                }
                            case ExpressionType.GreaterThanOrEqual:
                                {
                                    string result = ">=";
                                    return result;
                                }
                            case ExpressionType.LessThan:
                                {
                                    string result = "<";
                                    return result;
                                }
                            case ExpressionType.LessThanOrEqual:
                                {
                                    string result = "<=";
                                    return result;
                                }
                        }
                        break;
                }
            }
            else
            {
                switch (expType)
                {
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                        {
                            string result = "*";
                            return result;
                        }
                    default:
                        switch (expType)
                        {
                            case ExpressionType.NotEqual:
                                {
                                    string result = "<>";
                                    return result;
                                }
                            case ExpressionType.Or:
                                {
                                    string result = "|";
                                    return result;
                                }
                            case ExpressionType.OrElse:
                                {
                                    string result = " OR ";
                                    return result;
                                }
                            case ExpressionType.Subtract:
                            case ExpressionType.SubtractChecked:
                                {
                                    string result = "-";
                                    return result;
                                }
                        }
                        break;
                }
            }
            throw new InvalidCastException("不支持的运算符");
        }

        /// <summary>
        /// 将表达式解析成SQL参数(查询条件)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Tuple<string, ConcurrentDictionary<string, object>> FormatExpression<TEntity>(EntityInfo entityInfo, string paramPrefix, Expression<Func<TEntity, bool>> expression, SqlType sqlType = SqlType.Select)
        {
            des = entityInfo;
            this.paramPrefix = paramPrefix;
            ParamValues = new ConcurrentDictionary<string, object>();

            string result = "";
            string delete = "";

            if (sqlType == SqlType.Page || sqlType == SqlType.Select || sqlType == SqlType.Count)
            {
                foreach (var item in entityInfo.Properties)
                {
                    if (item.CusAttribute.Any(ab => ab.ColumnType == ColumnTypeEnum.Delete))
                    {
                        ParamValues.TryAdd(string.Format("{0}{1}", this.paramPrefix, item.Field), "0");
                        delete = string.Format("{1} = {0}{2} AND ", this.paramPrefix, item.Column, item.Field);
                    }
                }
            }

            if (expression == null)
            {
                return new Tuple<string, ConcurrentDictionary<string, object>>(string.Format("{0}", delete.Replace("AND ", "")), ParamValues);
            }

            string condition;

            var body = expression.Body as BinaryExpression;

            if (body != null)
            {
                BinaryExpression be = body;
                condition = this.BinaryExpressionHandler(be.Left, be.Right, be.NodeType);
            }
            else
            {
                condition = this.RouteExpressionHandler(expression.Body, false);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                result = string.Format("({0} {1})", delete, condition);
            }

            return new Tuple<string, ConcurrentDictionary<string, object>>(result, ParamValues);
        }

        /// <summary>
        /// 解析表达式的字段并返回数据库列名
        /// </summary>
        /// <typeparam name="TEntity">实体对象</typeparam>
        /// <param name="keySelector">表达式树</param>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        public IEnumerable<PropertyDes> GetPropertyByExpress<TEntity>(EntityInfo entityInfo, Expression<Func<TEntity, object>> keySelector, SqlType sqlType = SqlType.Select)
        {
            List<PropertyDes> propertyList = new List<PropertyDes>();

            if (sqlType == SqlType.Update || sqlType == SqlType.Delete)
            {
                foreach (var item in entityInfo.Properties)
                {
                    if (item.CusAttribute.Any(ab => ab.ColumnType == ColumnTypeEnum.Update))
                    {
                        propertyList.Add(item);
                    }
                }
            }

            if (keySelector == null) return propertyList;

            if ((keySelector != null) && (!(keySelector.Body is NewExpression) || ((keySelector.Body as NewExpression).Members.Count == 0)))
            {
                throw new ArgumentNullException("keySelector 至少包含一个字段");
            }

            foreach (MemberInfo info in (keySelector.Body as NewExpression).Members)
            {
                if (propertyList.All(item => item.Field != info.Name))
                {
                    propertyList.Add(entityInfo.Properties.FirstOrDefault(m => m.Field == info.Name));
                }
            }

            return propertyList;
        }
    }
}
