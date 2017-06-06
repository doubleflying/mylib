using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Domain.Attributes;

namespace SqlBuilder.Infrastructure
{
    public class EntityInfo
    {
        public EntityInfo()
        {
            Properties = new List<PropertyDes>();
            ParamColumns = new Dictionary<SqlType, List<ParamColumnModel>>();
        }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// 对象的所有属性,只包含有映射关系的属性
        /// </summary>
        public IList<PropertyDes> Properties { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public Type ClassType { get; set; }

        /// <summary>
        /// 是否需要记录日志
        /// </summary>
        public bool IsLog { get; set; }

        public Dictionary<SqlType, List<ParamColumnModel>> ParamColumns { get; set; }
    }

    /// <summary>
    /// 转换实体属性描述,只包含有映射关系的属性
    /// </summary>
    public class PropertyDes
    {
        public PropertyDes()
        {
            CusAttribute = new List<BaseAttribute>();
        }

        /// <summary>
        /// 表列名
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// 属性字段名
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
        /// <summary>
        /// 映射特性
        /// </summary>
        public IEnumerable<BaseAttribute> CusAttribute { get; set; }
    }
}
