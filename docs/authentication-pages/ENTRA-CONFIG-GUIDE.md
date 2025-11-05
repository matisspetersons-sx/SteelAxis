# Microsoft Entra External ID Configuration Guide

**Project:** SteelAxis Multi-Tenant Authentication  
**Created:** October 27, 2025  
**Tenant:** steelaxistenants.onmicrosoft.com

---

## 📋 Overview

This guide provides step-by-step instructions for configuring Microsoft Entra External ID (CIAM) to support SteelAxis's multi-tenant authentication system with:
- Admin registration and tenant creation
- User invitations with role assignment
- Custom user attributes (tenantId, role)
- Token claims configuration
- Email templates for invitations

---

## 🔧 Prerequisites

- **Microsoft Entra External ID Tenant:** steelaxistenants.onmicrosoft.com (✅ Already created)
- **Azure Subscription:** Active subscription with permissions to manage Entra resources
- **Admin Access:** Global Administrator or Application Administrator role
- **App Registrations:** SteelAxis Web App and API already registered

---

## 1️⃣ Custom User Attributes

### Purpose
Store tenant-specific information in user profiles for multi-tenancy support.

### Required Attributes

| Attribute Name | Data Type | Required | Description |
|----------------|-----------|----------|-------------|
| `tenantId` | String (GUID) | No | Primary tenant ID for user |
| `role` | String | No | User role in tenant (Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker) |
| `invitationToken` | String | No | Invitation token for pending users |

### Configuration Steps

1. **Navigate to Entra Admin Center**
   - Go to https://entra.microsoft.com
   - Sign in with admin credentials
   - Select **External Identities** from left menu

2. **Access Custom Attributes**
   - Click on your tenant: **steelaxistenants.onmicrosoft.com**
   - Navigate to **User attributes** → **Custom user attributes**

3. **Add `tenantId` Attribute**
   - Click **+ Add custom attribute**
   - Fill in details:
     ```
     Name: tenantId
     Data type: String
     Description: Primary tenant ID for multi-tenant access
     Maximum length: 36 (for GUID format)
     ```
   - Click **Create**

4. **Add `role` Attribute**
   - Click **+ Add custom attribute**
   - Fill in details:
     ```
     Name: role
     Data type: String
     Description: User role in tenant (Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker)
     ```
   - Click **Create**

5. **Add `invitationToken` Attribute**
   - Click **+ Add custom attribute**
   - Fill in details:
     ```
     Name: invitationToken
     Data type: String
     Description: Temporary invitation token
     Maximum length: 100
     ```
   - Click **Create**

6. **Verify Attributes**
   - Confirm all three attributes appear in the list
   - Note the attribute IDs (e.g., `extension_<appId>_tenantId`)

### Usage in Code

```csharp
// Writing custom attributes
var user = await graphClient.Users[userId]
    .Request()
    .UpdateAsync(new User
    {
        AdditionalData = new Dictionary<string, object>
        {
            { "extension_<appId>_tenantId", tenantId.ToString() },
            { "extension_<appId>_role", "Admin" }
        }
    });

// Reading custom attributes
var user = await graphClient.Users[userId]
    .Request()
    .Select($"id,mail,displayName,extension_<appId>_tenantId,extension_<appId>_role")
    .GetAsync();
    
var tenantId = user.AdditionalData["extension_<appId>_tenantId"]?.ToString();
var role = user.AdditionalData["extension_<appId>_role"]?.ToString();
```

---

## 2️⃣ User Flows Configuration

### Flow 1: Admin Registration (Tenant Creation)

**Purpose:** First-time admin user creates a new tenant account

**Configuration:**

1. **Create New User Flow**
   - Navigate to **User flows** in Entra admin center
   - Click **+ New user flow**
   - Select **Sign up and sign in** as flow type
   - Name: `B2C_1_admin_signup`

2. **Configure Identity Providers**
   - Select **Email signup**
   - Enable **Email verification**

