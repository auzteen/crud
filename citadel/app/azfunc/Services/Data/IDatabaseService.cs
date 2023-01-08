using System.Data.SqlClient;

namespace Citadel.Services.Data
{
    public interface IDatabaseService
    {
        public SqlConnection GetSqlConnection();
        public int GetScalarValueFromQuery(string sqlQuery);
    }
}