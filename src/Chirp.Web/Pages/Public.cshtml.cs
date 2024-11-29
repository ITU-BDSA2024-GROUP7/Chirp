using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using System.Globalization;
using System.Text;
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
    public AuthorDTO UserAuthor { get; set; }
    public bool ShowPopularCheeps { get; set; }

    public required List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public SharedChirpViewModel SharedChirpView { get; set; } = new SharedChirpViewModel();

    public PublicModel(CheepService service)
    {
        _service = service;
    }
    public string GetFormattedTimeStamp(string timeStamp)
    {
        if (!DateTime.TryParse(timeStamp, out DateTime timeStampDateTime))
        {
            return "Invalid date";
        }
        // Format the timestamp
        var CurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
        
        var timeDifference = CurrentTime - timeStampDateTime;
        
        if (timeDifference.TotalSeconds < 60)
        {
            return "just now";
        }
        else if (timeDifference.TotalMinutes < 60)
        {
            if ((int)timeDifference.TotalMinutes == 1)
            {
                return $"{(int)timeDifference.TotalMinutes} minute ago";
            }
            return $"{(int)timeDifference.TotalMinutes} minutes ago";
        }
        else if (timeDifference.TotalHours < 24)
        {
            if ((int)timeDifference.TotalHours == 1)
            {
                return $"{(int)timeDifference.TotalHours} hour ago";
            }
            return $"{(int)timeDifference.TotalHours} hours ago";
        }
        else if (timeDifference.TotalDays < 30)
        {
            if ((int)timeDifference.TotalDays == 1)
            {
                return $"{(int)timeDifference.TotalDays} day ago";
            }
            return $"{(int)timeDifference.TotalDays} days ago";
        }
        else if (timeDifference.TotalDays < 365)
        {
            if ((int)(timeDifference.TotalDays / 30) == 1)
            {
                return $"{(int)(timeDifference.TotalDays / 30)} month ago";
            }
            return $"{(int)(timeDifference.TotalDays / 30)} months ago";
        }
        else
        {
            if ((int)(timeDifference.TotalDays / 365) == 1)
            {
                return $"{(int)(timeDifference.TotalDays / 365)} year ago";
            }
            return $"{(int)(timeDifference.TotalDays / 365)} years ago";
        }
    }
    
    /// <summary>
    /// Gets cheeps and stores them in a list when the page is loaded.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGet([FromQuery] int page)
    {
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
            UserAuthor = await _service.FindAuthorByName(currentUserName);
        }
        
        return Page();
    }
    
    [BindProperty]
    public int pageNumber { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1} characters")]
    public string CheepText { get; set; } = string.Empty;
    public async Task<IActionResult> OnPost()
    {
        
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Gets current page number
            PageNumber = pageNumber;
            
            // Ensure Cheeps and other required properties are populated
            Cheeps = await _service.GetCheeps(PageNumber);
            
            TotalPageNumber = await _service.GetTotalPageNumber();
            
            var currentUserName = User.Identity.Name;
            UserAuthor = await _service.FindAuthorByName(currentUserName);
            
            return Page(); // Return the page with validation messages
        }

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;
            var authorEmail = User.Identity.Name;

            if (authorName != null && authorEmail != null)
            {
                // Create the new CheepDTO
                var cheepDTO = new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = authorName, // this needs to be changed to user names going forward
                        Email = authorEmail
                    },
                    Text = CheepText,
                    FormattedTimeStamp = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture) // Or however you want to format this
                };

                await _service.CreateCheep(cheepDTO);
            }
        }

        return RedirectToPage("Public", new { page = 1 });
    }
    
    /// <summary>
    /// Follows an author
    /// </summary>
    /// <param name="followedAuthorName"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostFollowMethod(string followedAuthorName)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) // Check if the user is authenticated
        {
            PageNumber = pageNumber;
            var userAuthor = User.Identity.Name; // Get the user's name
            await _service.FollowAuthor(userAuthor, followedAuthorName);
            
        }
        return Redirect($"/?page={PageNumber}");
    }

    /// <summary>
    /// Unfollows an author
    /// </summary>
    /// <param name="followedAuthor"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostUnfollowMethod(string followedAuthor)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) // Check if the user is authenticated
        {
            PageNumber = pageNumber;
            var userAuthor = User.Identity.Name; // Get the user's name
            await _service.UnfollowAuthor(userAuthor, followedAuthor);
            
        }
        return Redirect($"/?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostLikeMethod(int cheepId)
    {
        await _service.HandleLike(User.Identity.Name, cheepId);
        
        return Redirect($"/?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDislikeMethod(int cheepId)
    {
        await _service.HandleDislike(User.Identity.Name, cheepId);
        
        return Redirect($"/?page={PageNumber}");
    }
    
    
    public async Task<IActionResult> OnPostDeleteMethod(int cheepId)
    {
        await _service.DeleteCheep(cheepId);
        
        return Redirect($"/?page={PageNumber}");
    }
    
    
    public async Task<IActionResult> OnPostViewCommentsMethod(int cheepId, string commentText)
    {
        Console.WriteLine("Commenting on cheep with id: " + cheepId);
        
        return Redirect($"/{cheepId}/comments");
    }

}