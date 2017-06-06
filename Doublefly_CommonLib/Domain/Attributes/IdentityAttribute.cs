using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Attributes
{
    /// <summary>
    /// 自增长列
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IdentityAttribute : BaseAttribute
    {
        public IdentityAttribute()
        {
        }
    }
}
