using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrustIdentity.Saml.Services;
using System.Security.Claims;
using System.Web;

namespace TrustIdentity.Saml.Endpoints;

/// <summary>
/// SAML SSO Endpoint (Identity Provider)
/// </summary>
public static class SamlSsoEndpoint
{
    /// <summary>
    /// Handles SAML Single Sign-On requests
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<SamlIdentityProvider>>();
        var samlIdp = context.RequestServices.GetRequiredService<SamlIdentityProvider>();
        var config = context.RequestServices.GetRequiredService<SamlIdentityProviderConfig>();

        try
        {
            // Get SAMLRequest parameter
            var samlRequest = context.Request.Form["SAMLRequest"].ToString();
            if (string.IsNullOrEmpty(samlRequest))
            {
                samlRequest = context.Request.Query["SAMLRequest"].ToString();
            }

            if (string.IsNullOrEmpty(samlRequest))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing SAMLRequest parameter");
                return;
            }

            // Process AuthnRequest
            var authnRequest = samlIdp.ProcessAuthnRequest(samlRequest);
            if (authnRequest == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid SAMLRequest");
                return;
            }

            logger.LogInformation("Processing SAML SSO request from {Issuer}", authnRequest.Issuer);

            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                // Redirect to login with return URL
                var returnUrl = $"/saml/sso?SAMLRequest={HttpUtility.UrlEncode(samlRequest)}";
                context.Response.Redirect($"/login?returnUrl={HttpUtility.UrlEncode(returnUrl)}");
                return;
            }

            // Create SAML Response
            var samlResponse = samlIdp.CreateResponse(context.User, authnRequest, config);
            var encodedResponse = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(samlResponse));

            // Security: Encode destination to prevent XSS
            var destination = authnRequest.AssertionConsumerServiceURL ?? string.Empty;
            
            // Basic validation - in production, check against registered SP metadata
            if (!Uri.TryCreate(destination, UriKind.Absolute, out _))
            {
                logger.LogWarning("Invalid AssertionConsumerServiceURL: {Url}", destination);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid ACS URL");
                return;
            }

            // Create HTML form for POST binding
            var html = CreatePostForm(destination, encodedResponse);

            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing SAML SSO request");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }

    private static string CreatePostForm(string destination, string samlResponse)
    {
        var encoder = System.Text.Encodings.Web.HtmlEncoder.Default;
        var encodedDestination = encoder.Encode(destination);
        var encodedSamlResponse = encoder.Encode(samlResponse);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>SAML POST</title>
</head>
<body onload=""document.forms[0].submit()"">
    <noscript>
        <p>JavaScript is disabled. Please click the button below to continue.</p>
    </noscript>
    <form method=""post"" action=""{encodedDestination}"">
        <input type=""hidden"" name=""SAMLResponse"" value=""{encodedSamlResponse}"" />
        <noscript>
            <button type=""submit"">Continue</button>
        </noscript>
    </form>
</body>
</html>";
    }
}

/// <summary>
/// SAML ACS Endpoint (Service Provider - Assertion Consumer Service)
/// </summary>
public static class SamlAcsEndpoint
{
    /// <summary>
    /// Handles SAML Assertion Consumer Service requests
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<SamlServiceProvider>>();
        var samlSp = context.RequestServices.GetRequiredService<SamlServiceProvider>();
        var config = context.RequestServices.GetRequiredService<SamlServiceProviderConfig>();

        try
        {
            // Get SAMLResponse parameter
            var samlResponse = context.Request.Form["SAMLResponse"].ToString();
            if (string.IsNullOrEmpty(samlResponse))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing SAMLResponse parameter");
                return;
            }

            logger.LogInformation("Processing SAML Response at ACS endpoint");

            // Process and validate response
            var result = samlSp.ProcessResponse(samlResponse, config);

            if (!result.IsValid)
            {
                logger.LogWarning("SAML Response validation failed: {Error}", result.Error);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Authentication failed: {result.Error}");
                return;
            }

            // Create authentication ticket
            var identity = new ClaimsIdentity(result.Claims, "SAML");
            var principal = new ClaimsPrincipal(identity);

            // Sign in user
            await context.SignInAsync("Cookies", principal);

            logger.LogInformation("User {NameId} authenticated via SAML", result.NameId);

            // Redirect to application
            context.Response.Redirect("/");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing SAML ACS request");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }
}

/// <summary>
/// SAML Metadata Endpoint
/// </summary>
public static class SamlMetadataEndpoint
{
    /// <summary>
    /// Handles requests for Identity Provider metadata
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleIdpMetadataAsync(HttpContext context)
    {
        var samlIdp = context.RequestServices.GetRequiredService<SamlIdentityProvider>();
        var config = context.RequestServices.GetRequiredService<SamlIdentityProviderConfig>();

        var metadata = samlIdp.GenerateMetadata(config);

        context.Response.ContentType = "application/samlmetadata+xml";
        await context.Response.WriteAsync(metadata);
    }

    /// <summary>
    /// Handles requests for Service Provider metadata
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the operation</returns>
    public static async Task HandleSpMetadataAsync(HttpContext context)
    {
        var samlSp = context.RequestServices.GetRequiredService<SamlServiceProvider>();
        var config = context.RequestServices.GetRequiredService<SamlServiceProviderConfig>();

        var metadata = samlSp.GenerateMetadata(config);

        context.Response.ContentType = "application/samlmetadata+xml";
        await context.Response.WriteAsync(metadata);
    }
}