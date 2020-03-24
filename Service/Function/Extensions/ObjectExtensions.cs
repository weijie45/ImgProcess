using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace Service.Function.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 處理 Null 資料
        /// </summary>
        /// <param name="obj">原始資料</param>
        /// <returns></returns>
        public static string FixNull(this object obj, bool isTrim = true)
        {
            if (obj == null || obj == DBNull.Value) {
                return "";
            } else {
                return isTrim ? obj.ToString().Trim() : obj.ToString();
            }
        }

        /// <summary>
        /// 修正 SQL Injection
        /// </summary>
        /// <param name="value">欄位值</param>
        /// <returns></returns>
        public static string FixSql(this object value)
        {
            if (value == null) {
                return "";
            } else {
                return value.ToString().Replace("'", "''");
            }
        }

        /// <summary>
        /// 處理 Http Request 參數
        /// </summary>
        /// <param name="obj">參數</param>
        /// <returns></returns>
        public static string FixReq(this object obj)
        {
            if (obj == null) {
                return "";
            } else {
                return obj.ToString().Replace(" ", "+");
            }
        }

        /// <summary>
        /// 修正 Int 數值
        /// </summary>
        /// <param name="num">原始資料</param>
        /// <returns></returns>
        public static int FixInt(this object num)
        {
            if (IsNumeric(num)) {
                var value = num.ToString();
                if (!value.StartsWith("-")) {
                    value = "0" + value;
                }
                return Int32.Parse((value).Replace(",", "").Replace(" ", ""));
            } else {
                return 0;
            }
        }

        /// <summary>
        /// 修正 double 數值
        /// </summary>
        /// <param name="num">原始資料</param>
        /// <returns></returns>
        public static double FixNum(this object num)
        {
            if (IsNumeric(num)) {
                var value = num.ToString();
                if (!value.StartsWith("-")) {
                    value = "0" + value;
                }
                return double.Parse((value).Replace(",", "").Replace(" ", ""));
            } else {
                return 0;
            }
        }

        /// <summary>
        /// 判斷是否數值
        /// </summary>
        /// <param name="s">原始資料</param>
        /// <returns></returns>
        public static bool IsNumeric(this object s)
        {
            float output;
            return float.TryParse(s.FixNull().Replace(".", "").Replace(",", "").Replace(" ", ""), out output);
        }

        /// <summary>
        /// 判斷是否日期
        /// </summary>
        /// <param name="date">原始資料</param>
        /// <returns></returns>
        public static bool IsDate(this object date)
        {
            var tmp = Convert.ToString(date);
            var len = Regex.Replace(tmp, @"[年月日/\-]", "/").Trim('/').Split('/').Length;
            if (len >= 3) {
                DateTime retDate;
                return DateTime.TryParse(Convert.ToString(date), out retDate);
            } else {
                return false;
            }
        }

        /// <summary>
        /// 確認日期格式
        /// </summary>
        /// <param name="obj">原日期</param>
        /// <returns></returns>
        public static DateTime FixDate(this object obj)
        {
            DateTime date = DateTime.MinValue;
            if (obj != null && obj != DBNull.Value) {
                try {
                    date = DateTime.Parse(obj.ToString().Trim());
                } catch {
                }
            }
            return date;
        }

        /// <summary>
        /// 格式化民國日期
        /// </summary>
        /// <param name="obj">原始日期資料</param>
        /// <param name="useSlash">是否使用 / 分隔字元</param>
        /// <returns></returns>
        public static string ChineseDate(this object obj, bool useSlash = true, bool useTime = false)
        {
            var chineseDate = "---";
            if (obj == null) {
                return chineseDate;
            }
            var slash = useSlash ? "/" : "";
            var dateOrg = Regex.Replace(obj.FixNull(), @"[年月日/\-]", "/").Trim('/');
            int year = 0;
            string month = "", day = "";

            if (dateOrg.IsDate()) {
                var date = DateTime.Parse(dateOrg);
                if (date.Year > 0 && date.Month > 0 && date.Day > 0) {
                    chineseDate = $"{(date.Year > 1911 ? date.Year - 1911 : date.Year).ToString().PadLeft(3, '0')}{slash}{date.Month.ToString().PadLeft(2, '0')}{slash}{date.Day.ToString().PadLeft(2, '0')}{(useTime ? $" {date.ToString("HH:mm:ss")}" : "")}";
                }
            } else {
                var date = dateOrg;
                var trueDate = date.Replace(" ", "");
                if (date.IsNumeric()) {
                    switch (trueDate.Length) {
                        case 6:
                        case 7:
                            year = Int32.Parse(date.Left(trueDate.Length - 4));
                            if (year > 0) {
                                chineseDate = $"{year}{slash}{date.Mid(trueDate.Length - 3, 2)}{slash}{date.Right(2)}";
                            }
                            break;
                        case 8:
                            year = Int32.Parse(date.Left(4));
                            if (year > 0) {
                                chineseDate = $"{(year > 1911 ? year - 1911 : year).ToString().PadLeft(3, '0')}{slash}{date.Mid(5, 2)}{slash}{date.Right(2)}";
                            }
                            break;
                        default:
                            var tmp = Regex.Split(date, @"[/\- ]");
                            if (tmp[2].Length >= 3) {
                                year = tmp[2].FixInt();
                                month = tmp[0];
                                day = tmp[1];
                            } else {
                                year = tmp[0].FixInt();
                                month = tmp[1];
                                day = tmp[2];
                            }
                            if (year > 0) {
                                chineseDate = $"{(year > 1911 ? year - 1911 : year).ToString().PadLeft(3, '0')}{slash}{month.PadLeft(2, '0')}{slash}{day.PadLeft(2, '0')}";
                            }
                            break;
                    }
                } else {
                    var tmp = Regex.Split(date, @"[/\- ]");
                    if (tmp.Length == 3) {
                        if (tmp[2].Length >= 3) {
                            year = tmp[2].FixInt();
                            month = tmp[0];
                            day = tmp[1];
                        } else {
                            year = tmp[0].FixInt();
                            month = tmp[1];
                            day = tmp[2];
                        }
                    } else if (tmp.Length == 2) {
                        year = tmp[0].FixInt();
                        month = tmp[1];
                    }
                    if (year > 0) {
                        chineseDate = $"{(year > 1911 ? year - 1911 : year).ToString().PadLeft(3, '0')}{slash}{month.PadLeft(2, '0')}";
                    }
                }
            }
            return chineseDate;
        }

        /// <summary>
        /// 格式化西元日期
        /// </summary>
        /// <param name="obj">原始日期資料</param>
        /// <param name="useSlash">是否使用 / 分隔字元</param>
        /// <returns></returns>
        public static string EnglishDate(this object obj, bool useSlash = false)
        {
            var englishDate = "---";
            if (obj == null) {
                return englishDate;
            }
            var slash = useSlash ? "/" : "";
            var dateOrg = Regex.Replace(obj.FixNull(), @"[年月日/\-]", "/").Trim('/');

            if (dateOrg.Length == 14) {
                return $"{dateOrg.Mid(1, 4)}{slash}{dateOrg.Mid(5, 2)}{slash}{dateOrg.Mid(7, 2)} {dateOrg.Mid(9, 2)}:{dateOrg.Mid(11, 2)}:{dateOrg.Mid(13, 2)}";
            }

            if (dateOrg.IsDate()) {
                var date = DateTime.Parse(dateOrg);
                if (date.Year > 0) {
                    englishDate = $"{(date.Year < 1911 ? date.Year + 1911 : date.Year).ToString().PadLeft(4, '0')}{slash}{date.Month.ToString().PadLeft(2, '0')}{slash}{date.Day.ToString().PadLeft(2, '0')}";
                }
            } else {
                var date = dateOrg;
                if (date.IsNumeric()) {
                    if (date.Length == 8) {
                        var year = Int32.Parse(date.Left(4));
                        if (year > 0) {
                            englishDate = $"{(year < 1911 ? year + 1911 : year).ToString().PadLeft(4, '0')}{slash}{date.Mid(5, 2)}{slash}{date.Right(2)}";
                        }
                    } else if (date.Length == 6) {
                        if (date.IndexOf("/") != -1) {
                            var tmp = date.Split('/');
                            var year = tmp[0].FixInt();
                            if (year > 0) {
                                englishDate = $"{(year < 1911 ? year + 1911 : year).ToString().PadLeft(4, '0')}{slash}{tmp[1].PadLeft(2, '0')}";
                            }
                        } else {
                            var year = date.Left(4).FixInt();
                            if (year > 0) {
                                englishDate = $"{(year < 1911 ? year + 1911 : year).ToString().PadLeft(4, '0')}{slash}{date.Right(2)}";
                            }
                        }
                    }
                } else {
                    if (date.IndexOf("/") != -1) {
                        var tmp = date.Split('/');
                        var year = tmp[0].FixInt();
                        if (year > 0) {
                            englishDate = $"{(year < 1911 ? year + 1911 : year).ToString().PadLeft(4, '0')}{slash}{tmp[1].PadLeft(2, '0')}";
                        }
                    }
                }
            }
            return englishDate;
        }

        /// <summary>
        /// 轉換為 json 文字
        /// </summary>
        /// <param name="obj">原始資料</param>
        /// <param name="name">指定名稱</param>
        /// <returns></returns>
        public static string ToJson(this object obj, string name = "SN")
        {
            var json = JsonConvert.SerializeObject(obj);
            if (!json.StartsWith("{\"")) {
                json = $"{{\"{name}\":{json}}}";
            }
            return json;
        }

        /// <summary>
        /// 檢查是否為 Anonymous 類別
        /// </summary>
        /// <param name="obj">原始物件</param>
        /// <returns></returns>
        public static bool IsAnonymousType(this object obj)
        {
            if (obj == null) {
                return false;
            }
            var type = obj.GetType();
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false) && type.IsGenericType && type.Name.Contains("AnonymousType");
        }

        /// <summary>
        ///  轉換為 Model
        /// </summary>
        /// <param name="obj">原始物件</param>
        /// <returns></returns>
        public static ExpandoObject ToModel(this object obj)
        {
            var dict = new RouteValueDictionary(obj);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in dict) {
                expando.Add(item);
            }
            return (ExpandoObject)expando;
        }

    }
}
