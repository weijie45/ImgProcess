using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Function.Extensions
{
    public static class DictionaryExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static string GetValue(this Dictionary<string, string> dict, string key, string defVal = "")
        {
            if (dict == null) { return ""; }
            dict.TryGetValue(key, out string value);
            return string.IsNullOrEmpty(value) ? defVal : value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defVal"></param>
        /// <returns></returns>
        public static string GetValue(this Dictionary<string, object> dict, string key, string defVal = "")
        {
            if (dict == null) { return ""; }
            dict.TryGetValue(key, out object value);
            return string.IsNullOrEmpty(value.FixNull()) ? defVal : value.FixNull();
        }

    }
}
