using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extension
{
    public static partial class BaseExtension
    {
        #region List扩展
        /// <summary>
        /// List分页拆分成多个List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>List&lt;List&lt;T&gt;&gt;.</returns>
        public static List<List<T>> Paging<T>(this List<T> list, int pageSize)
        {
            var result = new List<List<T>>();
            var totalPage = Math.Ceiling(((decimal)list.Count) / ((decimal)pageSize));
            for (var i = 1; i <= totalPage; i++)
            {
                result.Add(
                    list.Skip((i - 1) * pageSize).Take(pageSize).ToList()
                    );
            }
            return result;
        }

        /// <summary>
        /// Joins the specified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns>System.String.</returns>
        public static string Join<T>(this IEnumerable<T> values, string seperator)
        {
            var sb = new StringBuilder();
            foreach (var value in values)
            {
                if (sb.Length > 0)
                    sb.Append(seperator);
                sb.Append(value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 连接实体泛型里面指定属性的所有值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="list">实体集合</param>
        /// <param name="propertyName">指定属性</param>
        /// <param name="seperator">分隔符号</param>
        /// <returns>System.String.</returns>
        public static string JoinString<T>(this List<T> list, string propertyName, string seperator)
        {
            var vals = new List<string>();

            foreach (var t in list)
            {
                var propertys = t.GetType().GetProperties();

                var t1 = t;
                vals.AddRange(from pi in propertys where pi.Name == propertyName select pi.GetValue(t1, null) into value where value != DBNull.Value select value.ToString());
            }
            var result = string.Join(seperator, vals.ToArray());

            return result;
        }

        /// <summary>
        /// Determines whether [is null or empty] [the specified list].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns><c>true</c> if [is null or empty] [the specified list]; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// 不为空的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty<T>(this List<T> list)
        {
            return list != null && list.Any();
        }

        #endregion
    }
}
