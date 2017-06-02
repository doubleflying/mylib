using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Utility.Extension
{
    /// <summary>
    /// String扩展
    /// </summary>
    public static partial class BaseExtension
    {
        /// <summary>
        /// 为空或者NULL
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string @string)
        {
            return string.IsNullOrEmpty(@string);
        }

        /// <summary>
        /// 不等于空
        /// </summary>
        /// <param name="@string">字符串</param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string @string)
        {
            return !string.IsNullOrEmpty(@string);
        }

        /// <summary>
        /// 不等于空（清空空格）
        /// </summary>
        /// <param name="@string">字符串</param>
        /// <returns></returns>
        public static bool IsNotNullOrEmptyByTrim(this string @string)
        {
            return @string != null && !string.IsNullOrEmpty(@string.Trim());
        }

        /// <summary>
        /// 是否是数字类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInt32(this string str)
        {
            int i;

            var flag = Int32.TryParse(str.Trim(), out i);

            return flag;
        }

        /// <summary>
        /// 是否Decimal类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDecimal(this string str)
        {
            var result = 0m;

            var flag = decimal.TryParse((str + "").Trim(), out result);

            return flag;
        }

        public static bool ToBoolean(this string str)
        {
            bool result;
            bool.TryParse(str.Trim(), out result);
            return result;
        }

        public static DateTime ToDatetime(this string str)
        {
            return Convert.ToDateTime(str);
        }

        public static bool IsInt64(this string str)
        {
            Int64 result;

            var flag = Int64.TryParse(str.Trim(), out result);

            return flag;
        }

        public static bool IsFloat(this string str)
        {
            float result;
            var flag = float.TryParse(str.Trim(), out result);

            return flag;
        }

        public static bool IsDouble(this string str)
        {
            double result;
            var flag = double.TryParse(str.Trim(), out result);

            return flag;
        }

        public static bool IsDatetime(this string str)
        {
            DateTime result;
            var flag = DateTime.TryParse(str.Trim(), out result);

            return flag;
        }

        public static bool IsBoolean(this string str)
        {
            bool result;
            var flag = bool.TryParse(str.Trim(), out result);

            return flag;
        }

        /// <summary>
        /// 检查是否为数字
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if [is u number] [the specified string]; otherwise, <c>false</c>.</returns>
        public static bool IsUNumber(this string str)
        {
            var rg = new Regex(@"^\d+");
            return rg.IsMatch(str);
        }

        /// <summary>
        /// 检测是否是正确的单电子邮箱格式
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if the specified string is email; otherwise, <c>false</c>.</returns>
        public static bool IsEmail(this string str)
        {
            //Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return Regex.IsMatch(str, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }

        /// <summary>
        /// 检测是否是正确的电话号码格式
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if the specified string is telephone; otherwise, <c>false</c>.</returns>
        public static bool IsTelephone(this string str)
        {

            return Regex.IsMatch(str, @"^(\d{3,4}[-_－—]{1}\d{5,8})?[-_－—]?\d{1,8}?$");

        }

        /// <summary>
        /// 检测是否是手机号码
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if the specified string is mobile; otherwise, <c>false</c>.</returns>
        public static bool IsMobile(this string str)
        {
            //Regex r = new Regex("^(13[0-9]|15[012356789]|18[02356789]|147)\\d{8}$");
            return Regex.IsMatch(str, @"^(13[0-9]|15[0-9]|14[0-9]|18[0-9]|17[0-9])\d{8}$");
        }

        /// <summary>
        /// 是否国内身份证号码位数是否正确
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns><c>true</c> if [is identity card] [the specified string]; otherwise, <c>false</c>.</returns>
        public static bool IsIdentityCard(this string str)
        {
            return Regex.IsMatch(str, @"^(13[0-9]|15[0|3|6|7|8|9]|18[8|9])\d{8}$");
        }

        /// <summary>
        /// 是否为中文
        /// </summary>
        /// <param name="str">验证字符串</param>
        /// <returns><c>true</c> if the specified string is chinese; otherwise, <c>false</c>.</returns>
        public static bool IsChinese(this string str)
        {
            var reg = new Regex("^[\u4e00-\u9fa5]$");

            return str.Any(t => reg.IsMatch(t.ToString()));
        }

        public static JObject ToJObject(this string str)
        {
            var result = JObject.Parse(str);
            return result;
        }

        /// <summary>
        /// 从Json字符串转化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="string"></param>
        /// <returns></returns>
        public static T ToObjectFromJson<T>(this string @string)
        {
            if (@string.IsNullOrEmpty())
            {
                return default(T);
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(@string);
            }
            catch
            {
                throw new JsonSerializationException();
            }
        }
    }
}
