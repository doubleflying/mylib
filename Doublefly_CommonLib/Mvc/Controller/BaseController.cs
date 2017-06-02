using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mvc.Result;

namespace Mvc.Controller
{
    public class BaseController : System.Web.Mvc.Controller
    {
        #region protected方法
        protected ActionResult Success(object data = null)
        {
            return new CustomJsonResult(data);
        }

        protected ActionResult Fail(string message = "操作失败")
        {
            return new CustomJsonResult(0, message);
        }

        protected ActionResult AdvancedJson(bool isSuccess)
        {
            return new CustomJsonResult(isSuccess);
        }

        protected ActionResult AdvancedJson(object data)
        {
            return new CustomJsonResult(data);
        }

        protected ActionResult SimpleJson(object data)
        {
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 异步执行带一个参数的委托
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="param"></param>
        protected void AsyncExecute<T>(Action<T> action, T param)
        {
            Task.Factory.StartNew(a => action(param), param);
        }

        /// <summary>
        /// 异步执行带一个参数的委托
        /// </summary>
        /// <param name="action"></param>
        protected void AsyncExecute(Action action)
        {
            Task.Factory.StartNew(action);
        }
        #endregion
    }
}
