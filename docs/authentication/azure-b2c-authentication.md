# Azure AD B2C Authentication - Manimp Integration Guide

**Last Updated:** October 14, 2025  
**Status:** Architecture Defined, Ready for Implementation

---

## 🎯 Overview

Manimp uses **Azure AD B2C** for authentication across all three domains:
- `{tenant}.manimp.com` - Internal employees (Admin, Manager, User roles)
- `{tenant}.files.manimp.com` - Internal file portal (same auth as main app)
- `{tenant}.docs.manimp.com` - External clients (separate B2C policy)

### Why Azure AD B2C?

✅ **Multi-tenant friendly:** Separate user flows for internal vs external users  
✅ **Enterprise-grade security:** MFA, conditional access, identity protection  
✅ **Social login ready:** Google, Microsoft, Facebook integration  
✅ **Free tier:** First 50,000 monthly active users free  
✅ **OpenID Connect:** Standards-based, integrates with ASP.NET Core  
✅ **Custom branding:** White-label login pages per tenant  

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure AD B2C Tenant                       │
│                  manimp.onmicrosoft.com                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────┐  ┌──────────────────────────┐│
│  │   Internal User Flow     │  │  External User Flow      ││
│  │  B2C_1_SignUpSignIn      │  │ B2C_1_External_SignUp    ││
│  ├──────────────────────────┤  ├──────────────────────────┤│
│  │ - Email signup           │  │ - Email signup only      ││
│  │ - Microsoft Account      │  │ - Self-service recovery  ││
│  │ - Google (optional)      │  │ - Simple claims          ││
│  │ - MFA (Admin required)   │  │ - No MFA                 ││
│  │ - Rich claims            │  │ - Anonymous fallback     ││
│  └──────────────────────────┘  └──────────────────────────┘│
│           │                              │                   │
│           ↓                              ↓                   │
│  ┌──────────────────────────┐  ┌──────────────────────────┐│
│  │   App Registration       │  │  App Registration        ││
│  │   Manimp-Internal        │  │  Manimp-External         ││
│  ├──────────────────────────┤  ├──────────────────────────┤│
│  │ Client ID: xxx-yyy-zzz   │  │ Client ID: aaa-bbb-ccc   ││
│  │ Redirect URIs:           │  │ Redirect URIs:           ││
│  │ - *.manimp.com/signin    │  │ - *.docs.manimp.com/...  ││
│  │ - *.files.manimp.com/... │  │                          ││
│  └──────────────────────────┘  └──────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
           │                              │
           ↓                              ↓
┌──────────────────────────┐  ┌──────────────────────────────┐
│   Manimp.Web (Blazor)    │  │  Manimp.Web (Blazor)         │
│   Internal Domains       │  │  External Domain             │
├──────────────────────────┤  ├──────────────────────────────┤
│ - acme.manimp.com        │  │ - acme.docs.manimp.com       │
│ - acme.files.manimp.com  │  │                              │
│                          │  │                              │
│ Authentication Scheme:   │  │ Authentication Scheme:       │
│ - OpenIdConnect (default)│  │ - ExternalB2C                │
│ - Cookie: .AspNetCore... │  │ - Cookie: ExternalUserScheme │
│                          │  │                              │
│ Claims:                  │  │ Claims:                      │
│ - TenantId               │  │ - TenantId                   │
│ - Role (Admin/Manager)   │  │ - ExternalUserId             │
│ - UserId                 │  │ - Email                      │
│ - Email                  │  │ - ProjectIds (custom)        │
└──────────────────────────┘  └──────────────────────────────┘
```

---

## 📋 Azure AD B2C Setup

### Step 1: Create B2C Tenant

**Via Azure Portal:**

```
1. Search "Azure AD B2C" in Azure Portal
2. Click "Create Azure AD B2C Tenant"
3. Configuration:
   ├─ Organization name: Manimp
   ├─ Initial domain: manimp (→ manimp.onmicrosoft.com)
   ├─ Country/Region: Latvia (or your country)
   ├─ Subscription: Your Azure subscription
   └─ Resource group: rg-manimp-prod
