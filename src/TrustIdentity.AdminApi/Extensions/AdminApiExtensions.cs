using Microsoft.Extensions.DependencyInjection;
using TrustIdentity.AdminApi.Controllers;

namespace TrustIdentity.AdminApi.Extensions;

/// <summary>
/// Extension methods for setting up TrustIdentity Admin API services in an <see cref="IServiceCollection" />.
/// </summary>
public static class AdminApiExtensions
{
    /// <summary>
    /// Adds the TrustIdentity Admin API services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns></returns>
    public static IServiceCollection AddTrustIdentityAdminApi(this IServiceCollection services)
    {
        // Add controllers from this assembly and configure JSON options
        services.AddControllers()
            .AddApplicationPart(typeof(ClientsController).Assembly)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

        // Add authorization policy if not already present
        services.AddAuthorization(options =>
        {
            if (options.GetPolicy("AdminApiAccess") == null)
            {
                options.AddPolicy("AdminApiAccess", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "admin_api");
                });
            }
        });

        return services;
    }
}
