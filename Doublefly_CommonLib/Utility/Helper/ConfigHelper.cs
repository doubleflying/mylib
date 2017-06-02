using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Helper
{
    public sealed class ConfigHelper
    {
        /// <summary>
        /// 获取AppSettings中的配置字符串信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            var value = string.Empty;
            var cacheKey = "AppSettings_" + key;
            var objModel = CacheHelper.GetCache(cacheKey);

            if (objModel != null) return objModel.ToString();

            try
            {
                objModel = ConfigurationManager.AppSettings[key];
                if (objModel != null)
                {
                    value = objModel.ToString();
                    CacheHelper.SetCache(cacheKey, objModel, DateTime.Now.AddMinutes(180), TimeSpan.Zero);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex);
            }

            return value;
        }
    }
}
