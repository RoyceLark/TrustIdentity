# TrustIdentity - Licensing Configuration Guide

## 📋 Current Status

**Licensing is DISABLED by default** and will remain disabled until you're ready to monetize.

---

## 🔧 Current Configuration

### Default Setting
```csharp
// In TrustIdentityOptions.cs
public bool EnableLicensing { get; set; } = false;
```

**This means:**
- ✅ TrustIdentity runs **without any license checks**
- ✅ All features are **fully functional**
- ✅ No license validation occurs
- ✅ No license expiration warnings
- ✅ Perfect for development, testing, and initial deployment

---

## 🚀 When to Enable Licensing

Enable licensing when you:
1. Have a user base ready to purchase licenses
2. Want to monetize your identity server
3. Need to enforce usage limits
4. Want to offer tiered pricing (Free, Pro, Enterprise)

---

## 🔓 How Licensing Works (When Enabled)

### Architecture
```
┌─────────────────────────────────────────────────┐
│  TrustIdentity Server                           │
│  ┌───────────────────────────────────────────┐  │
│  │ License Validation Middleware             │  │
│  │ - Checks license on startup               │  │
│  │ - Validates signature                     │  │
│  │ - Checks expiration                       │  │
│  │ - Enforces feature limits                 │  │
│  └───────────────────────────────────────────┘  │
│                     ↓                           │
│  ┌───────────────────────────────────────────┐  │
│  │ License Store (Database)                  │  │
│  │ - License key                             │  │
│  │ - Expiration date                         │  │
│  │ - Feature flags                           │  │
│  │ - Usage limits                            │  │
│  └───────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

### Components Available
1. **TrustIdentity.Licensing** - Core licensing library
2. **TrustIdentity.Licensing.Manager** - Web-based license management UI
3. **License Generator** - Creates signed licenses
4. **License Validator** - Validates licenses

---

## 🎯 How to Enable Licensing (Future)

### Step 1: Update Configuration

**In `appsettings.json`:**
```json
{
  "TrustIdentity": {
    "EnableLicensing": true,
    "IssuerUri": "https://identity.yourdomain.com"
  }
}
```

**Or in code:**
```csharp
builder.Services.AddTrustIdentity(options =>
{
    options.EnableLicensing = true;
    options.IssuerUri = "https://identity.yourdomain.com";
});
```

### Step 2: Generate License Keys

**Run the License Manager:**
```bash
cd src/TrustIdentity.Licensing.Manager
dotnet run
```

**Navigate to:** `https://localhost:5002`

**Generate a license:**
1. Click "Generate License"
2. Set expiration date
3. Select features to enable
4. Generate signed license key
5. Distribute to customers

### Step 3: Customers Install License

**Customers add license to their configuration:**
```json
{
  "TrustIdentity": {
    "EnableLicensing": true,
    "LicenseKey": "YOUR-SIGNED-LICENSE-KEY-HERE"
  }
}
```

### Step 4: License Validation

**On startup, TrustIdentity will:**
1. ✅ Check if license exists
2. ✅ Validate cryptographic signature
3. ✅ Check expiration date
4. ✅ Verify feature flags
5. ✅ Enforce usage limits

**If license is invalid:**
- ❌ Server will not start (or run in limited mode)
- ❌ Error message displayed
- ❌ Admin notified

---

## 💰 Licensing Tiers (Example)

### Free Tier (No License Required)
- ✅ Up to 100 users
- ✅ Basic OAuth/OIDC
- ✅ Community support
- ✅ No expiration

### Pro Tier ($99/month)
- ✅ Unlimited users
- ✅ All protocols (OAuth, OIDC, SAML, WS-Fed)
- ✅ AI fraud detection
- ✅ Email support
- ✅ 1-year license

### Enterprise Tier ($499/month)
- ✅ Everything in Pro
- ✅ Multi-tenancy
- ✅ Priority support
- ✅ Custom SLA
- ✅ Perpetual license option

---

## 🔐 License Security

### How Licenses Are Secured

1. **Cryptographic Signing**
   - Licenses are signed with RSA-2048
   - Private key kept secure on license server
   - Public key embedded in TrustIdentity

