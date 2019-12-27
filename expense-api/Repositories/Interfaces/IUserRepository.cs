using expense_api.Models;
using expense_api.Utils;
using System.Threading.Tasks;

namespace expense_api.Repositories
{
    public interface IUserRepository
    {
        Task<Result> Create(User user);
        Task<Result<bool>> IsUserExists(User user);
    }
}
