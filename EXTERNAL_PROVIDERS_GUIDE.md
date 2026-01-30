# External Authentication Providers Guide

## Overview

TrustIdentity supports external authentication providers, allowing users to sign in with their existing accounts from Azure AD, Azure AD B2C, Google, Facebook, GitHub, and other OAuth 2.0/OpenID Connect providers.

## Supported Providers

- ✅ **Azure AD** - Enterprise identity (Microsoft Entra ID)
- ✅ **Azure AD B2C** - Consumer identity with custom policies
- ✅ **Google** - Google accounts
- ✅ **Facebook** - Facebook accounts
- ✅ **GitHub** - GitHub accounts
- ✅ **Generic OIDC** - Any OpenID Connect provider

## Quick Start

### 1. Install Package

The external providers are included in the `TrustIdentity.ExternalProviders` package:

```bash
dotnet add package TrustIdentity.ExternalProviders
```

### 2. Configure Provider

In your `appsettings.json`:

```json
{
  "ExternalProviders": {
    "AzureADB2C": {
      "Instance": "myb2ctenant.b2clogin.com",
      "Domain": "myb2ctenant.onmicrosoft.com",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "RedirectUri": "https://yourapp.com/signin-azureadb2c",
      "SignUpSignInPolicyId": "B2C_1_signupsignin1",
      "Scopes": ["openid", "profile", "email"]
    },
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret",
      "RedirectUri": "https://yourapp.com/signin-google"
    }
  }
}
```

### 3. Register Provider

In your `Program.cs`:

```csharp
using TrustIdentity.ExternalProviders;
using TrustIdentity.ExternalProviders.Azure;
using TrustIdentity.ExternalProviders.Google;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure AD B2C
var b2cConfig = builder.Configuration
    .GetSection("ExternalProviders:AzureADB2C")
    .Get<AzureADB2CConfiguration>();

builder.Services.AddSingleton(b2cConfig!);
builder.Services.AddScoped<IExternalAuthenticationProvider, AzureADB2CProvider>();

// Configure Google
var googleConfig = builder.Configuration
    .GetSection("ExternalProviders:Google")
    .Get<GoogleConfiguration>();

builder.Services.AddSingleton(googleConfig!);
builder.Services.AddScoped<IExternalAuthenticationProvider, GoogleProvider>();

var app = builder.Build();
app.Run();
```

## Azure AD B2C Setup

### 1. Create Azure AD B2C Tenant

