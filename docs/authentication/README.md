# Authentication Documentation

Azure AD B2C authentication implementation for Manimp's multi-domain architecture.

---

## 📚 Documents

### 🔐 [azure-b2c-authentication.md](./azure-b2c-authentication.md)
**Complete Azure AD B2C setup guide** (800+ lines) ⭐

**Contents:**
- Architecture overview with diagrams
- Dual authentication schemes (internal + external users)
- B2C tenant creation
- User flow (policy) configuration
- App registration setup
- ASP.NET Core integration
- Custom claims transformation
- Multi-domain authentication middleware
- Role management strategies
- Testing procedures
- Pricing breakdown
- 5-week implementation plan

---

## 🏗️ Architecture Overview

```
Internal Users (Employees)
├─ Domain: *.manimp.com, *.files.manimp.com
├─ B2C Policy: B2C_1_SignUpSignIn
├─ Roles: Admin, Manager, User
├─ Identity Providers: Email, Microsoft Account, Google
├─ MFA: Optional (recommended for Admin)
└─ Claims: email, role, tenant_id, user_id

External Users (Clients)
├─ Domain: *.docs.manimp.com
├─ B2C Policy: B2C_1_External_SignUpSignIn
├─ Roles: Project-based access
├─ Identity Providers: Email only
├─ MFA: Disabled
└─ Claims: email, external_user_id, project_ids
```

---

## 🎯 Quick Start

### 1. Create B2C Tenant
```bash
# Via Azure Portal:
# Search "Azure AD B2C" → Create tenant
# Organization: Manimp
# Domain: manimp.onmicrosoft.com
# Resource group: rg-manimp-prod
```

### 2. Create User Flows
```
Internal Flow:
- Name: B2C_1_SignUpSignIn
- Providers: Email, Microsoft, Google
- MFA: Optional
- Attributes: email, given_name, surname, display_name

External Flow:
- Name: B2C_1_External_SignUpSignIn
- Providers: Email only
- MFA: Disabled
- Attributes: email, display_name
```

### 3. Create App Registrations
```bash
# Internal App
Name: Manimp-Internal
Redirect URIs:
  - https://*.manimp.com/signin-oidc
  - https://*.files.manimp.com/signin-oidc

# External App
Name: Manimp-External
Redirect URIs:
  - https://*.docs.manimp.com/signin-oidc-external
```

### 4. Install NuGet Packages
```bash
cd Manimp.Web
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.Identity.Web.UI
```

### 5. Configure Application
```json
// appsettings.json
{
  "AzureAdB2C": {
    "Instance": "https://manimp.b2clogin.com",
    "Domain": "manimp.onmicrosoft.com",
    "ClientId": "YOUR-INTERNAL-CLIENT-ID",
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn",
    "ExternalClientId": "YOUR-EXTERNAL-CLIENT-ID",
    "ExternalSignUpSignInPolicyId": "B2C_1_External_SignUpSignIn"
  }
}
```