4. Click "Review + Create"
5. Wait 2-5 minutes for provisioning
```

**Result:**
- Tenant created: `manimp.onmicrosoft.com`
- B2C directory is SEPARATE from your main Azure AD
- Switch between directories in Azure Portal (top-right corner)

---

### Step 2: Create User Flows (Policies)

#### Internal Employee Flow

```
Via Azure Portal (in B2C tenant):
1. Navigate: Azure AD B2C → User flows → + New user flow
2. Select: "Sign up and sign in" (v3 - Recommended)
3. Configuration:

   Name: SignUpSignIn
   (Full name will be: B2C_1_SignUpSignIn)

   Identity providers:
   ✅ Email signup
   ⬜ Local account signin (disable username, use email only)
   ✅ Microsoft Account (optional for employees)
   ✅ Google (optional)

   Multifactor authentication:
   • Optional for all users
   • Consider enforcing for Admin role (via Conditional Access)

   User attributes (collect during signup):
   ✅ Email Address (required)
   ✅ Given Name (required)
   ✅ Surname (required)
   ✅ Display Name (optional)
   ✅ Job Title (optional)
   ✅ Country/Region (optional)

   Application claims (return in token):
   ✅ Email Addresses
   ✅ Given Name
   ✅ Surname
   ✅ Display Name
   ✅ User's Object ID (sub claim)
   ✅ Identity Provider

4. Click "Create"
```

#### External Client Flow

```
Via Azure Portal (in B2C tenant):
1. Navigate: Azure AD B2C → User flows → + New user flow
2. Select: "Sign up and sign in" (v3 - Recommended)
3. Configuration:

   Name: External_SignUpSignIn
   (Full name will be: B2C_1_External_SignUpSignIn)

   Identity providers:
   ✅ Email signup (ONLY - no social providers)

   Multifactor authentication:
   • Disabled (keep simple for clients)

   User attributes (minimal):
   ✅ Email Address (required)
   ✅ Display Name (optional)

   Application claims:
   ✅ Email Addresses
   ✅ Display Name (optional)
   ✅ User's Object ID

   Conditional Access:
   • Self-service account recovery: Enabled
   • Age gating: Disabled

4. Click "Create"
```

**Additional Policies (Optional):**

```
Create these for password reset and profile editing:

1. B2C_1_PasswordReset
   - Type: "Password reset"
   - Claims: Email, Object ID

2. B2C_1_ProfileEdit
   - Type: "Profile editing"
   - Attributes: Given Name, Surname, Display Name, Job Title
   - Claims: Same as attributes + Email, Object ID

3. B2C_1_External_PasswordReset
   - Type: "Password reset" (for external users)
   - Claims: Email, Object ID
```

---

### Step 3: Create App Registrations

#### Internal Users App

```
Via Azure Portal (in B2C tenant):
1. Navigate: Azure AD B2C → App registrations → + New registration
2. Configuration:

   Name: Manimp-Internal

   Supported account types:
   • Accounts in any identity provider or organizational directory
     (for authenticating users with user flows)

   Redirect URIs (Web):
   • https://acme.manimp.com/signin-oidc
   • https://acme.files.manimp.com/signin-oidc
   
   Note: Azure B2C supports wildcards in production with proper setup
   For now, you'll need to add each tenant subdomain manually or use
   a custom domain validation approach

3. Click "Register"

4. After creation:
   • Copy "Application (client) ID" → Save to Key Vault
   • Navigate to "Certificates & secrets"
   • Click "+ New client secret"
   • Description: "Manimp Internal Auth"
   • Expires: 24 months (set calendar reminder!)
   • Copy secret VALUE → Save to Key Vault immediately

5. Configure tokens:
   • Navigate to "Token configuration"
   • Add optional claims:
     - ID token: email, family_name, given_name
     - Access token: email

6. API permissions (already configured):
   ✅ Microsoft Graph → User.Read (Delegated)
   ✅ OpenID permissions (openid, offline_access, profile, email)
```

**Save to Azure Key Vault:**

```bash
# Save Client ID
az keyvault secret set \
  --vault-name kv-manimp-prod \
  --name "AzureAdB2C--ClientId" \
  --value "<paste-client-id-here>"