3. **User Attributes to Collect**
   Check these attributes to collect during signup:
   - ✅ Email Address (required)
   - ✅ Display Name (required)
   - ✅ Given Name (optional)
   - ✅ Surname (optional)

4. **Application Claims to Return**
   Select these claims to include in tokens:
   - ✅ Email Addresses
   - ✅ Display Name
   - ✅ Given Name
   - ✅ Surname
   - ✅ User's Object ID
   - ✅ tenantId (custom attribute)
   - ✅ role (custom attribute)

5. **Page Layouts (Optional)**
   - Customize sign-up page with SteelAxis branding
   - Add company logo
   - Update color scheme to match brand

6. **Save User Flow**

### Flow 2: Invited User Signup

**Purpose:** Users accepting invitations to join existing tenants

**Configuration:**

1. **Create New User Flow**
   - Navigate to **User flows**
   - Click **+ New user flow**
   - Select **Sign up and sign in**
   - Name: `B2C_1_invited_user_signup`

2. **Configure Identity Providers**
   - Select **Email signup**
   - Enable **Email verification**

3. **User Attributes to Collect**
   - ✅ Email Address (required, pre-filled from invitation)
   - ✅ Display Name (required)
   - ✅ Given Name (optional)
   - ✅ Surname (optional)
   - ✅ invitationToken (custom attribute, hidden, pre-filled)

4. **Application Claims to Return**
   - ✅ Email Addresses
   - ✅ Display Name
   - ✅ Given Name
   - ✅ Surname
   - ✅ User's Object ID
   - ✅ tenantId (custom attribute)
   - ✅ role (custom attribute)
   - ✅ invitationToken (custom attribute)

5. **Save User Flow**

### Flow 3: Sign In

**Purpose:** Standard login for existing users

**Configuration:**

1. **Create New User Flow**
   - Name: `B2C_1_signin`
   - Type: **Sign up and sign in** (or **Sign in** only)

2. **Configure Identity Providers**
   - Select **Email signin**

3. **Application Claims**
   - Same as admin signup flow
   - Include custom attributes (tenantId, role)

4. **Multi-Factor Authentication (Optional)**
   - Enable MFA for enhanced security
   - Options: SMS, Email, Authenticator app

5. **Save User Flow**

---

## 3️⃣ Token Claims Configuration

### Purpose
Include custom attributes (tenantId, role) in ID tokens and access tokens for authorization.

### Configuration Steps

1. **Navigate to App Registrations**
   - Go to **App registrations** in Entra admin center
   - Select **SteelAxis Web App** registration

2. **Add Optional Claims**
   - Click **Token configuration** in left menu
   - Click **+ Add optional claim**
   - Select **ID** token type
   - Add these claims:
     - ✅ email
     - ✅ family_name
     - ✅ given_name
     - ✅ upn (User Principal Name)

3. **Add Custom Extension Claims**
   - Click **+ Add custom claim**
   - Select **ID token** and **Access token**
   - Add custom attributes:
     - ✅ extension_tenantId (map to claim name: `tenantId`)
     - ✅ extension_role (map to claim name: `role`)

4. **Configure API App Registration**
   - Repeat steps for **SteelAxis API** app registration
   - Ensure same claims are included in access tokens

5. **Token Configuration Example**
   
   Expected ID Token claims:
   ```json
   {
     "iss": "https://steelaxistenants.b2clogin.com/<tenant-id>/v2.0/",
     "sub": "<user-object-id>",
     "aud": "<client-id>",
     "exp": 1730000000,
     "iat": 1729996400,
     "email": "admin@example.com",
     "name": "John Doe",
     "given_name": "John",
     "family_name": "Doe",
     "oid": "<user-object-id>",
     "tenantId": "<tenant-guid>",
     "role": "Admin"
   }
   ```

### Reading Claims in Code

