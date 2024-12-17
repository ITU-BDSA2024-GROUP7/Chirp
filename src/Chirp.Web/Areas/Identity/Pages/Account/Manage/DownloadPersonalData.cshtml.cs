// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable


using System.IO.Compression;
using System.Text;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;



namespace Chirp.Web.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private readonly CheepService _cheepService;
        private readonly AuthorService _authorService;

        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger,
            CheepService cheepService,
            AuthorService authorService)
        {
            _userManager = userManager;
            _logger = logger;
            _cheepService = cheepService;
            _authorService = authorService;
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
            var personalDataProps = typeof(IdentityUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            foreach (var p in personalDataProps)
            {
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
                    csvContent.AppendLine($"{l.LoginProvider} external login provider key: {l.ProviderKey}");
                }
            }
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            if (authenticatorKey) csvContent.AppendLine("Authenticator Key: " + authenticatorKey);
            
            // Retrieve authors that the user follows
            if (user.UserName != null)
            {
                var followedAuthors = await _authorService.FindAuthorByName(user.UserName);
                var i = 0;
                foreach (var author in followedAuthors.AuthorsFollowed)
                {
                    csvContent.AppendLine("Follows: " + author);
                    i++;
                }
            }

            
            // Retrieve cheeps that the user has made
            if (user.UserName != null)
            {
                var ListOfCheeps = await _cheepService.RetrieveAllCheepsFromAnAuthor(user.UserName);
            
                int pictureCounter = 0; // Counter for images and GIFs
                foreach (var cheep in ListOfCheeps) 
                {
                    csvContent.AppendLine($"Cheep: {cheep.Text}, Author: {cheep.Author.Name}, TimeStamp: {cheep.FormattedTimeStamp}"); 
                    if (!string.IsNullOrEmpty(cheep.ImageReference))
                    {
                        // Identify the MIME type based on the beginning of the Base64 string (GIFs start with "R0lGODlh)"
                        string mimeType = cheep.ImageReference.StartsWith("R0lGODlh") ? "image/gif" : "image/png"; 
                        string fileExtension = mimeType == "image/gif" ? ".gif" : ".png"; 

                        var imageBytes = Convert.FromBase64String(cheep.ImageReference); // Decode the Base64 string
                        imageFiles.Add(($"cheep_media_{pictureCounter + 1}{fileExtension}", imageBytes)); 
                        pictureCounter++; // Increment picture counter
                    }
                }
            }

            var k = 0;
            if (user.UserName != null)
            {
                var ListOfComments = await _cheepService.RetrieveAllCommentsFromAnAuthor(user.UserName);
                foreach (var Comment in ListOfComments)
                {
                    csvContent.AppendLine($"Comment {k}: Comment.Text {Comment.Text}, Author: {Comment.Author.Name}, Comment TimeStamp: {Comment.FormattedTimeStamp}");
                    k++;
                }
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
            Response.Headers.Append("Content-Disposition", "attachment; filename=PersonalData.zip"); 
            return File(memoryStream.ToArray(), "application/zip"); 
        }
    }
}