# Save Client Secret
az keyvault secret set \
  --vault-name kv-manimp-prod \
  --name "AzureAdB2C--ClientSecret" \
  --value "<paste-secret-value-here>"
```

---

#### External Users App

```
Via Azure Portal (in B2C tenant):
1. Navigate: Azure AD B2C → App registrations → + New registration
2. Configuration:

   Name: Manimp-External

   Supported account types:
   • Accounts in any identity provider or organizational directory

   Redirect URIs (Web):
   • https://acme.docs.manimp.com/signin-oidc-external
   • https://buildco.docs.manimp.com/signin-oidc-external
   (Add more as tenants onboard)

3. Click "Register"

4. After creation:
   • Copy "Application (client) ID"
   • Create client secret (24 months)
   • Copy secret value immediately

5. Token configuration:
   • Add optional claims:
     - ID token: email
     - Access token: email
```

**Save to Key Vault:**

```bash
az keyvault secret set \
  --vault-name kv-manimp-prod \
  --name "AzureAdB2C--ExternalClientId" \
  --value "<external-client-id>"

az keyvault secret set \
  --vault-name kv-manimp-prod \
  --name "AzureAdB2C--ExternalClientSecret" \
  --value "<external-secret-value>"
```

---

## 💻 Application Configuration

### Install NuGet Packages

```bash
cd Manimp.Web
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.Identity.Web.UI

cd ../Manimp.Api
dotnet add package Microsoft.Identity.Web
```

---

### appsettings.json Configuration

**Manimp.Web/appsettings.json:**

```json
{
  "AzureAdB2C": {
    "Instance": "https://manimp.b2clogin.com",
    "Domain": "manimp.onmicrosoft.com",
    "TenantId": "YOUR-B2C-TENANT-ID",
    "ClientId": "YOUR-INTERNAL-CLIENT-ID",
    "ClientSecret": "FROM-KEY-VAULT",
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn",
    "EditProfilePolicyId": "B2C_1_ProfileEdit",
    "ResetPasswordPolicyId": "B2C_1_PasswordReset",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    
    "ExternalClientId": "YOUR-EXTERNAL-CLIENT-ID",
    "ExternalClientSecret": "FROM-KEY-VAULT",
    "ExternalSignUpSignInPolicyId": "B2C_1_External_SignUpSignIn",
    "ExternalResetPasswordPolicyId": "B2C_1_External_PasswordReset"
  },
  
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Identity": "Information"
    }
  }
}
```

**Production (with Key Vault):**

```json
{
  "AzureAdB2C": {
    "Instance": "https://manimp.b2clogin.com",
    "Domain": "manimp.onmicrosoft.com",
    "TenantId": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/AzureAdB2C--TenantId/)",
    "ClientId": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/AzureAdB2C--ClientId/)",
    "ClientSecret": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/AzureAdB2C--ClientSecret/)",
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn",
    
    "ExternalClientId": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/AzureAdB2C--ExternalClientId/)",
    "ExternalClientSecret": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/AzureAdB2C--ExternalClientSecret/)"
  }
}
```

---

### Program.cs Implementation

**Manimp.Web/Program.cs:**

```csharp
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Manimp.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// INTERNAL USER AUTHENTICATION (Employees)
// ============================================
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        
        // Use SignUpSignIn policy by default
        options.Instance = builder.Configuration["AzureAdB2C:Instance"];
        options.Domain = builder.Configuration["AzureAdB2C:Domain"];
        options.TenantId = builder.Configuration["AzureAdB2C:TenantId"];
        options.ClientId = builder.Configuration["AzureAdB2C:ClientId"];
        options.ClientSecret = builder.Configuration["AzureAdB2C:ClientSecret"];
        options.SignUpSignInPolicyId = builder.Configuration["AzureAdB2C:SignUpSignInPolicyId"];
        options.CallbackPath = builder.Configuration["AzureAdB2C:CallbackPath"];
        
        // Handle policy redirects (password reset, edit profile)
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                // Check if password reset requested
                if (context.Properties.Items.ContainsKey("policy") &&
                    context.Properties.Items["policy"] == "PasswordReset")
                {
                    context.ProtocolMessage.Scope = "openid profile offline_access";
                    context.ProtocolMessage.ResponseType = "code";
                    context.ProtocolMessage.IssuerAddress = 
                        context.ProtocolMessage.IssuerAddress.ToLower()
                            .Replace("b2c_1_signupsignin", 
                                builder.Configuration["AzureAdB2C:ResetPasswordPolicyId"].ToLower());
                }
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                // Handle password reset cancellation
                context.HandleResponse();
                if (context.Failure.Message.Contains("AADB2C90118"))
                {
                    context.Response.Redirect("/Account/ResetPassword");
                }
                else
                {
                    context.Response.Redirect("/Error?message=" + context.Failure.Message);
                }
                return Task.CompletedTask;
            }
        };
    },
    options =>
    {
        // Cookie configuration
        options.Cookie.Name = ".Manimp.Internal.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    },
    openIdConnectScheme: "InternalB2C",
    cookieScheme: CookieAuthenticationDefaults.AuthenticationScheme);

