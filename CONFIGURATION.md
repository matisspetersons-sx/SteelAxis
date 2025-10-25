# Configuration Management & Secrets

This document explains how secrets and configuration are managed in the SteelAxis application.

## 🔒 Security Model

### Production (Azure App Services)
All sensitive configuration is stored in **Azure Key Vault** (`kv-Steelaxis-dev`) and automatically injected into the application at runtime.

**Key Vault Secrets:**
- `AzureAdB2C--Authority` - CIAM authority URL
- `AzureAdB2C--ClientId` - Web application client ID
- `AzureAdB2C--ApiClientId` - API resource client ID
- `AzureAdB2C--DefaultScopes` - API scopes for token acquisition

### Local Development
Actual configuration values are stored in `appsettings.Development.json` files which are **excluded from git**.

## 📁 Configuration Files

### `appsettings.json` (Committed to Git)
Contains placeholder values using token replacement syntax:
```json
{
  "AzureAdB2C": {
    "Authority": "#{AzureAdB2C--Authority}#",
    "ClientId": "#{AzureAdB2C--ClientId}#"
  }
}
```

**Purpose:**
- Checked into source control
- Used as template for CI/CD token replacement
- Azure pipelines replace `#{...}#` tokens with actual Key Vault values during deployment

### `appsettings.Development.json` (NOT Committed)
Contains actual development configuration values.

**⚠️ Important:**
- Contains real CIAM credentials
- **NEVER** commit this file to git
- Already added to `.gitignore`
- Each developer must create their own local copy

## 🛠️ Setting Up Local Development

### Option 1: Create appsettings.Development.json (Recommended)

**For Web Application:**
Create `SteelAxis.Web/appsettings.Development.json`:
```json
{
  "AzureAdB2C": {
    "Authority": "https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi",
    "ClientId": "c18d34dc-20da-408c-bfaa-d61760a88957",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "DefaultScopes": "api://3b6c5177-6c96-46ce-a15a-818f1738dc7d/access_as_user"
  }
}
```

**For API:**
Create `SteelAxis.Api/appsettings.Development.json`:
```json
{
  "AzureAdB2C": {
    "Authority": "https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi",
    "ApiClientId": "3b6c5177-6c96-46ce-a15a-818f1738dc7d"
  }
}
```

### Option 2: Use User Secrets (Alternative)

```bash
# Navigate to project directory
cd SteelAxis.Web

# Set secrets
dotnet user-secrets set "AzureAdB2C:Authority" "https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi"
dotnet user-secrets set "AzureAdB2C:ClientId" "c18d34dc-20da-408c-bfaa-d61760a88957"
dotnet user-secrets set "AzureAdB2C:DefaultScopes" "api://3b6c5177-6c96-46ce-a15a-818f1738dc7d/access_as_user"

# For API
cd ../SteelAxis.Api
dotnet user-secrets set "AzureAdB2C:Authority" "https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi"
dotnet user-secrets set "AzureAdB2C:ApiClientId" "3b6c5177-6c96-46ce-a15a-818f1738dc7d"
```

### Option 3: Environment Variables

Set environment variables (useful for CI/CD or containers):
```bash
export AzureAdB2C__Authority="https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi"
export AzureAdB2C__ClientId="c18d34dc-20da-408c-bfaa-d61760a88957"
export AzureAdB2C__ApiClientId="3b6c5177-6c96-46ce-a15a-818f1738dc7d"
export AzureAdB2C__DefaultScopes="api://3b6c5177-6c96-46ce-a15a-818f1738dc7d/access_as_user"
```

## 🚀 CI/CD Configuration

### GitHub Actions
The deployment workflows use Azure Key Vault references in App Service configuration:

```yaml
- name: Deploy to Azure Web App
  uses: azure/webapps-deploy@v2
  with:
    app-name: 'steelaxis-dev'
    # App Service automatically injects Key Vault secrets
```

### Azure App Service Configuration
App Service Application Settings reference Key Vault:
```
AzureAdB2C__Authority = @Microsoft.KeyVault(SecretUri=https://kv-steelaxis-dev.vault.azure.net/secrets/AzureAdB2C--Authority/)
AzureAdB2C__ClientId = @Microsoft.KeyVault(SecretUri=https://kv-steelaxis-dev.vault.azure.net/secrets/AzureAdB2C--ClientId/)
```

## 🔐 GitHub Repository Secrets

The following secrets are configured in GitHub repository settings for CI/CD:

- `AZURE_CLIENT_ID` - Service principal client ID for OIDC authentication
- `AZURE_TENANT_ID` - Azure tenant ID
- `AZURE_SUBSCRIPTION_ID` - Azure subscription ID

**To view/edit:**
https://github.com/matisspetersons-sx/SteelAxis/settings/secrets/actions

## 📋 Configuration Precedence

ASP.NET Core loads configuration in this order (later sources override earlier ones):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments
6. Azure Key Vault (Production via App Service)

## ✅ Best Practices

### ✓ DO:
- Store secrets in Azure Key Vault for production
- Use `appsettings.Development.json` for local development (not committed)
- Use User Secrets for sensitive local config
- Use environment variables in containers/CI
- Keep `appsettings.json` with placeholder tokens in git
- Document all required configuration keys

### ✗ DON'T:
- Commit actual secrets to git
- Share `appsettings.Development.json` files
- Hardcode credentials in code
- Store production secrets in local files
- Commit User Secrets files

## 🔍 Verifying Configuration

### Check what configuration is loaded:
```csharp
// In Startup.cs or Program.cs
var authority = builder.Configuration["AzureAdB2C:Authority"];
builder.Logging.AddConsole();
Console.WriteLine($"Using authority: {authority}");
```

### Check current secrets (safe for local dev):
```bash
# Show configuration sources
dotnet run --project SteelAxis.Web -- --help

# User secrets location
dotnet user-secrets list --project SteelAxis.Web
```

## 🆘 Troubleshooting

### "Configuration value is null" error:
1. Check `appsettings.Development.json` exists and has correct values
2. Verify environment is set: `ASPNETCORE_ENVIRONMENT=Development`
3. Check User Secrets: `dotnet user-secrets list`
4. Verify Key Vault access (production only)

### "The configuration key was not found":
The key name must match exactly:
- JSON: `"AzureAdB2C": { "ClientId": "..." }`
- Environment: `AzureAdB2C__ClientId` (double underscore)
- Code: `Configuration["AzureAdB2C:ClientId"]` (colon)

## 📞 Getting Credentials

### Development Team Members:
1. Contact DevOps team for access to Azure Key Vault
2. Copy values from Key Vault to local `appsettings.Development.json`
3. Never share these values in chat/email

### Production Deployment:
All credentials are automatically injected from Key Vault - no manual configuration needed.

---

**Remember: Secrets in git = security incident. Always use Key Vault or User Secrets!**
