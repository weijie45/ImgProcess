using Oracle.ManagedDataAccess.Client;
using Service.Function.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DAL
{

    public class Db
    {
        public static string ConnString;
        public static DbType ProvideDb = DbType.Mssql;

        public enum DbType
        {
            Oracle = 0,
            Mssql = 1
        }

        private static Dictionary<string, string> ConnDict = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>().ToDictionary(x => x.Name, x => x.ConnectionString);

        public static IDbConnection Conn(string dbKey = "")
        {
            dbKey = dbKey == "" ? ConnDict.First().Key : dbKey;
            if (ConnDict.Count() == 0) {
                throw new Exception($"請至少建立一條連線字串 !");
            }

            ConnString = ConnDict.GetValue(dbKey);

            if (ConnString == "") {
                throw new Exception($"無此連線字串 ({dbKey}) !");
            }

            IDbConnection conn = null;

            switch (ProvideDb) {
                case DbType.Mssql:
                    conn = new SqlConnection(ConnString);
                    break;
                default:
                    conn = new OracleConnection(ConnString);
                    break;
            }

            if (conn == null) {
                throw new Exception($"{ConnString} 連線為空 !");
            } else {
                return conn;
            }

        }
    }

}
