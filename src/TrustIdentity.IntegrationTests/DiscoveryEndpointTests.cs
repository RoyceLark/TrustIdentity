using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.AspNetCore.Extensions;
using Xunit;

namespace TrustIdentity.IntegrationTests
{
    public class DiscoveryEndpointTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public DiscoveryEndpointTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddTrustIdentity(options =>
                    {
                        options.IssuerUri = "https://localhost";
                    })
                    .AddDeveloperSigningCredential()
                    .AddInMemoryClients(new System.Collections.Generic.List<Client>())
                    .AddInMemoryApiResources(new System.Collections.Generic.List<ApiResource>())
                    .AddInMemoryApiScopes(new System.Collections.Generic.List<ApiScope>())
                    .AddInMemoryIdentityResources(new System.Collections.Generic.List<IdentityResource>());
                    
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        TrustIdentity.AspNetCore.Endpoints.TrustIdentityEndpoints.MapEndpoints(endpoints);
                    });
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetDiscoveryDocument_Returns200AndCorrectJson()
        {
            var response = await _client.GetAsync("/.well-known/openid-configuration");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("https://localhost/connect/authorize", json);
            Assert.Contains("https://localhost/connect/token", json);
            // Verify new features
            Assert.Contains("pushed_authorization_request_endpoint", json);
            Assert.Contains("registration_endpoint", json);
            Assert.Contains("dpop_signing_alg_values_supported", json);
            Assert.Contains("tls_client_certificate_bound_access_tokens", json);
        }

        public void Dispose()
        {
            _server.Dispose();
            _client.Dispose();
        }
    }
}
