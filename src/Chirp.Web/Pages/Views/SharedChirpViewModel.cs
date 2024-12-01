using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages.Views;

public class SharedChirpViewModel
{
    // We should use these constraints here aswell. When implementing the e2e tests has to be changed.
    // It make the user not able to write a cheep with more than 160 characters.
    // [Required(ErrorMessage = "At least write something before you click me....")]
    // [StringLength(160, ErrorMessage = "Maximum length is {1} characters")]
    public string CheepText { get; set; } = string.Empty;
    public IFormFile? CheepImage { get; set; }
}
