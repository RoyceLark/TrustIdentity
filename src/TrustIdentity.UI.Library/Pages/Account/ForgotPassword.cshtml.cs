using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrustIdentity.Core.Services;
using System.Threading.Tasks;

namespace TrustIdentity.UI.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    private readonly AccountService _accountService;

    public ForgotPasswordModel(AccountService accountService)
    {
        _accountService = accountService;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await _accountService.CreatePasswordResetTokenAsync(Input.Email);
        
        // In a real system, the email is sent by AccountService or here
        // For this demo, we assume the token is created and logged
        
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
