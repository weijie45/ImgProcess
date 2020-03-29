using DapperExtensions.Mapper;
using System;

namespace Log
{

    public class Log
    {
        private static DateTime _Now = DateTime.Now;

        public int LogNo { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public DateTime LogDate { get; set; } = _Now;

    }

    public class LoglMapper : ClassMapper<Log>
    {
        public LoglMapper()
        {
            Table("ErrorLog");

            Map(p => p.LogNo).Key(KeyType.Identity);

            AutoMap();
        }
    }


}
