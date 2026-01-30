using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Stores;

namespace TrustIdentity.AspNetCore.Endpoints;

/// <summary>
/// Pushed Authorization Request endpoint (RFC 9126)
/// </summary>
public static class PushedAuthorizationEndpoint
{
    /// <summary>
    /// Handles pushed authorization requests
    /// </summary>
    public static async Task HandleAsync(
        HttpContext context,
        IPushedAuthorizationService parService,
        IClientStore clientStore,
        ILogger logger)
    {
        if (context.Request.Method != "POST")
        {
            context.Response.StatusCode = 405;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "invalid_request",
                error_description = "Only POST method is allowed"
            });
            return;
        }

        // Read form parameters
        var form = await context.Request.ReadFormAsync();
        var parameters = form.ToDictionary(k => k.Key, v => v.Value.ToString());

        // Extract client credentials
        var clientId = parameters.GetValueOrDefault("client_id");
        var clientSecret = parameters.GetValueOrDefault("client_secret");

        // Try Basic authentication if not in form
        if (string.IsNullOrEmpty(clientId))
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Basic "))
            {
                var credentials = System.Text.Encoding.UTF8.GetString(
                    System.Convert.FromBase64String(authHeader.Substring(6))).Split(':');
                if (credentials.Length == 2)
                {
                    clientId = credentials[0];
                    clientSecret = credentials[1];
                }
            }
        }

        if (string.IsNullOrEmpty(clientId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "invalid_client",
                error_description = "Client authentication failed"
            });
            return;
        }

        // Validate client
        var client = await clientStore.FindClientByIdAsync(clientId);
        if (client == null)
        {
            logger.LogWarning("PAR request from unknown client: {ClientId}", clientId);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "invalid_client",
                error_description = "Client not found"
            });
            return;
        }

        // Validate client secret if required
        if (client.RequireClientSecret)
        {
            if (string.IsNullOrEmpty(clientSecret))
            {
                logger.LogWarning("PAR request without secret for client: {ClientId}", clientId);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_client",
                    error_description = "Client secret required"
                });
                return;
            }
            
            // Validate secret by hashing and comparing
            var isValid = false;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(clientSecret);
                var hash = sha.ComputeHash(bytes);
                var hashedSecret = System.Convert.ToBase64String(hash);
                
                foreach (var secret in client.ClientSecrets)
                {
                    if (secret.Value == hashedSecret)
                    {
                        // Check expiration
                        if (secret.Expiration == null || secret.Expiration > System.DateTime.UtcNow)
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
            }
            
            if (!isValid)
            {
                logger.LogWarning("PAR request with invalid secret for client: {ClientId}", clientId);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_client",
                    error_description = "Invalid client secret"
                });
                return;
            }
        }

        // Remove client credentials from parameters (they shouldn't be in the request URI)
        parameters.Remove("client_id");
        parameters.Remove("client_secret");

        // Validate required parameters
        if (!parameters.ContainsKey("response_type"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "invalid_request",
                error_description = "Missing required parameter: response_type"
            });
            return;
        }

        // Store the request and get request URI
        var response = await parService.StorePushedRequestAsync(parameters, clientId);

        logger.LogInformation("PAR request successful for client {ClientId}, request_uri: {RequestUri}", 
            clientId, response.RequestUri);

        // Return success response
        context.Response.StatusCode = 201;
        context.Response.Headers["Cache-Control"] = "no-store";
        await context.Response.WriteAsJsonAsync(new
        {
            request_uri = response.RequestUri,
            expires_in = response.ExpiresIn
        });
    }
}
