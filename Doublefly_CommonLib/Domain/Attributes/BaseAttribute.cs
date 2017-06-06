using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Attributes
{
    public class BaseAttribute : Attribute
    {
        public BaseAttribute()
        {
            this.ColumnType = ColumnTypeEnum.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnType">列特殊类型</param>
        public BaseAttribute(ColumnTypeEnum columnType)
        {
            this.ColumnType = columnType;
        }

        /// <summary>
        /// 列特殊类型
        /// </summary>
        public ColumnTypeEnum ColumnType { get; set; }
    }
}
