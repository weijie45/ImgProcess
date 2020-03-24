using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Setting
{
    public class SettingContextModule
    {

        // Methods
        public SettingContext Create(string dbKey)
        {
            // Create
            var context = this.CreateContext(dbKey);
            if (context == null) throw new InvalidOperationException("context=null");

            // Return
            return context;
        }


        private SettingContext CreateContext(string dbKey)
        {
            // SettingRepository
            var appConfigSettingRepository = new AppConfigSettingRepository();
            //var sqlSettingRepository = new SettingImplement(dbKey);

            // SettingRepositoryList
            var settingRepositoryList = new List<SettingRepository>()
            {                
                appConfigSettingRepository,
                new SettingImplement(dbKey)
            };

            // Return
            return new SettingContext(settingRepositoryList);
        }
    }

}
