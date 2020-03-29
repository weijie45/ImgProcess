using DapperExtensions;
using Service.Function.Common;
using System;
using System.Data.SqlClient;
using static Log.LogContext;

namespace Log
{
    class LogImplement : LogRepository
    {
        // Fields
        private readonly string _connectionString = string.Empty;

        public LogImplement(string connectionString)
        {
            // Contracts

            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException();

            // Default
            _connectionString = connectionString;
        }

        public  void SysLog(Exception e)
        {
            using(var db = new SqlConnection(_connectionString)) {
                db.Open();
                var l = new Log();
                l.Message = e.Message;
                l.StackTrace = e.StackTrace;
                l.Action = Data.RouteData("Action");
                l.Controller = Data.RouteData("Controller");
                db.Insert(l);
            }
        }

        public void AddLog(string msg, string content)
        {
            using (var db = new SqlConnection(_connectionString)) {
                db.Open();
                var l = new Log();
                l.Message = msg;
                l.StackTrace = content;
                l.Action = Data.RouteData("Action");
                l.Controller = Data.RouteData("Controller");
                db.Insert(l);
            }
        }


    }
}
