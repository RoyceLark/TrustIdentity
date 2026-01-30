using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IUserStore _userStore;

    public ProfileModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _userStore.FindBySubjectIdAsync(userId);
        if (user == null) return NotFound();

        Input = new InputModel
        {
            DisplayName = user.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
            PhoneNumber = user.PhoneNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _userStore.FindBySubjectIdAsync(userId);
        if (user == null) return NotFound();

        // Update phone
        user.PhoneNumber = Input.PhoneNumber;

        // Update name claim
        var nameClaim = user.Claims.FirstOrDefault(c => c.Type == "name");
        if (nameClaim != null)
        {
            user.Claims.Remove(nameClaim);
        }
        if (!string.IsNullOrEmpty(Input.DisplayName))
        {
            user.Claims.Add(new Claim("name", Input.DisplayName));
        }

        await _userStore.UpdateUserAsync(user);

        return RedirectToPage("./Index");
    }
}
