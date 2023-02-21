using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Configurations;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.Data.Initializer
{
    public class SeedData
    {
        public async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Seeding database...");

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                var config = serviceProvider.GetService<IdentityServerConfig>();

                await EnsureSeedDataAsync(context, config);
            }

            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }

        private static async Task EnsureSeedDataAsync(ConfigurationDbContext context, IdentityServerConfig config)
        {
            await SeedClientsAsync(context, config);
            await SeedIdentityResourceAsync(context);
            await SeedApiResourcesAsync(context);
        }

        private static async Task SeedClientsAsync(ConfigurationDbContext context, IdentityServerConfig config)
        {
            if (!context.Clients.Any())
            {
                Console.WriteLine("Clients being populated");

                context.Clients.Add(GetClient("cottageUI", "CottageUI", config, "cottage").ToEntity());
                context.Clients.Add(GetClient("cottageMobile", "CottageMobile", config, "cottageMobile").ToEntity());

                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Clients already populated");
            }
        }

        private static async Task SeedIdentityResourceAsync(ConfigurationDbContext context)
        {
            if (!context.IdentityResources.Any())
            {
                Console.WriteLine("IdentityResources being populated");

                context.IdentityResources.Add(new IdentityResources.OpenId().ToEntity());
                context.IdentityResources.Add(new IdentityResources.Profile().ToEntity());

                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("IdentityResource already populated");
            }
        }

        private static async Task SeedApiResourcesAsync(ConfigurationDbContext context)
        {
            if (!context.ApiResources.Any())
            {
                Console.WriteLine("ApiResources being populated");

                var scopeAccess = new Scope
                {
                    Name = "ScopeAccess",
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Name
                    }
                };

                var apiResource = new ApiResource
                {
                    Name = "ApiResource",
                    DisplayName = "Api Resource",
                    Scopes = { scopeAccess }
                };

                context.ApiResources.Add(apiResource.ToEntity());

                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("ApiResources already populated");
            }
        }

        private static Client GetClient(string clientId, string clientName, IdentityServerConfig config, string secret)
        {
            var grantTypes = GrantTypes.ResourceOwnerPassword;
            grantTypes.Add("biometric");

            return new Client
            {
                ClientId = clientId,
                ClientName = clientName,
                AllowedGrantTypes = grantTypes,
                ClientSecrets =
                {
                    new Secret(secret.Sha256())
                },
                RequireClientSecret = false,
                AllowedScopes =
                {
                    "ScopeAccess",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess
                },
                AllowAccessTokensViaBrowser = true,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AllowOfflineAccess = true,
                AbsoluteRefreshTokenLifetime = 2147483640,
                RefreshTokenUsage = TokenUsage.OneTimeOnly
            };
        }
    }
}
