using System.Data;
using System.Threading.Tasks;
using Dapper;
using IdentityServer.Configurations;
using IdentityServer.Core.Entities;
using IdentityServer.Core.Repositories;

namespace IdentityServer.Data.Repositories
{
    public class UserRepository : BaseDapperRepository, IUserRepository
    {
        public UserRepository(IdentityServerConfig config) : base(config)
        {
        }

        public async Task<User> GetByIdAsync(int id)
        {
            const string sql = "SELECT TOP 1 * FROM Users WHERE Id = @id";

            var parameters = new DynamicParameters();
            parameters.Add("@id", id, DbType.Int32);

            var user = await WithConnection(async c => await c.QueryFirstOrDefaultAsync<User>(sql, parameters));

            return user;
        }

        public async Task<User> GetByNameAsync(string name)
        {
            const string sql = "SELECT TOP 1 * FROM Users WHERE Name = @name";

            var parameters = new DynamicParameters();
            parameters.Add("@name", name, DbType.String);

            var user = await WithConnection(async c => await c.QueryFirstOrDefaultAsync<User>(sql, parameters));

            return user;
        }

        public async Task<User> GetByBiometricsAsync(string signature)
        {
            const string sql = "SELECT TOP 1 * FROM Users WHERE BiometricsSignature = @signature";

            var parameters = new DynamicParameters();
            parameters.Add("@signature", signature, DbType.String);

            var user = await WithConnection(async c => await c.QueryFirstOrDefaultAsync<User>(sql, parameters));

            return user;
        }
    }
}