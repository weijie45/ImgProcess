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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="startIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<Log> FindAllByLogDate(string startDate, string endDate, int startIndex, int pageSize);

        int CountAll(string startDate, string endDate);
    }
}
