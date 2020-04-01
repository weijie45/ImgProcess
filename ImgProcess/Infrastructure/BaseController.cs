using Log;
using Service.Function.Base;
using Service.Function.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace ImgProcess.Infrastructure
{
    public class BaseController : Controller
    {
        public string ErrMsg = "";
        // Fields
        private LogContext _logContext = null;

        public BaseController()
        {
            // Resolve
            _logContext = new Log.LogContextModule().Create("Test");
            if (_logContext == null) throw new InvalidOperationException("_logContextt=null");
        }

        #region base
        public string AreaName
        {
            get
            {
                return ControllerContext.RouteData.DataTokens["area"].FixNull();
            }
        }

        public string ControllerName
        {
            get
            {
                return ControllerContext.RouteData.Values["controller"].FixNull();
            }
        }

        public string ActionName
        {
            get
            {
                return ControllerContext.RouteData.Values["action"].FixNull();
            }
        }

        //protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        //{
        //    var routeData = requestContext.RouteData.Values;
        //    var list = new[] { "area", "controller" };
        //    ViewData["ControllerUrl"] = AppConfig.ApplicationPath + list.Where(x => routeData.ContainsKey(x)).Select(x => routeData[x].ToString()).ToArray().Join("/");

        //    return base.BeginExecute(requestContext, callback, state);
        //}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

        #region error
        public List<string> GetErrorListFromModelState(ModelStateDictionary modelState)
        {
            var query = from state in modelState.Values
                        from error in state.Errors
                        select error.ErrorMessage;

            var errorList = query.ToList();
            return errorList;
        }

        /// <summary>
        /// 攔截 exception 判斷是否為 ajax request or 其他.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
           

            var ex = filterContext.Exception;

            if (Debugger.IsAttached) {
                Debugger.Break();
            }
            if (ex is HttpAntiForgeryException) {
                var errorMsg = new ContentResult();
                errorMsg.Content = "無效的網站識別碼 !";
                filterContext.Result = errorMsg;
            } else {            

                _logContext.LogRepoistory.SysLog(ex);

                var isAjaxCall = string.Equals("XMLHttpRequest", filterContext.RequestContext.HttpContext.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);
                if (isAjaxCall) {
                    filterContext.Result = new JsonNetResult()
                    {
                        Data = new JsonResponseModel { Errors = new List<string>() { "發生例外狀況，請重整頁面後再嘗試，或通知服務人員。" }, Status = "500" }
                    };
                } else {
                    var data = new ViewDataDictionary();
                    data["ErrMsg"] = ex.Message;
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Error500.html",
                        ViewData = data
                    };
                }
            }
            filterContext.ExceptionHandled = true;
        }
        #endregion
    }
}