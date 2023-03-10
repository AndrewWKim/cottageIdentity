using System;
using System.Data;
using System.Threading.Tasks;
using IdentityServer.Configurations;
using Microsoft.Data.SqlClient;

namespace IdentityServer.Data.Repositories
{
    public abstract class BaseDapperRepository
    {
        private readonly string _connectionString;

        protected BaseDapperRepository(IdentityServerConfig config)
        {
            _connectionString = config.CottageConnectionString;
        }

        public async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Asynchronously open a connection to the database
                    return await getData(connection); // Asynchronously execute getData, which has been passed in as a Func<IDBConnection, Task<T>>
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout)", ex);
            }
        }
    }
}