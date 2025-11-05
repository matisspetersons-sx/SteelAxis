# Entra External ID Setup Guide

This guide explains how to configure Microsoft Entra External ID (formerly Azure AD B2C) authentication for the SteelAxis application.

## Prerequisites

- An Azure subscription
- Access to Microsoft Entra admin center
- .NET 8 SDK installed

## Step 1: Create an Entra External ID Tenant

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Microsoft Entra External ID**
3. Click **Create a tenant**
4. Choose **External** as the tenant type
5. Follow the wizard to create your tenant
6. Note down your **Tenant ID** (also called Directory ID)

## Step 2: Register the Web Application

1. In your Entra External ID tenant, go to **App registrations**
2. Click **New registration**
3. Configure the registration:
   - **Name**: SteelAxis Web
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: 
     - Platform: Web
     - URI: `https://localhost:7071/signin-oidc` (for development)
     - Add production URL later: `https://yourdomain.com/signin-oidc`
4. Click **Register**
5. Note down the **Application (client) ID**

### Configure Web App Settings

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add a description and select expiration
4. Copy the **Value** (this is your Client Secret - save it securely!)

5. Go to **Authentication**
6. Under **Implicit grant and hybrid flows**, enable:
   - ✅ ID tokens (used for implicit and hybrid flows)
7. Under **Logout URL**, add: `https://localhost:7071/signout-callback-oidc`

8. Go to **Token configuration**
9. Click **Add optional claim**
10. Select **ID** and add these claims:
    - email
    - family_name
    - given_name

## Step 3: Register the API Application

1. In **App registrations**, click **New registration**
2. Configure the registration:
   - **Name**: SteelAxis API
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Leave blank for API
3. Click **Register**
4. Note down the **Application (client) ID**

### Configure API Settings

1. Go to **Expose an API**
2. Click **Add a scope**
3. Accept the default Application ID URI or customize it (e.g., `api://your-api-client-id`)
4. Add a scope:
   - **Scope name**: access_as_user
   - **Who can consent**: Admins and users
   - **Admin consent display name**: Access SteelAxis API
   - **Admin consent description**: Allows the app to access SteelAxis API on behalf of the signed-in user
   - **User consent display name**: Access SteelAxis API
   - **User consent description**: Allows the app to access SteelAxis API on your behalf
5. Click **Add scope**

### Authorize the Web App to Call the API

1. Still in the API app registration, go to **Expose an API**
2. Under **Authorized client applications**, click **Add a client application**
3. Enter the **Web Application Client ID** from Step 2
4. Select the `access_as_user` scope
5. Click **Add application**

## Step 4: Configure the Applications

### Update Web Application Settings

Edit `SteelAxis.Web/appsettings.json`:

```json
{
  "AzureAdExternalId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_WEB_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

For production, use **User Secrets** or **Azure Key Vault**:

```bash
cd SteelAxis.Web
dotnet user-secrets set "AzureAdExternalId:TenantId" "your-tenant-id"
dotnet user-secrets set "AzureAdExternalId:ClientId" "your-web-client-id"
dotnet user-secrets set "AzureAdExternalId:ClientSecret" "your-client-secret"
```

### Update API Application Settings

Edit `SteelAxis.Api/appsettings.json`:

```json
{
  "AzureAdExternalId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_API_CLIENT_ID",
    "Audience": "api://YOUR_API_CLIENT_ID"
  }
}
```

For production:

```bash
cd SteelAxis.Api
dotnet user-secrets set "AzureAdExternalId:TenantId" "your-tenant-id"
dotnet user-secrets set "AzureAdExternalId:ClientId" "your-api-client-id"
```

## Step 5: Test the Authentication

### Test Web Application

1. Run the web application:
   ```bash
   cd SteelAxis.Web
   dotnet run
   ```

2. Navigate to `https://localhost:7071`
3. Try accessing a protected page - you should be redirected to the Entra External ID login page
4. Sign in with a user account from your Entra External ID tenant
5. After successful authentication, you should be redirected back to your application

### Test API

1. Run the API:
   ```bash
   cd SteelAxis.Api
   dotnet run
   ```

2. Try to access a protected endpoint without a token - should return 401 Unauthorized
3. Obtain a token using the web application and call the API with it

## Step 6: Create External User Flows (Optional)

For external users (customers, partners), you can create sign-up and sign-in flows:

1. In Entra External ID, go to **User flows**
2. Click **New user flow**
3. Select **Sign up and sign in**
4. Configure:
   - User attributes to collect during sign-up
   - Application claims to return in tokens
   - Branding and customization
5. Associate the user flow with your web application

## Security Best Practices

1. **Never commit secrets to source control**
   - Use User Secrets for local development
   - Use Azure Key Vault for production
   - Add `appsettings.*.json` files with secrets to `.gitignore`

2. **Use HTTPS everywhere**
   - Enforce HTTPS in production
   - Configure HSTS headers

3. **Rotate client secrets regularly**
   - Set expiration dates on secrets
   - Have a rotation process in place

4. **Implement proper authorization**
   - Use role-based or claims-based authorization
   - Don't rely on authentication alone

5. **Monitor and audit**
   - Enable sign-in logs in Entra External ID
   - Monitor failed authentication attempts
   - Set up alerts for suspicious activity

## Troubleshooting

### Common Issues

1. **Redirect URI mismatch**
   - Ensure the redirect URI in your app registration exactly matches your callback URL
   - Check for trailing slashes, http vs https, port numbers

2. **Invalid client secret**
   - Verify the client secret is correct and hasn't expired
   - Generate a new secret if needed

3. **Token validation failed**
   - Check that Tenant ID and Client ID are correct
   - Ensure the Audience in the API matches the Application ID URI

4. **Users can't sign in**
   - Verify users exist in your Entra External ID tenant
   - Check if user flow is properly configured (if using external users)

### Enable Detailed Logging

Add to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Identity.Web": "Debug"
    }
  }
}
```

## Additional Resources

- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/entra/external-id/)
- [Microsoft.Identity.Web Documentation](https://github.com/AzureAD/microsoft-identity-web/wiki)
- [ASP.NET Core Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/)

## Next Steps

1. Configure user roles and claims
2. Implement authorization policies
3. Set up multi-factor authentication
4. Configure custom branding
5. Implement token caching for better performance
