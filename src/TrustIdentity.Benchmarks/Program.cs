using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Core.Security;
using TrustIdentity.Core.Services;

namespace TrustIdentity.Benchmarks
{
    /// <summary>
    /// Benchmarks for password hashing operations
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class PasswordHasherBenchmarks
    {
        private readonly PasswordHasher _hasher;
        private readonly User _user;
        private readonly string _password = "ComplexPassword123!@#";

        public PasswordHasherBenchmarks()
        {
            _hasher = new PasswordHasher(NullLogger<PasswordHasher>.Instance);
            _user = new User { SubjectId = "bench_user", Username = "benchmark" };
            var hash = _hasher.HashPassword(_user, _password);
            _user.PasswordHash = hash;
        }

        [Benchmark]
        public string HashPassword() => _hasher.HashPassword(_user, _password);

        [Benchmark]
        public bool VerifyPassword() => _hasher.VerifyPassword(_user, _password);
    }

    /// <summary>
    /// Benchmarks for JWT token signing with different algorithms
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class TokenSigningBenchmarks
    {
        private readonly RsaSecurityKey _rsaKey;
        private readonly ECDsaSecurityKey _ecdsaKey;
        private readonly SigningCredentials _rsaCredentials;
        private readonly SigningCredentials _ecdsaCredentials;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly ClaimsIdentity _identity;

        public TokenSigningBenchmarks()
        {
            // RSA 2048
            var rsa = RSA.Create(2048);
            _rsaKey = new RsaSecurityKey(rsa);
            _rsaCredentials = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);

            // ECDSA P-256
            var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            _ecdsaKey = new ECDsaSecurityKey(ecdsa);
            _ecdsaCredentials = new SigningCredentials(_ecdsaKey, SecurityAlgorithms.EcdsaSha256);

            _handler = new JwtSecurityTokenHandler();
            
            _identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", "1234567890"),
                new Claim("name", "John Doe"),
                new Claim("email", "john@example.com"),
                new Claim("role", "admin"),
                new Claim("scope", "openid profile email api1")
            });
        }

        [Benchmark]
        public string SignJwtRsa256()
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = _identity,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = _rsaCredentials,
                Issuer = "https://benchmark.trustidentity",
                Audience = "benchmark_api"
            };
            
            var token = _handler.CreateToken(descriptor);
            return _handler.WriteToken(token);
        }

        [Benchmark]
        public string SignJwtES256()
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = _identity,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = _ecdsaCredentials,
                Issuer = "https://benchmark.trustidentity",
                Audience = "benchmark_api"
            };
            
            var token = _handler.CreateToken(descriptor);
            return _handler.WriteToken(token);
        }

        [Benchmark]
        public ClaimsPrincipal ValidateJwtRsa256()
        {
            var tokenString = SignJwtRsa256();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://benchmark.trustidentity",
                ValidateAudience = true,
                ValidAudience = "benchmark_api",
                ValidateLifetime = true,
                IssuerSigningKey = _rsaKey
            };

            return _handler.ValidateToken(tokenString, validationParameters, out _);
        }
    }

    /// <summary>
    /// Benchmarks for AI fraud detection
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class FraudDetectionBenchmarks
    {
        private readonly TrustIdentity.AI.Analyzers.FraudDetectionService _service;
        private readonly string _userId = "bench_user";
        private readonly string _ip = "127.0.0.1";
        private readonly string _ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        public FraudDetectionBenchmarks()
        {
            var userStore = new MockUserStore();
            _service = new TrustIdentity.AI.Analyzers.FraudDetectionService(
                NullLogger<TrustIdentity.AI.Analyzers.FraudDetectionService>.Instance, 
                userStore);
        }

        [Benchmark]
        public async Task<double> AnalyzeLogin() => 
            await _service.AnalyzeLoginAttemptAsync(_userId, _ip, _ua);


    }

    /// <summary>
    /// Benchmarks for multi-tenant resolution
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class TenantResolutionBenchmarks
    {
        private readonly TrustIdentity.AspNetCore.Services.CompositeTenantResolver _resolver;
        private readonly Microsoft.AspNetCore.Http.DefaultHttpContext _cookieContext;
        private readonly Microsoft.AspNetCore.Http.DefaultHttpContext _hostContext;
        private readonly Microsoft.AspNetCore.Http.DefaultHttpContext _headerContext;

        public TenantResolutionBenchmarks()
        {
            var tenantStore = new MockTenantStore();
            _resolver = new TrustIdentity.AspNetCore.Services.CompositeTenantResolver(
                tenantStore,
                NullLogger<TrustIdentity.AspNetCore.Services.CompositeTenantResolver>.Instance);

            // Setup Context with Cookie
            _cookieContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var cookieCollection = new SimpleCookieCollection(
                new Dictionary<string, string> { { "Ti-Tenant-Id", "tenant_1" } });
            _cookieContext.Request.Cookies = cookieCollection;

            // Setup Context with Host
            _hostContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            _hostContext.Request.Host = new Microsoft.AspNetCore.Http.HostString("tenant_1.local");

            // Setup Context with Header
            _headerContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            _headerContext.Request.Headers["X-Tenant-Id"] = "tenant_1";
        }

        [Benchmark]
        public async Task ResolveFromCookie() => await _resolver.ResolveAsync(_cookieContext);

        [Benchmark]
        public async Task ResolveFromHost() => await _resolver.ResolveAsync(_hostContext);

        [Benchmark]
        public async Task ResolveFromHeader() => await _resolver.ResolveAsync(_headerContext);
    }

    /// <summary>
    /// Benchmarks for authorization code operations
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class AuthorizationCodeBenchmarks
    {
        private readonly string _code;
        private readonly Dictionary<string, AuthorizationCode> _codeStore;

        public AuthorizationCodeBenchmarks()
        {
            _code = GenerateAuthorizationCode();
            _codeStore = new Dictionary<string, AuthorizationCode>();
        }

        [Benchmark]
        public string GenerateCode() => GenerateAuthorizationCode();

        [Benchmark]
        public void StoreCode()
        {
            var code = GenerateAuthorizationCode();
            _codeStore[code] = new AuthorizationCode
            {
                Code = code,
                ClientId = "test_client",
                SubjectId = "test_user",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(300)
            };
        }

        [Benchmark]
        public AuthorizationCode? RetrieveCode()
        {
            _codeStore.TryGetValue(_code, out var authCode);
            return authCode;
        }

        private static string GenerateAuthorizationCode()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

    /// <summary>
    /// Benchmarks for client validation
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class ClientValidationBenchmarks
    {
        private readonly Client _client;
        private readonly string _clientSecret = "super_secret_value";
        private readonly string _hashedSecret;

        public ClientValidationBenchmarks()
        {
            _client = new Client
            {
                ClientId = "test_client",
                ClientName = "Test Client",
                AllowedGrantTypes = new List<string> { "authorization_code" },
                RedirectUris = new List<string> { "https://localhost:5002/signin-oidc" },
                AllowedScopes = new List<string> { "openid", "profile", "email", "api1" },
                RequirePkce = true,
                RequireClientSecret = true
            };

            using var sha256 = SHA256.Create();
            _hashedSecret = Convert.ToBase64String(
                sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_clientSecret)));
        }

        [Benchmark]
        public bool ValidateRedirectUri()
        {
            var uri = "https://localhost:5002/signin-oidc";
            return _client.RedirectUris.Contains(uri);
        }

        [Benchmark]
        public bool ValidateScope()
        {
            var requestedScopes = new[] { "openid", "profile", "api1" };
            return requestedScopes.All(s => _client.AllowedScopes.Contains(s));
        }

        [Benchmark]
        public bool ValidateSecret()
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(
                sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_clientSecret)));
            return hash == _hashedSecret;
        }
    }

    /// <summary>
    /// Benchmarks for claim operations
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class ClaimsBenchmarks
    {
        private readonly ClaimsPrincipal _principal;
        private readonly List<Claim> _claims;

        public ClaimsBenchmarks()
        {
            _claims = new List<Claim>
            {
                new Claim("sub", "1234567890"),
                new Claim("name", "John Doe"),
                new Claim("email", "john@example.com"),
                new Claim("role", "admin"),
                new Claim("role", "user"),
                new Claim("scope", "openid"),
                new Claim("scope", "profile"),
                new Claim("scope", "email"),
                new Claim("scope", "api1")
            };

            var identity = new ClaimsIdentity(_claims, "Bearer");
            _principal = new ClaimsPrincipal(identity);
        }

        [Benchmark]
        public Claim? FindClaim() => _principal.FindFirst("sub");

        [Benchmark]
        public IEnumerable<Claim> FindAllClaims() => _principal.FindAll("role");

        [Benchmark]
        public bool HasClaim() => _principal.HasClaim("scope", "api1");

        [Benchmark]
        public ClaimsPrincipal CreatePrincipal()
        {
            var identity = new ClaimsIdentity(_claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
    }

    /// <summary>
    /// Benchmarks for rate limiting
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class RateLimitingBenchmarks
    {
        private readonly Dictionary<string, (int Count, DateTime Window)> _rateLimits;
        private readonly string _clientId = "test_client";
        private readonly int _maxRequests = 100;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

        public RateLimitingBenchmarks()
        {
            _rateLimits = new Dictionary<string, (int, DateTime)>();
        }

        [Benchmark]
        public bool CheckRateLimit()
        {
            var now = DateTime.UtcNow;
            
            if (_rateLimits.TryGetValue(_clientId, out var limit))
            {
                if (now - limit.Window < _window)
                {
                    if (limit.Count >= _maxRequests)
                        return false;
                    
                    _rateLimits[_clientId] = (limit.Count + 1, limit.Window);
                }
                else
                {
                    _rateLimits[_clientId] = (1, now);
                }
            }
            else
            {
                _rateLimits[_clientId] = (1, now);
            }

            return true;
        }
    }

    // Helper Classes
    public class SimpleCookieCollection : Microsoft.AspNetCore.Http.IRequestCookieCollection, 
        IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _cookies;
        
        public SimpleCookieCollection(Dictionary<string, string> cookies) => _cookies = cookies;
        
        string? Microsoft.AspNetCore.Http.IRequestCookieCollection.this[string key] => 
            _cookies.TryGetValue(key, out var value) ? value : null;
        
        public int Count => _cookies.Count;
        public ICollection<string> Keys => _cookies.Keys;
        public bool ContainsKey(string key) => _cookies.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _cookies.GetEnumerator();
        
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
        {
            var res = _cookies.TryGetValue(key, out var v);
            value = v;
            return res;
        }
    }

    // Mock Stores
    public class MockUserStore : Abstractions.Stores.IUserStore
    {
        public Task<User?> FindBySubjectIdAsync(string subjectId) => 
            Task.FromResult<User?>(new User { SubjectId = subjectId, FailedLoginAttempts = 0 });
        
        public Task AddUserAsync(User user, string password) => Task.CompletedTask;
        public Task DeleteUserAsync(string subjectId) => Task.CompletedTask;
        
        public Task<(IEnumerable<User> Users, int TotalCount)> GetAllUsersAsync(
            string? search = null, int skip = 0, int take = 20) => 
            Task.FromResult((Enumerable.Empty<User>(), 0));
        
        public Task IncrementFailedAttemptsAsync(string subjectId) => Task.CompletedTask;
        public Task LockAccountAsync(string subjectId, DateTimeOffset? lockoutEnd) => Task.CompletedTask;
        public Task ResetFailedAttemptsAsync(string subjectId) => Task.CompletedTask;
        public Task SetPasswordAsync(string subjectId, string password) => Task.CompletedTask;
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> ValidateCredentialsAsync(string username, string password) => Task.FromResult(true);
        public Task<User?> FindByUsernameAsync(string username) => Task.FromResult<User?>(null);
    }

    public class MockTenantStore : Abstractions.Stores.ITenantStore
    {
        private readonly Tenant _tenant = new Tenant 
        { 
            Id = "tenant_1", 
            Identifier = "tenant_1", 
            Host = "tenant_1.local" 
        };
        
        public Task<Tenant?> GetByHostAsync(string host) => Task.FromResult<Tenant?>(_tenant);
        public Task<Tenant?> GetByIdAsync(string id) => Task.FromResult<Tenant?>(_tenant);
        public Task<Tenant?> GetByIdentifierAsync(string identifier) => Task.FromResult<Tenant?>(_tenant);
        public Task<Tenant> CreateAsync(Tenant tenant) => Task.FromResult(tenant);
        public Task<bool> DeleteAsync(string id) => Task.FromResult(true);
        
        public Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 100) => 
            Task.FromResult<IEnumerable<Tenant>>(new List<Tenant> { _tenant });
        
        public Task<Tenant> UpdateAsync(Tenant tenant) => Task.FromResult(tenant);
        public Task<int> GetCountAsync() => Task.FromResult(1);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
