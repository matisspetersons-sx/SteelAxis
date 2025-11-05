# Entra External ID Authentication - Implementation Summary

This document summarizes the Entra External ID authentication implementation for the SteelAxis application.

## What Was Implemented

### 1. NuGet Packages Installed

**SteelAxis.Web (Blazor Server)**
- `Microsoft.Identity.Web` v3.3.0 - Core authentication library
- `Microsoft.Identity.Web.UI` v3.3.0 - UI components for sign-in/sign-out

**SteelAxis.Api (Web API)**
- `Microsoft.Identity.Web` v3.3.0 - Core authentication library for JWT bearer tokens

### 2. Configuration Files Updated

**SteelAxis.Web/appsettings.json**
- Added `AzureAdB2C` section with CIAM configuration
- Production values stored in Azure Key Vault
- Local development uses appsettings.json configuration

**SteelAxis.Api/appsettings.json**
- Added `AzureAdB2C` section with CIAM configuration
- Production values stored in Azure Key Vault
- Local development uses appsettings.json configuration

### 3. Program.cs Updated

**SteelAxis.Web/Program.cs**
- Added OpenID Connect authentication using `AddMicrosoftIdentityWebApp()`
- Added authorization services
- Added Microsoft Identity UI controllers
- Added authentication and authorization middleware
- Added consent handler for incremental consent

**SteelAxis.Api/Program.cs**
- Added JWT Bearer authentication using `AddMicrosoftIdentityWebApi()`
- Added authorization services
- Added authentication and authorization middleware

### 4. Blazor Components Created

**Login.razor**
- Login page with sign-in button
- Redirects to Microsoft Identity for authentication

**LoginDisplay.razor**
- Component to show user info when authenticated
- Sign out button
- Sign in button for unauthenticated users

**RedirectToLogin.razor**
- Helper component to redirect unauthenticated users to login page

**App.razor.new**
- Updated App component with `CascadingAuthenticationState`
- Configured `AuthorizeRouteView` for protected routes
- Handles unauthorized access

**_Imports.razor**
- Added `Microsoft.AspNetCore.Components.Authorization` namespace

### 5. API Controller Created

**SecureDataController.cs**
- Example of protected API endpoints
- Demonstrates `[Authorize]` attribute usage
- Shows role-based authorization
- Implements scope validation with `[RequiredScope]`
- Examples of accessing user claims

### 6. Documentation Created

**ENTRA_EXTERNAL_ID_SETUP.md**
- Complete setup guide for Entra External ID
- Step-by-step tenant creation
- App registration instructions
- Configuration guide
- Security best practices
- Troubleshooting tips

## How to Complete the Setup

1. **Create Entra External ID Tenant**
   - Follow the guide in `ENTRA_EXTERNAL_ID_SETUP.md`
   - Complete Steps 1-3 to register your applications

2. **Update Configuration**
   - Replace placeholders in `appsettings.json` files
   - OR use User Secrets for local development:
     ```bash
     cd SteelAxis.Web
     dotnet user-secrets set "AzureAdExternalId:TenantId" "your-tenant-id"
     dotnet user-secrets set "AzureAdExternalId:ClientId" "your-client-id"
     dotnet user-secrets set "AzureAdExternalId:ClientSecret" "your-secret"
     ```

3. **Test Authentication**
   - Run the web application: `dotnet run --project SteelAxis.Web`
   - Navigate to a protected route
   - You should be redirected to Microsoft login
   - After authentication, you'll be redirected back

4. **Update App.razor**
   - Replace the content of `Components/App.razor` with the content from `Components/App.razor.new`
   - This enables authentication throughout the application

## Key Features

### Authentication Flow
1. User accesses protected route
2. Application redirects to Entra External ID
3. User signs in with their credentials
4. Entra External ID validates and issues tokens
5. User is redirected back to application
6. Application validates tokens and establishes session

### Security Features
- OpenID Connect authentication
- JWT Bearer tokens for API
- Scope-based authorization
- Role-based authorization
- Claims-based identity
- HTTPS enforcement
- CSRF protection with anti-forgery tokens

### User Experience
- Seamless login/logout
- Single sign-on (SSO) support
- Remember me functionality
- Secure session management
- Automatic token refresh

## Using Authentication in Your Code

### Protecting Blazor Pages
```razor
@page "/protected"
@attribute [Authorize]

<h3>This page requires authentication</h3>
```

### Protecting API Endpoints
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var userId = User.FindFirst("oid")?.Value;
        return Ok($"Hello {User.Identity?.Name}!");
    }
}
```

### Accessing User Information
```csharp
// In a Blazor component
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private async Task GetUserInfo()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.Identity?.IsAuthenticated == true)
        {
            var name = user.Identity.Name;
            var email = user.FindFirst("preferred_username")?.Value;
            var userId = user.FindFirst("oid")?.Value;
        }
    }
}
```

### Configuration

Production credentials are stored in Azure Key Vault (`kv-Steelaxis-dev`):
- `AzureAdB2C--Authority`
- `AzureAdB2C--ClientId`
- `AzureAdB2C--ApiClientId`
- `AzureAdB2C--DefaultScopes`

For local development, values are configured in `appsettings.json`.

### Role-Based Authorization
```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    return Ok("Admin access granted");
}
```

## Next Steps

1. **Configure User Roles**
   - Add app roles in Entra External ID
   - Assign roles to users
   - Implement role-based authorization

2. **Add Custom Claims**
   - Configure token claims in Entra External ID
   - Map claims to application properties
   - Use claims for authorization decisions

3. **Implement Authorization Policies**
   - Create custom authorization policies
   - Use policies for fine-grained access control

4. **Add External Identity Providers**
   - Enable social login (Google, Facebook, etc.)
   - Configure federation with other identity providers

5. **Set Up Multi-Factor Authentication**
   - Enable MFA in Entra External ID
   - Configure conditional access policies

6. **Monitor and Audit**
   - Enable sign-in logs
   - Set up alerts for suspicious activity
   - Implement audit logging

## Security Recommendations

1. ✅ Use HTTPS in production
2. ✅ Store secrets in Azure Key Vault (not in appsettings.json)
3. ✅ Enable HSTS headers
4. ✅ Implement proper CORS policies for API
5. ✅ Use scope validation for API endpoints
6. ✅ Rotate client secrets regularly
7. ✅ Enable logging and monitoring
8. ✅ Implement proper error handling
9. ✅ Use anti-forgery tokens
10. ✅ Keep packages up to date

## Troubleshooting

If you encounter issues, check:
1. Configuration values are correct (TenantId, ClientId, etc.)
2. Redirect URIs match exactly in app registration
3. Client secret is valid and not expired
4. User has access to the application
5. HTTPS is enabled (required for authentication)
6. Logging is enabled to see detailed error messages

For more details, see `ENTRA_EXTERNAL_ID_SETUP.md`.
