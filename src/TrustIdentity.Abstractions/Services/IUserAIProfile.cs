namespace TrustIdentity.Abstractions.Services;

using System;
using System.Collections.Generic;
/// <summary>
/// Represents an AI-generated security profile for a user
/// </summary>
public interface IUserAIProfile
{
    /// <summary>The user ID</summary>
    string UserId { get; }
    /// <summary>The calculated risk score</summary>
    double RiskScore { get; }
    /// <summary>List of suspicious activities detected</summary>
    List<string> SuspiciousActivities { get; }
    /// <summary>When the profile was last analyzed</summary>
    DateTime LastAnalyzed { get; }
}