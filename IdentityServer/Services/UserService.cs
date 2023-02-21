using System.Threading.Tasks;
using IdentityServer.Core.Entities;
using IdentityServer.Core.Repositories;
using IdentityServer.Core.Services;
using IdentityServer.Core.Utils;
using IdentityServer.Core;

namespace IdentityServer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> ValidateCredentials(string name, string password)
        {
            var user = await FindByName(name);

            if (user == null)
            {
                return false;
            }

            var passwordHash = EncryptionFactory.HashWithMD5(password);

            return passwordHash == user.Password;
        }

        public async Task<User> FindByName(string name)
        {
            var user = await _userRepository.GetByNameAsync(name);

            return user;
        }

        public async Task<User> FindById(int id)
        {
            var cacheKey = $"{CacheService.UserPrefix}_{id}";

            var user = await CacheService.GetDataFromCacheAsync(
                cacheKey,
                () => _userRepository.GetByIdAsync(id));

            return user;
        }

        public async Task<User> FindByBiometrics(string signature)
        {
            var cacheKey = $"{CacheService.UserPrefix}_{signature}";

            var user = await CacheService.GetDataFromCacheAsync(
                cacheKey,
                () => _userRepository.GetByBiometricsAsync(signature));

            return user;
        }
    }
}
