using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EmitMapper;
using Utility.Extension;
using Utility.Helper;

namespace Utility.Mapper.Adapters
{
    public static class EmitMapperAdapter
    {
        /// <summary>
        /// 实体转换(EmitMapper)
        /// </summary>
        /// <typeparam name="TFrom">源类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="from">源对象</param>
        /// <param name="mappingConfigurator">配置器</param>
        /// <returns>目标对象</returns>
        public static TTo Mapper<TFrom, TTo>(this TFrom from, IMappingConfigurator mappingConfigurator = null)
        {
            if (mappingConfigurator == null)
            {
                return ObjectMapperManager.DefaultInstance.GetMapper<TFrom, TTo>().Map(from);
            }
            else
            {
                return ObjectMapperManager.DefaultInstance.GetMapper<TFrom, TTo>(mappingConfigurator).Map(from);
            }
        }

        /// <summary>
        /// 实体克隆(EmitMapper)
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        public static TFrom Clone<TFrom>(this TFrom from)
        {
            return ObjectMapperManager.DefaultInstance.GetMapper<TFrom, TFrom>().Map(from);
        }

        /// <summary>
        /// 将指定的列mapper给实体对象
        /// </summary>
        /// <typeparam name="TEntity">实体对象类型</typeparam>
        /// <param name="fromEntity">来源实体</param>
        /// <param name="toEntity">需要转换的实体</param>
        /// <param name="propertySelector">包含的字段表达式</param>
        /// <returns></returns>
        public static TEntity MapperInclude<TEntity>(this TEntity fromEntity, TEntity toEntity, Expression<Func<TEntity, object>> propertySelector) where TEntity : class
        {
            if (propertySelector == null) return toEntity;

            var propertyList = ExpressionHelper.GetPropertyByExpress(propertySelector);

            if (propertyList == null) return toEntity;

            foreach (var name in fromEntity.GetPropertyList().Where(propertyList.Contains))
            {
                object value = fromEntity.GetType().GetProperty(name).GetValue(fromEntity, null);
                toEntity.GetType().GetProperty(name).SetValue(toEntity, value, null);
            }
            return toEntity;
        }

        /// <summary>
        /// mapper给实体对象排除指定字段
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fromEntity"></param>
        /// <param name="toEntity"></param>
        /// <param name="propertySelector">排除的字段表达式</param>
        /// <returns></returns>
        public static TEntity MapperExclude<TEntity>(this TEntity fromEntity, TEntity toEntity, Expression<Func<TEntity, object>> propertySelector) where TEntity : class
        {
            if (propertySelector == null) return toEntity;

            var propertyList = ExpressionHelper.GetPropertyByExpress(propertySelector);

            if (propertyList == null) return toEntity;

            foreach (var name in fromEntity.GetPropertyList().Where(item => !propertyList.Contains(item)))
            {
                object value = fromEntity.GetType().GetProperty(name).GetValue(fromEntity, null);
                toEntity.GetType().GetProperty(name).SetValue(toEntity, value, null);
            }
            return toEntity;
        }

        public static List<TToItem> MapperList<TFromItem, TToItem>(this IEnumerable<TFromItem> from, IMappingConfigurator mappingConfigurator = null)
            where TToItem : new()
            where TFromItem : new()
        {
            List<TToItem> list = null;
            var fromItems = @from as TFromItem[] ?? @from.ToArray();
            if (from != null && fromItems.Any())
            {
                list = fromItems.Select(fromItem => fromItem.Mapper<TFromItem, TToItem>()).ToList();
            }
            return list;
        }
    }
}
