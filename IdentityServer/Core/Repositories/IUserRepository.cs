using System.Threading.Tasks;
using IdentityServer.Core.Entities;

namespace IdentityServer.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);

        Task<User> GetByNameAsync(string name);

        Task<User> GetByBiometricsAsync(string signature);
    }
}
