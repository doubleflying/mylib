using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Attribute
{
    /// <summary>
    /// 枚举详细说明
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class SummaryAttribute : System.Attribute
    {
        /// <summary>
        /// 说明内容
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// 代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="summary"></param>
        public SummaryAttribute(string summary)
        {
            this.Summary = summary;
        }
    }
}
