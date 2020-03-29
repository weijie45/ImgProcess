using Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log
{
    public class LogContextModule
    {
        public LogContext Create(string dbKey)
        {
            var context = this.CreateContext(dbKey);
            return context;
        }
        private LogContext CreateContext(string dbKey)
        {

            // Setting
            var settingContext = new Setting.SettingContextModule().Create(dbKey);

            var connString = settingContext.GetValue(dbKey);
            if (string.IsNullOrEmpty(connString) == true) throw new InvalidOperationException("connString=null");

            var logRepository = new LogImplement(connString);

            return new LogContext(logRepository);
        }
    }
}

