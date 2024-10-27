using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zen.Web.Auth.Identity {
    public class ZenSignInManager : SignInManager<IdentityUser>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserClaimsPrincipalFactory<IdentityUser> _claimsFactory;
        private readonly IOptions<IdentityOptions> _optionsAccessor;
        private readonly ILogger<SignInManager<IdentityUser>> _logger;
        private readonly IAuthenticationSchemeProvider _schemes;
        private readonly IUserConfirmation<IdentityUser> _confirmation;

        public ZenSignInManager(UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<IdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<IdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<IdentityUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

            _claimsFactory = claimsFactory;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
            _schemes = schemes;
            _confirmation = confirmation;
        }

        public override async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {

            // search for user
            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user is null) { return SignInResult.Failed; }


            var error = await PreSignInCheck(user);
            if (error != null) return error;

            return await SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);
        }
    }
}