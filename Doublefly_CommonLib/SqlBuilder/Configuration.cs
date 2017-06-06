using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Domain;
using SqlBuilder.Infrastructure;
using SqlBuilder.Resolve;

namespace SqlBuilder
{
    public class Configuration
    {
        /// <summary>
        /// 用于缓存对象转换实体
        /// </summary>
        public static readonly ConcurrentDictionary<string, EntityInfo> EntityInfos = new ConcurrentDictionary<string, EntityInfo>();
        public static readonly ConcurrentDictionary<string, EntityInfo> TableInfos = new ConcurrentDictionary<string, EntityInfo>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assembly">Domain程序集用于初始化加载所有的实体映射信息</param>
        public static void Initialize(Assembly assembly)
        {
            var types = assembly
                .GetTypes()
                .Where(t => t.BaseType != null && (t.BaseType.GUID == typeof(BaseEntity).GUID && !t.IsGenericType));
            EntityResolve er = new EntityResolve();

            foreach (var type in types)
            {
                var model = er.GetEntityInfo(type);

                AddEntityInfo(type.FullName, model);

                AddTableInfo(model.TableName, model);
            }
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="des"></param>
        private static void AddEntityInfo(string key, EntityInfo des)
        {
            if (!EntityInfos.ContainsKey(key) && des != null)
            {
                EntityInfos.TryAdd(key, des);
            }
        }

        private static void AddTableInfo(string key, EntityInfo des)
        {
            if (!TableInfos.ContainsKey(key) && des != null)
            {
                TableInfos.TryAdd(key, des);
            }
        }
    }
}
