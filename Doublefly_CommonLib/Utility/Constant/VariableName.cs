using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Helper;

namespace Utility.Constant
{
    public static class VariableName
    {
        /// <summary>
        /// 上下文唯一Key
        /// </summary>
        public static string ContextKey = string.Format("{0}.{1}", ConfigHelper.GetValue("AppId"), "ContextKey");

        /// <summary>
        /// action请求统计名称
        /// </summary>
        public const string StopwatchKey = "action.log.stopwatch";

        /// <summary>
        /// 操作人Key
        /// </summary>
        public static string OperatorKey = string.Format("{0}.{1}", ConfigHelper.GetValue("AppId"), "Operator");
    }
}
