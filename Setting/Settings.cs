using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{

    //Constructors
    public class Settings
    {
        // Constructors
        public Settings(string key, string value = null)
        {
            #region Contracts

            if (string.IsNullOrEmpty(key) == true) throw new ArgumentException();

            #endregion

            // Default
            this.Key = key;
            this.Value = value;
        }


        // Properties
        public string Key { get; set; }

        public string Value { get; set; }
    }

    //public class Setting
    //{
    //    private static DateTime _Now = DateTime.Now;

    //    public int LogNo { get; set; }

    //    public string Controller { get; set; }

    //    public string Action { get; set; }

    //    public string Message { get; set; }

    //    public string StackTrace { get; set; }

    //    public DateTime LogDate { get; set; } = _Now;

    //}

    //public class SettingMapper : ClassMapper<Setting>
    //{
    //    public SettingMapper()
    //    {
    //        Table("SystemSetting");

    //        Map(p => p.LogNo).Key(KeyType.Identity);

    //        AutoMap();
    //    }
    //}

}