### 6. Update Program.cs
See complete implementation in [azure-b2c-authentication.md](./azure-b2c-authentication.md#program-cs-implementation)

---

## 🔑 Key Features

### Dual Authentication Schemes
✅ Internal users authenticate via default B2C policy  
✅ External users use separate B2C policy  
✅ Middleware auto-detects domain type  
✅ Separate cookie schemes prevent conflicts

### Claims Enrichment
✅ Internal: Role, TenantId, UserId added via API connector  
✅ External: ProjectIds added via claims transformation  
✅ Seamless integration with existing database

### Role-Based Authorization
```csharp
[Authorize(Policy = "Admin")]
public async Task<IActionResult> AdminOnly() { }

[Authorize(Policy = "Manager")]
public async Task<IActionResult> ManagerAndAbove() { }

[Authorize(Policy = "ExternalUser")]
public async Task<IActionResult> ClientPortal() { }
```

---

## 💰 Pricing

```
Azure AD B2C Cost Structure:

Monthly Active Users (MAU):
├─ 0 - 50,000:          FREE ✅
├─ 50,001 - 100,000:    $0.00325/user
└─ 100,001+:            $0.0016/user

MFA (Multi-Factor Auth):
├─ First 10,000:        FREE ✅
└─ Additional:          $0.03/auth

Example Scenarios:
─────────────────────────────────────────
Scenario 1: 250 users (5 tenants)
Cost: $0.00/month ✅

Scenario 2: 10,500 users (50 tenants)
Cost: $0.00/month ✅

Scenario 3: 62,000 users (200 tenants)
First 50k: FREE
Next 12k: $39.00
Total: $39.00/month
```

**Result:** FREE for most SaaS startups! 🎉

---

## 🧪 Testing

### Test Internal Login
```bash
# Start app
cd Manimp.Web
dotnet run --urls https://localhost:5001

# Visit
https://localhost:5001

# Should redirect to B2C login
# After login, check claims at:
/Account/Claims
```

### Test External Login
```bash
# Update /etc/hosts
127.0.0.1 acme.docs.localhost

# Visit
https://acme.docs.localhost:5001

# Should use external B2C policy
```

### Verify Claims
```csharp
// In Blazor component
@inject AuthenticationStateProvider AuthState

@code {
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState.GetAuthenticationStateAsync();
        var user = authState.User;
        
        foreach (var claim in user.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
    }
}
```

---

## 🔧 Common Tasks

### Add User to B2C
```bash
# Via Azure Portal:
# Azure AD B2C → Users → New user
# Or let them self-register via user flow
```

### Assign Role
```csharp
// Via API connector (recommended)
// Or via Microsoft Graph API:
POST https://graph.microsoft.com/v1.0/users/{user-id}
{
  "extension_<app-id>_Role": "Admin"
}
```

### Custom Branding
```
Azure AD B2C → Company branding
- Logo, background color, custom CSS
- Per user flow customization
- Localization support
```

### Password Reset
```csharp
// Create B2C_1_PasswordReset policy
// Link from login page:
<a href="/MicrosoftIdentity/Account/ResetPassword">Forgot password?</a>
```

---

## 🐛 Troubleshooting

### "AADB2C90118" Error
**Issue:** User clicked "Forgot password?"  
**Solution:** Already handled in `Program.cs` events - redirects to password reset flow

### Redirect URI Mismatch
**Issue:** "Reply URL mismatch"  
**Solution:** Add exact redirect URI to B2C app registration (case-sensitive)

### Claims Not Appearing
**Issue:** Custom claims not in token  
**Solution:** 
1. Check API connector is configured in user flow
2. Verify endpoint returns correct JSON structure
3. Check "Application claims" in user flow settings

### Token Expired
**Issue:** User logged out unexpectedly  
**Solution:** Adjust cookie expiration in `Program.cs`:
```csharp
options.ExpireTimeSpan = TimeSpan.FromDays(30);
options.SlidingExpiration = true;
```

---

## 📊 Monitoring

### Track Authentication Events
```csharp
// In OpenIdConnectEvents
OnTokenValidated = context =>
{
    var logger = context.HttpContext.RequestServices
        .GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation(
        "User authenticated: {Email}",
        context.Principal.FindFirst("email")?.Value);
    
    return Task.CompletedTask;
}
```

### Application Insights
```kusto
// Failed logins
customEvents
| where name == "OnRemoteFailure"
| summarize count() by tostring(customDimensions.Reason)

// Successful logins
customEvents
| where name == "OnTokenValidated"
| summarize count() by bin(timestamp, 1h)
```

---

## 🔗 Related Documentation

- **[../azure-infrastructure/azure-infrastructure-setup.md](../azure-infrastructure/azure-infrastructure-setup.md)** - B2C tenant creation
- **[../file-storage/file-storage-multi-domain-architecture.md](../file-storage/file-storage-multi-domain-architecture.md)** - Multi-domain routing
- **[../general/manimp-strategic-guide.md](../general/manimp-strategic-guide.md)** - Overall authentication strategy

---

## 📅 Implementation Timeline

**Week 1:** B2C tenant + user flows  
**Week 2:** App registrations + secrets  
**Week 3:** ASP.NET Core integration  
**Week 4:** Claims transformation + role management  
**Week 5:** Testing + production deployment

---

## 🎯 What's Next

### Week 1: B2C Tenant Setup
- [ ] Create Azure AD B2C tenant (manimp.onmicrosoft.com)
- [ ] Create B2C_1_SignUpSignIn user flow (internal)
- [ ] Create B2C_1_External_SignUpSignIn user flow (external)
- [ ] Configure identity providers (Email, Microsoft, Google)
- [ ] Customize B2C UI with company branding

### Week 2: App Registrations
- [ ] Register Manimp-Internal app in B2C
- [ ] Register Manimp-External app in B2C
- [ ] Configure redirect URIs (wildcard domains)
- [ ] Generate client secrets
- [ ] Store credentials in Azure Key Vault

### Week 3: Application Integration
- [ ] Install Microsoft.Identity.Web NuGet packages
- [ ] Update Program.cs with dual authentication schemes
- [ ] Implement MultiDomainAuthenticationMiddleware
- [ ] Create ExternalUserClaimsTransformation service
- [ ] Test local authentication flows

### Week 4: Role Management
- [ ] Set up API connector for claims enrichment
- [ ] Configure custom attributes in B2C
- [ ] Implement role assignment UI in admin panel
- [ ] Test role-based authorization policies
- [ ] Link B2C users to internal Users table

### Week 5: Production Deployment
- [ ] Update B2C app registrations with production domains
- [ ] Enable MFA for Admin role users
- [ ] Configure custom branding per tenant
- [ ] Set up monitoring and alerts
- [ ] Document user onboarding process

### Future Enhancements
- [ ] Implement password complexity policies
- [ ] Add Conditional Access policies
- [ ] Configure Identity Protection
- [ ] Add social login providers (LinkedIn, GitHub)
- [ ] Implement single sign-out across all domains

---

**Secure by design!** 🔐
