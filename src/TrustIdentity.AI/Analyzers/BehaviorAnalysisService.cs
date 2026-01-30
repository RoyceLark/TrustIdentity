using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.AI.Analyzers;

/// <summary>
/// Service for analyzing user behavior patterns to detect anomalies and potential fraud
/// </summary>
public class BehaviorAnalysisService : IBehaviorAnalysisService
{
    private readonly ILogger<BehaviorAnalysisService> _logger;
    private readonly Dictionary<string, List<BehaviorPattern>> _userPatterns = new();

    /// <summary>
    /// Initializes a new instance of the BehaviorAnalysisService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public BehaviorAnalysisService(ILogger<BehaviorAnalysisService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyzes the current behavior context for a user
    /// </summary>
    /// <param name="context">The behavior context to analyze</param>
    /// <returns>A behavior score indicating anomaly level</returns>
    public async Task<BehaviorScore> AnalyzeAsync(BehaviorAnalysisContext context)
    {
        _logger.LogInformation("Analyzing behavior for user: {UserId}", context.UserId);

        if (!_userPatterns.ContainsKey(context.UserId))
        {
            // New user, no patterns yet
            return await Task.FromResult(new BehaviorScore
            {
                AnomalyScore = 0.0,
                IsAnomaly = false,
                DetectedPatterns = new List<string> { "NewUser" }
            });
        }

        var patterns = _userPatterns[context.UserId];
        var score = 0.0;
        var detectedPatterns = new List<string>();

        // Compare current behavior with learned patterns
        foreach (var pattern in patterns)
        {
            if (pattern.PatternType == "LoginTime")
            {
                var currentHour = context.Timestamp.Hour;
                if (pattern.Metadata.TryGetValue("TypicalHour", out var typicalHourObj))
                {
                    var typicalHour = Convert.ToInt32(typicalHourObj);
                    var hourDiff = Math.Abs(currentHour - typicalHour);
                    if (hourDiff > 6)
                    {
                        score += 0.3;
                        detectedPatterns.Add("UnusualLoginTime");
                    }
                }
            }
        }

        return await Task.FromResult(new BehaviorScore
        {
            AnomalyScore = Math.Min(score, 1.0),
            IsAnomaly = score >= 0.8,
            DetectedPatterns = detectedPatterns
        });
    }

    /// <summary>
    /// Records a new behavior pattern for a user
    /// </summary>
    /// <param name="pattern">The pattern to record</param>
    /// <returns>A task representing the operation</returns>
    public async Task RecordBehaviorAsync(BehaviorPattern pattern)
    {
        _logger.LogInformation("Recording behavior pattern for user: {UserId}", pattern.UserId);

        if (!_userPatterns.ContainsKey(pattern.UserId))
        {
            _userPatterns[pattern.UserId] = new List<BehaviorPattern>();
        }

        _userPatterns[pattern.UserId].Add(pattern);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets all recorded behavior patterns for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A collection of behavior patterns</returns>
    public async Task<IEnumerable<BehaviorPattern>> GetUserBehaviorAsync(string userId)
    {
        _logger.LogInformation("Getting behavior patterns for user: {UserId}", userId);

        if (_userPatterns.ContainsKey(userId))
        {
            return await Task.FromResult(_userPatterns[userId].AsEnumerable());
        }

        return await Task.FromResult(Enumerable.Empty<BehaviorPattern>());
    }
}