# Azure Deployment Guide - Multi-Tenant Architecture

**Manimp Production Deployment** - Database-per-tenant SaaS on Azure Web App with automated provisioning, subdomain routing, and zero-secret security.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Naming Conventions](#naming-conventions)
3. [Quick Start Guide](#quick-start-guide)
4. [Tenant Provisioning](#tenant-provisioning)
5. [Domain & SSL Setup](#domain--ssl-setup)
6. [Security Hardening](#security-hardening)
7. [CI/CD with GitHub Actions](#cicd-with-github-actions)
8. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

### What You're Building

```
User Request (tenant1.yourdomain.com)
    ↓
Front Door Premium + WAF (DDoS protection, wildcard SSL)
    ↓
Azure Web App (.NET 8) via Private Link
    ↓ (Managed Identity auth - no passwords)
    ├→ Azure SQL (per-tenant database)
    └→ Key Vault (secrets if needed)
```

### Core Principles

✅ **Zero plaintext secrets** - Managed Identity for everything  
✅ **Private networking** - No public endpoints in production  
✅ **Least privilege** - Separate identities for app vs provisioner  
✅ **Background provisioning** - Queue-based tenant creation  
✅ **Wildcard domains** - No DNS changes per tenant  

---

## Naming Conventions

Use consistent naming across environments (`dev`, `stg`, `prod`):

| Resource Type | Name Pattern | Example |
|--------------|--------------|---------|
| Resource Group | `rg-<app>-<env>` | `rg-manimp-dev` |
| App Service Plan | `asp-<app>-<env>` | `asp-manimp-dev` |
| Web App | `app-<app>-<env>` | `app-manimp-dev` |
| SQL Server | `sql-<app>-<env>` | `sql-manimp-dev` |
| Key Vault | `kv-<app>-<env>` | `kv-manimp-dev` |
| Front Door | `fd-<app>-<env>` | `fd-manimp-dev` |
| VNet | `vnet-<app>-<env>` | `vnet-manimp-dev` |

### Database Naming

- **Directory DB**: `Directory_<Env>` → `ManimpDirectory_Dev`
- **Tenant DBs**: `Tenant_<Env>_<TenantId>` → `ManimpTenant_Dev_acme`

---

## Quick Start Guide

### Step 1: Resource Group & App Service

1. **Create Resource Group**
   - Name: `rg-manimp-dev`
   - Location: Choose your region

2. **Create App Service Plan**
   - Name: `asp-manimp-dev`
   - OS: **Linux**
   - Pricing: **B1** (dev) or **S1+** (prod with slots)

3. **Create Web App**
   - Name: `app-manimp-dev`
   - Runtime: **.NET 8**
   - App Service Plan: Select the one created above
   - Enable Application Insights: ✅
   - Enable System-assigned Managed Identity: ✅

---

### Step 2: Azure SQL Database

1. **Create SQL Server**
   - Name: `sql-manimp-dev.database.windows.net`
   - Authentication: **Microsoft Entra ID only**
   - Set yourself as Entra ID admin

2. **Create Elastic Pool** (optional but recommended)
   - Name: `sqlepool-manimp-dev`
   - Tier: Standard (100-400 eDTUs for many small DBs)

3. **Create Directory Database**
   - Name: `ManimpDirectory_Dev`
   - Assign to elastic pool if created

4. **Configure SQL Permissions** (run as Entra admin in `master`):
   ```sql
   -- Allow provisioner MI to create databases
   CREATE USER [provisioner-mi] FROM EXTERNAL PROVIDER;
   ALTER ROLE dbmanager ADD MEMBER [provisioner-mi];
   ALTER ROLE loginmanager ADD MEMBER [provisioner-mi];
   ```

---

### Step 3: Key Vault Setup

1. **Create Key Vault**
   - Name: `kv-manimp-dev`
   - Access model: **Azure RBAC** (not Access Policies)

2. **Assign Roles**
   - Web App Managed Identity → **Key Vault Secrets User**
   - Provisioner Managed Identity → **Key Vault Secrets Officer** (if writing secrets)
   - Your account → **Key Vault Administrator** (for management)

3. **Store Directory Connection String** (if not using MI auth everywhere):
   - Secret name: `Directory-ConnectionString-Dev`
   - Value:
     ```
     Server=tcp:sql-manimp-dev.database.windows.net,1433;
     Initial Catalog=ManimpDirectory_Dev;
     Encrypt=True;TrustServerCertificate=False;
     Authentication=Active Directory Managed Identity;
     ```

---

### Step 4: Web App Configuration

Go to **App Service** → **Configuration**:

#### Connection Strings

| Name | Value | Type |
|------|-------|------|
| `Directory` | `@Microsoft.KeyVault(SecretUri=https://kv-manimp-dev.vault.azure.net/secrets/Directory-ConnectionString-Dev/)` | SQLAzure |

#### Application Settings

| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Development` |

---

## Tenant Provisioning

### How It Works (Background Worker Pattern)

```
1. User signs up → Queue message (tenantId, subdomain)
2. Background worker (Function/HostedService) picks up message
3. Worker creates database using Provisioner MI
4. Worker grants Web App MI access to new DB
5. Worker runs EF migrations
6. Worker inserts record in Directory table
7. App runtime resolves subdomain → DatabaseName → builds MI connection string
```

### Provisioner Code (Pseudo-C#)

```csharp
// Connect to master using Provisioner MI
var masterCs = new SqlConnectionStringBuilder {
    DataSource = "sql-manimp-dev.database.windows.net",
    InitialCatalog = "master",
    Encrypt = true,
    Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity
}.ToString();

using var conn = new SqlConnection(masterCs);
await conn.OpenAsync();

// Create tenant database
var dbName = $"ManimpTenant_Dev_{tenantId}";
await new SqlCommand(
    $"CREATE DATABASE [{dbName}] (EDITION='Standard')", 
    conn
).ExecuteNonQueryAsync();

// Grant Web App MI access
var grantSql = $@"
    USE [{dbName}];
    CREATE USER [webapp-mi] FROM EXTERNAL PROVIDER;
    ALTER ROLE db_owner ADD MEMBER [webapp-mi];";
await new SqlCommand(grantSql, conn).ExecuteNonQueryAsync();

// Run EF migrations (using Web App MI auth)
await ApplyMigrationsAsync(dbName);

// Insert into Directory
await InsertDirectoryRecordAsync(tenantId, subdomain, dbName);
```

### Runtime Connection String (No Secrets!)

```csharp
// Parse subdomain from Request.Host
var subdomain = GetSubdomainFromHost(Request.Host);

// Look up DatabaseName in Directory
var databaseName = await _directory.GetDatabaseNameAsync(subdomain);

// Build MI connection string (no Key Vault lookup!)
var tenantCs = $@"
    Server=tcp:sql-manimp-dev.database.windows.net,1433;
    Initial Catalog={databaseName};
    Encrypt=True;TrustServerCertificate=False;
    Connection Timeout=30;
    Authentication=Active Directory Managed Identity;";

// Use with EF Core
var options = new DbContextOptionsBuilder<TenantDbContext>()
    .UseSqlServer(tenantCs, o => o.EnableRetryOnFailure())
    .Options;
```

---

## Domain & SSL Setup

### DNS Configuration

You need **two domains**:

1. **Apex domain**: `yourdomain.com` (login/marketing)
2. **Wildcard**: `*.yourdomain.com` (all tenants)

#### Option A: Azure DNS (Recommended)

```
yourdomain.com        → A/AAAA Alias to Front Door endpoint
*.yourdomain.com      → CNAME to <fd>.azurefd.net
```

#### Option B: GoDaddy/External DNS

```
*.yourdomain.com      → CNAME to <fd>.azurefd.net
www.yourdomain.com    → CNAME to <fd>.azurefd.net
(redirect apex → www) → Use GoDaddy forwarding
```

⚠️ **GoDaddy limitation**: Apex records can't CNAME. Use `www` for login or migrate to Azure DNS.

---

### Front Door Premium Setup

1. **Create Front Door Premium**
   - Name: `fd-manimp-dev`
   - Enable WAF: ✅

2. **Add Custom Domains**
   - `yourdomain.com` (apex)
   - `*.yourdomain.com` (wildcard)

3. **SSL Certificates**
   - **Wildcard cert** (`*.yourdomain.com`): 
     - Buy from vendor → Store in Key Vault
     - Front Door → BYOC (Bring Your Own Certificate)
   - **Apex cert** (`yourdomain.com`):
     - Use Front Door managed certificate (free)

4. **Configure Origin**
   - Type: **App Service**
   - Name: `app-manimp-dev.azurewebsites.net`
   - Enable Private Link: ✅ (after VNet setup)

5. **Add Route**
   - Pattern: `/*`
   - Origin group: Your Web App
   - Forwarding protocol: HTTPS only

6. **WAF Policy**
   - Managed rules: **Enable Default Rule Set**
   - Mode: **Prevention** (block threats)

---

## Security Hardening

### Phase 1: Identity & Access

✅ **Web App Managed Identity**: System-assigned, enabled  
✅ **Provisioner Managed Identity**: Separate identity with `dbmanager` role  
✅ **Key Vault RBAC**: No secrets for SQL (MI auth only)  

### Phase 2: Private Networking

1. **Create VNet**
   - Name: `vnet-manimp-dev`
   - Address space: `10.0.0.0/16`
   - Subnets:
     - `integration`: `10.0.1.0/24` (App Service integration)
     - `priv-endpoints`: `10.0.2.0/24` (Private Endpoints)

2. **Enable VNet Integration**
   - App Service → Networking → Outbound traffic → VNet Integration
   - Select `integration` subnet

3. **Create Private Endpoints**
   - **SQL Server**: Link to `priv-endpoints` subnet
   - **Key Vault**: Link to `priv-endpoints` subnet
   - Both: Enable Private DNS Zone integration

4. **Front Door Private Link**
   - Front Door origin → Enable Private Link to Web App
   - Approve connection in Web App → Networking

5. **Disable Public Access**
   - SQL Server: Networking → Public access = **Disabled**
   - Key Vault: Networking → Allow public access = **No**
   - Web App: Networking → Public access = **Disabled** (after Private Link works)

---

### Phase 3: Governance & Monitoring

#### Azure Policy (apply to subscription/RG)

```
✅ Deny public network access on SQL databases
✅ Deny public network access on Key Vaults  
✅ Require HTTPS for App Services
✅ Enforce allowed Azure regions
✅ Require tags (Environment, Owner, CostCenter)
```

#### Microsoft Defender for Cloud

Enable plans:
- ✅ App Service
- ✅ Azure SQL
- ✅ Key Vault

#### SQL Security

- **TDE**: Enabled by default (transparent data encryption)
- **Auditing**: Send to Log Analytics workspace
- **Threat Detection**: Enable Advanced Data Security
- **Backups**: PITR (7-35 days) + LTR (weekly/monthly)

#### Key Vault Security

- ✅ Soft delete: Enabled by default
- ✅ Purge protection: Enable in production
- ✅ RBAC: No access policies (use Azure RBAC only)

---

## CI/CD with GitHub Actions

### Setup (One-Time)

1. **Create Service Principal** (Entra ID)
   ```bash
   az ad sp create-for-rbac --name "github-manimp-deployer" \
       --role Contributor \
       --scopes /subscriptions/{sub-id}/resourceGroups/rg-manimp-dev
   ```

2. **Add Federated Credential**
   - Entra ID → App registrations → Your SP
   - Certificates & secrets → Federated credentials → Add
   - Issuer: `https://token.actions.githubusercontent.com`
   - Subject: `repo:{owner}/{repo}:ref:refs/heads/main`

3. **Add GitHub Secrets**
   - `AZURE_CLIENT_ID`: From SP
   - `AZURE_TENANT_ID`: Your Entra tenant ID
   - `AZURE_SUBSCRIPTION_ID`: Your subscription ID

---

### Workflow File

`.github/workflows/deploy-dev.yml`:

```yaml
name: Deploy to Dev

on:
  push:
    branches: [main]

permissions:
  id-token: write
  contents: read

env:
  AZURE_WEBAPP_NAME: app-manimp-dev
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Azure login (OIDC)
        uses: azure/login@v1
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Restore & Build
        run: dotnet build --configuration Release

      - name: Publish
        run: dotnet publish Manimp.Web/Manimp.Web.csproj -c Release -o publish

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}
          package: publish
```

**No publish profiles. No connection strings in repo. No secrets.**

---

## Troubleshooting

### Tenant Creation Fails

**Problem**: `CREATE DATABASE` denied  
**Solution**: Verify Provisioner MI has `dbmanager` role in SQL `master` database

**Problem**: Can't connect to new tenant DB  
**Solution**: Verify Web App MI was granted `db_owner` in the new database

---

### Key Vault Access Denied

**Problem**: `403 Forbidden` when accessing secrets  
**Solution**: Check RBAC role assignments (Secrets User for Web App MI)

**Problem**: Private endpoint not resolving  
**Solution**: Verify Private DNS zone is linked to VNet

---

### Front Door Issues

**Problem**: Custom domain validation fails  
**Solution**: Check DNS CNAME/TXT records have propagated (use `nslookup`)

**Problem**: SSL certificate not applied  
**Solution**: Wildcard certs require BYOC via Key Vault (not managed certs)

---

### Connection Errors

**Problem**: `Login failed for user '<token-identified principal>'`  
**Solution**: Ensure connection string uses `Authentication=Active Directory Managed Identity`

**Problem**: Cannot connect to SQL from local dev  
**Solution**: Add your IP to SQL firewall temporarily, or use Azure Bastion/VPN

---

## Deployment Checklist

### Development Environment

- [ ] Resource group created
- [ ] App Service Plan (Linux, .NET 8)
- [ ] Web App with System MI enabled
- [ ] SQL Server with Entra ID admin
- [ ] Directory database created
- [ ] Elastic pool created (optional)
- [ ] Key Vault with RBAC roles assigned
- [ ] App Configuration set (ASPNETCORE_ENVIRONMENT)
- [ ] GitHub Actions OIDC configured

### Provisioning Flow

- [ ] Background worker deployed (Function/HostedService)
- [ ] Provisioner MI has `dbmanager` + `loginmanager`
- [ ] Queue trigger configured
- [ ] Worker creates DB → grants access → runs migrations
- [ ] Directory table updated with DatabaseName
- [ ] Runtime builds MI connection strings (no KV lookups)

### Production Security

- [ ] Front Door Premium deployed with WAF
- [ ] Custom domains configured (`yourdomain.com` + `*.yourdomain.com`)
- [ ] Wildcard SSL cert in Key Vault
- [ ] VNet with subnets created
- [ ] Private Endpoints for SQL + Key Vault
- [ ] VNet Integration enabled on Web App
- [ ] Front Door Private Link to Web App
- [ ] Public access disabled on SQL, KV, Web App
- [ ] Azure Policy rules applied
- [ ] Defender for Cloud enabled
- [ ] SQL Auditing + Threat Detection enabled
- [ ] Budgets and alerts configured

---

## Quick Reference

### Runtime Connection String Template

```csharp
$@"Server=tcp:{sqlServer}.database.windows.net,1433;
   Initial Catalog={databaseName};
   Encrypt=True;TrustServerCertificate=False;
   Connection Timeout=30;
   Authentication=Active Directory Managed Identity;"
```

### Grant Web App MI in New Tenant DB

```sql
USE [Tenant_Dev_<tenantId>];
CREATE USER [webapp-mi] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [webapp-mi];
```

### Key Vault Reference (if needed)

```
@Microsoft.KeyVault(SecretUri=https://kv-manimp-dev.vault.azure.net/secrets/Directory-ConnectionString-Dev/)
```

---

## Final Recommendations

1. **Never store tenant connection strings** → Store only `DatabaseName` in Directory
2. **Use Managed Identity everywhere** → SQL, Key Vault, Storage
3. **Wildcard domain + cert** → Zero DNS work per tenant
4. **Background provisioning** → Separate MI with elevated permissions
5. **Private Link everywhere** → No public endpoints in production
6. **Elastic Pool** → Cost control for many small databases
7. **Azure Policy** → Preventive governance (deny public access)
8. **OIDC for CI/CD** → No secrets in GitHub repo

---

**Need help?** Review Azure SQL MI auth docs: https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-overview