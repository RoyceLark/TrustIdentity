using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("TrustIdentity Tester Service");
        Console.WriteLine("============================");

        var client = new HttpClient();
        var authority = "https://localhost:5001";

        Console.WriteLine($"\n1. Discovering endpoints at {authority}...");
        var disco = await client.GetDiscoveryDocumentAsync(authority);
        
        if (disco.IsError)
        {
            Console.WriteLine($"Error: {disco.Error}");
            return;
        }

        Console.WriteLine($"Discovery Successful!");
        Console.WriteLine($"Token Endpoint: {disco.TokenEndpoint}");
        Console.WriteLine($"UserInfo Endpoint: {disco.UserInfoEndpoint}");

        Console.WriteLine("\n2. Requesting Token (Client Credentials)...");
        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = "api-client",
            ClientSecret = "secret",
            Scope = "api",
            ClientCredentialStyle = ClientCredentialStyle.PostBody
        });

        if (tokenResponse.IsError)
        {
            Console.WriteLine($"Error: {tokenResponse.Error}");
            Console.WriteLine($"Description: {tokenResponse.ErrorDescription}");
            return;
        }

        Console.WriteLine($"Token Retrieved!");
        Console.WriteLine($"Access Token: {tokenResponse.AccessToken.Substring(0, 20)}... (truncated)");

        Console.WriteLine("\n3. Testing API Access (Simulated)...");
        // In a real scenario, we would use the token to call an API
        client.SetBearerToken(tokenResponse.AccessToken);
        // Simulate call (e.g., to userinfo logic if supported)
        
        Console.WriteLine("\n4. Requesting Token (Resource Owner Password)...");
        var passwordToken = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = "web-client",
            ClientSecret = "secret",
            UserName = "alice",
            Password = "password",
            Scope = "openid profile email api",
            ClientCredentialStyle = ClientCredentialStyle.PostBody
        });

        if (passwordToken.IsError)
        {
             Console.WriteLine($"Error: {passwordToken.Error}");
             // Note: This might fail if the endpoint logic isn't fully wired for ROPC yet
             Console.WriteLine("Note: ROPC might not be fully active in this build configuration."); 
        }
        else
        {
            Console.WriteLine("User Token Retrieved!");
            Console.WriteLine($"Access Token: {passwordToken.AccessToken.Substring(0, 20)}...");
            
            Console.WriteLine("\n5. Calling UserInfo Endpoint...");
            var userInfo = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = passwordToken.AccessToken
            });
            
            if (userInfo.IsError)
            {
                Console.WriteLine($"UserInfo Error: {userInfo.Error}");
            }
            else
            {
                Console.WriteLine("UserInfo Claims:");
                foreach (var claim in userInfo.Claims)
                {
                    Console.WriteLine($"- {claim.Type}: {claim.Value}");
                }
            }
        }

        Console.WriteLine("\nTest Run Complete. Use this service to validate deployment.");
    }
}