// ============================================
// EXTERNAL USER AUTHENTICATION (Clients)
// ============================================
builder.Services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(
        options =>
        {
            options.Instance = builder.Configuration["AzureAdB2C:Instance"];
            options.Domain = builder.Configuration["AzureAdB2C:Domain"];
            options.TenantId = builder.Configuration["AzureAdB2C:TenantId"];
            options.ClientId = builder.Configuration["AzureAdB2C:ExternalClientId"];
            options.ClientSecret = builder.Configuration["AzureAdB2C:ExternalClientSecret"];
            options.SignUpSignInPolicyId = builder.Configuration["AzureAdB2C:ExternalSignUpSignInPolicyId"];
            options.CallbackPath = "/signin-oidc-external";
            options.SignedOutCallbackPath = "/signout-callback-oidc-external";
            
            // Simpler event handling for external users
            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = async context =>
                {
                    // Link B2C user to ExternalUser table
                    var claimsTransformer = context.HttpContext.RequestServices
                        .GetRequiredService<ExternalUserClaimsTransformation>();
                    context.Principal = await claimsTransformer.TransformAsync(context.Principal);
                }
            };
        },
        options =>
        {
            options.Cookie.Name = ".Manimp.External.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
        },
        openIdConnectScheme: "ExternalB2C",
        cookieScheme: "ExternalUserScheme"
    );

// ============================================
// AUTHORIZATION POLICIES
// ============================================
builder.Services.AddAuthorization(options =>
{
    // Internal user policies
    options.AddPolicy("Admin", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin")
              .AddAuthenticationSchemes("InternalB2C"));
              
    options.AddPolicy("Manager", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "Manager")
              .AddAuthenticationSchemes("InternalB2C"));
              
    options.AddPolicy("User", policy => 
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "Manager", "User")
              .AddAuthenticationSchemes("InternalB2C"));
    
    // External user policy
    options.AddPolicy("ExternalUser", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes("ExternalB2C"));
});

// ============================================
// CUSTOM CLAIMS TRANSFORMATION
// ============================================
builder.Services.AddScoped<ExternalUserClaimsTransformation>();
builder.Services.AddTransient<IClaimsTransformation, ExternalUserClaimsTransformation>();

// ============================================
// MVC + RAZOR PAGES
// ============================================
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI(); // Adds default login/logout pages

builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

// ... rest of your services

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // ⚠️ Must come before UseAuthorization
app.UseAuthorization();

