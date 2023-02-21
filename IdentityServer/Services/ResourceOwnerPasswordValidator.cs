using System;
using System.Threading.Tasks;
using IdentityServer.Core.Entities;
using IdentityServer.Core.Enum;
using IdentityServer.Core.Services;
using IdentityServer.Core.Utils;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Services
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public ResourceOwnerPasswordValidator(
            IUserService userService,
            ILogger<ResourceOwnerPasswordValidator> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            _logger.LogInformation("validating user {0}", context.UserName);
            const string message = "Введенный логин и/или пароль неверные.";

            try
            {
                // get user by name
                User user = await _userService.FindByName(context.UserName);
                _logger.LogInformation("Validating user. Getting user with username: {0}.", context.UserName);

                if (user == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, message);
                    _logger.LogInformation($"User not found for username {context.UserName}.");
                    return;
                }

                // check credentials
                if (!await _userService.ValidateCredentials(context.UserName, context.Password))
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, message);
                    _logger.LogInformation($"Password is not correct for username {context.UserName}.");
                    return;
                }

                if (!ContextHelper.ValidateClientPermission(context.Request.ClientId, user.Role))
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "У вас нет доступа.");
                    _logger.LogInformation($"Invalid permission {context.UserName}.");
                    return;
                }

                _logger.LogInformation("Getting claims for the user: {0}", user.Name);

                var userClaims = ContextHelper.GetUserClaims(user);
                context.Result = new GrantValidationResult(user.Id.ToString(), string.Empty, userClaims);
                _logger.LogInformation("User has been validated successfully.");
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, message);
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
