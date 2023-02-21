using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Core.Entities;
using IdentityServer.Core.Services;
using IdentityServer.Core.Utils;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public ProfileService(IUserService userService, ILogger<ProfileService> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectIdentityName = context.Subject.Identity.Name;

            await Task.Run(() => _logger.LogInformation($"Started getting profile data for the {subjectIdentityName}"));

            // depending on the scope accessing the user data
            var user = !string.IsNullOrEmpty(subjectIdentityName)
                ? await _userService.FindByName(subjectIdentityName)
                : await GetUserBySubjectAsync(context.Subject);

            await SetIssuedClaimsAsync(context, user);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = false;

            _logger.LogInformation("Started to check active");
            User user = await GetUserBySubjectAsync(context.Subject);

            context.IsActive = user != null;
        }

        private async Task<User> GetUserBySubjectAsync(ClaimsPrincipal subject)
        {
            Claim subjClaim = subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);

            _logger.LogInformation("Getting user by subject: {0}", subjClaim?.Value);

            if (string.IsNullOrEmpty(subjClaim?.Value) || !int.TryParse(subjClaim.Value, out var id))
            {
                return null;
            }

            var user = await _userService.FindById(id);

            _logger.LogInformation(user != null
                ? $"User with subject {subjClaim.Value} was found. Name is {user.Name}"
                : $"User with subject {subjClaim.Value} was not found.");

            return user;
        }

        private async Task SetIssuedClaimsAsync(ProfileDataRequestContext context, User user)
        {
            if (user == null)
            {
                return;
            }

            _logger.LogInformation("User is not null. Started getting claims for the: {0}", user.Name);

            var claims = await Task.Run(() => ContextHelper.GetUserClaims(user));

            context.IssuedClaims = claims;
            _logger.LogInformation("Filled required claims. For the user: {0}", user.Name);
        }
    }
}
