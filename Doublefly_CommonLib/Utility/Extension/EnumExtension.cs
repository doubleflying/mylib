using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.Attribute;

namespace Utility.Extension
{
    public static partial class BaseExtension
    {
        private static readonly ConcurrentDictionary<Enum, string> ConcurrentDictionary = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }
            return ConcurrentDictionary.GetOrAdd(enumValue, (key) =>
            {
                var type = key.GetType();
                var field = type.GetField(key.ToString());

                if (field == null)
                {
                    return string.Empty;
                }

                var descAttr = field.GetCustomAttribute<DescriptionAttribute>(false);

                return descAttr == null ? field.Name : descAttr.Description;
            });
        }

        /// <summary>
        /// 获取简要内容
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetSummary(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }
            return ConcurrentDictionary.GetOrAdd(enumValue, (key) =>
            {
                var type = key.GetType();
                var field = type.GetField(key.ToString());

                if (field == null)
                {
                    return string.Empty;
                }

                var descAttr = field.GetCustomAttribute<SummaryAttribute>(false);

                return descAttr == null ? field.Name : descAttr.Summary;
            });
        }

        /// <summary>
        /// 将枚举value和description转换成Key-Value字典集合
        /// </summary>
        /// <param name="enum">枚举类型</param>
        /// <returns></returns>
        public static Dictionary<int, string> ConvertDictionaryDescriptionValue(this Enum @enum)
        {
            var enumDic = new Dictionary<int, string>();

            var enumType = @enum.GetType();

            if (!enumType.IsEnum) return enumDic;
            foreach (var item in Enum.GetValues(enumType))
            {
                enumDic.Add((int)item, GetDescription((Enum)item));
            }

            return enumDic;
        }

        /// <summary>
        /// 将枚举转换成Key-Value字典集合
        /// </summary>
        /// <param name="enum">枚举类型</param>
        /// <returns></returns>
        public static Dictionary<int, string> ConvertDictionaryValue(this Enum @enum)
        {
            var enumDic = new Dictionary<int, string>();

            var enumType = @enum.GetType();

            if (!enumType.IsEnum) return enumDic;
            foreach (var item in Enum.GetValues(enumType))
            {
                enumDic.Add((int)item, item.ToString());
            }

            return enumDic;
        }

        /// <summary>
        /// 将枚举转换成Key-Value字典集合
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static Dictionary<int, string> ConvertDictionaryValue(Type enumType)
        {
            var enumDic = new Dictionary<int, string>();

            if (!enumType.IsEnum) return enumDic;
            foreach (var item in Enum.GetValues(enumType))
            {
                enumDic.Add((int)item, item.ToString());
            }

            return enumDic;
        }
    }
}
