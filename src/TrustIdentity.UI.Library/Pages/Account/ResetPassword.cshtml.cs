using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Core.Services;
using System.Threading.Tasks;

namespace TrustIdentity.UI.Pages.Account;

public class ResetPasswordModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string Code { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }

    private readonly AccountService _accountService;

    public ResetPasswordModel(AccountService accountService)
    {
        _accountService = accountService;
    }

    public IActionResult OnGet(string? code = null)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        Code = code;
        Input.Code = code;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _accountService.ResetPasswordAsync(Input.Code, Input.Password);
        if (result)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        ModelState.AddModelError(string.Empty, "Invalid or expired reset token.");
        return Page();
    }
}