```csharp
// In ASP.NET Core API
[HttpGet("profile")]
[Authorize]
public IActionResult GetProfile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    var tenantId = User.FindFirst("tenantId")?.Value;
    var role = User.FindFirst("role")?.Value;
    
    return Ok(new
    {
        UserId = userId,
        Email = email,
        TenantId = tenantId != null ? Guid.Parse(tenantId) : (Guid?)null,
        Role = role ?? "User"
    });
}
```

```csharp
// In Blazor Server
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private string? tenantId;
    private string? role;
    
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        tenantId = user.FindFirst("tenantId")?.Value;
        role = user.FindFirst("role")?.Value ?? "User";
    }
}
```

---

## 4️⃣ API Permissions

### Required Microsoft Graph Permissions

| Permission | Type | Purpose |
|------------|------|---------|
| `User.Read` | Delegated | Read signed-in user profile |
| `User.ReadWrite.All` | Application | Write custom attributes to user profiles |
| `Directory.Read.All` | Application | Read directory data |
| `email` | Delegated | Read user email address |
| `openid` | Delegated | OpenID Connect sign-in |
| `profile` | Delegated | Read user profile |

### Configuration Steps

1. **Configure API App Registration**
   - Go to **App registrations** → **SteelAxis API**
   - Click **API permissions** in left menu

2. **Add Microsoft Graph Permissions**
   - Click **+ Add a permission**
   - Select **Microsoft Graph**
   - Select **Delegated permissions**:
     - ✅ User.Read
     - ✅ email
     - ✅ openid
     - ✅ profile
   - Select **Application permissions**:
     - ✅ User.ReadWrite.All
     - ✅ Directory.Read.All

3. **Grant Admin Consent**
   - Click **Grant admin consent for [your tenant]**
   - Confirm in popup dialog
   - Verify all permissions show "Granted" status

4. **Configure Web App Registration**
   - Go to **App registrations** → **SteelAxis Web App**
   - Click **API permissions**
   - Add **Microsoft Graph** delegated permissions:
     - ✅ User.Read
     - ✅ email
     - ✅ openid
     - ✅ profile
   - Add **SteelAxis API** permission:
     - ✅ access_as_user (custom scope)
   - Grant admin consent

5. **Client Secret (Application Credentials)**
   - Navigate to **Certificates & secrets**
   - Click **+ New client secret**
   - Description: `SteelAxis API Secret`
   - Expires: 24 months
   - Click **Add**
   - **IMPORTANT:** Copy the secret value immediately (shown only once)
   - Store in Azure Key Vault as `AzureAdB2C--ClientSecret`

---

## 5️⃣ Email Templates

### Invitation Email Template

**Purpose:** Email sent to users when invited to join a tenant

**Configuration Steps:**

1. **Navigate to Company Branding**
   - Go to **External Identities** → **Company branding**
   - Click **Configure** or edit existing branding

2. **Select Email Templates**
   - Click **Email templates** tab
   - Select **Invitation email**

3. **Customize Template**

**Subject Line:**
```
You've been invited to join {TenantName} on SteelAxis
```

