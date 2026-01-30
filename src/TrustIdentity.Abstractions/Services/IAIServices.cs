using System.Collections.Generic;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Models;
namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for detecting fraud and suspicious activities
/// </summary>
public interface IFraudDetectionService
{
    /// <summary>
    /// Analyzes a login attempt for potential fraud
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ipAddress">The IP address</param>
    /// <param name="userAgent">The user agent</param>
    /// <returns>A fraud score between 0.0 and 1.0</returns>
    Task<double> AnalyzeLoginAttemptAsync(string userId, string ipAddress, string userAgent);
    
    /// <summary>
    /// Determines if a behavior pattern represents suspicious activity
    /// </summary>
    /// <param name="pattern">The behavior pattern</param>
    /// <returns>True if suspicious; otherwise false</returns>
    Task<bool> IsSuspiciousActivityAsync(BehaviorPattern pattern);
}

/// <summary>
/// Service for calculating risk scores for users and actions
/// </summary>
public interface IRiskScoringService
{
    /// <summary>
    /// Calculates a risk score for a user based on context
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="context">The risk assessment context</param>
    /// <returns>A risk score between 0.0 and 1.0</returns>
    Task<double> CalculateRiskScoreAsync(string userId, Dictionary<string, object> context);
}