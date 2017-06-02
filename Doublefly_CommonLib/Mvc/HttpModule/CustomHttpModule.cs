using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utility.Constant;
using Utility.Helper;

namespace Mvc.HttpModule
{
    internal class CustomHttpModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;
        }

        private static void context_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                var treeId = Guid.NewGuid();
                if (!HttpContext.Current.Items.Contains(VariableName.ContextKey))
                {
                    HttpContext.Current.Items.Add(VariableName.ContextKey, treeId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex);
            }
        }

        private static void context_EndRequest(object sender, EventArgs e)
        {

        }
    }
}
