using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Mvc.Result
{
    public class CustomJsonResult : ActionResult
    {
        #region 构造函数
        public CustomJsonResult()
        {
        }

        public CustomJsonResult(int code = 1, string message = "操作成功", object data = null)
        {
            Data = new CustomJsonModel()
            {
                Code = code,
                Message = message,
                Data = data,
                IsSuccess = code > 0
            };
        }

        public CustomJsonResult(object data)
        {
            Data = new CustomJsonModel()
            {
                Code = 1,
                Message = "操作成功",
                Data = data,
                IsSuccess = true
            };
        }
        public CustomJsonResult(bool isSuccess = true)
        {
            Data = new CustomJsonModel()
            {
                IsSuccess = isSuccess
            };

            if (isSuccess)
            {
                Data.Code = 1;
                Data.Message = "操作成功";
            }
            else
            {
                Data.Code = 0;
                Data.Message = "操作失败";
            }
        }
        #endregion

        internal CustomJsonModel Data { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (this.Data == null) return;
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";

            var jsonSetting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy/MM/dd HH:mm:ss",
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            response.Write(JsonConvert.SerializeObject(this.Data, jsonSetting));
        }
    }

    internal class CustomJsonModel
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public bool IsSuccess { get; set; }
    }
}
