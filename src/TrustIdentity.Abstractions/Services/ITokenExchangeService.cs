using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services
{
    /// <summary>
    /// Service for handling RFC 8693 Token Exchange
    /// </summary>
    public interface ITokenExchangeService
    {
        /// <summary>
        /// Exchanges a token for a new one
        /// </summary>
        Task<TokenExchangeResult> ExchangeAsync(string subjectToken, string subjectTokenType, string? actorToken = null, string? actorTokenType = null);
    }

    /// <summary>
    /// Result of a token exchange operation
    /// </summary>
    public class TokenExchangeResult
    {
        /// <summary>Whether the operation resulted in an error</summary>
        public bool IsError { get; set; }
        /// <summary>The error code</summary>
        public string? Error { get; set; }
        /// <summary>Description of the error</summary>
        public string? ErrorDescription { get; set; }
        /// <summary>The resolved user for the exchanged token</summary>
        public User? User { get; set; }
    }
}
