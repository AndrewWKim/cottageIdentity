using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace IdentityServer.Middlewares
{
    public class SerilogMiddleware
    {
        private const string RemoteIpPropertyName = "RemoteIp";
        private const string RequestPathPropertyName = "RequestPath";
        private const string RequestClientIdPropertyName = "ClientId";
        private const string RequestUserNamePropertyName = "UserName";

        private readonly RequestDelegate _next;

        public SerilogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IClientStore clients, SecretParser parser, IUserSession userSession)
        {
            var clientId = await GetClientFromContext(context, clients, parser);

            var user = await userSession.GetUserAsync();
            var userName = user?.GetDisplayName();

            using (LogContext.PushProperty(RemoteIpPropertyName, context.Connection.RemoteIpAddress))
            {
                using (LogContext.PushProperty(RequestPathPropertyName, context.Request.Path))
                {
                    using (LogContext.PushProperty(RequestClientIdPropertyName, clientId))
                    {
                        using (LogContext.PushProperty(RequestUserNamePropertyName, userName))
                        {
                            await _next.Invoke(context);
                        }
                    }
                }
            }
        }

        private static string GetClientIdFromJwt(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                var clientId = jwt.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.ClientId);

                return clientId?.Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> GetClientFromContext(HttpContext context, IClientStore clients, SecretParser parser)
        {
            // Retrieve client from request parameters - IS endpoints
            NameValueCollection parameters = null;
            if (context.Request.Method == "GET")
            {
                parameters = context.Request.Query.AsNameValueCollection();
            }
            else if (context.Request.Method == "POST" && context.Request.HasFormContentType)
            {
                parameters = context.Request.Form.AsNameValueCollection();
            }

            var clientId = parameters?.Get(OidcConstants.AuthorizeRequest.ClientId);

            // Retrieve client from token - ManagementApi, UserInfo endpoints
            string tokenValue;
            if (string.IsNullOrWhiteSpace(clientId))
            {
                var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    var header = authorizationHeader.Trim();
                    if (header.StartsWith(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer))
                    {
                        tokenValue = header
                            .Substring(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer.Length)
                            .Trim();
                        if (!string.IsNullOrWhiteSpace(tokenValue))
                        {
                            clientId = GetClientIdFromJwt(tokenValue);
                        }
                    }
                }
            }

            // Retrieve client from token in request body - Introspection, Revocation endpoint
            if (string.IsNullOrWhiteSpace(clientId))
            {
                tokenValue = parameters?.Get("token");
                if (!string.IsNullOrWhiteSpace(tokenValue))
                {
                    clientId = GetClientIdFromJwt(tokenValue);
                }
            }

            // Retrieve client from code in request body - Token endpoint in case of authorization_code grant
            if (string.IsNullOrWhiteSpace(clientId))
            {
                var parsedSecret = await parser.ParseAsync(context);
                if (parsedSecret != null)
                {
                    var clientIdModel = await clients.FindEnabledClientByIdAsync(parsedSecret.Id);
                    clientId = clientIdModel?.ClientId;
                }
            }

            return clientId;
        }
    }
}