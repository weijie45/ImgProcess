using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public class AppConfigSettingsRepository : SettingsRepository
    {
        // Fields
        private readonly object _syncRoot = new object();

        private readonly Dictionary<string, Settings> _settingDictionary = new Dictionary<string, Settings>();


        // Constructors
        public AppConfigSettingsRepository()
        {
            // AppSettings
            foreach (var key in ConfigurationManager.AppSettings.AllKeys) {
                // Require
                if (string.IsNullOrEmpty(key) == true) throw new InvalidOperationException("key=null");

                // AppSetting
                var appSettingKey = key.Trim();
                var appSettingValue = ConfigurationManager.AppSettings[key];
                var appSetting = new Settings(appSettingKey, appSettingValue);

                // Add
                _settingDictionary.Add(appSetting.Key, appSetting);
            }

            // ConnectionStrings
            foreach (var key in ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>()) {
                // Require
                if (string.IsNullOrEmpty(key.Name) == true) throw new InvalidOperationException("key=null");

                // AppSetting
                var appSettingKey = key.Name.Trim();
                var appSettingValue = key.ConnectionString;
                var appSetting = new Settings(appSettingKey, appSettingValue);

                // Add
                _settingDictionary.Add(appSetting.Key, appSetting);
            }
        }


        // Methods
        public void Add(Settings setting)
        {
            #region Contracts

            if (setting == null) throw new ArgumentException();

            #endregion

            // Sync
            lock (_syncRoot) {
                // Nothing

            }
        }

        public void Remove(string key)
        {
            #region Contracts

            if (string.IsNullOrEmpty(key) == true) throw new ArgumentException();

            #endregion

            // Sync
            lock (_syncRoot) {
                // Nothing

            }
        }

        public Settings FindByKey(string key)
        {
            #region Contracts

            if (string.IsNullOrEmpty(key) == true) throw new ArgumentException();

            #endregion

            // Sync
            lock (_syncRoot) {
                // Require
                if (_settingDictionary.ContainsKey(key) == false) return null;

                // Return
                return _settingDictionary[key];
            }
        }

        public List<Settings> FindAll()
        {
            // Sync
            lock (_syncRoot) {
                // Return
                return _settingDictionary.Values.ToList();
            }
        }
    }
}
