using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Dapper
{
    public class Configuration
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString;

        /// <summary>
        /// 单次插入最大数量（超过500后启用批量插入)
        /// </summary>
        public static int SingleMaxInsertCount = 1000;

        /// <summary>
        /// sqllog执行委托
        /// </summary>
        public static Action<string, object, long> DbLog = null;

        /// <summary>
        /// 业务日志委托
        /// </summary>
        public static Action<BusinessLog> BusinessLog = null;

        public static void Initialize(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
