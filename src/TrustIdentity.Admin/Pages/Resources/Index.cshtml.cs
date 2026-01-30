using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Admin.Pages.Resources;

public class IndexModel : PageModel
{
    private readonly IResourceStore _identityResourceStore;
    private readonly IApiResourceStore _apiResourceStore;
    private readonly IApiScopeStore _apiScopeStore;

    public IndexModel(
        IResourceStore identityResourceStore,
        IApiResourceStore apiResourceStore,
        IApiScopeStore apiScopeStore)
    {
        _identityResourceStore = identityResourceStore;
        _apiResourceStore = apiResourceStore;
        _apiScopeStore = apiScopeStore;
    }

    public List<IdentityResource> IdentityResources { get; set; } = new();
    public List<ApiResource> ApiResources { get; set; } = new();
    public List<ApiScope> ApiScopes { get; set; } = new();

    public async Task OnGetAsync()
    {
        IdentityResources = (await _identityResourceStore.GetAllResourcesAsync()).ToList();
        ApiResources = (await _apiResourceStore.GetAllResourcesAsync()).ToList();
        ApiScopes = (await _apiScopeStore.GetAllScopesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostDeleteIdentityAsync(string name)
    {
        await _identityResourceStore.DeleteResourceAsync(name);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteApiResourceAsync(string name)
    {
        await _apiResourceStore.DeleteResourceAsync(name);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteApiScopeAsync(string name)
    {
        await _apiScopeStore.DeleteScopeAsync(name);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExportAsync()
    {
        var exportData = new
        {
            IdentityResources = await _identityResourceStore.GetAllResourcesAsync(),
            ApiResources = await _apiResourceStore.GetAllResourcesAsync(),
            ApiScopes = await _apiScopeStore.GetAllScopesAsync(),
            ExportedAt = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"trustidentity-config-{DateTime.UtcNow:yyyyMMdd-HHmm}.json");
    }

    [BindProperty]
    public IFormFile? ImportFile { get; set; }

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (ImportFile == null || ImportFile.Length == 0)
        {
            TempData["Error"] = "Please select a file to import.";
            return RedirectToPage();
        }

        // Security: Limit file size to 2MB to prevent DoS
        if (ImportFile.Length > 2 * 1024 * 1024)
        {
            TempData["Error"] = "File is too large. Configuration files should be under 2MB.";
            return RedirectToPage();
        }

        // Security: Ensure it's a JSON file
        if (!ImportFile.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Invalid file type. Only .json files are accepted.";
            return RedirectToPage();
        }

        try
        {
            using var stream = ImportFile.OpenReadStream();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var importData = await System.Text.Json.JsonSerializer.DeserializeAsync<ImportModel>(stream, options);

            if (importData != null)
            {
                if (importData.IdentityResources != null)
                {
                    foreach (var r in importData.IdentityResources) await _identityResourceStore.UpdateResourceAsync(r); // Upsert? Helper assumes Add/Update distinction might be needed but Update usually handles if key exists or fails. Let's assume Update logic check exists or just try Add/Update. Actually Store logic usually requires check.
                    // For simplicity, we loop and check if exists? The standard stores in Abstractions don't have Upsert. 
                    // Let's implement smart upsert logic here.
                    foreach(var item in importData.IdentityResources)
                    {
                        var exists = await _identityResourceStore.FindIdentityResourceAsync(item.Name);
                        if(exists != null) await _identityResourceStore.UpdateResourceAsync(item);
                        else await _identityResourceStore.AddResourceAsync(item);
                    }
                }
                if (importData.ApiResources != null)
                {
                    foreach(var item in importData.ApiResources)
                    {
                        var exists = await _apiResourceStore.FindApiResourceAsync(item.Name);
                        if(exists != null) await _apiResourceStore.UpdateResourceAsync(item);
                        else await _apiResourceStore.AddResourceAsync(item);
                    }
                }
                if (importData.ApiScopes != null)
                {
                    foreach(var item in importData.ApiScopes)
                    {
                        var exists = await _apiScopeStore.FindApiScopeAsync(item.Name);
                        if(exists != null) await _apiScopeStore.UpdateScopeAsync(item);
                        else await _apiScopeStore.AddScopeAsync(item);
                    }
                }

                TempData["Success"] = "Configuration imported successfully.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Import failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    private class ImportModel
    {
        public List<IdentityResource>? IdentityResources { get; set; }
        public List<ApiResource>? ApiResources { get; set; }
        public List<ApiScope>? ApiScopes { get; set; }
    }
}
