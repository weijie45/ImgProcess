using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public class SettingsContext : IDisposable
    {
        // Fields
        private readonly List<SettingsRepository> _settingRepositoryList = null;


        // Constructors
        public SettingsContext(List<SettingsRepository> settingRepositoryList)
        {
            #region Contracts

            if (settingRepositoryList == null) throw new ArgumentException();

            #endregion

            // Default
            _settingRepositoryList = settingRepositoryList;
        }

        public void Start()
        {
            // Nothing

        }

        public void Dispose()
        {
            // Nothing

        }


        // Methods
        public void SetValue(string key, string value = null)
        {
            #region Contracts

            if (string.IsNullOrEmpty(key) == true) throw new ArgumentException();

            #endregion

            // SettingRepositoryList
            foreach (var settingRepository in _settingRepositoryList) {
                // Remove
                settingRepository.Remove(key);

                // Add
                settingRepository.Add(new Settings(key, value));
            }
        }

        public string GetValue(string key)
        {
            #region Contracts

            if (string.IsNullOrEmpty(key) == true) throw new ArgumentException();

            #endregion

            // SettingRepositoryList
            foreach (var settingRepository in _settingRepositoryList) {
                // Setting
                var setting = settingRepository.FindByKey(key);
                if (setting != null) return setting.Value;
            }

            // Return
            return null;
        }
    }
}
