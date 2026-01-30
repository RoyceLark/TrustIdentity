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
    [MemoryDiagnoser]
    public class PasswordHasherBenchmarks
    {
        private readonly PasswordHasher _hasher;
        private readonly User _user;
        private readonly string _password = "ComplexPassword123!@#";

        public PasswordHasherBenchmarks()
        {
            _hasher = new PasswordHasher(NullLogger<PasswordHasher>.Instance);
            _user = new User { SubjectId = "bench_user", Username = "benchmark" };
        }

        [Benchmark]
        public string HashPassword() => _hasher.HashPassword(_user, _password);
    }

    [MemoryDiagnoser]
    public class TokenSigningBenchmarks
    {
        private readonly RsaSecurityKey _rsaKey;
        private readonly SigningCredentials _rsaCredentials;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly ClaimsIdentity _identity;

        public TokenSigningBenchmarks()
        {
            var rsa = RSA.Create(2048);
            _rsaKey = new RsaSecurityKey(rsa);
            _rsaCredentials = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            _handler = new JwtSecurityTokenHandler();
            
            _identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", "1234567890"),
                new Claim("name", "John Doe"),
                new Claim("admin", "true")
            });
        }

        [Benchmark]
        public string SignJwtRsa256()
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = _identity,
                Expires = System.DateTime.UtcNow.AddHours(1),
                SigningCredentials = _rsaCredentials,
                Issuer = "https://benchmark.trustidentity",
                Audience = "benchmark_api"
            };
            
            var token = _handler.CreateToken(descriptor);
            return _handler.WriteToken(token);
        }
    }

    [MemoryDiagnoser]
    public class FraudDetectionBenchmarks
    {
        private readonly TrustIdentity.AI.Analyzers.FraudDetectionService _service;
        private readonly string _userId = "bench_user";
        private readonly string _ip = "127.0.0.1";
        private readonly string _ua = "Mozilla/5.0";

        public FraudDetectionBenchmarks()
        {
            var userStore = new MockUserStore();
            _service = new TrustIdentity.AI.Analyzers.FraudDetectionService(
                NullLogger<TrustIdentity.AI.Analyzers.FraudDetectionService>.Instance, 
                userStore);
        }

        [Benchmark]
        public async Task<double> AnalyzeLogin() => await _service.AnalyzeLoginAttemptAsync(_userId, _ip, _ua);
    }

    [MemoryDiagnoser]
    public class TenantResolutionBenchmarks
    {
        private readonly TrustIdentity.AspNetCore.Services.CompositeTenantResolver _resolver;
        private readonly Microsoft.AspNetCore.Http.DefaultHttpContext _cookieContext;
        private readonly Microsoft.AspNetCore.Http.DefaultHttpContext _hostContext;

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
        }

        [Benchmark]
        public async Task ResolveFromCookie() => await _resolver.ResolveAsync(_cookieContext);

        [Benchmark]
        public async Task ResolveFromHost() => await _resolver.ResolveAsync(_hostContext);
    }

    public class SimpleCookieCollection : Microsoft.AspNetCore.Http.IRequestCookieCollection, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>
    {
        private readonly System.Collections.Generic.Dictionary<string, string> _cookies;
        public SimpleCookieCollection(System.Collections.Generic.Dictionary<string, string> cookies) => _cookies = cookies;
        string? Microsoft.AspNetCore.Http.IRequestCookieCollection.this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;
        public int Count => _cookies.Count;
        public System.Collections.Generic.ICollection<string> Keys => _cookies.Keys;
        public bool ContainsKey(string key) => _cookies.ContainsKey(key);
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _cookies.GetEnumerator();
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) {
             var res = _cookies.TryGetValue(key, out var v);
             value = v;
             return res;
        }
    }

    // Mocks
    public class MockUserStore : Abstractions.Stores.IUserStore
    {
        public Task<User?> FindBySubjectIdAsync(string subjectId) => Task.FromResult<User?>(new User { SubjectId = subjectId, FailedLoginAttempts = 0 });
        public Task AddUserAsync(User user, string password) => Task.CompletedTask;
        public Task DeleteUserAsync(string subjectId) => Task.CompletedTask;
        public Task<(IEnumerable<User> Users, int TotalCount)> GetAllUsersAsync(string? search = null, int skip = 0, int take = 20) => Task.FromResult((Enumerable.Empty<User>(), 0));
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
        private readonly Tenant _tenant = new Tenant { Id = "tenant_1", Identifier = "tenant_1", Host = "tenant_1.local" };
        public Task<Tenant?> GetByHostAsync(string host) => Task.FromResult<Tenant?>(_tenant);
        public Task<Tenant?> GetByIdAsync(string id) => Task.FromResult<Tenant?>(_tenant);
        public Task<Tenant?> GetByIdentifierAsync(string identifier) => Task.FromResult<Tenant?>(_tenant);
        public Task<Tenant> CreateAsync(Tenant tenant) => Task.FromResult(tenant);
        public Task<bool> DeleteAsync(string id) => Task.FromResult(true);
        public Task<IEnumerable<Tenant>> GetAllAsync(int skip = 0, int take = 100) => Task.FromResult<IEnumerable<Tenant>>(new List<Tenant> { _tenant });
        public Task<Tenant> UpdateAsync(Tenant tenant) => Task.FromResult(tenant);
        public Task<int> GetCountAsync() => Task.FromResult(1);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
             BenchmarkRunner.Run<PasswordHasherBenchmarks>();
             BenchmarkRunner.Run<TokenSigningBenchmarks>();
            BenchmarkRunner.Run<FraudDetectionBenchmarks>();
            BenchmarkRunner.Run<TenantResolutionBenchmarks>();
        }
    }
}
