using Dapper;
using Service.DAL;
using Service.Function.Base;
using Service.Function.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Westwind.Web.Mvc;

namespace Service.Function.Common
{
    public class Page
    {
        public StringBuilder Sb = new StringBuilder();
        public string AreaID = "";

        public string Build()
        {
            return Uri.EscapeDataString(Sb.ToString());
        }

        /// <summary>
        /// 指定DOM範圍
        /// </summary>
        /// <param name="selector"></param>
        public void Area(string selector)
        {
            if (selector != "") {
                selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";
                AreaID = selector;
            }
        }

        /// <summary>
        /// 需勾選的Radio
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="status">空白: 不處理 / d: Disabled</param>
        /// <param name="val"></param>
        public void Radio(string selector, string status, string val)
        {
            if (selector != "") {
                selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";

                switch (status) {
                    case "d": // Disabled
                        Run($"$('{selector}', '{AreaID}').prop('disabled',true).prop('checked', false); $('{selector}[value={val}]', '{AreaID}').prop('checked', true )");
                        break;
                    default:
                        Run($"$('{selector}', '{AreaID}').prop('disabled',false).prop('checked', false); $('{selector}[value={val}]', '{AreaID}').prop('checked', true )");
                        break;
                }
            }
        }

        /// <summary>
        /// 需勾選的CheckBox
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="status">空白: 不處理 / d: Disabled</param>
        /// <param name="val">多選以逗號分隔</param>
        public void CheckBox(string selector, string status, string val)
        {
            if (selector != "") {
                selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";
                var tmp = selector;
                var valList = val.Split(',');
                if (val != "") {
                    selector = "";
                    for (var i = 0; i < valList.Length; i++) {
                        selector += $"{tmp}[value={valList[i]}],";
                    }
                    selector = selector.Substring(0, selector.Length - 1);
                }
                switch (status) {
                    case "d": // Disabled
                        Run($"$('{tmp}', '{AreaID}').prop('disabled',true).prop('checked', false); $('{selector}', '{AreaID}').prop('checked', true )");
                        break;
                    default:
                        Run($"$('{tmp}', '{AreaID}').prop('disabled',false).prop('checked', false); $('{selector}', '{AreaID}').prop('checked', true )");
                        break;
                }
            }
        }

        /// <summary>
        /// Input 設定
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="status">r: Readonly / d: Disabled / t: 純文字 / h: Hidden</param>
        /// <param name="val"></param>
        public void Input(string selector, string status, string val)
        {
            if (selector != "") {
                selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";
                switch (status) {
                    case "r": // ReadOnly
                        Run($"$('{selector}', '{AreaID}').prop('readonly',true).val('{val}')");
                        break;
                    case "d": // Disabled
                        Run($"$('{selector}', '{AreaID}').prop('disabled',true).val('{val}')");
                        break;
                    case "t": // Text
                        Run($"$('{selector}', '{AreaID}').after('<span>{val}</span>').remove()");
                        break;
                    case "h": // Hidden
                        Run($"$('{selector}', '{AreaID}').attr('type','hidden').val('{val}')");
                        break;
                    default:
                        Run($"$('{selector}', '{AreaID}').prop('readonly',false).prop('disabled',false).val('{val}')");
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="isShow"></param>
        public void Toggle(string selector, bool isShow = true)
        {
            if (selector != "") {
                selector = new string[] { "[", "#" }.Contains(selector) ? selector : $"[name={selector}]";
                Run($"$('{selector}', '{AreaID}').{(isShow ? "show()" : "hide()")}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="dt"></param>
        /// <param name="initText"></param>
        /// <param name="initVal"></param>
        /// <param name="defVal"></param>
        /// <param name="dbKey"></param>
        public void Select(string selector, Dictionary<string, string> dt, string initText = "", string initVal = "", string defVal = "", string dbKey = "")
        {
            if (selector != "") {
                selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";
                var options = "";
                var drp = new Dictionary<string, string>();
                drp.Add(initVal, initText);

                drp = drp.Concat(dt).ToDictionary(d => d.Key, d => d.Value);

                foreach (var d in drp) {
                    options += $"<option value='{d.Key}' {(d.Key.FixNull() == defVal ? "selected" : "")}>{d.Value}</option>";
                }

                Run($"$('{selector}', '{AreaID}').html(\"{options}\")");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="sql"></param>
        /// <param name="initText"></param>
        /// <param name="initVal"></param>
        /// <param name="defVal"></param>
        /// <param name="dbKey"></param>
        public void Select(string selector, string sql, string initText = "", string initVal = "", string defVal = "", string dbKey = "")
        {
            if (sql != "" && selector != "") {
                using (var db = Db.Conn(dbKey)) {
                    selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";
                    var drp = db.Query<KeyValuePair<string, string>>(sql).ToDictionary(d => d.Key, d => d.Value);
                    Select(selector, drp, initText, initVal, defVal, dbKey);
                }
            }
        }

        /// <summary>
        /// Script執行
        /// </summary>
        /// <param name="script"></param>
        public void Run(string script)
        {
            if (script != "") {
                Sb.AppendLine('=' + script);
            }
        }

        /// <summary>
        /// Render View
        /// </summary>
        /// <param name="viewName">預設為 Action Name </param>
        /// <param name="model"></param>
        /// <returns></returns>
        public void View(string selector, string viewName = "", object model = null)
        {
            viewName = viewName == "" ? RouteData("action") : viewName;

            var ext = Path.GetExtension(viewName);
            viewName = ext == "" ? $"{viewName}.cshtml" : viewName;

            if (model.IsAnonymousType()) {
                model = model.ToModel();
            }
            
            Run($"$('{selector}').html( decodeURIComponent('{Uri.EscapeDataString(ViewRenderer.RenderPartialView($"~/Views/{RouteData("controller")}/{viewName}", model))}')).show() ");
        }

        /// <summary>
        ///  轉換為 Model
        /// </summary>
        /// <param name="obj">原始物件</param>
        /// <returns></returns>
        public ExpandoObject ToModel(object obj)
        {
            var dict = new RouteValueDictionary(obj);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in dict) {
                expando.Add(item);
            }
            return (ExpandoObject)expando;
        }

        /// <summary>
        /// 取得當前Action/Controller名稱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string RouteData(string type)
        {
            string requiredString = "";
            try {
                var routeData = HttpContext.Current.Request.RequestContext.RouteData;
                string str2 = type.ToLower();
                if (str2 != "area") {
                    if ((str2 != "controller") && (str2 != "action")) {
                        return requiredString;
                    }
                } else {
                    return routeData.Values[type].ToString();
                }
                requiredString = routeData.GetRequiredString(type);
            } catch {
            }
            return requiredString;
        }

        /// <summary>
        /// 返回參數錯誤訊息
        /// </summary>
        /// <returns></returns>
        public ActionResult TagsError(string errMsg)
        {
            return new JsonResult()
            {
                Data = new JsonResponseModel { Errors = new List<string>() { errMsg } }
            };
        }

        /// <summary>
        /// 取得 Shared 下的 view
        /// </summary>
        /// <param name="viewName">檔案名稱</param>
        /// <returns></returns>
        public string SharedView(string viewName)
        {
            return $"~/Views/Shared/{viewName}";
        }

    }
}
