using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class EntityExtension
    {
        public static bool HasValue(this BaseEntity entity)
        {
            return entity != null && entity.Id != Guid.Empty;
        }

        public static bool HasValue(this List<BaseEntity> entitys)
        {
            return entitys != null && entitys.Any(entity => entity.Id != Guid.Empty);
        }
    }
}
