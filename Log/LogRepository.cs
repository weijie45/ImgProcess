using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log
{
    public interface LogRepository
    {
        void SysLog(Exception e);

        void AddLog(string msg, string content);

        List<Log> FindAllByLogDate(string logDate);
    }
}
