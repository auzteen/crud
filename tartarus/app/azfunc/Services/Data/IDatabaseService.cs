using System.Data.SqlClient;

namespace Tartarus.Services.Data
{
    public interface IDatabaseService
    {
        public SqlConnection GetSqlConnection();
        public int GetScalarValueFromQuery(string sqlQuery);
    }
}