using System;

namespace TrustIdentity.Saml.Security;

/// <summary>
/// Time Validation with Clock Skew Tolerance
/// </summary>
public class TimeValidator
{
    private readonly TimeSpan _clockSkewTolerance;

    /// <summary>
    /// Initializes a new instance of the TimeValidator
    /// </summary>
    /// <param name="clockSkewTolerance">Optional clock skew tolerance (default: 5 minutes)</param>
    public TimeValidator(TimeSpan? clockSkewTolerance = null)
    {
        _clockSkewTolerance = clockSkewTolerance ?? TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Validate time window with clock skew tolerance
    /// </summary>
    /// <param name="notBefore">The NotBefore timestamp</param>
    /// <param name="notOnOrAfter">The NotOnOrAfter timestamp</param>
    /// <returns>A result object containing validation details</returns>
    public TimeValidationResult ValidateTimeWindow(DateTime notBefore, DateTime notOnOrAfter)
    {
        var now = DateTime.UtcNow;
        
        var effectiveNotBefore = notBefore - _clockSkewTolerance;
        var effectiveNotOnOrAfter = notOnOrAfter + _clockSkewTolerance;

        var result = new TimeValidationResult
        {
            CurrentTime = now,
            NotBefore = notBefore,
            NotOnOrAfter = notOnOrAfter,
            EffectiveNotBefore = effectiveNotBefore,
            EffectiveNotOnOrAfter = effectiveNotOnOrAfter,
            ClockSkew = _clockSkewTolerance
        };

        if (now < effectiveNotBefore)
        {
            result.IsValid = false;
            result.Error = $"Assertion not yet valid. Current time: {now:o}, Assertion valid from: {notBefore:o}";
            result.TimeDifference = effectiveNotBefore - now;
            return result;
        }

        if (now >= effectiveNotOnOrAfter)
        {
            result.IsValid = false;
            result.Error = $"Assertion expired. Current time: {now:o}, Assertion expired at: {notOnOrAfter:o}";
            result.TimeDifference = now - effectiveNotOnOrAfter;
            return result;
        }

        result.IsValid = true;
        return result;
    }

    /// <summary>
    /// Validate single timestamp (e.g., IssueInstant)
    /// </summary>
    /// <param name="timestamp">The timestamp to validate</param>
    /// <param name="maxAge">The maximum allowed age</param>
    /// <returns>True if the timestamp is valid within the max age and skew; otherwise false</returns>
    public bool ValidateTimestamp(DateTime timestamp, TimeSpan maxAge)
    {
        var now = DateTime.UtcNow;
        var age = now - timestamp;
        var effectiveMaxAge = maxAge + _clockSkewTolerance;
        
        return age <= effectiveMaxAge && age >= -_clockSkewTolerance;
    }

    /// <summary>
    /// Check if time difference exceeds acceptable skew
    /// </summary>
    /// <param name="remoteTime">The time from the remote system</param>
    /// <returns>True if the skew is excessive; otherwise false</returns>
    public bool IsClockSkewExcessive(DateTime remoteTime)
    {
        var localTime = DateTime.UtcNow;
        var difference = Math.Abs((remoteTime - localTime).TotalSeconds);
        
        return difference > _clockSkewTolerance.TotalSeconds;
    }

    /// <summary>
    /// Get recommended clock skew for different security levels
    /// </summary>
    /// <param name="level">The security level</param>
    /// <returns>The recommended clock skew TimeSpan</returns>
    public static TimeSpan GetRecommendedClockSkew(SecurityLevel level)
    {
        return level switch
        {
            SecurityLevel.Low => TimeSpan.FromMinutes(10),
            SecurityLevel.Medium => TimeSpan.FromMinutes(5),
            SecurityLevel.High => TimeSpan.FromMinutes(2),
            SecurityLevel.VeryHigh => TimeSpan.FromMinutes(1),
            _ => TimeSpan.FromMinutes(5)
        };
    }
}

/// <summary>
/// Result of time validation
/// </summary>
public class TimeValidationResult
{
    /// <summary>
    /// Validation success status
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Error message if invalid
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// Current server time used for validation
    /// </summary>
    public DateTime CurrentTime { get; set; }
    
    /// <summary>
    /// NotBefore time from assertion
    /// </summary>
    public DateTime NotBefore { get; set; }
    
    /// <summary>
    /// NotOnOrAfter time from assertion
    /// </summary>
    public DateTime NotOnOrAfter { get; set; }
    
    /// <summary>
    /// Effective NotBefore time (including skew)
    /// </summary>
    public DateTime EffectiveNotBefore { get; set; }
    
    /// <summary>
    /// Effective NotOnOrAfter time (including skew)
    /// </summary>
    public DateTime EffectiveNotOnOrAfter { get; set; }
    
    /// <summary>
    /// Clock skew tolerance used
    /// </summary>
    public TimeSpan ClockSkew { get; set; }
    
    /// <summary>
    /// Time difference calculated
    /// </summary>
    public TimeSpan TimeDifference { get; set; }
}

/// <summary>
/// Security Level for time validation strictness
/// </summary>
public enum SecurityLevel
{
    /// <summary>
    /// Low security - 10 min skew
    /// </summary>
    Low,
    
    /// <summary>
    /// Medium security - 5 min skew (Default)
    /// </summary>
    Medium,
    
    /// <summary>
    /// High security - 2 min skew
    /// </summary>
    High,
    
    /// <summary>
    /// Very High security - 1 min skew
    /// </summary>
    VeryHigh
}