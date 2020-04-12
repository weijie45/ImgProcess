using Newtonsoft.Json;
using Service.Function.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Service.Function.Common
{
    public class TagsInfo
    {
        public bool Error = false; //判斷參數正確性
        public string ErrorMsg = "Error !";
        public Dictionary<string, string> Tags;

        /// <summary>
        /// 欄位資料集
        /// </summary>
        public string _tags
        {
            get
            {
                return null;
            }
            set
            {
                value = DecryptJs(HttpUtility.UrlDecode(value));

                if (value == "") {
                    Error = true;
                    ErrorMsg = "Value Is Empty !";
                }

                Tags = Dict(value);
            }
        }

        /// <summary>
        /// 轉換為 Key / Value 資料
        /// </summary>
        /// <param name="json">json 資料</param>
        /// <returns></returns>
        public Dictionary<string, string> Dict(string json)
        {
            var rtn = new Dictionary<string, string>();
            if (json == null || json == "") {
                return rtn;
            }
            try {
                var dt = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (dt.ContainsKey("m")) {
                    // 有設定Model
                    var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(dt["m"].ToString());
                    foreach (var m in model.Keys) {
                        var value = model[m].ToString();
                        switch (m) {

                        }
                    }
                }

                foreach (var a in dt.Where(w => w.Key != "m")) {
                    rtn[a.Key] = a.Value.FixNull();
                }
            } catch (Exception ex) {
                Error = true;
                if (ex.InnerException != null) {
                    if (ex.InnerException.InnerException == null) {
                        ErrorMsg = ex.InnerException.Message;
                    } else {
                        ErrorMsg = ex.InnerException.InnerException.Message;
                    }
                } else {
                    ErrorMsg = ex.Message;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 與前端 AES 互用, 使用 AES 解密
        /// </summary>
        /// <param name="encrypted_">AES 加密文字</param>
        /// <returns></returns>
        public static string DecryptJs(object encrypted_)
        {
            if (encrypted_ == null) {
                return "";
            }

            var encrypted = encrypted_.ToString().Replace(" ", "+");

            try {
                byte[] toEncryptArray = Convert.FromBase64String(encrypted);
                var key = Encoding.UTF8.GetBytes("HeHeHaHaWeiWei00");

                RijndaelManaged rm = new RijndaelManaged();
                rm.Key = key;
                rm.IV = key;
                rm.FeedbackSize = 128;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rm.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            } catch {
                return "";
            }
        }
    }
}