**Email Body (HTML):**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SteelAxis Invitation</title>
</head>
<body style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; padding: 0; margin: 0;">
    <table width="100%" cellpadding="0" cellspacing="0" style="background-color: #f5f5f5; padding: 40px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color: #1976D2; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
                            <h1 style="color: #ffffff; margin: 0; font-size: 28px;">SteelAxis</h1>
                            <p style="color: #ffffff; margin: 5px 0 0 0; font-size: 14px;">Steel Fabrication Management</p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style="padding: 40px;">
                            <h2 style="color: #333333; margin: 0 0 20px 0; font-size: 24px;">You've Been Invited!</h2>
                            
                            <p style="color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;">
                                Hi there,
                            </p>
                            
                            <p style="color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;">
                                <strong style="color: #1976D2;">{InviterName}</strong> from <strong>{TenantName}</strong> 
                                has invited you to join their steel fabrication workspace on SteelAxis.
                            </p>
                            
                            <table width="100%" cellpadding="0" cellspacing="0" style="background-color: #f5f5f5; border-radius: 4px; padding: 20px; margin: 0 0 20px 0;">
                                <tr>
                                    <td>
                                        <p style="margin: 0 0 10px 0; color: #333333; font-size: 14px;">
                                            <strong>Your Role:</strong> <span style="color: #1976D2;">{Role}</span>
                                        </p>
                                        <p style="margin: 0; color: #333333; font-size: 14px;">
                                            <strong>Company:</strong> {TenantName}
                                        </p>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style="color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;">
                                Click the button below to accept the invitation and create your account:
                            </p>
                            
                            <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td align="center">
                                        <a href="{InvitationLink}" style="background-color: #1976D2; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 4px; display: inline-block; font-size: 16px; font-weight: 600;">
                                            Accept Invitation
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style="color: #999999; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0; padding-top: 20px; border-top: 1px solid #eeeeee;">
                                This invitation expires in 7 days.
                            </p>
                            
                            <p style="color: #999999; font-size: 14px; line-height: 1.6; margin: 10px 0 0 0;">
                                If you have any questions, please contact <a href="mailto:{InviterEmail}" style="color: #1976D2;">{InviterEmail}</a>
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style="background-color: #f5f5f5; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;">
                            <p style="color: #999999; font-size: 12px; margin: 0 0 10px 0;">
                                Best regards,<br>
                                The SteelAxis Team
                            </p>
                            <p style="color: #cccccc; font-size: 11px; margin: 0;">
                                © 2025 SteelAxis. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
```

4. **Configure Template Variables**
   
   Ensure these variables are supported:
   - `{TenantName}` - Company name from invitation
   - `{InviterName}` - Name of admin who sent invitation
   - `{InviterEmail}` - Email of inviter
   - `{Role}` - Assigned role (Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker)
   - `{InvitationLink}` - Unique invitation acceptance URL

5. **Test Email Template**
   - Send test invitation to your email
   - Verify formatting, links, and branding
   - Check mobile responsiveness

### Welcome Email Template

**Subject:** `Welcome to SteelAxis - Get Started`

**Body:** Similar HTML structure welcoming new admin users after tenant creation.

---

## 6️⃣ Application Registration Settings

### Web App Registration: `SteelAxis Web App`

**Configuration:**

1. **Authentication Settings**
   - Platform: **Web**
   - Redirect URIs:
     ```
     https://localhost:7071/signin-oidc
     https://steelaxis-dev.azurewebsites.net/signin-oidc
     https://app.steelaxis.com/signin-oidc (production)
     ```
   - Front-channel logout URL:
     ```
     https://localhost:7071/signout-oidc
     https://steelaxis-dev.azurewebsites.net/signout-oidc
     ```
   - Implicit grant: ❌ (not needed for OIDC)
   - ID tokens: ✅

2. **Certificates & Secrets**
   - Client secret: Created (store in Key Vault)
   - Certificate: Optional (for production)

3. **API Permissions**
   - Microsoft Graph:
     - User.Read (Delegated)
     - email (Delegated)
     - openid (Delegated)
     - profile (Delegated)
   - SteelAxis API:
     - access_as_user (Delegated)

4. **Token Configuration**
   - Include custom claims: tenantId, role

### API Registration: `SteelAxis API`

**Configuration:**

1. **Expose an API**
   - Application ID URI: `api://<api-client-id>`
   - Scopes:
     ```
     Scope name: access_as_user
     Who can consent: Admins and users
     Admin consent display name: Access SteelAxis API
     Admin consent description: Allows the app to access SteelAxis API on behalf of the user
     User consent display name: Access SteelAxis on your behalf
     User consent description: Allows the app to access SteelAxis using your account
     State: Enabled
     ```

