using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class BusinessLog
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 业务主键ID
        /// </summary>
        public Guid BusinessId { get; set; }

        /// <summary>
        /// 执行sql
        /// </summary>
        public string BusinessSql { get; set; }

        /// <summary>
        /// 执行参数
        /// </summary>
        public string BusinessParameter { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifyBy { get; set; }

        /// <summary>
        /// 修改Ip地址
        /// </summary>
        public string ModifyIp { get; set; }

        /// <summary>
        /// 更新类型
        /// </summary>
        public string ChangeType
        {
            get
            {
                List<string> values = new List<string>();
                var lowerSql = BusinessSql.ToLower();
                if (lowerSql.Contains("update "))
                {
                    values.Add("update");
                }

                if (lowerSql.Contains("insert "))
                {
                    values.Add("insert");
                }

                if (lowerSql.Contains("delete "))
                {
                    values.Add("delete");
                }

                return string.Join("、", values);
            }
        }
    }
}
