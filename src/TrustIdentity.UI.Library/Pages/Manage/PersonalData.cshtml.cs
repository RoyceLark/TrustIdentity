using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using System.Security.Claims;

namespace TrustIdentity.UI.Pages.Manage;

[Authorize]
public class PersonalDataModel : PageModel
{
    private readonly IUserStore _userStore;

    public PersonalDataModel(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public async Task<IActionResult> OnPostDownloadAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return NotFound();

        var user = await _userStore.FindBySubjectIdAsync(userId);
        if (user == null) return NotFound();

        var personalData = new Dictionary<string, string>();
        personalData.Add("UserId", user.SubjectId);
        personalData.Add("Username", user.Username);
        personalData.Add("Email", user.Email ?? "");

        foreach (var claim in user.Claims)
        {
            personalData.Add(claim.Type, claim.Value);
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(personalData);
        return File(bytes, "application/json", "PersonalData.json");
    }
}
