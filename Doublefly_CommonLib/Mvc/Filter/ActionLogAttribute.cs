using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Utility.Constant;
using Utility.Extension;
using Utility.Helper;

namespace Mvc.Filter
{
    public class ActionLogAttribute : ActionFilterAttribute
    {
        readonly LogHelper _log = new LogHelper("actionLog");

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var treeId = HttpContext.Current.Items[VariableName.ContextKey].ToString();
                CallContext.LogicalSetData(VariableName.ContextKey, treeId);
                filterContext.HttpContext.Items[VariableName.StopwatchKey] = Stopwatch.StartNew();

                var actionName = string.Format("{0}/{1}", filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName);
                var ip = HttpContextHelper.GetIpAddress().PadRight(12, ' ');
                var httpType = HttpContextHelper.GetRequestType().PadRight(5, ' ');
                var msg = string.Format("[{1}]:{3}{2} {0}{4}", actionName, treeId
              , ip, httpType, filterContext.ActionParameters.ToJson());
                _log.Debug(msg);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var stopwatch = (Stopwatch)filterContext.HttpContext.Items[VariableName.StopwatchKey];
            stopwatch.Stop();

            var treeId = HttpContextHelper.GetContextKey();
            var msg = string.Format("[{0}]:耗时[{1}]毫秒 ", treeId
                , stopwatch.ElapsedMilliseconds);

            _log.Debug(msg);
        }
    }
}
