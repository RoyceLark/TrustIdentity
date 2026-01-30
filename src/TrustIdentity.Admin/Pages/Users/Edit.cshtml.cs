using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Users;

public class EditModel : PageModel
{
    private readonly IUserStore _userStore;

    public EditModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string SubjectId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userStore.FindBySubjectIdAsync(id);
        if (user == null) return NotFound();

        Input = new InputModel
        {
            SubjectId = user.SubjectId,
            Username = user.Username,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userStore.FindBySubjectIdAsync(Input.SubjectId);
        if (user == null) return NotFound();

        user.Username = Input.Username;
        user.Email = Input.Email;
        user.IsActive = Input.IsActive;

        await _userStore.UpdateUserAsync(user);

        return RedirectToPage("./Details", new { id = user.SubjectId });
    }
}
