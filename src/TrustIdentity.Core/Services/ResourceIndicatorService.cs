using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Stores;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for handling resource indicators (RFC 8707)
/// </summary>
public class ResourceIndicatorService : IResourceIndicatorService
{
    private readonly IApiResourceStore _apiResourceStore;
    private readonly IApiScopeStore _apiScopeStore;
    private readonly ILogger<ResourceIndicatorService> _logger;

    /// <summary>
    /// Initializes a new instance of ResourceIndicatorService
    /// </summary>
    public ResourceIndicatorService(
        IApiResourceStore apiResourceStore,
        IApiScopeStore apiScopeStore,
        ILogger<ResourceIndicatorService> logger)
    {
        _apiResourceStore = apiResourceStore;
        _apiScopeStore = apiScopeStore;
        _logger = logger;
    }

    /// <summary>
    /// Validates resource indicators in an authorization request
    /// </summary>
    public async Task<ResourceValidationResult> ValidateResourcesAsync(
        IEnumerable<string> requestedResources,
        IEnumerable<string> requestedScopes)
    {
        var result = new ResourceValidationResult();
        
        if (requestedResources == null || !requestedResources.Any())
        {
            // No resources specified - allow all resources based on scopes
            result.Success = true;
            return result;
        }

        var resources = new List<ApiResource>();
        foreach (var resourceIndicator in requestedResources)
        {
            var resource = await _apiResourceStore.FindApiResourceAsync(resourceIndicator);
            if (resource == null)
            {
                result.Success = false;
                result.Error = "invalid_target";
                result.ErrorDescription = $"Resource '{resourceIndicator}' not found";
                _logger.LogWarning("Invalid resource indicator requested: {Resource}", resourceIndicator);
                return result;
            }
            resources.Add(resource);
        }

        // Validate that requested scopes are valid for the requested resources
        var validScopes = resources.SelectMany(r => r.Scopes).Distinct().ToList();
        var invalidScopes = requestedScopes.Where(s => !validScopes.Contains(s)).ToList();
        
        if (invalidScopes.Any())
        {
            result.Success = false;
            result.Error = "invalid_scope";
            result.ErrorDescription = $"Scopes not valid for requested resources: {string.Join(", ", invalidScopes)}";
            _logger.LogWarning("Invalid scopes for resources: {Scopes}", string.Join(", ", invalidScopes));
            return result;
        }

        result.Success = true;
        result.Resources = resources;
        result.ParsedScopes = requestedScopes.ToList();
        
        return result;
    }

    /// <summary>
    /// Gets the audience claims for the specified resources
    /// </summary>
    public async Task<List<string>> GetAudiencesAsync(IEnumerable<string> resources)
    {
        var audiences = new List<string>();
        
        foreach (var resourceIndicator in resources)
        {
            var resource = await _apiResourceStore.FindApiResourceAsync(resourceIndicator);
            if (resource != null)
            {
                audiences.Add(resource.Name);
            }
        }
        
        return audiences.Distinct().ToList();
    }
}
