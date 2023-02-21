using System.Threading.Tasks;
using IdentityServer.Core.Entities;

namespace IdentityServer.Core.Services
{
    public interface IUserService
    {
        Task<bool> ValidateCredentials(string name, string password);

        Task<User> FindByName(string name);

        Task<User> FindById(int id);

        Task<User> FindByBiometrics(string signature);
    }
}
