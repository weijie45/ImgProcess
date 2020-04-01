using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Service.Function.Common
{
    public class AppConfig
    {
        public static string ThumbnailPixel;
        private static Dictionary<string, string> ConnDict = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().ToDictionary(x => x.Name, x => x.ConnectionString);
        //private static SettingsContext _settingsContext = null;

        // Constructors
        public AppConfig()
        {
           // _settingsContext = SettingsContext;
        }


        // Properties
        //public SettingsContext SettingsContext
        //{
        //    get
        //    {
        //        // Creat
        //        if (_settingsContext == null) {
        //            // Resolve
        //            _settingsContext = new Settings.SettingsContextModule().Create("Test");
        //            if (_settingsContext == null) throw new InvalidOperationException("_settingsContext=null");
        //        }

        //        // Return
        //        return _settingsContext;
        //    }
        //}

        //public static void Init()
        //{

        //    ThumbnailPixel = _settingsContext.GetValue("ThumbnailPixel");
        //}
    }
}
