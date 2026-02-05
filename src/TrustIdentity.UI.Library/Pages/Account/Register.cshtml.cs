using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Core.Services;
using System.Threading.Tasks;
using System;

namespace TrustIdentity.UI.Pages.Account;

public class RegisterModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "I agree to the Terms of Service and Privacy Policy")]
        public bool AgreeToTerms { get; set; }
    }

    private readonly IUserService _userService;
    private readonly TrustIdentity.Abstractions.Stores.IUserStore _userStore;
    private readonly AccountService _accountService;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        IUserService userService, 
        TrustIdentity.Abstractions.Stores.IUserStore userStore, 
        AccountService accountService,
        ILogger<RegisterModel> logger)
    {
        _userService = userService;
        _userStore = userStore;
        _accountService = accountService;
        _logger = logger;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!Input.AgreeToTerms)
        {
            ModelState.AddModelError("Input.AgreeToTerms", "You must agree to the terms.");
            return Page();
        }

        // Check if user already exists
        var existingUser = await _userService.FindByUsernameAsync(Input.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError("Input.Username", "Username is already taken.");
            return Page();
        }

        // Create user
        var user = new TrustIdentity.Abstractions.Models.User
        {
            SubjectId = Guid.NewGuid().ToString(),
            Username = Input.Username,
            Email = Input.Email,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            await _userStore.AddUserAsync(user, Input.Password);
            _logger.LogInformation("User {Username} registered successfully.", Input.Username);
            
            // Generate email verification
            var token = await _accountService.CreateEmailVerificationTokenAsync(user.SubjectId);
            
            // Send email Logic here...
            
            return RedirectToPage("./RegisterConfirmation", new { email = Input.Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Username}", Input.Username);
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return Page();
        }
    }
}