2. **Tamper Protection**
   - Any modification invalidates signature
   - Cannot be forged or altered

3. **Expiration Enforcement**
   - Checked on every startup
   - Optional runtime checks
   - Grace period configurable

4. **Feature Flags**
   - Enable/disable specific features
   - Enforce usage limits
   - Control access to premium features

---

## 📊 License Management

### License Manager Features

**Available at:** `src/TrustIdentity.Licensing.Manager`

**Features:**
- ✅ Generate new licenses
- ✅ Revoke licenses
- ✅ View active licenses
- ✅ Track usage
- ✅ Manage keys
- ✅ Export reports

**Access:**
```bash
cd src/TrustIdentity.Licensing.Manager
dotnet run
```

Then navigate to: `https://localhost:5002`

---

## 🛠️ Implementation Details

### License Model
```csharp
public class License
{
    public string LicenseKey { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public LicenseTier Tier { get; set; }
    public Dictionary<string, bool> Features { get; set; }
    public int MaxUsers { get; set; }
    public string Signature { get; set; }
}
```

### License Validation Service
```csharp
public interface ILicenseValidator
{
    Task<LicenseValidationResult> ValidateAsync(string licenseKey);
    Task<bool> IsFeatureEnabledAsync(string feature);
    Task<int> GetRemainingUsersAsync();
}
```

---

## 📝 Migration Path

### Current State (Licensing Disabled)
```
User installs TrustIdentity
    ↓
Server starts immediately
    ↓
All features available
    ↓
No license checks
```

### Future State (Licensing Enabled)
```
User installs TrustIdentity
    ↓
User obtains license key
    ↓
User configures license in appsettings.json
    ↓
Server validates license on startup
    ↓
Features enabled based on license tier
```

---

## 🎓 Best Practices

### When to Enable Licensing

**✅ Good Reasons:**
- You have paying customers
- You want to enforce usage limits
- You need tiered pricing
- You want to track installations

**❌ Bad Reasons:**
- Just starting out
- Still in development
- Testing/staging environments
- Internal use only

### Recommended Approach

1. **Phase 1 (Now):** Keep licensing disabled
   - Focus on building user base
   - Get feedback
   - Stabilize product

2. **Phase 2 (Later):** Soft launch licensing
   - Announce upcoming licensing
   - Grandfather existing users
   - Offer migration path

3. **Phase 3 (Future):** Full licensing
   - Enable for new installations
   - Enforce for commercial use
   - Offer free tier for small deployments

---

## 🔄 Re-enabling Licensing

### When You're Ready

**1. Update Configuration:**
```csharp
// In TrustIdentityOptions.cs (or appsettings.json)
EnableLicensing = true
```

**2. Generate Master Keys:**
```bash
cd src/TrustIdentity.Licensing.Manager
dotnet run
# Generate RSA key pair
# Store private key securely
```

**3. Create License Tiers:**
```csharp
public enum LicenseTier
{
    Free,      // Up to 100 users
    Pro,       // Unlimited users, all features
    Enterprise // Everything + support
}
```

**4. Build License Distribution:**
- Create customer portal
- Automate license delivery
- Set up payment processing
- Implement license activation

**5. Communicate to Users:**
- Announce licensing plans
- Provide migration timeline
- Offer grandfathering options
- Create FAQ

---

## 📞 Support

### Questions About Licensing?

**For now:**
- Licensing is disabled
- All features are free
- No action required

**When enabling:**
- Review this guide
- Test license generation
- Plan migration strategy
- Communicate with users

---

## ✅ Summary

### Current Status
```
EnableLicensing = false (DEFAULT)
```

**What this means:**
- ✅ TrustIdentity is **completely free**
- ✅ All features are **fully enabled**
- ✅ No license validation occurs
- ✅ Perfect for getting started

### Future Plans
- 📅 Enable when you have paying customers
- 📅 Use built-in licensing system
- 📅 Offer tiered pricing
- 📅 Enforce usage limits

### Action Required
- ✅ **None right now!**
- ✅ Focus on building your product
- ✅ Grow your user base
- ✅ Re-enable licensing when ready

---

**Current Configuration: 🟢 Licensing DISABLED (Free for all users)**

*Last Updated: 2026-01-29*
