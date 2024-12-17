// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Build.Framework.Profiler;
using Microsoft.Extensions.Logging;

namespace Chirp.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly AuthorService _authorService;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            AuthorService authorService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _authorService = authorService;
        }
        
        public string ReturnUrl { get; set; }
        
        [TempData]
        public string ErrorMessage { get; set; }
        
        // Users should not be able to access this page directly. If so, redirect to login page.
        public IActionResult OnGet() => RedirectToPage("./Login");
        
        // When user clicks on a 'login using (an oauth application) button', this method is called.
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // This method is called when the external login provider has authenticated the user.
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            // If there is an error from the external provider, display it.
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            
            // Get the information about the user from the external login provider.
            var info = await _signInManager.GetExternalLoginInfoAsync();
            
            // If there is no external login information, redirect to login page.
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Signs in the user with the external login provider, if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded) // If the user is successfully signed in.
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
                returnUrl = Url.Content("/");
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut) // If the user is locked out i.e. too many login attempts.
            {
                return RedirectToPage("./Lockout");
            }
            
            // If the user does not have an account, then create an account.
            if (ModelState.IsValid) // If the model state is valid (i.e. the user has provided all the required information)
            {
                var user = CreateUser();

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                // Get claimtypes image
                
                var profilePicture = info.Principal.FindFirstValue("urn:github:avatar_url");
                await _userStore.SetUserNameAsync(user, name, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, email, CancellationToken.None);
                
                user.EmailConfirmed = true;

                var accountResult = await _userManager.CreateAsync(user); // Create the user account and save the result.
                
                if (accountResult.Succeeded) // If the account is successfully created.
                {
                    await _authorService.CreateAuthor(name, email, profilePicture);
                    
                    accountResult = await _userManager.AddLoginAsync(user, info); // Add the external login to the user account.
                    if (accountResult.Succeeded) // If the external login is successfully added to the user account.
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider); // Sign in the user.
                        return LocalRedirect(returnUrl); // Redirect to the return URL.
                    }
                }
                
                // If the account creation or adding the external login to the account fails, then add the errors to the model state.
                foreach (var error in accountResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If the model state is not valid, then redisplay the page.
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
