using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Core.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for managing user consent for client applications
/// </summary>
public class ConsentService
{
    private readonly ILogger<ConsentService> _logger;
    private readonly IConsentStore _consentStore;
    private readonly IClientStore _clientStore;
    private readonly IResourceStore _resourceStore;
    private readonly IApiScopeStore _apiScopeStore;
    private readonly Abstractions.Configuration.TrustIdentityOptions _options;

    /// <summary>
    /// Initializes a new instance of the ConsentService
    /// </summary>
    public ConsentService(
        ILogger<ConsentService> logger, 
        IConsentStore consentStore,
        IClientStore clientStore,
        IResourceStore resourceStore,
        IApiScopeStore apiScopeStore,
        Abstractions.Configuration.TrustIdentityOptions options)
    {
        _logger = logger;
        _consentStore = consentStore;
        _clientStore = clientStore;
        _resourceStore = resourceStore;
        _apiScopeStore = apiScopeStore;
        _options = options;
    }

    /// <summary>
    /// Parses the return URL to create a consent request model for the UI
    /// </summary>
    public async Task<ConsentRequest?> GetConsentRequestAsync(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl)) return null;

        var queryIndex = returnUrl.IndexOf('?');
        var queryString = queryIndex >= 0 ? returnUrl.Substring(queryIndex) : string.Empty;
        var query = QueryHelpers.ParseQuery(queryString);
        
        var clientId = query["client_id"].ToString();
        var scopeString = query["scope"].ToString();
        
        if (string.IsNullOrEmpty(clientId)) return null;

        var client = await _clientStore.FindClientByIdAsync(clientId);
        if (client == null) return null;

        var requestedScopes = scopeString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var consentScopes = new List<ConsentScope>();

        // Load Identity Resources
        var identityResources = await _resourceStore.FindIdentityResourcesByScopeAsync(requestedScopes);
        foreach (var res in identityResources)
        {
            consentScopes.Add(new ConsentScope
            {
                Name = res.Name,
                DisplayName = res.DisplayName ?? res.Name,
                Description = res.Description ?? string.Empty,
                Required = res.Required,
                Default = true
            });
        }

        // Load API Scopes
        var apiScopes = await _apiScopeStore.FindApiScopesByNameAsync(requestedScopes);
        foreach (var scope in apiScopes)
        {
            consentScopes.Add(new ConsentScope
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName ?? scope.Name,
                Description = scope.Description ?? string.Empty,
                Required = scope.Required,
                Default = true
            });
        }

        return new ConsentRequest
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName ?? client.ClientId,
            ClientLogoUrl = client.LogoUri,
            Scopes = consentScopes
        };
    }

    /// <summary>
    /// Handles the granting of consent
    /// </summary>
    public async Task GrantConsentAsync(string returnUrl, string subjectId, IEnumerable<string> consentedScopes, bool remember)
    {
        if (string.IsNullOrEmpty(returnUrl)) return;

        var queryIndex = returnUrl.IndexOf('?');
        var queryString = queryIndex >= 0 ? returnUrl.Substring(queryIndex) : string.Empty;
        var query = QueryHelpers.ParseQuery(queryString);
        var clientId = query["client_id"].ToString();
        
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(subjectId)) return;

        var consent = new UserConsent
        {
            SubjectId = subjectId,
            ClientId = clientId,
            Scopes = consentedScopes.ToList(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = remember 
                ? DateTime.UtcNow.AddSeconds(_options.UserInteraction.RememberConsentLifetime) 
                : DateTime.UtcNow.AddSeconds(_options.UserInteraction.ConsentLifetime)
        };

        await _consentStore.StoreAsync(consent);
    }

    /// <summary>
    /// Handles the denial of consent
    /// </summary>
    public async Task DenyConsentAsync(string returnUrl)
    {
        _logger.LogInformation("Consent denied for return URL: {ReturnUrl}", returnUrl);
        // Additional cleanup if needed
    }

    /// <summary>
    /// Retrieves user consent for a subject and client
    /// </summary>
    public async Task<UserConsent?> GetUserConsentAsync(string subjectId, string clientId)
    {
        return await _consentStore.GetAsync(subjectId, clientId);
    }

    /// <summary>
    /// Stores user consent
    /// </summary>
    public async Task StoreUserConsentAsync(UserConsent consent)
    {
        await _consentStore.StoreAsync(consent);
    }

    /// <summary>
    /// Removes user consent
    /// </summary>
    public async Task RemoveUserConsentAsync(string subjectId, string clientId)
    {
        await _consentStore.RemoveAsync(subjectId, clientId);
    }

    /// <summary>
    /// Checks if a client requires consent for the specified scopes and user
    /// </summary>
    public async Task<bool> RequiresConsentAsync(TrustIdentity.Abstractions.Models.Client client, ClaimsPrincipal user, IEnumerable<string> scopes)
    {
        if (!client.RequireConsent)
            return false;

        var subjectId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(subjectId))
            return true;

        var consent = await GetUserConsentAsync(subjectId, client.ClientId);
        if (consent == null || (consent.ExpiresAt.HasValue && consent.ExpiresAt < DateTime.UtcNow))
            return true;

        // Check if all requested scopes are in the consent
        var consentedScopes = consent.Scopes.ToHashSet();
        return !scopes.All(s => consentedScopes.Contains(s));
    }
}