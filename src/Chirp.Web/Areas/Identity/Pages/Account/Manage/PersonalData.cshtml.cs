// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Services;
using Chirp.Web.Pages.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;
        private readonly CheepService _cheepService;
        private readonly AuthorService _authorService;
        public int PageNumber { get; set; }
        public int TotalPageNumber { get; set; }
        public SharedChirpViewModel SharedViewModel { get; set; } = new SharedChirpViewModel();
        public required List<CheepDTO> Cheeps { get; set; }
        public required List<String> FollowList { get; set; }
        public required List<String> UserData { get; set; } = new List<String>();
        public string CurrentAuthor { get; set; } = string.Empty;
        public AuthorDTO? userAuthor { get; set; }

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger, 
            CheepService cheepService,
            AuthorService authorService)
        {
            _userManager = userManager;
            _logger = logger;
            _cheepService = cheepService;
            _authorService = authorService;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            CurrentAuthor = User.Identity?.Name ?? string.Empty;

            if (User.Identity != null && User.Identity.Name == CurrentAuthor)
            {
                Cheeps = await _cheepService.GetCheepsFromAuthor(CurrentAuthor, 0);
            }

            // Retrieve authors that the user follows
            var authorDTO = await _authorService.FindAuthorByName(user.UserName ?? string.Empty);
            // FollowList = authorDTO.AuthorsFollowed as List<string>;
            FollowList = await _authorService.GetFollowedAuthors(user.UserName ?? string.Empty);

            userAuthor = authorDTO;

            var personalDataProps = typeof(IdentityUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            foreach (var p in personalDataProps)
            {
                // personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                if (p.GetValue(user) != null) UserData.Add($"{p.Name}: {p.GetValue(user)?.ToString()}");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                // personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                UserData.Add($"{l.LoginProvider} external login provider key: {l.ProviderKey}");
            }

            // personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            if (authenticatorKey) UserData.Add("Authenticator Key: " + authenticatorKey);

            return Page();
        }
    }
}