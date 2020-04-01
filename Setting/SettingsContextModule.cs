using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public class SettingsContextModule
    {

        // Methods
        public SettingsContext Create(string dbKey)
        {
            // Create
            var context = this.CreateContext(dbKey);
            if (context == null) throw new InvalidOperationException("context=null");

            // Return
            return context;
        }


        private SettingsContext CreateContext(string dbKey)
        {
            // SettingRepository
            var appConfigSettingRepository = new AppConfigSettingsRepository();
            //var sqlSettingRepository = new SettingImplement(dbKey);

            // SettingRepositoryList
            var settingRepositoryList = new List<SettingsRepository>()
            {                
                appConfigSettingRepository,
                new SettingsImplement(dbKey)
            };

            // Return
            return new SettingsContext(settingRepositoryList);
        }
    }

}
