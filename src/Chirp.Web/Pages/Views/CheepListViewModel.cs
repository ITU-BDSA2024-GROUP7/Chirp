using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.ViewModels;

public class CheepListViewModel : PageModel
{
    private readonly CheepService _service;

    public CheepListViewModel(CheepService service, List<CheepDTO> cheeps)
    {
        _service = service;
        Cheeps = cheeps;
    }

    public AuthorDTO? UserAuthor { get; set; }
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public Dictionary<int, List<string>> TopReactions { get; set; } = new Dictionary<int, List<string>>();
    
    public string ConvertLinksToAnchors(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Regular expression to detect URLs
        var regex = new Regex(@"((http|https):\/\/)?(www\.)?[a-zA-Z0-9-]+\.[a-zA-Z]{2,}(\S*[^.,\s])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        // Replace URLs with anchor tags
        return regex.Replace(text, match => $"<a href=\"{match.Value}\" target=\"_blank\">{match.Value}</a>");
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
}