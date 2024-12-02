// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable


using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing.Constraints;
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
            List<(string fileName, byte[] fileContent)> imageFiles = new List<(string fileName, byte[] fileContent)>();
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
                if (l.ProviderKey != null)
                {
                    // personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                    csvContent.AppendLine($"{l.LoginProvider} external login provider key: {l.ProviderKey}");
                }
            }

            // personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            if (authenticatorKey) csvContent.AppendLine("Authenticator Key: " + authenticatorKey);
            
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
                csvContent.AppendLine($"Cheep {j}: cheep.Text {cheep.Text}, Author: {cheep.Author.Name}, Cheep TimeStamp: {cheep.FormattedTimeStamp}");
                if(!string.IsNullOrEmpty(cheep.ImageReference))
                {
                    var imageBytes = Convert.FromBase64String(cheep.ImageReference);
                    imageFiles.Add(($"cheep_image_{j}.png", imageBytes));
                    
                }
                j++;
            }

            var k = 0;
            var ListOfComments = await _service.RetrieveAllCommentsFromAnAuthor(user.UserName);
            foreach (var Comment in ListOfComments)
            {
                csvContent.AppendLine($"Comment {k}: Comment.Text {Comment.Text}, Author: {Comment.Author.Name}, Comment TimeStamp: {Comment.FormattedTimeStamp}");
                k++;
            }


            // Create ZIP in memory
            using var memoryStream = new MemoryStream(); 
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) 
            {
                // Add CSV file
                var csvEntry = zipArchive.CreateEntry("PersonalData.csv"); 
                using (var entryStream = csvEntry.Open()) 
                using (var streamWriter = new StreamWriter(entryStream)) 
                {
                    streamWriter.Write(csvContent.ToString()); 
                }

                // Add image files
                foreach (var (fileName, fileContent) in imageFiles) 
                {
                    var imageEntry = zipArchive.CreateEntry(fileName); 
                    using (var entryStream = imageEntry.Open()) 
                    {
                        entryStream.Write(fileContent, 0, fileContent.Length); 
                    }
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin); 
            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.zip"); 
            return File(memoryStream.ToArray(), "application/zip"); 
        }
    }
}