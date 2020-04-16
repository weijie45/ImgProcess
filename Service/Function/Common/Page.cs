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
using System.Text.RegularExpressions;
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
            return escape(Sb.ToString());
        }

        /// <summary>
        /// 指定DOM範圍
        /// </summary>
        /// <param name="selector"></param>
        public void Area(string selector)
        {
            if (selector != "") Sb.AppendLine($"AreaId={selector}");
        }

        /// <summary>
        /// 需勾選的Radio
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="status">空白: 不處理 / d: Disabled</param>
        /// <param name="val"></param>
        public void Radio(string selector, string status, string val)
        {
            if (selector != "") Sb.AppendLine($"Radio={selector}|{val}|{status}");
            //if (selector != "") {
            //    selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";

            //    switch (status) {
            //        case "d": // Disabled
            //            Run($"$('{selector}', '{AreaID}').prop('disabled',true).prop('checked', false); $('{selector}[value={val}]', '{AreaID}').prop('checked', true )");
            //            break;
            //        default:
            //            Run($"$('{selector}', '{AreaID}').prop('disabled',false).prop('checked', false); $('{selector}[value={val}]', '{AreaID}').prop('checked', true )");
            //            break;
            //    }
            //}
        }

        /// <summary>
        /// 需勾選的CheckBox
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="status">空白: 不處理 / d: Disabled</param>
        /// <param name="val">多選以逗號分隔</param>
        public void CheckBox(string selector, string status, string val)
        {
            if (selector != "") Sb.AppendLine($"Radio={selector}|{val}|{status}");
            //if (selector != "") {
            //    selector = (selector.IndexOf("#") > -1 || selector.IndexOf("[") > -1) ? selector : $"[name={selector}]";
            //    var tmp = selector;
            //    var valList = val.Split(',');
            //    if (val != "") {
            //        selector = "";
            //        for (var i = 0; i < valList.Length; i++) {
            //            selector += $"{tmp}[value={valList[i]}],";
            //        }
            //        selector = selector.Substring(0, selector.Length - 1);
            //    }

            //    switch (status) {
            //        case "d": // Disabled
            //            Run($"$('{tmp}', '{AreaID}').prop('disabled',true).prop('checked', false); $('{selector}', '{AreaID}').prop('checked', true )");
            //            break;
            //        default:
            //            Run($"$('{tmp}', '{AreaID}').prop('disabled',false).prop('checked', false); $('{selector}', '{AreaID}').prop('checked', true )");
            //            break;
            //    }
            //}
        }

        /// <summary>
        /// Input 設定
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="status">r: Readonly / d: Disabled / t: 純文字 / h: Hidden</param>
        /// <param name="val"></param>
        public void Input(string selector, string status, string val)
        {
            if (selector != "") Sb.AppendLine($"Input={selector}|{val}|{status}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="isShow"></param>
        public void Toggle(string selector, bool isShow = true)
        {
            if (selector != "") Sb.AppendLine($"Toggle={selector}|{isShow}|");

            //if (selector != "") {
            //    selector = new string[] { "[", "#" }.Contains(selector) ? selector : $"[name={selector}]";
            //    Run($"$('{selector}', '{AreaID}').{(isShow ? "show()" : "hide()")}");
            //}
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
        public void Select(string selector, Dictionary<string, string> dt, string initText = "", string initVal = "", string defVal = "", string status = "")
        {
            var options = "";
            var drp = new Dictionary<string, string>();
            drp.Add(initVal, initText);
            drp = drp.Concat(dt).ToDictionary(d => d.Key, d => d.Value);

            foreach (var d in drp) {
                options += $"<option value='{d.Key}' {(d.Key.FixNull() == defVal ? "selected" : "")}>{d.Value}</option>";
            }

            //Run($"$('{selector}', '{AreaID}').html(\"{options}\")");

            if (selector != "") Sb.AppendLine($"Radio={selector}|{options}|{status}");

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

            var value = ViewRenderer.RenderPartialView($"~/Views/{RouteData("controller")}/{viewName}", model);

            Run($"$('{selector}').html( unescape('{escape(Regex.Replace((value + "").TrimEnd(), @"(\s*\n){3,}", "\n\n").TrimEnd('\n'))}')).show() ");
            //Run($"$('{selector}').html( decodeURIComponent('{Uri.EscapeDataString(ViewRenderer.RenderPartialView($"~/Views/{RouteData("controller")}/{viewName}", model))}')).show() ");
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

        /// <summary>
        /// 同 Javascript 的 escape() 編碼
        /// </summary>
        /// <param name="string">原始字串</param>
        /// <returns></returns>
        public static string escape(object @string)
        {
            string str = Convert.ToString(@string);
            int length = str.Length;
            var sb = new StringBuilder(length * 2);
            string str2 = "0123456789ABCDEF";
            int num3 = -1;
            while (++num3 < length) {
                char ch = str[num3];
                int num2 = ch;
                if ((((0x41 > num2) || (num2 > 90)) && ((0x61 > num2) || (num2 > 0x7a))) && ((0x30 > num2) || (num2 > 0x39))) {
                    switch (ch) {
                        case '@':
                        case '*':
                        case '_':
                        case '+':
                        case '-':
                        case '.':
                        case '/':
                            goto Label_0125;
                    }
                    sb.Append('%');
                    if (num2 < 0x100) {
                        sb.Append(str2[num2 / 0x10]);
                        ch = str2[num2 % 0x10];
                    } else {
                        sb.Append('u');
                        sb.Append(str2[(num2 >> 12) % 0x10]);
                        sb.Append(str2[(num2 >> 8) % 0x10]);
                        sb.Append(str2[(num2 >> 4) % 0x10]);
                        ch = str2[num2 % 0x10];
                    }
                }
            Label_0125:
                sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 同 Javascript 的 unescape() 解碼
        /// </summary>
        /// <param name="string">原始字串</param>
        /// <returns></returns>
        public static string unescape(object @string)
        {
            string str = Convert.ToString(@string);
            int length = str.Length;
            StringBuilder builder = new StringBuilder(length);
            int num6 = -1;
            while (++num6 < length) {
                char ch = str[num6];
                if (ch == '%') {
                    int num2;
                    int num3;
                    int num4;
                    int num5;
                    if (((((num6 + 5) < length) && (str[num6 + 1] == 'u')) && (((num2 = HexDigit(str[num6 + 2])) != -1) && ((num3 = HexDigit(str[num6 + 3])) != -1))) && (((num4 = HexDigit(str[num6 + 4])) != -1) && ((num5 = HexDigit(str[num6 + 5])) != -1))) {
                        ch = (char)((((num2 << 12) + (num3 << 8)) + (num4 << 4)) + num5);
                        num6 += 5;
                    } else if ((((num6 + 2) < length) && ((num2 = HexDigit(str[num6 + 1])) != -1)) && ((num3 = HexDigit(str[num6 + 2])) != -1)) {
                        ch = (char)((num2 << 4) + num3);
                        num6 += 2;
                    }
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        private static byte HexValue(char ch1, char ch2)
        {
            int num = 0;
            int num1 = HexDigit(ch1);
            if ((num1 < 0) || ((num = HexDigit(ch2)) < 0)) {
                throw new Exception();
            }
            return (byte)((num1 << 4) | num);
        }

        internal static int HexDigit(char c)
        {
            if ((c >= '0') && (c <= '9')) {
                return (c - '0');
            }
            if ((c >= 'A') && (c <= 'F')) {
                return (('\n' + c) - 0x41);
            }
            if ((c >= 'a') && (c <= 'f')) {
                return (('\n' + c) - 0x61);
            }
            return -1;
        }

    }
}
