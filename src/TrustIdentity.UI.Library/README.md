# TrustIdentity UI Library 🛡️

**The Premium, AI-Powered Interface for TrustIdentity**

`TrustIdentity.UI.Library` is a state-of-the-art **Razor Class Library (RCL)** that delivers a stunning, secure, and highly-performant user interface for the TrustIdentity ecosystem. Designed with modern **Glassmorphism** aesthetics and deeply integrated with **AI-powered fraud detection**, it provides a "plug-and-play" experience for enterprise identity management.

---

## ✨ Key Features

### 🔐 Authentication Suite
- **Secure Login**: Responsive login interface with support for local and external providers.
- **External Authentication**: Seamless integration with Google, Microsoft, GitHub, Facebook, and more.
- **Rich Multi-Factor (MFA)**: Beautiful TOTP and conditional MFA challenge screens.
- **Smart Registration**: User-friendly sign-up flows with real-time validation.
- **Self-Service**: Complete password reset and email confirmation workflows.

### 👤 Account Management (Manage)
- **Profile Dashboard**: High-fidelity personal information management.
- **Security Control**: Password updates and 2FA configuration.
- **External Logins**: Manage linked social and enterprise accounts.
- **Privacy First**: Export and delete personal data (GDPR/CCPA ready).

### ⚖️ OAuth / OpenID Connect Flows
- **Glassmorphic Consent**: Elegant authorization screens for third-party apps.
- **Device Authorization**: Seamless cross-device "User Code" verification.
- **CIBA Support**: Client Initiated Backchannel Authentication success/error states.
- **Grants & Sessions**: End-user management of active permissions and login sessions.

---

## 🚀 Quick Start

### 1. Install the Package
```bash
dotnet add package TrustIdentity.UI.Library
```

### 2. Integration
Register the UI services in your `Program.cs` using the builder pattern:

```csharp
using TrustIdentity.UI.Library.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add TrustIdentity Core & UI
builder.Services.AddTrustIdentity()
    .AddTrustIdentityUI(options => {
        options.ApplicationName = "Horizon Enterprise";
        options.PrimaryColor = "#6366F1";
        options.Theme = "dark"; // Choose "light" or "dark"
        options.LogoUrl = "/images/logo.png";
    });

var app = builder.Build();

// 2. Middleware Configuration
app.UseStaticFiles(); 
app.UseTrustIdentity();
app.MapRazorPages();

app.Run();
```

---

## 🧠 Intelligent Security

The library isn't just a face; it's a security powerhouse:

- **AI Fraud Shield**: Real-time behavioral analysis flags suspicious login patterns based on IP, User-Agent, and historical data.
- **Risk-Based MFA**: Automatically triggers multi-factor challenges when risk scores exceed safety thresholds.
- **Anti-Redirect Protection**: Strict validation of `ReturnUrl` parameters to prevent open redirect vulnerabilities.
- **Security Auditing**: Comprehensive event tracking for all critical UI interactions (logins, failures, MFA completions).

---

## 🎨 Design & Aesthetics

The UI is built on a custom design system that prioritizes visual excellence and user engagement:

- **Glassmorphism**: Subtle translucent backgrounds with frosted glass effects for a modern look.
- **Dynamic Themes**: Native support for **Light** and **Dark** modes that respect system preferences or manual overrides.
- **Rich Typography**: Integrated with **Inter** from Google Fonts for maximum readability.
- **Micro-animations**: Smooth transitions and loading states for a premium "app-like" feel.

### 🏢 Dynamic Client Branding
One of the most powerful features of TrustIdentity UI is its ability to adapt dynamically to the calling application. When a `client_id` is detected in the OIDC request flow, the UI automatically:
1. Queries the `ClientStore` for that specific client.
2. Injects custom **Primary/Secondary Colors**.
3. Displays the **Client-Specific Logo**.
4. Applies **Custom CSS** overrides defined in the client's configuration.

---

## ⚙️ Configuration Options

| Option | Description | Default |
| :--- | :--- | :--- |
| `ApplicationName` | The name shown in headers and titles. | `TrustIdentity` |
| `LogoUrl` | Path to the default platform logo. | `/images/logo.png` |
| `Theme` | Initial UI theme (`light` or `dark`). | `light` |
| `PrimaryColor` | CSS brand color for primary actions. | `#007bff` |
| `EnableRegistration` | Whether to show the "Sign Up" links. | `true` |
| `EnableForgotPassword`| Toggles the password recovery flow. | `true` |

---

## 🔑 External Provider Setup

To use social logins (Google, GitHub, etc.), you must configure them in your host application. **Crucially**, the UI expects an intermediate cookie scheme named `"External"` to handle the transition:

```csharp
builder.Services.AddAuthentication()
    .AddCookie("TrustIdentity") // Local session
    .AddCookie("External")      // Temporary external session
    .AddGoogle(options => {
        options.SignInScheme = "External"; // Must match
        options.ClientId = "...";
        options.ClientSecret = "...";
    });
```

---

## 🧩 Customization

### Overriding Pages
Need to change the logic or HTML for a specific page? Simply create a file in your host project with the matching path:

| Scenario | Local Path |
| :--- | :--- |
| **Custom Login** | `/Pages/Account/Login.cshtml` |
| **Custom Profile** | `/Pages/Manage/Index.cshtml` |
| **Custom Consent** | `/Pages/Consent/Index.cshtml` |

### Theming with CSS
The library uses CSS Variables for easy styling. You can override them in your `wwwroot/css/site.css`:

```css
:root {
    --primary: #6366F1;
    --primary-dark: #4f46e5;
    --auth-card-radius: 20px;
    --glass-opacity: 0.8;
}
```

---

## 🛠️ Troubleshooting

- **Assets not loading?** Ensure `app.UseStaticFiles()` is called *before* `app.UseRouting()`.
- **404 on Pages?** Verify `app.MapRazorPages()` is present in your `Program.cs`.
- **Branding not showing?** Check that the `client_id` in your login URL matches a record in your `IClientStore` and that the `Client` object has branding properties populated.
- **External Login Errors?** Ensure the `SignInScheme` for your external provider is set to `"External"`.

---

## 📦 Asset Structure
Static assets are embedded and served automatically from:
- `~/css/trust-identity.css`: The core design system.
- `~/js/site.js`: Interaction logic and micro-animations.
- `~/images/logo.png`: The default platform logo.
- `~/lib/`: Pre-bundled Font Awesome, jQuery, and Bootstrap.

---

## 📄 License
Licensed under the **Apache 2.0 License**. See the [LICENSE](../../../LICENSE) file for details.


