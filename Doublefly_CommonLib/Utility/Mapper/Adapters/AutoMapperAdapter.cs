using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Mapper.Adapters
{
    public static class AutoMapperAdapter
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assemblys"></param>
        public static void InitProfile(Assembly[] assemblys)
        {
            if (assemblys == null)
            {
                throw new ArgumentNullException("assembly 不能为空");
            }

            List<Type> memberMapses = new List<Type>();

            foreach (var memberMapses1 in assemblys.Select(assembly => assembly
                .GetTypes()
                .Where(t => t.BaseType != null && (t.BaseType.GUID == typeof(DefaultProfile<,>).GUID && !t.IsGenericType))
                .ToList()))
            {
                memberMapses.AddRange(memberMapses1);
            }

            AutoMapper.Mapper.Initialize(m => m.AddProfiles(memberMapses));
        }

        /// <summary>
        /// 类型转化通过AutoMapper进行转换(需要提前初始化关系映射）
        /// </summary>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">原类型值</param>
        /// <returns></returns>
        public static TTarget MapperA<TSource, TTarget>(this TSource source)
            where TSource : class
            where TTarget : class, new()
        {
            return AutoMapper.Mapper.Map<TSource, TTarget>(source);
        }

        /// <summary>
        /// 集合类型转化通过AutoMapper进行转换(需要提前初始化关系映射）
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static IEnumerable<TTarget> MapperList<TSource, TTarget>(this List<TSource> sources)
            where TSource : class
            where TTarget : class, new()
        {
            return sources.Select(AutoMapper.Mapper.Map<TSource, TTarget>);
        }

        /// <summary>
        /// 类型转化通过AutoMapper进行转换(需要提前初始化关系映射）
        /// </summary>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">原类型值</param>
        /// <returns></returns>
        public static TTarget Mapper<TTarget>(this object source) where TTarget : class, new()
        {
            return AutoMapper.Mapper.Map<TTarget>(source);
        }
    }
}
