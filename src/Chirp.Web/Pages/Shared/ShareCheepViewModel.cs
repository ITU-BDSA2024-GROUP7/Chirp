using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Chirp.Core.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;
using Chirp.Infrastructure.Services;


namespace Chirp.Web.Pages.Shared;

public class ShareCheepViewModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    
    public async Task<IActionResult> OnPost()
    {
        
        if (string.IsNullOrWhiteSpace(CheepText))
        {
            // Add a custom model error if CheepText is empty
            ModelState.AddModelError(nameof(CheepText), "At least write something before you click me....MO");
            return Page(); // Return the page with the new error message
        }
        
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;
            var authorEmail = User.Identity.Name;

            // Create the new CheepDTO
            var cheepDTO = new CheepDTO
            {
                Author = new AuthorDTO
                {
                    Name = authorName, // this needs to be changed to user names going forward
                    Email = authorEmail 
                },
                Text = CheepText,
                FormattedTimeStamp = DateTime.UtcNow.ToString() // Or however you want to format this
            };

            await _service.CreateCheep(cheepDTO);
        }
        
        return RedirectToPage("Public", new { page = 1 });
    }
    
    [BindProperty]
    public string CheepText { get; set; }
}