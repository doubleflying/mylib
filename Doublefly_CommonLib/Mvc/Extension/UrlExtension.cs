using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility.Helper;

namespace Mvc.Extension
{
    public static class UrlExtension
    {
        private static readonly string WebsiteUrl = ConfigHelper.GetValue("WebUrl");
        private static readonly string ResourceModule = ConfigHelper.GetValue("ResourceModule");
        private static readonly string ResourceUrl = ConfigHelper.GetValue("ResourceUrl");
        private static readonly string ResourceVersion = ConfigHelper.GetValue("ResourceVersion");

        /// <summary>
        /// 获取静态资源路径
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fileName">文件名</param>
        /// <param name="module">模块</param>
        /// <param name="resourceVersion">文件的版本号</param>
        /// <returns></returns>
        public static string GetResourceUrl(this UrlHelper helper, string fileName, string module = "", string resourceVersion = "")
        {
            return GetResourceUrl(ResourceUrl, fileName, module, true, resourceVersion);
        }

        /// <summary>
        /// 获取指定路径的静态资源文件URL
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="filePath">静态资源文件路径</param>
        /// <param name="resourceVersion">静态资源版本</param>
        /// <returns></returns>
        public static string GetResourceUrlByPath(this UrlHelper helper, string filePath, string resourceVersion = "")
        {
            var version = !string.IsNullOrWhiteSpace(resourceVersion) ? resourceVersion : ResourceVersion;

            filePath += "?v=" + version;

            return filePath;
        }

        /// <summary>
        /// 获取静态资源路径
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fileName">文件名</param>
        /// <param name="module">模块</param>
        /// <returns></returns>
        public static string GetResourceUrlNoVersion(this UrlHelper helper, string fileName, string module = "")
        {
            return GetResourceUrl(ResourceUrl, fileName, module, false);
        }

        private static string GetResourceUrl(string baseUrl, string fileName, string module, bool isEnableVersion = true, string resourceVersion = "")
        {
            module = string.IsNullOrEmpty(module) ? ResourceModule : module;
            var resourceUrl = string.Format("{0}/{1}/{2}",
                baseUrl, module, fileName);

            if (!isEnableVersion) return resourceUrl;

            var version = !string.IsNullOrWhiteSpace(resourceVersion) ? resourceVersion : ResourceVersion;
            resourceUrl += "?v=" + version;

            return resourceUrl;
        }

        /// <summary>
        /// 获取带域名的url
        /// </summary>
        public static string ActionFullUrl(this UrlHelper helper, string actionName)
        {
            return ActionFullUrl(helper, actionName, null, null);
        }

        /// <summary>
        /// 获取带域名的url
        /// </summary>
        public static string ActionFullUrl(this UrlHelper helper, string actionName, string controllerName)
        {
            return ActionFullUrl(helper, actionName, controllerName, null);
        }

        /// <summary>
        /// 获取带域名的url
        /// </summary>
        public static string ActionFullUrl(this UrlHelper helper, string actionName, string controllerName, object routeValue)
        {
            var requestUrl = helper.RequestContext.HttpContext.Request.Url;
            var p = requestUrl != null ? requestUrl.Scheme : "http";
            return helper.Action(actionName, controllerName, routeValue, p);
        }

        /// <summary>
        /// 获取带域名的url
        /// </summary>
        public static string ActionFullUrl(this UrlHelper helper, string actionName, object routeValue)
        {
            return ActionFullUrl(helper, actionName, null, routeValue);
        }
    }
}
