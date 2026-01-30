namespace TrustIdentity.Abstractions.Models;

using System.Collections.Generic;
/// <summary>
/// Represents the result of a behavior analysis
/// </summary>
public class BehaviorScore
{
    /// <summary>The calculated anomaly score (0.0 to 1.0)</summary>
    public double AnomalyScore { get; set; }
    /// <summary>Whether the behavior is considered an anomaly</summary>
    public bool IsAnomaly { get; set; }
    /// <summary>List of patterns detected during analysis</summary>
    public List<string> DetectedPatterns { get; set; } = new();
    /// <summary>Scores breakdown for individual features</summary>
    public Dictionary<string, double> FeatureScores { get; set; } = new();
}