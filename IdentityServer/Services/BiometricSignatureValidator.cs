using System;
using System.Threading.Tasks;
using IdentityServer.Core.Entities;
using IdentityServer.Core.Services;
using IdentityServer.Core.Utils;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Services
{
    public class BiometricSignatureValidator : IExtensionGrantValidator
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public BiometricSignatureValidator(
            IUserService userService,
            ILogger<BiometricSignatureValidator> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public string GrantType => "biometric";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var signature = context.Request.Raw.Get(2);
            _logger.LogInformation("validating user");
            const string message = "Профиль не найден.";

            try
            {
                // get user by signature
                User user = await _userService.FindByBiometrics(signature);
                _logger.LogInformation("Validating user. Getting user with signature: {0}.", signature);

                if (user == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, message);
                    _logger.LogInformation($"User not found for signature {signature}.");
                    return;
                }

                if (!ContextHelper.ValidateClientPermission(context.Request.ClientId, user.Role))
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "У вас нет доступа.");
                    _logger.LogInformation($"Invalid permission {user.Name}.");
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
