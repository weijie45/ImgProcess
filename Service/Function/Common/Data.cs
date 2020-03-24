using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Service.Function.Common
{
    public class Data
    {
        /// <summary>
        /// 取得當前Action/Controller名稱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string RouteData(string type)
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
    }
}