2. **App Roles**
   Create three app roles for role-based access:
   
   **Admin Role:**
   ```
   Display name: Administrator
   Value: Admin
   Description: Full administrator access
   Allowed member types: Users/Groups
   ```
   
   **User Role:**
   ```
   Display name: Standard User
   Value: User
   Description: Standard user access
   Allowed member types: Users/Groups
   ```
   
   **Viewer Role:**
   ```
   Display name: Viewer
   Value: Viewer
   Description: Read-only access
   Allowed member types: Users/Groups
   ```

3. **API Permissions**
   - Microsoft Graph:
     - User.ReadWrite.All (Application)
     - Directory.Read.All (Application)
   - Admin consent: Granted

4. **Authentication**
   - Access tokens: ✅
   - Allow public client flows: ❌

---

## 7️⃣ Testing Configuration

### Test Checklist

- [ ] Custom attributes visible in user profile
- [ ] Admin signup flow creates user with email verification
- [ ] Token includes tenantId and role claims
- [ ] Invitation email sends with correct template
- [ ] Invited user signup prefills email
- [ ] Custom attributes writable via Microsoft Graph API
- [ ] API permissions granted and functional
- [ ] Web app can acquire tokens with custom scopes

### Test Users

Create test users for each role:

1. **Admin User:**
   - Email: `admin@yourdomain.com`
   - Role: Admin
   - TenantId: (will be set after tenant creation)

2. **Upper Management User:**
   - Email: `uppermgmt@yourdomain.com`
   - Role: UpperManagement
   - TenantId: (same as admin)

3. **Lower Management User:**
   - Email: `lowermgmt@yourdomain.com`
   - Role: LowerManagement
   - TenantId: (same as admin)

4. **Supervisor User:**
   - Email: `supervisor@yourdomain.com`
   - Role: Supervisor
   - TenantId: (same as admin)

5. **Shopfloor Worker User:**
   - Email: `worker@yourdomain.com`
   - Role: ShopFloorWorker
   - TenantId: (same as admin)

6. **Office Worker User:**
   - Email: `office@yourdomain.com`
   - Role: OfficeWorker
   - TenantId: (same as admin)

---

## 8️⃣ Security Best Practices

1. **Client Secrets**
   - Store in Azure Key Vault, never in code
   - Rotate every 6-12 months
   - Use managed identities where possible

2. **Token Validation**
   - Validate issuer, audience, expiration
   - Verify signature with public keys
   - Check custom claims (tenantId, role)

3. **Multi-Factor Authentication**
   - Enable for admin users (required)
   - Recommend for all users
   - Configure in user flows

4. **Conditional Access**
   - Require compliant devices
   - Block legacy authentication
   - Require MFA for privileged roles

5. **Monitoring & Alerts**
   - Enable sign-in logs
   - Monitor failed authentications
   - Alert on suspicious activity

---

## 9️⃣ Troubleshooting

### Common Issues

**Issue:** Custom attributes not appearing in tokens
- **Solution:** Verify token configuration includes extension claims
- **Solution:** Check user flow includes custom attributes in application claims

**Issue:** "AADSTS50105: The signed in user is not assigned to a role"
- **Solution:** Assign user to app role in Enterprise Applications
- **Solution:** Update app manifest to not require role assignment

**Issue:** Invitation email not sending
- **Solution:** Verify email template configuration
- **Solution:** Check SMTP settings in Entra
- **Solution:** Ensure invitation link format is correct

**Issue:** Cannot write custom attributes via Graph API
- **Solution:** Verify Application permission User.ReadWrite.All is granted
- **Solution:** Check extension attribute name format: `extension_<appId without dashes>_attributeName`

---

## 🎯 Summary

This configuration enables:
- ✅ Multi-tenant user authentication
- ✅ Custom user attributes (tenantId, role)
- ✅ Token claims with tenant context
- ✅ User invitation system
- ✅ Role-based access control
- ✅ Branded email templates

**Next Steps:**
1. Complete configuration following this guide
2. Test with demo tenant and users
3. Implement application code to use custom claims
4. Configure production environment

---

**Configuration Status:** 📋 Ready to implement  
**Estimated Time:** 2-3 hours for complete setup
