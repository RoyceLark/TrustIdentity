using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Web;
using TrustIdentity.WsFederation.Models;
using TrustIdentity.WsFederation.Services;

namespace TrustIdentity.WsFederation.Endpoints;

/// <summary>
/// WS-Federation Sign-In Endpoint
/// </summary>
public static class WsFederationSignInEndpoint
{
    /// <summary>
    /// Handles the WS-Federation sign-in request
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<WsFederationIdentityProvider>>();
        var wsfedIdp = context.RequestServices.GetRequiredService<WsFederationIdentityProvider>();
        var config = context.RequestServices.GetRequiredService<WsFederationConfiguration>();

        try
        {
            // Parse WS-Federation parameters
            var request = ParseRequest(context.Request);

            // Validate action
            if (request.Wa != WsFederationConstants.Actions.SignIn)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Unsupported action: {request.Wa}");
                return;
            }

            logger.LogInformation("Processing WS-Federation sign-in for realm: {Realm}", request.Wtrealm);

            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                // Redirect to login
                var returnUrl = context.Request.QueryString.ToString();
                context.Response.Redirect($"/login?returnUrl={HttpUtility.UrlEncode(returnUrl)}");
                return;
            }

            // Process sign-in
            var response = wsfedIdp.ProcessSignInRequest(request, context.User, config);

            // Return auto-posting form
            var html = CreatePostForm(request.Wreply ?? request.Wtrealm!, response);
            
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        }
        catch (WsFederationException ex)
        {
            logger.LogWarning(ex, "WS-Federation validation error");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing WS-Federation request");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }

    private static WsFederationSignInRequest ParseRequest(HttpRequest request)
    {
        return new WsFederationSignInRequest
        {
            Wa = request.Query[WsFederationConstants.Parameters.Action].ToString(),
            Wtrealm = request.Query[WsFederationConstants.Parameters.Realm].ToString(),
            Wreply = request.Query[WsFederationConstants.Parameters.Reply].ToString(),
            Wctx = request.Query[WsFederationConstants.Parameters.Context].ToString(),
            Wct = request.Query[WsFederationConstants.Parameters.CurrentTime].ToString(),
            Whr = request.Query[WsFederationConstants.Parameters.HomeRealm].ToString(),
            Wreq = request.Query[WsFederationConstants.Parameters.Request].ToString(),
            Wfresh = request.Query[WsFederationConstants.Parameters.Freshness].ToString(),
            Wauth = request.Query[WsFederationConstants.Parameters.AuthenticationType].ToString()
        };
    }

    private static string CreatePostForm(string action, WsFederationSignInResponse response)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <title>Working...</title>
</head>
<body onload=""document.forms[0].submit()"">
    <noscript>
        <p>JavaScript is disabled. Please click Continue.</p>
    </noscript>
    <form method=""post"" action=""{action}"">
        <input type=""hidden"" name=""{WsFederationConstants.Parameters.Action}"" value=""{response.Wa}"" />
        <input type=""hidden"" name=""{WsFederationConstants.Parameters.Result}"" value=""{HttpUtility.HtmlEncode(response.Wresult)}"" />
        {(string.IsNullOrEmpty(response.Wctx) ? "" : $@"<input type=""hidden"" name=""{WsFederationConstants.Parameters.Context}"" value=""{HttpUtility.HtmlEncode(response.Wctx)}"" />")}
        <noscript>
            <button type=""submit"">Continue</button>
        </noscript>
    </form>
</body>
</html>";
    }
}

/// <summary>
/// WS-Federation Metadata Endpoint
/// </summary>
public static class WsFederationMetadataEndpoint
{
    /// <summary>
    /// Handles the WS-Federation metadata request
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var wsfedIdp = context.RequestServices.GetRequiredService<WsFederationIdentityProvider>();
        var config = context.RequestServices.GetRequiredService<WsFederationConfiguration>();

        var metadata = wsfedIdp.GenerateMetadata(config);

        context.Response.ContentType = "application/xml";
        await context.Response.WriteAsync(metadata);
    }
}