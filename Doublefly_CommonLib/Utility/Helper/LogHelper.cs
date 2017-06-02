using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utility.Helper
{
    public class LogHelper
    {
        /// <summary>
        /// localAllLog本地版
        /// </summary>
        private readonly NLog.Logger _localLog;
        private readonly static LogHelper Errorlog = new LogHelper("error");
        private readonly static LogHelper InfoLog = new LogHelper("info");

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggerName"></param>
        /// <param name="isCurrent"></param>
        public LogHelper(string loggerName = "default", bool isCurrent = false)
        {
            _localLog = isCurrent ? NLog.LogManager.GetCurrentClassLogger() : NLog.LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// 写入提示日志
        /// </summary>
        /// <param name="content"></param>
        /// <param name="loggerName"></param>
        public static void WriteInfo(dynamic content, string loggerName = "info")
        {
            if (!loggerName.Equals("info"))
            {
                var log = new LogHelper(loggerName);
                log.Info(content);
            }
            else
            {
                InfoLog.Info(content);
            }
        }

        /// <summary>
        /// 写入错误提示
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteError(Exception ex)
        {
            Errorlog.Error(ex);
        }

        /// <summary>
        /// 写入提示信息
        /// </summary>
        public void Info(dynamic content)
        {
            _localLog.Info(BuildContent(content));
        }

        /// <summary>
        /// 写入调试信息
        /// </summary>
        public void Debug(dynamic content)
        {
            _localLog.Debug(BuildContent(content));
        }

        /// <summary>
        /// 写入异常
        /// </summary>
        /// <param name="ex"></param>
        public void Error(Exception ex)
        {
            _localLog.Error(ex);
        }

        /// <summary>
        /// 创建内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static dynamic BuildContent(dynamic content)
        {
            if (content.GetType().IsClass == true && content.GetType().Name != "String" && content.GetType().Name != "Int32")
            {
                return JsonConvert.SerializeObject(content);
            }

            return content;
        }
    }
}
