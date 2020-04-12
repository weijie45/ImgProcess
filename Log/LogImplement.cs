using Dapper;
using DapperExtensions;
using Service.Function.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
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

        public void SysLog(Exception e)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress)) {
                using (var db = new SqlConnection(_connectionString)) {
                    var l = new Log();
                    l.Message = e.Message;
                    l.StackTrace = e.StackTrace;
                    l.Action = Data.RouteData("Action");
                    l.Controller = Data.RouteData("Controller");
                    db.Insert(l);
                }
            }
        }

        public void AddLog(string msg, string content)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress)) {
                using (var db = new SqlConnection(_connectionString)) {
                    var l = new Log();
                    l.Message = msg;
                    l.StackTrace = content;
                    l.Action = Data.RouteData("Action");
                    l.Controller = Data.RouteData("Controller");
                    db.Insert(l);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate">起使日期</param>
        /// <param name="endDate">截止日期</param>
        /// <param name="startIndex">起始筆數</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns></returns>
        public List<Log> FindAllByLogDate(string startDate, string endDate, int startIndex, int pageSize)
        {
            using (var db = new SqlConnection(_connectionString)) {
                string sql = "";

                sql = "Select * From ErrorLog  ";
                sql += $" Where Convert(varchar(10),LogDate,112) Between '{startDate}' AND '{endDate}' ";
                sql += " ORDER BY LogDate Desc ";

                if ( pageSize != 0) {
                    sql += $" OFFSET {startIndex} ROWS ";
                    sql += $" FETCH NEXT {pageSize} ROWS ONLY ";
                }

                return db.Query<Log>(sql).ToList();
            }
        }

        public int CountAll(string startDate, string endDate)
        {
            using (var db = new SqlConnection(_connectionString)) {
                string sql = "";

                sql = "Select COUNT(LogNo) From ErrorLog  ";
                sql += $" Where Convert(varchar(10),LogDate,112) Between '{startDate}' AND '{endDate}' ";
             
                return db.QueryFirstOrDefault<int>(sql);
            }
        }
    }
}
