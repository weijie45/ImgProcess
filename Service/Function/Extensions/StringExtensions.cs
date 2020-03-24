using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service.Function.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 將 json 轉換為指定類型的資料
        /// </summary>
        /// <typeparam name="T">指定類型</typeparam>
        /// <param name="obj">json 資料</param>
        /// <returns></returns>
        public static T Json<T>(this string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

        /// <summary>
        /// 資料內是否包含指定的檢查字串
        /// </summary>
        /// <param name="obj">原資料</param>
        /// <param name="checkId">檢查字串集</param>
        /// <param name="isUpper">是否轉成大寫</param>
        /// <returns></returns>
        public static bool Contains(this string obj, string checkId, bool isUpper = false)
        {
            var value = isUpper ? obj.ToUpper() : obj;
            return checkId.Split(',').Any(x => value.IndexOf(x) != -1);
        }

        /// <summary>
        /// 補齊英數文字
        /// </summary>
        /// <param name="data">原資料</param>
        /// <param name="maxLen">最大長度</param>
        /// <param name="isPadRight">是否往右補齊空白</param>
        /// <returns></returns>
        public static string PadText(this string data, int maxLen, bool isPadRight = false)
        {
            if (data == "") {
                return new string(' ', maxLen);
            }
            var encBIG5 = Encoding.GetEncoding("BIG5");
            var org = encBIG5.GetBytes(data).Take(maxLen).ToArray();
            var append = new string(' ', maxLen - org.Length);
            return isPadRight ? encBIG5.GetString(org) + append : append + encBIG5.GetString(org);
        }

        /// <summary>
        /// 指定替換字串
        /// </summary>
        /// <typeparam name="string">字串</typeparam>
        /// <param name="value">原字串</param>
        /// <param name="start">起點</param>
        /// <param name="length">長度</param>
        /// <param name="criteria">替換字串</param>
        /// <returns></returns>
        public static string Mask(this string value, int start, int length, string criteria)
        {
            value = value.FixNull();
            string result = "";
            int end = start + length;
            if (value.Length < end) end = value.Length - 1;

            for (int i = 0; i < value.Length; i++) {
                if (i >= start && i < end) {
                    result = result + criteria;
                } else {
                    result = result + value.Substring(i, 1);
                }
            }

            return result;
        }

        public static int ToInt(this string value)
        {
            if (value.IsNumeric()) {
                return Convert.ToInt32(value);
            }
            return 0;
        }

        public static string Mid(this string s, int start, int length)
        {
            if (start <= 0 || length <= 0 || s == null) {
                return "";
            }
            int strLen = s.Length;
            if (start > strLen) {
                return "";
            }
            if ((start + length) > strLen) {
                return s.Substring(start - 1);
            }
            return s.Substring(start - 1, length);
        }

        /// <summary>
        /// Returns the last few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="s">the string to process/// <param name="length">Number of characters to return/// <returns></returns>
        public static string Right(this string s, int length)
        {
            length = Math.Max(length, 0);

            if (s.Length > length) {
                return s.Substring(s.Length - length, length);
            } else {
                return s;
            }
        }

        /// <summary>
        /// Returns the first few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="s">the string to process</param>
        /// <param name="length">Number of characters to return</param>
        /// <returns></returns>
        public static string Left(this string s, int length)
        {
            length = Math.Max(length, 0);

            if (s.Length > length) {
                return s.Substring(0, length);
            } else {
                return s;
            }
        }

        //public static string Left(this string str, int len)
        //{
        //    if ((len <= 0) || (str == null)) {
        //        return "";
        //    }
        //    if (len >= str.Length) {
        //        return str;
        //    }
        //    return str.Substring(0, len);
        //}

        //public static string Right(this string str, int len)
        //{
        //    if ((len <= 0) || (str == null)) {
        //        return "";
        //    }
        //    int strLen = str.Length;
        //    if (len >= strLen) {
        //        return str;
        //    }
        //    return str.Substring(strLen - len, len);
        //}

        public static string Format(this string s, params Object[] obj)
        {
            for (var i = 0; i < obj.Length; i++) {
                s = s.Replace("{" + i + "}", obj[i] + "");
            }
            return s;
        }

        /// <summary>
        /// 修正 SQL Injection
        /// </summary>
        /// <param name="value">欄位值</param>
        /// <returns></returns>
        public static string FixSql(this string value)
        {
            if (value == null) {
                return "";
            } else {
                return value.Replace("'", "''");
            }
        }

    }
}
