using System.Data;

namespace expense_api.Utils
{
    public interface ISqlConnHelper
    {
        IDbConnection Connection { get; }
    }
}
