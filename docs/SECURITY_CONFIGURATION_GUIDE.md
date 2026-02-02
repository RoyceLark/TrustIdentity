# Security Configuration Guide

This guide provides practical examples for configuring TrustIdentity security features in production.

---

## Table of Contents

1. [HTTP Security Headers](#1-http-security-headers)
2. [Token Security](#2-token-security)
3. [Rate Limiting](#3-rate-limiting)
4. [CORS Configuration](#4-cors-configuration)
5. [HTTPS & Certificates](#5-https--certificates)
6. [Production Checklist](#6-production-checklist)

---

## 1. HTTP Security Headers

### Enable Security Headers Middleware

Add to your `Program.cs`:

```csharp
using TrustIdentity.AspNetCore.Middleware;
using TrustIdentity.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TrustIdentity services
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.yourdomain.com";
    options.RequireHttps = true;
    
    // Enable CSP
    options.Csp.Enabled = true;
    options.Csp.Level = "2";
});

var app = builder.Build();

// Apply security headers (add this BEFORE other middleware)
app.UseMiddleware<SecurityHeadersMiddleware>();

// OR use the extension method
app.UseTrustIdentitySecurityHeaders();

app.Run();
```

### Custom CSP Configuration

If you need to customize the Content Security Policy:

```csharp
app.Use(async (context, next) =>
{
    var csp = "default-src 'self'; " +
              "script-src 'self' https://trusted-cdn.com; " +
              "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
              "img-src 'self' data: https:; " +
              "font-src 'self' https://fonts.gstatic.com; " +
              "connect-src 'self' https://api.yourdomain.com; " +
              "frame-ancestors 'none'; " +
              "form-action 'self'; " +
              "base-uri 'self'; " +
              "object-src 'none';";
    
    context.Response.Headers["Content-Security-Policy"] = csp;
    await next();
});
```

### Verify Headers

Test your headers using:
- https://securityheaders.com
- Browser DevTools → Network tab → Response Headers

Expected headers:
```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
Content-Security-Policy: default-src 'self'; ...
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), camera=()
```

---

## 2. Token Security

### Production Token Configuration

**appsettings.Production.json:**

```json
{
  "TrustIdentity": {
    "IssuerUri": "https://identity.yourdomain.com",
    "RequireHttps": true,
    "Authentication": {
      "AccessTokenLifetime": 900,        // 15 minutes (recommended)
      "RefreshTokenLifetime": 2592000,   // 30 days
      "AuthorizationCodeLifetime": 300,  // 5 minutes
      "MaximumTokenLifetime": 31536000,  // 365 days (hard limit)
      "CookieLifetime": 3600,            // 1 hour
      "CookieSlidingExpiration": true
    }
  }
}
```

### Use X.509 Certificate for Token Signing

**Recommended for Production:**

```csharp
using System.Security.Cryptography.X509Certificates;
using TrustIdentity.Abstractions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load certificate from file
var certPath = builder.Configuration["Certificates:SigningCertPath"];
var certPassword = builder.Configuration["Certificates:SigningCertPassword"];
var certificate = new X509Certificate2(certPath, certPassword);

// OR load from certificate store (Windows)
var certificate = LoadCertificateFromStore("CN=identity.yourdomain.com");

// Add TrustIdentity with certificate
builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://identity.yourdomain.com";
})
.AddSigningCredential(certificate);

// Helper method to load from Windows Certificate Store
X509Certificate2 LoadCertificateFromStore(string subjectName)
{
    using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
    store.Open(OpenFlags.ReadOnly);
    
    var certs = store.Certificates.Find(
        X509FindType.FindBySubjectDistinguishedName,
        subjectName,
        validOnly: true);
    
    if (certs.Count == 0)
        throw new InvalidOperationException($"Certificate not found: {subjectName}");
    
    return certs[0];
}
```

### Secure Key Storage

**Azure Key Vault:**

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;

var keyVaultUrl = builder.Configuration["KeyVault:Url"];
var certificateName = builder.Configuration["KeyVault:CertificateName"];

var client = new CertificateClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
var certificate = await client.DownloadCertificateAsync(certificateName);

builder.Services.AddTrustIdentity(options => { /* ... */ })
    .AddSigningCredential(certificate.Value);
```

### Token Validation Best Practices

```csharp
using TrustIdentity.Abstractions.Services;

public class SecureTokenValidator
{
    private readonly ITokenService _tokenService;
    private readonly ITokenRevocationService _revocationService; // Implement this
    
    public async Task<bool> ValidateTokenSecurely(string token)
    {
        // 1. Validate signature and expiration
        var result = await _tokenService.ValidateTokenDetailedAsync(token);
        if (!result.IsValid)
            return false;
        
        // 2. Check if token is revoked
        var jti = result.Principal.FindFirst("jti")?.Value;
        if (jti != null && await _revocationService.IsRevokedAsync(jti))
            return false;
        
        // 3. Validate audience (if needed)
        var audience = result.Principal.FindFirst("aud")?.Value;
        if (!IsValidAudience(audience))
            return false;
        
        // 4. Check for suspicious patterns
        if (await IsSuspiciousActivity(result.Principal))
            return false;
        
        return true;
    }
    
    private bool IsValidAudience(string? audience)
    {
        var allowedAudiences = new[] { "api1", "api2", "https://identity.yourdomain.com" };
        return audience != null && allowedAudiences.Contains(audience);
    }
    
    private async Task<bool> IsSuspiciousActivity(ClaimsPrincipal principal)
    {
        // Implement fraud detection logic
        // Check IP address, user agent, login patterns, etc.
        return false;
    }
}
```

---

## 3. Rate Limiting

### Basic Rate Limiting Configuration

```csharp
using TrustIdentity.AspNetCore.Middleware;
using TrustIdentity.AspNetCore.Extensions;

var app = builder.Build();

// Apply rate limiting
app.UseTrustIdentityRateLimiting(new RateLimitingOptions
{
    Enabled = true,
    Window = TimeSpan.FromMinutes(1),
    PermitLimit = 60,  // 60 requests per minute
    QueueLimit = 0
});
```

### Advanced: Per-Endpoint Rate Limiting

```csharp
app.UseTrustIdentityRateLimiting(new RateLimitingOptions
{
    Enabled = true,
    EndpointLimits = new Dictionary<string, EndpointRateLimit>
    {
        ["/connect/token"] = new EndpointRateLimit 
        { 
            PermitLimit = 10,  // Stricter for token endpoint
            Window = TimeSpan.FromMinutes(1)
        },
        ["/connect/authorize"] = new EndpointRateLimit 
        { 
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1)
        },
        ["/connect/userinfo"] = new EndpointRateLimit 
        { 
            PermitLimit = 50,
            Window = TimeSpan.FromMinutes(1)
        }
    }
});
```

### Production: Distributed Rate Limiting with Redis

```csharp
using StackExchange.Redis;

// Add Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "TrustIdentity:RateLimit:";
});

// Implement distributed rate limiter
public class RedisRateLimiter
{
    private readonly IDistributedCache _cache;
    
    public async Task<bool> IsAllowedAsync(string clientId, string endpoint, int limit, TimeSpan window)
    {
        var key = $"{clientId}:{endpoint}";
        var countStr = await _cache.GetStringAsync(key);
        var count = int.Parse(countStr ?? "0");
        
        if (count >= limit)
            return false;
        
        await _cache.SetStringAsync(key, (count + 1).ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = window
        });
        
        return true;
    }
}
```

---

## 4. CORS Configuration

### Configure CORS for Production

**appsettings.Production.json:**

```json
{
  "TrustIdentity": {
    "Cors": {
      "CorsPolicyName": "TrustIdentity",
      "AllowedOrigins": [
        "https://app.yourdomain.com",
        "https://admin.yourdomain.com"
      ]
    }
  }
}
```

**Program.cs:**

```csharp
var allowedOrigins = builder.Configuration
    .GetSection("TrustIdentity:Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustIdentity", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

var app = builder.Build();

app.UseCors("TrustIdentity");
```

### Dynamic CORS (Database-driven)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustIdentity", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Check against database or configuration
            using var scope = app.Services.CreateScope();
            var clientStore = scope.ServiceProvider.GetRequiredService<IClientStore>();
            return clientStore.IsOriginAllowedAsync(origin).Result;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

---

## 5. HTTPS & Certificates

### Force HTTPS Redirection

```csharp
var app = builder.Build();

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Add HSTS
app.UseHsts();

app.Run();
```

### Configure HTTPS in appsettings.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/app/certs/identity.pfx",
          "Password": "YourCertPassword"
        }
      }
    }
  }
}
```

### Let's Encrypt with Certbot (Linux)

```bash
# Install Certbot
sudo apt-get update
sudo apt-get install certbot

# Get certificate
sudo certbot certonly --standalone -d identity.yourdomain.com

# Certificate will be at:
# /etc/letsencrypt/live/identity.yourdomain.com/fullchain.pem
# /etc/letsencrypt/live/identity.yourdomain.com/privkey.pem

# Convert to PFX for .NET
sudo openssl pkcs12 -export \
  -out /app/certs/identity.pfx \
  -inkey /etc/letsencrypt/live/identity.yourdomain.com/privkey.pem \
  -in /etc/letsencrypt/live/identity.yourdomain.com/fullchain.pem

# Auto-renewal (add to crontab)
0 0 * * * certbot renew --quiet
```

---

## 6. Production Checklist

### Pre-Deployment Security Checklist

```markdown
## Configuration
- [ ] Set `RequireHttps = true`
- [ ] Configure production `IssuerUri`
- [ ] Set appropriate token lifetimes (15-60 minutes for access tokens)
- [ ] Configure CORS with specific allowed origins (not `*`)
- [ ] Enable CSP with strict policy
- [ ] Enable rate limiting with appropriate limits

## Certificates & Keys
- [ ] Replace development signing key with X.509 certificate
- [ ] Store certificates securely (Key Vault, HSM, or encrypted storage)
- [ ] Set up certificate rotation process
- [ ] Configure certificate expiration monitoring
- [ ] Back up signing certificates securely

## Database & Storage
- [ ] Use persistent database (not in-memory)
- [ ] Enable database encryption at rest
- [ ] Configure database connection string securely (environment variables)
- [ ] Set up database backups
- [ ] Use distributed cache (Redis) for multi-server deployments

## Monitoring & Logging
- [ ] Enable audit logging
- [ ] Configure log retention policy
- [ ] Set up alerts for:
  - Failed authentication attempts
  - Rate limit violations
  - Certificate expiration
  - Unusual traffic patterns
- [ ] Implement security event monitoring

## Network & Infrastructure
- [ ] Configure firewall rules
- [ ] Set up DDoS protection (CloudFlare, AWS Shield, etc.)
- [ ] Use reverse proxy (Nginx, IIS) with security headers
- [ ] Enable TLS 1.2+ only (disable TLS 1.0, 1.1)
- [ ] Configure load balancer health checks

## Testing
- [ ] Run security scan (OWASP ZAP, Burp Suite)
- [ ] Test rate limiting
- [ ] Verify security headers (securityheaders.com)
- [ ] Test SSL/TLS configuration (ssllabs.com)
- [ ] Perform penetration testing
- [ ] Test token validation and revocation

## Documentation
- [ ] Document security incident response procedures
- [ ] Create runbook for certificate rotation
- [ ] Document backup and recovery procedures
- [ ] Create security disclosure policy
- [ ] Document compliance requirements (GDPR, HIPAA, etc.)

## Compliance
- [ ] Review GDPR requirements (if applicable)
- [ ] Review HIPAA requirements (if applicable)
- [ ] Review PCI DSS requirements (if applicable)
- [ ] Conduct security audit
- [ ] Obtain necessary certifications
```

### Environment-Specific Configuration

**Development:**
```json
{
  "TrustIdentity": {
    "RequireHttps": false,
    "Authentication": {
      "AccessTokenLifetime": 3600
    }
  },
  "JwtSettings": {
    "SigningKey": "development-key-min-32-chars-long-12345"
  }
}
```

**Production:**
```json
{
  "TrustIdentity": {
    "IssuerUri": "https://identity.yourdomain.com",
    "RequireHttps": true,
    "Authentication": {
      "AccessTokenLifetime": 900
    }
  },
  "Certificates": {
    "SigningCertPath": "/app/certs/identity.pfx",
    "SigningCertPassword": "${CERT_PASSWORD}"  // From environment variable
  }
}
```

---

## Quick Reference: Security Commands

### Test Security Headers
```bash
curl -I https://identity.yourdomain.com
```

### Test Rate Limiting
```bash
for i in {1..100}; do
  curl -X POST https://identity.yourdomain.com/connect/token \
    -d "grant_type=client_credentials" \
    -d "client_id=test" \
    -d "client_secret=secret"
done
```

### Verify Certificate
```bash
openssl s_client -connect identity.yourdomain.com:443 -showcerts
```

### Check TLS Configuration
```bash
nmap --script ssl-enum-ciphers -p 443 identity.yourdomain.com
```

---

## Support & Resources

- [OWASP Security Headers](https://owasp.org/www-project-secure-headers/)
- [OAuth 2.0 Security Best Practices](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
- [Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)

---

**Last Updated:** 2026-02-02  
**Version:** 1.0
