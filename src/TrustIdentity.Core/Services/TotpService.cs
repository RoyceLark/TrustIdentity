using System;
using System.Security.Cryptography;
using System.Text;

namespace TrustIdentity.Core.Services;

/// <summary>
/// Service for generating and validating TOTP (Time-based One-Time Passwords).
/// </summary>
public class TotpService
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Generates a TOTP code for a given secret and timestamp.
    /// </summary>
    public string GenerateCode(string secret, DateTime? timestamp = null)
    {
        var timeStep = (long)( (timestamp ?? DateTime.UtcNow) - UnixEpoch).TotalSeconds / 30;
        var bytes = Encoding.UTF8.GetBytes(secret);
        
        using var hmac = new HMACSHA1(bytes);
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian) Array.Reverse(timeBytes);

        var hash = hmac.ComputeHash(timeBytes);
        var offset = hash[hash.Length - 1] & 0xf;
        var binary = ( (hash[offset] & 0x7f) << 24) |
                     ( (hash[offset + 1] & 0xff) << 16) |
                     ( (hash[offset + 2] & 0xff) << 8) |
                     (hash[offset + 3] & 0xff);

        var otp = binary % 1000000;
        return otp.ToString("D6");
    }

    /// <summary>
    /// Validates a TOTP code.
    /// </summary>
    public bool ValidateCode(string secret, string code)
    {
        // Allow a window of +/- 1 time step (30 seconds)
        for (int i = -1; i <= 1; i++)
        {
            var timestamp = DateTime.UtcNow.AddSeconds(i * 30);
            if (GenerateCode(secret, timestamp) == code) return true;
        }
        return false;
    }

    /// <summary>
    /// Generates a new random secret.
    /// </summary>
    public string GenerateSecret()
    {
        var bytes = new byte[20];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
