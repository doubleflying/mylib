using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Utility.Helper
{
    public class CookieHelper
    {
        private static readonly string Domain = ConfigHelper.GetValue("domain");

        /// <summary>
        /// 清除指定Cookie
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        public static void ClearCookie(string cookiename)
        {
            Set(cookiename, null, DateTime.Now.AddYears(-3));
        }

        /// <summary>
        /// 获取指定Cookie值
        /// </summary>
        /// <param name="cookiename">cookiename</param>
        /// <returns></returns>
        public static string Get(string cookiename)
        {
            if (HttpContext.Current == null)
            {
                LogHelper.WriteInfo("无法读取Cookie因为HttpContext.Current为空");
                return "";
            }

            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];

            string str = string.Empty;
            if (cookie != null)
            {
                str = HttpContext.Current.Server.UrlDecode(cookie.Value);
            }
            return str;
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename"></param>
        /// <param name="cookievalue"></param>
        /// <param name="minute">过期分钟（0表示当前会话有效）</param>
        public static void Set(string cookiename, string cookievalue, int minute)
        {
            Set(cookiename, cookievalue, minute > 0 ? DateTime.Now.AddMinutes(minute) : DateTime.Now.AddDays(-1));
        }

        public static void Set(string name, string value, string domain, DateTime expires)
        {
            HttpCookie cookie = new HttpCookie(name, value)
            {
                Domain = domain,
                Path = "/",
                Expires = expires
            };

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void Set(string name, string value, string domain)
        {
            DateTime expires = DateTime.Now.AddDays(1.0f);
            Set(name, value, domain, expires);
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename"></param>
        /// <param name="cookievalue"></param>
        public static void Set(string cookiename, string cookievalue)
        {
            Set(cookiename, cookievalue, DateTime.Now.AddDays(-1));
        }

        public static void Set(string cookiename, object cookievalue)
        {
            Set(cookiename, cookievalue.ToString(), DateTime.Now.AddDays(-1));
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename">cookie名</param>
        /// <param name="cookievalue">cookie值</param>
        /// <param name="expires">过期时间 DateTime</param>
        public static void Set(string cookiename, string cookievalue, DateTime expires)
        {
            if (HttpContext.Current == null)
            {
                LogHelper.WriteInfo("无法写入Cookie因为HttpContext.Current为空");
                return;
            }

            HttpCookie cookie = new HttpCookie(cookiename)
            {
                Value = HttpContext.Current.Server.UrlEncode(cookievalue),
                Domain = Domain,
                Path = "/"
            };

            if (expires > DateTime.Now)
            {
                cookie.Expires = expires;
            }

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 添加一个Cookie（24小时过期）
        /// </summary>
        /// <param name="cookiename"></param>
        /// <param name="cookievalue"></param>
        public static void Set(string cookiename, string cookievalue, bool httpOnly)
        {
            if (HttpContext.Current == null)
            {
                LogHelper.WriteInfo("无法写入Cookie因为HttpContext.Current为空");
                return;
            }
            //SetCookie(cookiename, cookievalue, DateTime.Now.AddDays(1.0));
            HttpCookie cookie = new HttpCookie(cookiename)
            {
                Value = HttpContext.Current.Server.UrlEncode(cookievalue),
                //Expires = expires,
                Domain = Domain,
                Path = "/",
                HttpOnly = httpOnly
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// 添加一个Cookie
        /// </summary>
        /// <param name="cookiename">cookie名</param>
        /// <param name="cookievalue">cookie值</param>
        /// <param name="expires">过期时间 DateTime</param>
        public static void Set(string cookiename, string cookievalue, DateTime expires, bool httpOnly)
        {
            if (HttpContext.Current == null)
            {
                LogHelper.WriteInfo("无法写入Cookie因为HttpContext.Current为空");
                return;
            }

            HttpCookie cookie = new HttpCookie(cookiename)
            {
                Value = HttpContext.Current.Server.UrlEncode(cookievalue),
                Expires = expires,
                Domain = Domain,
                Path = "/",
                HttpOnly = httpOnly
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}
