using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Interface for behavior analysis services
/// </summary>
public interface IBehaviorAnalysisService
{
    /// <summary>
    /// Analyzes the behavior context
    /// </summary>
    /// <param name="context">The context to analyze</param>
    /// <returns>A behavior score</returns>
    Task<BehaviorScore> AnalyzeAsync(BehaviorAnalysisContext context);
    
    /// <summary>
    /// Records a behavior pattern
    /// </summary>
    /// <param name="pattern">The pattern to record</param>
    /// <returns>A task representing the operation</returns>
    Task RecordBehaviorAsync(BehaviorPattern pattern);
    
    /// <summary>
    /// Gets recorded behavior patterns for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A collection of patterns</returns>
    Task<IEnumerable<BehaviorPattern>> GetUserBehaviorAsync(string userId);
}