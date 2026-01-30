using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IUserStore _userStore;

    public CreateModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _userStore.FindByUsernameAsync(Input.Username);
        if (existing != null)
        {
            ModelState.AddModelError("Input.Username", "Username is already taken.");
            return Page();
        }

        var user = new User
        {
            SubjectId = Guid.NewGuid().ToString(),
            Username = Input.Username,
            Email = Input.Email,
            IsActive = Input.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        await _userStore.AddUserAsync(user, Input.Password);

        return RedirectToPage("./Index");
    }
}