// Custom middleware to detect domain and set correct auth scheme
app.UseMiddleware<MultiDomainAuthenticationMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
```

---

### Custom Claims Transformation

**Manimp.Services/Implementation/ExternalUserClaimsTransformation.cs:**

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Manimp.Data;

namespace Manimp.Services.Implementation;

public class ExternalUserClaimsTransformation : IClaimsTransformation
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExternalUserClaimsTransformation> _logger;

    public ExternalUserClaimsTransformation(
        AppDbContext context,
        ILogger<ExternalUserClaimsTransformation> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Only transform external B2C users
        if (principal.Identity?.AuthenticationType != "ExternalB2C")
            return principal;

        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                    ?? principal.FindFirst("emails")?.Value;
                    
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("External B2C user has no email claim");
            return principal;
        }

        // Find external user record by email
        var externalUser = await _context.ExternalUsers
            .Include(u => u.ProjectAccess)
            .ThenInclude(pa => pa.Project)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (externalUser == null)
        {
            _logger.LogWarning("No ExternalUser record found for {Email}", email);
            return principal;
        }

        // Add custom claims
        var identity = (ClaimsIdentity)principal.Identity;
        
        identity.AddClaim(new Claim("external_user_id", externalUser.Id.ToString()));
        identity.AddClaim(new Claim("tenant_id", externalUser.TenantId.ToString()));
        
        // Add project access claims
        foreach (var projectAccess in externalUser.ProjectAccess)
        {
            identity.AddClaim(new Claim("project_id", projectAccess.ProjectId.ToString()));
            identity.AddClaim(new Claim("project_name", projectAccess.Project.Name));
        }

        _logger.LogInformation(
            "Transformed external user {Email} with {ProjectCount} projects",
            email, externalUser.ProjectAccess.Count);

        return principal;
    }
}
```

---

### Multi-Domain Authentication Middleware

**Manimp.Services/Middleware/MultiDomainAuthenticationMiddleware.cs:**

```csharp
using Microsoft.AspNetCore.Authentication;

namespace Manimp.Services.Middleware;

public class MultiDomainAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MultiDomainAuthenticationMiddleware> _logger;

    public MultiDomainAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<MultiDomainAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();

        // Detect domain type and set default challenge scheme
        if (host.Contains(".docs."))
        {
            // External client portal - use external B2C
            context.Items["AuthScheme"] = "ExternalB2C";
            _logger.LogDebug("Detected external domain: {Host}", host);
        }
        else
        {
            // Internal domains (main app or files)
            context.Items["AuthScheme"] = "InternalB2C";
            _logger.LogDebug("Detected internal domain: {Host}", host);
        }

        await _next(context);
    }
}
```

---

## 🔐 Role Management

### Assigning Roles to B2C Users

Azure AD B2C doesn't have built-in role management like Azure AD. You have two options:

#### Option 1: Custom Claims via API Connectors (Recommended)

```csharp
// Manimp.Api/Controllers/B2CClaimsController.cs
[ApiController]
[Route("api/b2c/claims")]
public class B2CClaimsController : ControllerBase
{
    private readonly AppDbContext _context;

    [HttpPost("enrich")]
    public async Task<IActionResult> EnrichClaims([FromBody] B2CClaimRequest request)
    {
        // B2C will call this during token issuance
        var email = request.Email;
        
        // Look up user in your database
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Ok(new { 
                version = "1.0.0",
                action = "Continue" 
            });
        }

        // Return custom claims
        return Ok(new
        {
            version = "1.0.0",
            action = "Continue",
            extension_Role = user.Role, // "Admin", "Manager", "User"
            extension_TenantId = user.TenantId.ToString(),
            extension_UserId = user.Id.ToString()
        });
    }
}

public class B2CClaimRequest
{
    public string Email { get; set; }
    public string ObjectId { get; set; }
}
```

**Configure in Azure Portal:**
```
1. Azure AD B2C → API connectors → + New API connector
2. Name: ClaimsEnrichment
3. Endpoint URL: https://api.manimp.com/api/b2c/claims/enrich
4. Authentication: API key or client certificate
5. Claims to send: email, objectId
6. Claims to receive: role, tenant_id, user_id

7. User flows → B2C_1_SignUpSignIn → API connectors
8. After user signs in: Select "ClaimsEnrichment"
9. Save
```

---

#### Option 2: Store Roles in B2C Custom Attributes

```bash
# Via Azure Portal:
# 1. Azure AD B2C → User attributes → + Add
# 2. Name: Role
# 3. Data type: String
# 4. Add to user flow: B2C_1_SignUpSignIn

# Then in your user flow:
# - User attributes: ✅ Role
# - Application claims: ✅ Role

# Set role via Microsoft Graph API:
POST https://graph.microsoft.com/v1.0/users/{user-id}
Content-Type: application/json

{
  "extension_<app-id>_Role": "Admin"
}
```

---

## 🧪 Testing Authentication

### Test Internal Login

