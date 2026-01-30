namespace TrustIdentity.Core.Models;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
//public class Secret
//{
//    public string Value { get; set; } = string.Empty;
//    public string Type { get; set; } = "SharedSecret";
//    public string? Description { get; set; }
//    public DateTime? Expiration { get; set; }
//}

//public static class SecretExtensions
//{
//    public static Secret Sha256(this string value)
//    {
//        using var sha = System.Security.Cryptography.SHA256.Create();
//        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
//        var hash = sha.ComputeHash(bytes);
//        return new Secret
//        {
//            Value = Convert.ToBase64String(hash),
//            Type = "SharedSecret"
//        };
//    }
//}