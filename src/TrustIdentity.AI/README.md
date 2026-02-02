# TrustIdentity.AI

**AI-powered fraud detection and behavioral analysis**

---

## 📦 Overview

`TrustIdentity.AI` provides AI and machine learning capabilities for fraud detection, behavioral analysis, and adaptive authentication. This is a **unique feature** not available in Duende IdentityServer.

---

## ✨ Features

- ✅ **Real-time Fraud Detection** - ML-based anomaly detection
- ✅ **Behavioral Analysis** - User behavior profiling
- ✅ **Risk Scoring** - Composite risk calculation
- ✅ **Adaptive Authentication** - AI-driven MFA triggers
- ✅ **Device Fingerprinting** - Track user devices
- ✅ **Anomaly Detection** - Unusual access patterns

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.AI
dotnet add package TrustIdentity.ML  # Optional: ML.NET integration
```

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.AspNetCore.Extensions;

builder.Services.AddTrustIdentity(options =>
{
    options.IssuerUri = "https://localhost:5001";
    options.EnableAI = true;
    options.EnableFraudDetection = true;
})
.AddAIFraudDetection()
.AddBehaviorAnalysis()
.AddRiskScoring();
```

### Advanced Configuration

```csharp
builder.Services.AddTrustIdentity(options =>
{
    options.EnableAI = true;
    options.EnableFraudDetection = true;
    
    // AI Configuration
    options.AIOptions = new AIOptions
    {
        FraudDetectionThreshold = 0.7,
        EnableBehavioralAnalysis = true,
        EnableDeviceFingerprinting = true,
        EnableAnomalyDetection = true,
        RiskScoreThreshold = 0.8
    };
});
```

---

## 🧠 AI Services

### IFraudDetectionService

Detects fraudulent login attempts in real-time.

```csharp
public interface IFraudDetectionService
{
    Task<FraudDetectionResult> AnalyzeLoginAttemptAsync(LoginAttempt attempt);
    Task<bool> IsSuspiciousAsync(string userId, string ipAddress);
}
```

**Usage:**

```csharp
public class LoginController
{
    private readonly IFraudDetectionService _fraudDetection;

    public async Task<IActionResult> Login(LoginModel model)
    {
        var attempt = new LoginAttempt
        {
            UserId = model.Username,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"],
            Timestamp = DateTime.UtcNow
        };

        var result = await _fraudDetection.AnalyzeLoginAttemptAsync(attempt);

        if (result.IsFraudulent)
        {
            // Block login or require additional verification
            return Forbid("Suspicious activity detected");
        }

        // Continue with normal login
    }
}
```

### IBehaviorAnalysisService

Analyzes user behavior patterns.

```csharp
public interface IBehaviorAnalysisService
{
    Task<BehaviorProfile> GetUserProfileAsync(string userId);
    Task UpdateBehaviorAsync(string userId, UserActivity activity);
    Task<bool> IsAnomalousAsync(string userId, UserActivity activity);
}
```

**Usage:**

```csharp
var profile = await _behaviorAnalysis.GetUserProfileAsync(userId);

var activity = new UserActivity
{
    UserId = userId,
    IpAddress = ipAddress,
    Location = location,
    DeviceId = deviceId,
    Timestamp = DateTime.UtcNow
};

if (await _behaviorAnalysis.IsAnomalousAsync(userId, activity))
{
    // Trigger MFA or additional verification
}
```

### Risk Scoring

Calculate composite risk scores:

```csharp
var riskScore = await _riskScoring.CalculateRiskScoreAsync(new RiskContext
{
    UserId = userId,
    IpAddress = ipAddress,
    DeviceId = deviceId,
    Location = location,
    TimeOfDay = DateTime.UtcNow.TimeOfDay
});

if (riskScore > 0.8)
{
    // High risk - require MFA
}
else if (riskScore > 0.5)
{
    // Medium risk - additional verification
}
else
{
    // Low risk - allow login
}
```

---

## 🎯 Use Cases

### 1. Adaptive MFA

Trigger MFA based on risk score:

```csharp
var riskScore = await _riskScoring.CalculateRiskScoreAsync(context);

if (riskScore > 0.7)
{
    // Require MFA
    return RedirectToAction("MFA");
}
```

### 2. Fraud Prevention

Block suspicious login attempts:

```csharp
var fraudResult = await _fraudDetection.AnalyzeLoginAttemptAsync(attempt);

if (fraudResult.IsFraudulent)
{
    await _logger.LogSecurityEventAsync("Fraudulent login blocked", userId);
    return Forbid();
}
```

### 3. Device Tracking

Track and verify user devices:

```csharp
var deviceId = await _deviceFingerprinting.GetDeviceIdAsync(request);
var isKnownDevice = await _deviceTracking.IsKnownDeviceAsync(userId, deviceId);

if (!isKnownDevice)
{
    // New device - send verification email
    await _emailService.SendNewDeviceNotificationAsync(userId, deviceId);
}
```

---

## 📊 AI Models

### Fraud Detection Model

- **Algorithm**: Isolation Forest
- **Features**: IP address, location, time of day, device, user agent
- **Training**: Continuous learning from login patterns

### Behavioral Analysis Model

- **Algorithm**: LSTM (Long Short-Term Memory)
- **Features**: Login times, locations, devices, access patterns
- **Training**: Per-user behavior profiling

### Risk Scoring Model

- **Algorithm**: Ensemble (Random Forest + Gradient Boosting)
- **Features**: Composite of fraud and behavior scores
- **Training**: Supervised learning on labeled data

---

## 🔧 Configuration

### appsettings.json

```json
{
  "TrustIdentity": {
    "AI": {
      "EnableFraudDetection": true,
      "EnableBehavioralAnalysis": true,
      "EnableRiskScoring": true,
      "FraudDetectionThreshold": 0.7,
      "RiskScoreThreshold": 0.8,
      "ModelUpdateInterval": 3600,
      "EnableDeviceFingerprinting": true
    }
  }
}
```

---

## 🏗️ Architecture

```
TrustIdentity.AI/
├── Analyzers/          # AI analyzers
│   ├── FraudDetectionService.cs
│   ├── BehaviorAnalysisService.cs
│   └── RiskScoringService.cs
├── Models/            # ML models
├── Services/          # AI services
└── Extensions/        # Configuration extensions
```

---

## 📚 Documentation

- **[Setup Guide](../../../SETUP_GUIDE.md)** - General setup
- **[Main Documentation](../../../README.md)** - Overview

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
