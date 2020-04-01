using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class UAParserModel
    {
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string EngineName { get; set; }
        public string EngineVersion { get; set; }
        public string OsName { get; set; }
        public string OsVersion { get; set; }
        public string DeviceType { get; set; }
        public string DeviceVendor { get; set; }
    }
}
