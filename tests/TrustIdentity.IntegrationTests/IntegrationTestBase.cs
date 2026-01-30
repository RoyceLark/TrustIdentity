using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TrustIdentity.IntegrationTests;

/// <summary>
/// Custom web application factory for testing
/// Note: Integration tests require a running instance of TestWebApp
/// These tests are designed to validate endpoint behavior
/// </summary>
public class TrustIdentityWebApplicationFactory : WebApplicationFactory<TrustIdentityWebApplicationFactory>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://localhost:5555"); // Use different port to avoid conflicts
        
        builder.ConfigureServices(services =>
        {
            // Services will be configured by the test project
        });

        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync("OK");
                });
            });
        });
    }
}

/// <summary>
/// Base class for integration tests
/// NOTE: These tests are designed to run against a live TestWebApp instance
/// Start TestWebApp before running these tests
/// </summary>
public class IntegrationTestBase : IDisposable
{
    protected readonly HttpClient Client;

    public IntegrationTestBase()
    {
        // Create HTTP client that connects to the running TestWebApp
        Client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5001")
        };
    }

    protected async Task<string> GetAccessTokenAsync(
        string grantType,
        string clientId,
        string clientSecret,
        Dictionary<string, string>? additionalParams = null)
    {
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = grantType,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        if (additionalParams != null)
        {
            foreach (var param in additionalParams)
            {
                formData[param.Key] = param.Value;
            }
        }

        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(formData));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Token request failed: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(json);
        
        return tokenResponse?.access_token ?? throw new Exception("No access token in response");
    }

    protected class TokenResponse
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public int expires_in { get; set; }
        public string? refresh_token { get; set; }
        public string? scope { get; set; }
    }

    protected class ErrorResponse
    {
        public string? error { get; set; }
        public string? error_description { get; set; }
    }

    public void Dispose()
    {
        Client?.Dispose();
    }
}
