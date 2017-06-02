using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extension
{
    /// <summary>
    /// DateTime扩展
    /// </summary>
    public static partial class BaseExtension
    {
        #region DateTime
        /// <summary>
        /// 取得某月的第一天
        /// </summary>
        /// <param name="datetime">要取得月份第一天的时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfMonth(this DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day);
        }

        /// <summary>
        /// 取得某月的最后一天
        /// </summary>
        /// <param name="datetime">要取得月份最后一天的时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(this DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime ConvertToDate(this string timeStamp)
        {
            var dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var lTime = long.Parse(timeStamp + "0000000");
            var toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static string GetTimeStamp(this System.DateTime time)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return ((long)(time - startTime).TotalSeconds).ToString();
        }

        /// <summary>
        /// 日期时间转换成指定格式字符串
        /// </summary>
        /// <param name="time"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToAdvancedString(this DateTime time, string format = "")
        {
            return time.Year < 1900 ? string.Empty : time.ToString(format);
        }

        #endregion
    }
}
