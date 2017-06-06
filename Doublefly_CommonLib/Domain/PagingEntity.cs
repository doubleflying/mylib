using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// 分页基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PagingEntity<TEntity>
    {
        public List<TEntity> Data { get; set; }
        public int Count { get; set; }
    }
}
