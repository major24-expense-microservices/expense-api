using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace expense_api.Utils
{
    public class SqlConnHelper : ISqlConnHelper
    {
        private readonly IConfiguration _configuration;
        public SqlConnHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_configuration.GetConnectionString("SQLMN24-SQLADMIN"));
            }
        }
    }
}
