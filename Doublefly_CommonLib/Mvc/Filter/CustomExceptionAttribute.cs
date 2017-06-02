using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mvc.Result;
using Utility.Exception;
using Utility.Helper;

namespace Mvc.Filter
{
    public class CustomExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var ex = filterContext.Exception;

            var message = ex.Message;

            const int code = -1;

            if (ex is CustomException)
            {
                LogHelper.WriteInfo(ex);
            }
            else
            {
                message = "操作失败";
                LogHelper.WriteError(ex);
            }
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new CustomJsonResult(code, message);
            }
            else
            {
                var context = new ContentResult()
                {
                    Content = message
                };

                filterContext.Result = context;
            }

            filterContext.ExceptionHandled = true;
        }
    }
}