1. Go to [Azure Portal](https://portal.azure.com)
2. Create a new Azure AD B2C tenant
3. Note your tenant name (e.g., `myb2ctenant`)

### 2. Register Application

1. In your B2C tenant, go to **App registrations**
2. Click **New registration**
3. Enter application name
4. Set redirect URI: `https://yourapp.com/signin-azureadb2c`
5. Click **Register**
6. Note the **Application (client) ID**

### 3. Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add description and expiration
4. **Copy the secret value immediately** (you won't see it again)

### 4. Create User Flows

1. Go to **User flows**
2. Click **New user flow**
3. Select **Sign up and sign in**
4. Choose version (Recommended)
5. Enter name (e.g., `signupsignin1`)
6. Select identity providers (Email, Google, Facebook, etc.)
7. Select user attributes to collect
8. Select application claims to return
9. Click **Create**

### 5. Configure Application

```csharp
var b2cConfig = new AzureADB2CConfiguration
{
    Instance = "myb2ctenant.b2clogin.com",
    Domain = "myb2ctenant.onmicrosoft.com",
    ClientId = "your-client-id-from-step-2",
    ClientSecret = "your-client-secret-from-step-3",
    RedirectUri = "https://yourapp.com/signin-azureadb2c",
    SignUpSignInPolicyId = "B2C_1_signupsignin1",  // From step 4
    Scopes = new List<string> { "openid", "profile", "email" }
};
```

## Using External Providers

### Login Flow

```csharp
using TrustIdentity.ExternalProviders;

public class AccountController : Controller
{
    private readonly IExternalAuthenticationProvider _provider;
    
    public AccountController(IExternalAuthenticationProvider provider)
    {
        _provider = provider;
    }
    
    // Step 1: Redirect to external provider
    public IActionResult ExternalLogin(string provider, string returnUrl)
    {
        var state = GenerateState(returnUrl);  // CSRF protection
        var authUrl = _provider.GetAuthorizationUrl(state);
        return Redirect(authUrl);
    }
    
    // Step 2: Handle callback from provider
    public async Task<IActionResult> ExternalLoginCallback(string code, string state)
    {
        // Validate state for CSRF protection
        if (!ValidateState(state))
        {
            return BadRequest("Invalid state");
        }
        
        // Exchange code for tokens
        var result = await _provider.AuthenticateAsync(code);
        
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        
        // Create or update local user
        var user = await GetOrCreateUser(result);
        
        // Sign in the user
        await SignInAsync(user);
        
        var returnUrl = GetReturnUrlFromState(state);
        return Redirect(returnUrl);
    }
}
```

### Account Linking

Allow users to link multiple external providers:

```csharp
public class ExternalLoginService
{
    public async Task LinkExternalAccount(
        string userId, 
        string provider, 
        string providerUserId)
    {
        var link = new ExternalLoginLink
        {
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            LinkedAt = DateTime.UtcNow
        };
        
        await _context.ExternalLogins.AddAsync(link);
        await _context.SaveChangesAsync();
    }
    
    public async Task<User?> FindByExternalLogin(
        string provider, 
        string providerUserId)
    {
        var link = await _context.ExternalLogins
            .FirstOrDefaultAsync(l => 
                l.Provider == provider && 
                l.ProviderUserId == providerUserId);
                
        if (link == null) return null;
        
        return await _context.Users.FindAsync(link.UserId);
    }
}
```

## Google Provider Setup

### 1. Create Google OAuth Client

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create a new project or select existing
3. Enable **Google+ API**
4. Go to **Credentials**
5. Click **Create Credentials** → **OAuth client ID**
6. Select **Web application**
7. Add authorized redirect URI: `https://yourapp.com/signin-google`
8. Click **Create**
9. Copy **Client ID** and **Client Secret**

### 2. Configure in Application

```csharp
var googleConfig = new GoogleConfiguration
{
    ClientId = "your-client-id.apps.googleusercontent.com",
    ClientSecret = "your-client-secret",
    RedirectUri = "https://yourapp.com/signin-google",
    Scopes = new List<string> 
    { 
        "openid", 
        "profile", 
        "email" 
    }
};
```

## Custom OIDC Provider

For any OpenID Connect provider:

```csharp
public class CustomOidcProvider : IExternalAuthenticationProvider
{
    private readonly CustomOidcConfiguration _config;
    
    public string ProviderName => "CustomOIDC";
    
    public string GetAuthorizationUrl(string state, string? redirectUri = null)
    {
        var authUrl = $"{_config.Authority}/authorize";
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = redirectUri ?? _config.RedirectUri,
            ["scope"] = string.Join(" ", _config.Scopes),
            ["state"] = state
        };
        
        var queryString = string.Join("&", parameters.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            
        return $"{authUrl}?{queryString}";
    }
    
    public async Task<ExternalAuthenticationResult> AuthenticateAsync(
        string code, 
        string? redirectUri = null)
    {
        // Implement token exchange
        // Parse ID token
        // Return user info
    }
}
```

## UI Integration

### Login Page with External Providers

```html
<div class="external-providers">
    <h3>Or sign in with:</h3>
    
    <a href="/Account/ExternalLogin?provider=AzureADB2C&returnUrl=@Model.ReturnUrl" 
       class="btn btn-primary">
        <i class="fab fa-microsoft"></i> Microsoft Account
    </a>
    
    <a href="/Account/ExternalLogin?provider=Google&returnUrl=@Model.ReturnUrl" 
       class="btn btn-danger">
        <i class="fab fa-google"></i> Google
    </a>
    
    <a href="/Account/ExternalLogin?provider=GitHub&returnUrl=@Model.ReturnUrl" 
       class="btn btn-dark">
        <i class="fab fa-github"></i> GitHub
    </a>
</div>
```

## Security Best Practices

### 1. State Parameter (CSRF Protection)

Always use a cryptographically secure state parameter:

```csharp
public string GenerateState(string returnUrl)
{
    var state = new
    {
        Nonce = Guid.NewGuid().ToString(),
        ReturnUrl = returnUrl,
        Timestamp = DateTime.UtcNow.Ticks
    };
    
    var json = JsonSerializer.Serialize(state);
    var encrypted = _dataProtector.Protect(json);
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(encrypted));
}

public bool ValidateState(string state)
{
    try
    {
        var bytes = Convert.FromBase64String(state);
        var encrypted = Encoding.UTF8.GetString(bytes);
        var json = _dataProtector.Unprotect(encrypted);
        var stateObj = JsonSerializer.Deserialize<StateObject>(json);
        
        // Check timestamp (prevent replay attacks)
        var age = DateTime.UtcNow - new DateTime(stateObj.Timestamp);
        return age.TotalMinutes < 10;
    }
    catch
    {
        return false;
    }
}
```

### 2. Validate Tokens

Always validate ID tokens:

```csharp
public bool ValidateIdToken(string idToken, string issuer, string audience)
{
    var handler = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = GetSigningKeys(issuer)
    };
    
    try
    {
        handler.ValidateToken(idToken, validationParameters, out _);
        return true;
    }
    catch
    {
        return false;
    }
}
```

### 3. Secure Token Storage

Store external tokens securely:

```csharp
public async Task StoreExternalTokens(
    string userId, 
    string provider, 
    string accessToken, 
    string? refreshToken)
{
    var encryptedAccess = _dataProtector.Protect(accessToken);
    var encryptedRefresh = refreshToken != null 
        ? _dataProtector.Protect(refreshToken) 
        : null;
    
    var tokenStore = new ExternalTokenStore
    {
        UserId = userId,
        Provider = provider,
        AccessToken = encryptedAccess,
        RefreshToken = encryptedRefresh,
        CreatedAt = DateTime.UtcNow
    };
    
    await _context.ExternalTokens.AddAsync(tokenStore);
    await _context.SaveChangesAsync();
}
```

## Troubleshooting

### Redirect URI Mismatch

**Error**: `redirect_uri_mismatch`

**Solution**: Ensure the redirect URI in your configuration exactly matches the one registered in the provider's console.

### Invalid Client

**Error**: `invalid_client`

**Solution**: Verify client ID and client secret are correct.

### Scope Not Granted

**Error**: `invalid_scope`

**Solution**: Check that requested scopes are enabled in the provider's configuration.

## API Reference

### IExternalAuthenticationProvider

- `ProviderName` - Unique provider identifier
- `GetAuthorizationUrl(state, redirectUri)` - Get OAuth authorization URL
- `AuthenticateAsync(code, redirectUri)` - Exchange code for tokens
- `RefreshTokenAsync(refreshToken)` - Refresh access token

### ExternalAuthenticationResult

- `Success` - Whether authentication succeeded
- `Provider` - Provider name
- `ProviderUserId` - User ID from provider
- `Email` - User's email
- `DisplayName` - User's display name
- `AccessToken` - Provider access token
- `RefreshToken` - Provider refresh token
- `TokenExpiration` - When token expires
- `Claims` - User claims from provider
- `ErrorMessage` - Error message if failed
- `ErrorCode` - Error code if failed

## Next Steps

- Configure additional providers (Facebook, GitHub, etc.)
- Implement account linking UI
- Set up token refresh background service
- Configure provider-specific branding
- Implement social login analytics
