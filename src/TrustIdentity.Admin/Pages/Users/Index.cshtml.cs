using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IUserStore _userStore;

    public IndexModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public List<UserViewModel> Users { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int TotalCount { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public class UserViewModel
    {
        public string SubjectId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
    }

    public async Task OnGetAsync()
    {
        var (users, total) = await _userStore.GetAllUsersAsync(Search, (CurrentPage - 1) * PageSize, PageSize);
        
        TotalCount = total;
        Users = users.Select(u => new UserViewModel
        {
            SubjectId = u.SubjectId,
            Username = u.Username,
            Email = u.Email,
            IsActive = u.IsActive,
            Created = u.CreatedDate
        }).ToList();
    }

    [BindProperty]
    public List<string> SelectedIds { get; set; } = new();

    public async Task<IActionResult> OnPostBulkActionAsync(string action)
    {
        if (SelectedIds == null || !SelectedIds.Any())
        {
            return RedirectToPage();
        }

        foreach (var id in SelectedIds)
        {
            switch (action.ToLower())
            {
                case "delete":
                    await _userStore.DeleteUserAsync(id);
                    break;
                case "lock":
                    await _userStore.LockAccountAsync(id, DateTimeOffset.UtcNow.AddYears(100)); // Permanent lock
                    break;
                case "unlock":
                    await _userStore.LockAccountAsync(id, null);
                    break;
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        await _userStore.DeleteUserAsync(id);
        return RedirectToPage();
    }
}
