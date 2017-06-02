using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utility.Extension
{
    public static partial class BaseExtension
    {
        /// <summary>
        /// 转换成Int32
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">默认返回值</param>
        /// <returns></returns>
        public static int ToInt32(this object obj, int defaultValue = 0)
        {
            int result;

            if (!int.TryParse(obj.ToString(), out result))
            {
                result = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// 转换成Int64
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToInt64(this object obj, int defaultValue = 0)
        {
            long result;

            if (!long.TryParse(obj.ToString(), out result))
            {
                result = defaultValue;
            }
            return result;
        }

        /// <summary>
        /// 转换成Double
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double ToDouble(this object obj)
        {
            double result;

            if (obj == null)
            {
                return 0;
            }

            double.TryParse(obj.ToString(), out result);

            return result;
        }

        public static decimal ToDecimal(this string str)
        {
            decimal result;

            var flag = decimal.TryParse(str.Trim(), out result);

            return flag ? result : 0m;
        }


        /// <summary>
        /// 将字符串转成decimal 型。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal ConvertToDecimal(this object str)
        {
            if (null == str)
            {
                return 0;
            }
            try
            {
                var num = Convert.ToDecimal(str);

                return num;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 转换成Json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 链接所有属性的值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string JoinProperties(this object obj, string seperator = "")
        {
            var propertyInfos = obj.GetType().GetProperties();

            return string.Join(seperator, (from pi in propertyInfos let value = pi.GetValue(obj, null) where value != null select string.Format("{0}={1}", pi.Name, value)).ToArray());
        }

        /// <summary>
        /// 对象不能为空
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="msg"></param>
        public static void NotNullThrowException(this object obj, string msg = "对象不能为空")
        {
            if (obj == null || string.IsNullOrEmpty(obj + "")) throw new CustomAttributeFormatException(msg);
        }

        /// <summary>
        /// 对象是否为空
        /// </summary>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            return obj == null;
        }

        /// <summary>
        /// 对象是否不为空不为空
        /// </summary>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this object obj)
        {
            return obj != null;
        }

        #region decimal
        /// <summary>
        /// The dx number
        /// </summary>
        private static readonly String[] DxNum = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
        /// <summary>
        /// The dx yuan
        /// </summary>
        private static readonly String[] DxYuan = { "元", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "万" };
        /// <summary>
        /// The dw x
        /// </summary>
        private static readonly String[] DwX = { "角", "分" };

        /// <summary>
        /// 金额小写转中文大写。
        /// 整数支持到万亿；小数部分支持到分(超过两位将进行Banker舍入法处理)
        /// </summary>
        /// <param name="money">需要转换的双精度浮点数</param>
        /// <returns>转换后的字符串</returns>
        public static string ToMoneyString(this decimal money)
        {
            var numSrX = "";//小数部分
            var numStrR = "";//返回的字符串

            money = Math.Round(money, 2);//四舍五入取两位
            //各种非正常情况处理
            if (money < 0)
                return "转换失败";
            else if (money > 9999999999999.99m)
                return "金额过大，无法转换";
            else if (money == 0)
                return DxNum[0] + DxYuan[0];
            else
            {
                var numStr = money.ToString(CultureInfo.InvariantCulture);//整个数字字符串
                //分开整数与小数处理
                string numStrZh;//整数部分
                if (numStr.IndexOf(".", System.StringComparison.Ordinal) != -1)
                {
                    numStrZh = numStr.Substring(0, numStr.IndexOf(".", System.StringComparison.Ordinal));
                    numSrX = numStr.Substring(numStr.IndexOf(".", System.StringComparison.Ordinal) + 1);
                }
                else
                {
                    numStrZh = numStr;
                }
                //判断是否有整数部分
                string numStrDq;//当前的数字字符
                if (long.Parse(numStrZh) > 0)
                {
                    long len = numStrZh.Length - 1;
                    //整数部分转换
                    for (var a = 0; a <= len; a++)
                    {
                        numStrDq = numStrZh.Substring(a, 1);
                        if (long.Parse(numStrDq) != 0)
                        {
                            numStrR += DxNum[long.Parse(numStrDq)] + DxYuan[len - a];
                        }
                        else
                        {
                            if ((len - a) == 0 || (len - a) == 4 || (len - a) == 8)
                                numStrR += DxYuan[len - a];
                            if ((a + 1) > len) continue;
                            numStrDq = numStrZh.Substring((a + 1), 1);
                            if (long.Parse(numStrDq) == 0)
                                continue;
                            else
                                numStrR += DxNum[0];
                        }
                    }
                }
                //判断是否含有小数部分
                if (numSrX != "" && long.Parse(numSrX) > 0)
                {
                    //小数部分转换
                    for (var b = 0; b < numSrX.Length; b++)
                    {
                        numStrDq = numSrX.Substring(b, 1);
                        if (int.Parse(numStrDq) != 0)
                            numStrR += DxNum[long.Parse(numStrDq)] + DwX[b];
                        else
                        {
                            if ((b + 1) < numSrX.Length)
                            {
                                numStrDq = numSrX.Substring((b + 1), 1);
                                if (long.Parse(numStrDq) == 0)
                                    continue;
                            }
                            if (b != (numSrX.Length - 1))
                                numStrR += DxNum[0];
                        }
                    }
                }
                else
                {
                    numStrR += "整";
                }
                return numStrR;
            }
        }

        #endregion

        #region 实体对象扩展

        /// <summary>
        /// 得到包含对象所有属性的字符串列表
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        public static List<string> GetPropertyList(this object model)
        {
            return model.GetType().GetProperties().Select(t => t.Name).ToList();
        }

        /// <summary>
        /// 连接实体对象的所有属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns>System.String.</returns>
        public static string JoinStringAll<T>(this T obj, string seperator = "")
        {
            var propertys = obj.GetType().GetProperties();

            var result = string.Join(seperator, (from pi in propertys select pi.GetValue(obj, null) into value where value != null select value.ToString()).ToArray());

            return result;
        }

        /// <summary>
        /// 复制一个model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <returns>T.</returns>
        public static T Copy<T>(this T model) where T : class , new()
        {
            var result = new T();
            foreach (var p in model.GetPropertyList())
            {
                result.GetType().GetProperty(p).SetValue(result,
                    model.GetType().GetProperty(p).GetValue(model, null), null);
            }
            return result;
        }

        /// <summary>
        /// 将请求参数转换成字典集合
        /// </summary>
        /// <param name="model">实体对象</param>
        /// <param name="isFirstlowerCase">是否需要首字母小写</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetProperty<T>(this T model, bool isFirstlowerCase = false) where T : class ,new()
        {
            var parameters = model.GetPropertyList()
                .ToDictionary(
                item => isFirstlowerCase ? item.Substring(0, 1).ToLower() + item.Substring(1) : item,
                item => model.GetType().GetProperty(item).GetValue(model, null).ToString()
                );
            return parameters;
        }
        #endregion

        /// <summary>
        /// 返回是否默认选中
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetChecked(this bool flag)
        {
            return flag ? "checked=\"checked\"" : "";
        }

        /// <summary>
        /// 返回是否默认选中
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetSelected(this bool flag)
        {
            return flag ? "selected=\"selected\"" : "";
        }

        /// <summary>
        /// 根据int返回bool（1true，0false）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetBool(this int value)
        {
            return value == 1;
        }

        /// <summary>
        /// 根据int返回bool（1 =true = 是，0=false=否）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetBoolString(this int value)
        {
            return value == 1 ? "是" : "否";
        }

        /// <summary>
        /// 根据int返回bool（1 =true = 是，0=false=否）
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetBoolString(this bool flag)
        {
            return flag ? "是" : "否";
        }

        /// <summary>
        /// 当值为0的时候返回空
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToAdvancedString(this int value)
        {
            return value > 0 ? value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>
        /// 当值为0的时候返回空
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToAdvancedString(this long value)
        {
            return value > 0 ? value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>
        /// 当值为0的时候返回空
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToAdvancedString(this decimal value, string format = "")
        {
            return value > 0 ? value.ToString(format) : string.Empty;
        }

        /// <summary>
        /// 当值为0的时候返回空
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToAdvancedString(this double value, string format = "")
        {
            return value > 0 ? value.ToString(format) : string.Empty;
        }

        /// <summary>
        /// Guid默认值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToAdvancedString(this Guid value)
        {
            return value == Guid.Empty ? string.Empty : value.ToString();
        }
    }
}
