using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utility.Constant;

namespace Utility.Helper
{
    public sealed class HttpContextHelper
    {
        /// <summary>
        /// 获取用户登陆IP
        /// </summary>
        /// <returns>返回用户IP</returns>
        public static string GetIpAddress()
        {
            string userIp;

            if (System.Web.HttpContext.Current == null) return "";

            if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
            {
                userIp = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"];
            }
            else if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                userIp = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            else
            {
                userIp = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return userIp;
        }

        /// <summary>
        /// 获取当前上下文的地址
        /// </summary>
        /// <returns>返回Url</returns>
        public static string GetUrl()
        {
            return System.Web.HttpContext.Current == null ? "" : System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
        }

        /// <summary>
        /// 获取客户端浏览器信息
        /// </summary>
        /// <returns></returns>
        public static string GetUserAgent()
        {
            return System.Web.HttpContext.Current == null ? "" : System.Web.HttpContext.Current.Request.UserAgent;
        }

        /// <summary>
        /// 获取用户请求类型
        /// </summary>
        /// <returns></returns>
        public static string GetRequestType()
        {
            return System.Web.HttpContext.Current == null ? "" : System.Web.HttpContext.Current.Request.RequestType;
        }

        /// <summary>
        /// 获取当前上下文的地址
        /// </summary>
        /// <returns>返回Url</returns>
        public static string GetUrlReferrer()
        {
            if (System.Web.HttpContext.Current == null) return "";

            if (System.Web.HttpContext.Current.Request.UrlReferrer != null)
            {
                return System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
            }

            return "";
        }

        /// <summary>
        /// 获取当前上下文的唯一标识
        /// </summary>
        /// <returns>返回Url</returns>
        public static string GetContextKey()
        {
            if (System.Web.HttpContext.Current == null) return "";

            if (HttpContext.Current.Items.Contains(VariableName.ContextKey))
            {
                return HttpContext.Current.Items[VariableName.ContextKey].ToString();
            }

            return "";
        }

        /// <summary>
        /// 获取常用的http上下文信息({唯一标识}|{请求类型}|{IP地址}|{URL}|{URLReferrer}|{UserAgent})
        /// </summary>
        /// <returns></returns>
        public static string GetOften()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", GetContextKey(), GetRequestType(), GetIpAddress(), GetUrl(), GetUrlReferrer(), GetUserAgent());
        }

        /// <summary>
        /// 返回操作人信息
        /// </summary>
        /// <returns></returns>
        public static string GetOperator()
        {
            if (System.Web.HttpContext.Current == null) return "";

            var result = CookieHelper.Get(VariableName.OperatorKey);

            return result;
        }

        /// <summary>
        /// 设置操作人
        /// </summary>
        public static void SetOperator(string operatorName)
        {
            if (System.Web.HttpContext.Current == null) throw new ArgumentNullException();

            CookieHelper.Set(VariableName.OperatorKey, operatorName, DateTime.Now.AddMinutes(5));
        }
    }
}
