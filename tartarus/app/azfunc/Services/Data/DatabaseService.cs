using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Tartarus.Services.Data;
using Tartarus.Services;

namespace Tartarus.Services.Data
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger _log;

        public DatabaseService(ILogger<DatabaseService> log)
        {
            _log = log;
        }

        public SqlConnection GetSqlConnection()
        {
            SqlConnection conn = new SqlConnection();
            try
            {
                conn = new SqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
            return conn;
        }

        public int GetScalarValueFromQuery(string sqlQuery)
        {
            SqlConnection conn = GetSqlConnection();
            conn.Open();
            SqlCommand comm = new SqlCommand(sqlQuery, conn);
            Int32 value = (Int32)comm.ExecuteScalar();
            conn.Close();
            return value;
        }
    }
}