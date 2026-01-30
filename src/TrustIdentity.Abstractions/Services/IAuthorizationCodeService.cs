using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.Abstractions.Services;
/// <summary>
/// IAuthorizationCodeService
/// </summary>
public interface IAuthorizationCodeService
{
    /// <summary>
    /// CreateAuthorizationCodeAsync
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<string> CreateAuthorizationCodeAsync(AuthorizationCode code);
    /// <summary>
    /// GetAuthorizationCodeAsync
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<AuthorizationCode?> GetAuthorizationCodeAsync(string code);
    /// <summary>
    /// ConsumeAuthorizationCodeAsync
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task ConsumeAuthorizationCodeAsync(string code);
}
