using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Chirp.Web.Pages.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }

    public required List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public SharedChirpViewModel SharedChirpView { get; set; } = new SharedChirpViewModel { FormAction = "/Public" };

    public Dictionary<string, bool> AuthorFollowStatus { get; private set; } = new Dictionary<string, bool>();
    
    public PublicModel(CheepService service)
    {
        _service = service;
    }

    public Author userAuthor { get; set; }

    /// <summary>
    /// Gets cheeps and stores them in a list when the page is loaded.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGet([FromQuery] int page)
    {
        userAuthor = await _service.FindAuthorByName(User.Identity.Name);
        if (page <= 0)
        {
            page = 1;
        }
        
        PageNumber = page;
        Cheeps = await _service.GetCheeps(page);
        TotalPageNumber = await _service.GetTotalPageNumber();

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;

            foreach (var cheep in Cheeps)
            {
                bool isFollowed = await _service.IsAuthorAlreadyFollowed(currentUserName, cheep.Author.Name);
                AuthorFollowStatus[cheep.Author.Name] = isFollowed;
            }
        }
        
        return Page();
    }

    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1} characters")]
    public string CheepText { get; set; }
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Ensure Cheeps and other required properties are populated
            Cheeps = await _service.GetCheeps(PageNumber);
            TotalPageNumber = await _service.GetTotalPageNumber();
            return Page(); // Return the page with validation messages
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
    
    public async Task<IActionResult> OnPostFollowMethod(string followedAuthorName)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userAuthor = User.Identity.Name;
            await _service.FollowAuthor(userAuthor, followedAuthorName);
            
        }

        return RedirectToPage("Public", new { page = PageNumber });
    }

    public async Task<IActionResult> OnPostUnfollowMethod(string followedAuthor)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userAuthor = User.Identity.Name;
            await _service.UnfollowAuthor(userAuthor, followedAuthor);
            
        }

        return RedirectToPage("Public", new { page = PageNumber });
    }

    
    
    
}