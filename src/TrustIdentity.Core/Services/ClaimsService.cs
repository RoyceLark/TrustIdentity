using System.Security.Claims;
using TrustIdentity.Core.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for processing and filtering claims based on scopes
/// </summary>
public class ClaimsService
{
    private readonly ILogger<ClaimsService> _logger;

    /// <summary>
    /// Initializes a new instance of the ClaimsService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ClaimsService(ILogger<ClaimsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets claims for a specific scope from a user principal
    /// </summary>
    /// <param name="scope">The scope to filter for</param>
    /// <param name="subject">The user principal</param>
    /// <returns>A collection of claims associated with the scope</returns>
    public async Task<IEnumerable<Claim>> GetClaimsForScopeAsync(
        string scope, 
        ClaimsPrincipal subject)
    {
        var claims = new List<Claim>();

        switch (scope)
        {
            case "openid":
                claims.Add(new Claim("sub", subject.FindFirst("sub")?.Value ?? ""));
                break;

            case "profile":
                AddIfPresent(claims, subject, "name");
                AddIfPresent(claims, subject, "family_name");
                AddIfPresent(claims, subject, "given_name");
                AddIfPresent(claims, subject, "middle_name");
                AddIfPresent(claims, subject, "nickname");
                AddIfPresent(claims, subject, "preferred_username");
                AddIfPresent(claims, subject, "profile");
                AddIfPresent(claims, subject, "picture");
                AddIfPresent(claims, subject, "website");
                AddIfPresent(claims, subject, "gender");
                AddIfPresent(claims, subject, "birthdate");
                AddIfPresent(claims, subject, "zoneinfo");
                AddIfPresent(claims, subject, "locale");
                AddIfPresent(claims, subject, "updated_at");
                break;

            case "email":
                AddIfPresent(claims, subject, "email");
                AddIfPresent(claims, subject, "email_verified");
                break;

            case "phone":
                AddIfPresent(claims, subject, "phone_number");
                AddIfPresent(claims, subject, "phone_number_verified");
                break;

            case "address":
                AddIfPresent(claims, subject, "address");
                break;
        }

        return await Task.FromResult(claims);
    }

    private void AddIfPresent(List<Claim> claims, ClaimsPrincipal subject, string claimType)
    {
        var claim = subject.FindFirst(claimType);
        if (claim != null)
        {
            claims.Add(claim);
        }
    }
}