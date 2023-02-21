using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer.Core.Entities;
using IdentityServer.Core.Enum;

namespace IdentityServer.Core.Utils
{
    public static class ContextHelper
    {
        public static List<Claim> GetUserClaims(User user)
        {
            var claims = new List<Claim>();

            claims.AddRange(new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("role", user.Role.ToString())
            });

            return claims;
        }

        public static bool ValidateClientPermission(string clientId, UserRole role)
        {
            var result = clientId switch
            {
                "cottageMobile" => role == UserRole.Owner || role == UserRole.MainResident,
                "cottageUI" => role == UserRole.Admin || role == UserRole.Security,
                _ => false
            };

            return result;
        }
    }
}
