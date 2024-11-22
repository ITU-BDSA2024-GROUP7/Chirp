// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable


using System.Text;
using System.Text.Json;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Serialization;


namespace Chirp.Web.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly CheepService _service;

        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger,
            CheepService service)
        {
            _userManager = userManager;
            _logger = logger;
            _service = service;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.",
                _userManager.GetUserId(User));

            StringBuilder csvContent = new StringBuilder();

            // Only include personal data for download
            // var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(IdentityUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            foreach (var p in personalDataProps)
            {
                // personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                if (p.GetValue(user) != null)
                {
                    csvContent.AppendLine($"{p.Name}: {p.GetValue(user)?.ToString()}");
                }
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                // personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                csvContent.AppendLine($"{l.LoginProvider} external login provider key: {l.ProviderKey}");
            }

            // personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));
            csvContent.AppendLine("Authenticator Key: " + await _userManager.GetAuthenticatorKeyAsync(user));
            
            // Retrieve authors that the user follows
            var followedAuthors = await _service.FindAuthorByName(user.UserName);
            var i = 0;
            foreach (var author in followedAuthors.AuthorsFollowed)
            {
                // personalData.Add($"Follows_{i}", author);
                csvContent.AppendLine("Follows: " + author);
                i++;
            }

            var j = 0;
            // Retrieve cheeps that the user has made
            var ListOfCheeps = await _service.RetrieveAllCheepsFromAnAuthor(user.UserName);
            foreach (var cheep in ListOfCheeps)
            {
                // personalData.Add($"Cheep_{j}", cheep.Text);
                // personalData.Add($"Cheep Author_{j}", cheep.Author.Name);
                // personalData.Add($"Cheep TimeStamp_{j}", cheep.FormattedTimeStamp);
                csvContent.AppendLine($"Cheep {j}: cheep.Text, Author: {cheep.Author.Name}, Cheep TimeStamp: {cheep.FormattedTimeStamp}");
                j++;
            }


            Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.csv");
            // return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
            return new FileContentResult( Encoding.UTF8.GetBytes(csvContent.ToString()), "application/csv");
        }
    }
}