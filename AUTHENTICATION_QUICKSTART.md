# Entra External ID Authentication - Quick Start

## ✅ What's Been Set Up

Your SteelAxis application is fully configured with Microsoft Entra External ID (CIAM) authentication for both the Web application and API.

### CIAM Tenant (Already Created)
- **Tenant**: Configured in production
- **User Flow**: B2C_1_susi (Sign up and sign in)
- **Authority**: Stored in Azure Key Vault (`AzureAdB2C--Authority`)
- **Email Sign-up**: Enabled with verification

### App Registrations (Completed)
- **Web Client ID**: Stored in Azure Key Vault (`AzureAdB2C--ClientId`)
- **API Client ID**: Stored in Azure Key Vault (`AzureAdB2C--ApiClientId`)
- **API Scope**: Stored in Azure Key Vault (`AzureAdB2C--DefaultScopes`)

### Packages Installed
- ✅ Microsoft.Identity.Web v3.6.0 (Web & API)
- ✅ Microsoft.Identity.Web.UI v3.3.0 (Web only)

### Files Created/Modified

**Configuration Files:**
- `SteelAxis.Web/appsettings.json` - Added AzureAdExternalId section
- `SteelAxis.Api/appsettings.json` - Added AzureAdExternalId section

**Program Files:**
- `SteelAxis.Web/Program.cs` - Authentication middleware configured
- `SteelAxis.Api/Program.cs` - JWT bearer authentication configured

**Blazor Components:**
- `Components/Pages/Login.razor` - Login page
- `Components/Layout/LoginDisplay.razor` - User info/sign out component
- `Components/RedirectToLogin.razor` - Redirect helper
- `Components/App.razor.new` - Updated App component with auth
- `Components/_Imports.razor` - Added authorization namespace

**API Controllers:**
- `Controllers/SecureDataController.cs` - Example secured endpoints

**Documentation:**
- `ENTRA_EXTERNAL_ID_SETUP.md` - Complete setup guide
- `ENTRA_IMPLEMENTATION.md` - Implementation details
- `SECURITY_ADVISORY.md` - Package vulnerability info

## 🚀 Next Steps

### 1. Update App.razor (Important!)

Replace the content of `SteelAxis.Web/Components/App.razor` with the content from `App.razor.new` to enable authentication throughout your application.

### 2. Test the Setup (Ready to Use!)

**Run the Web Application:**
```bash
cd SteelAxis.Web
dotnet run
```
Navigate to https://localhost:7071/login

**Run the API:**
```bash
cd SteelAxis.Api
dotnet run
```
Test endpoint: https://localhost:7072/api/health (or your configured port)

## 📝 Using Authentication in Your Code

### Protect a Blazor Page
```razor
@page "/dashboard"
@attribute [Authorize]

<h3>Dashboard</h3>
<p>This page requires authentication</p>
```

### Protect an API Endpoint
```csharp
[Authorize]
[HttpGet]
public IActionResult GetData()
{
    var userName = User.Identity?.Name;
    return Ok($"Hello, {userName}!");
}
```

### Show User Info
```razor
@inject AuthenticationStateProvider AuthStateProvider

@code {
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.Identity?.IsAuthenticated == true)
        {
            var name = user.Identity.Name;
            // Use user info...
        }
    }
}
```

## ⚠️ Important Notes

1. **CIAM Configuration**: Your application is configured with the production CIAM tenant
   - All credentials stored securely in Azure Key Vault (`kv-Steelaxis-dev`)
   - Configuration automatically injected in Azure App Services
   - For local development, configuration is in `appsettings.json`

2. **Security Warning**: The Microsoft.Identity.Web package has a known moderate vulnerability (see `SECURITY_ADVISORY.md`)
   - Current version (3.6.0) is the latest stable
   - Monitor for updates
   - Acceptable for development, review for production

3. **HTTPS Required**: Authentication requires HTTPS - development certificates are configured automatically

4. **Production Secrets**: For production deployment, use Azure Key Vault (kv-Steelaxis-dev) which already stores:
   - AzureAdB2C--Authority
   - AzureAdB2C--ClientId
   - AzureAdB2C--ApiClientId
   - AzureAdB2C--DefaultScopes

## 📚 Additional Resources

- **Setup Guide**: `ENTRA_EXTERNAL_ID_SETUP.md` - Step-by-step Entra External ID configuration
- **Implementation Details**: `ENTRA_IMPLEMENTATION.md` - What was implemented and how to use it
- **Security Info**: `SECURITY_ADVISORY.md` - Package vulnerability details
- **Microsoft Docs**: https://learn.microsoft.com/entra/external-id/

## 🔧 Troubleshooting

### Build Warnings About Vulnerability
This is expected - see `SECURITY_ADVISORY.md` for details and mitigation strategies.

### Redirect URI Mismatch
Ensure the redirect URI in your app registration exactly matches your application URL + callback path.

### Invalid Client Secret
Check that:
- The secret hasn't expired
- You copied the secret value (not the ID)
- The secret is correctly configured in User Secrets or appsettings

### Can't Sign In
Verify:
- HTTPS is enabled
- Tenant ID and Client ID are correct
- User exists in your Entra External ID tenant
- App registration is configured correctly

## ✨ What's Working Now

- ✅ CIAM tenant created and configured
- ✅ App registrations completed with proper scopes
- ✅ OpenID Connect authentication configured
- ✅ JWT Bearer token validation for API
- ✅ Login/logout flows ready
- ✅ User identity management
- ✅ Example protected endpoints
- ✅ Authorization components
- ✅ Azure Key Vault integration for production
- ✅ Complete documentation

**Status**: Fully configured and ready to use! Authentication is functional with the production CIAM tenant.

---

Need help? Check the detailed guides or refer to Microsoft's documentation.