```bash
# Start application
cd Manimp.Web
dotnet run --urls https://localhost:5001

# Visit in browser:
https://localhost:5001

# Should redirect to:
https://manimp.b2clogin.com/manimp.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1_SignUpSignIn&...

# After login, redirects back to:
https://localhost:5001/signin-oidc

# Check claims:
# Navigate to a page with:
@using Microsoft.AspNetCore.Components.Authorization

<AuthorizeView>
    <Authorized>
        <p>Hello, @context.User.Identity.Name!</p>
        <ul>
            @foreach (var claim in context.User.Claims)
            {
                <li>@claim.Type: @claim.Value</li>
            }
        </ul>
    </Authorized>
    <NotAuthorized>
        <a href="MicrosoftIdentity/Account/SignIn">Log in</a>
    </NotAuthorized>
</AuthorizeView>
```

---

### Test External Login

```bash
# Update hosts file for local testing:
sudo nano /etc/hosts

# Add:
127.0.0.1 acme.docs.localhost

# Visit:
https://acme.docs.localhost:5001

# Should use ExternalB2C scheme and redirect to:
# B2C_1_External_SignUpSignIn policy
```

---

## 💰 Pricing

```
Azure AD B2C Pricing:

Monthly Active Users (MAU):
├─ 0 - 50,000:              FREE ✅
├─ 50,001 - 100,000:        $0.00325/user
├─ 100,001 - 1,000,000:     $0.0016/user
└─ 1,000,001+:              $0.0013/user

Multi-Factor Authentication:
├─ First 10,000 auths:      FREE ✅
└─ Additional:              $0.03/authentication

Premium Features (P1):
├─ Conditional Access:      $6/user/month
├─ Identity Protection:     $6/user/month
└─ Custom branding:         Included

Example Costs:
────────────────────────────────────────────
Scenario 1: 5 tenants, 50 internal users, 200 external users
├─ Total MAU: 250
├─ Cost: $0.00/month ✅ (under 50k threshold)

Scenario 2: 50 tenants, 500 internal users, 10,000 external users
├─ Total MAU: 10,500
├─ Cost: $0.00/month ✅ (still under 50k)

Scenario 3: 200 tenants, 2,000 internal users, 60,000 external users
├─ Total MAU: 62,000
├─ First 50,000: FREE
├─ Next 12,000: $39.00 (12k × $0.00325)
└─ Total: $39.00/month
```

---

## 🚀 Next Steps

### Phase 1: Setup B2C Tenant (Week 1)
- [ ] Create Azure AD B2C tenant
- [ ] Create user flows (internal + external)
- [ ] Create app registrations
- [ ] Save credentials to Key Vault
- [ ] Configure redirect URIs

### Phase 2: Integrate with Manimp.Web (Week 2)
- [ ] Install Microsoft.Identity.Web packages
- [ ] Update Program.cs with authentication
- [ ] Add claims transformation service
- [ ] Add multi-domain middleware
- [ ] Test local login

### Phase 3: Role Management (Week 3)
- [ ] Set up API connector for claims enrichment
- [ ] OR: Configure custom attributes
- [ ] Test role-based authorization
- [ ] Update admin UI for user role assignment

### Phase 4: External User Integration (Week 4)
- [ ] Link B2C external users to ExternalUsers table
- [ ] Test project-based access
- [ ] Test anonymous access (when enabled)
- [ ] Configure email templates in B2C

### Phase 5: Production Deployment (Week 5)
- [ ] Update wildcard redirect URIs in B2C
- [ ] Enable MFA for Admin users
- [ ] Configure custom branding per tenant
- [ ] Set up monitoring and alerts

---

## 📚 Additional Resources

- [Azure AD B2C Documentation](https://learn.microsoft.com/en-us/azure/active-directory-b2c/)
- [Microsoft.Identity.Web Library](https://github.com/AzureAD/microsoft-identity-web)
- [B2C User Flow Customization](https://learn.microsoft.com/en-us/azure/active-directory-b2c/customize-ui)
- [API Connectors for Claims Enrichment](https://learn.microsoft.com/en-us/azure/active-directory-b2c/api-connectors-overview)

---

**Ready to implement!** 🎉
