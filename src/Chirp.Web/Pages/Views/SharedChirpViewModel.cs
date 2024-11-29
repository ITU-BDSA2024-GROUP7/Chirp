using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages.Views;

public class SharedChirpViewModel
{
    // public string FormAction { get; set; }
    public string CheepText { get; set; } = string.Empty;
    public IFormFile CheepImage { get; set; }
}
