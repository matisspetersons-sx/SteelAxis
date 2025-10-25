# SteelAxis

Multi-tenant steel fabrication management system built with Blazor (MudBlazor) web application and a secured ASP.NET Core API, authenticated by Microsoft Entra External ID for customers (CIAM). The project includes Azure-hosted infrastructure and GitHub Actions CI/CD with OpenID Connect (no publish profiles).

## ⚠️ Security Notice

**DO NOT store secrets, passwords, or sensitive credentials in this repository.**

- Use Azure Key Vault or GitHub Secrets for all sensitive configuration values
- Never commit actual tenant IDs, subscription IDs, client IDs, or client secrets
- This README uses placeholders like `<YOUR_TENANT_ID>` - replace them with your actual values only in secure configuration stores
- Review all commits to ensure no sensitive data is accidentally included

## Solution Structure

The solution has been scaffolded with 8 projects following the Manimp architecture pattern:

```
SteelAxis/
├── SteelAxis.Shared/          # Common models and interfaces
├── SteelAxis.Auth/            # Authentication models
├── SteelAxis.Directory/       # Central directory service
├── SteelAxis.Data/            # Tenant database contexts
├── SteelAxis.Services/        # Business logic services
├── SteelAxis.Api/             # Web API endpoints
├── SteelAxis.Web/             # Blazor Server web application
└── SteelAxis.Tests/           # Unit and integration tests
```

## Technology Stack

- .NET 8.0
- ASP.NET Core
- Blazor Server
- MudBlazor 8.12.0
- Entity Framework Core 8.0
- SQL Server
- xUnit
- Microsoft Identity Web
- Azure Key Vault

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (for local development)
- Azure subscription (for deployment)

### Build the solution
```bash
dotnet restore
dotnet build
```

### Run tests
```bash
dotnet test
```

### Run the Web application
```bash
cd SteelAxis.Web
dotnet run
```

### Run the API
```bash
cd SteelAxis.Api
dotnet run
```

The API will be available at `https://localhost:7001` and the Web application at `https://localhost:7002` (ports may vary based on your configuration).

## What's been accomplished

### Project Structure
- Complete .NET 8.0 solution with 8 projects following the Manimp architecture pattern
- Blazor Server web application with MudBlazor UI components
- Secured ASP.NET Core Web API with health and data endpoints
- Entity Framework Core integration with SQL Server
- xUnit test project with InMemory database support
- Comprehensive documentation (48,000+ lines in /docs)

### Identity (CIAM)
- CIAM tenant: `<your-tenant>.onmicrosoft.com`
- User flow: `<YOUR_USER_FLOW>` (e.g., `B2C_1_susi` for Sign up and sign in)
- Any email sign-up enabled via Local accounts (email verification)
- App registrations in CIAM:
  - SPA client (SteelAxis.Web): `<YOUR_SPA_CLIENT_ID>`
  - API resource (SteelAxis.Api): `<YOUR_API_CLIENT_ID>`
  - Scope exposed: `api://<YOUR_API_CLIENT_ID>/access_as_user`
- Authority (for apps): `https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>`

### Azure resources
- **App Service (Web)**: `<your-app-service-web>` - Hosts the Blazor web application
- **App Service (API)**: `<your-app-service-api>` - Hosts the ASP.NET Core API
- **Key Vault**: `<your-key-vault>` - Stores CIAM configuration and secrets
  - `AzureAdB2C--Authority` = `https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>`
  - `AzureAdB2C--ClientId` = `<YOUR_SPA_CLIENT_ID>`
  - `AzureAdB2C--ApiClientId` = `<YOUR_API_CLIENT_ID>`
  - `AzureAdB2C--DefaultScopes` = `api://<YOUR_API_CLIENT_ID>/access_as_user`

### CI/CD (GitHub Actions with Azure OIDC)
- **Branch**: dev (production deployments from main)
- **OIDC federated credential subject**: `repo:<your-github-org>/<your-repo>:ref:refs/heads/dev`
- **Repo secrets required**:
  - `AZURE_CLIENT_ID` = `<YOUR_MANAGED_IDENTITY_CLIENT_ID>` (the managed identity/service principal client ID for OIDC)
  - `AZURE_TENANT_ID` = `<YOUR_TENANT_ID>`
  - `AZURE_SUBSCRIPTION_ID` = `<YOUR_SUBSCRIPTION_ID>`
- **Workflows added**:
  - `.github/workflows/deploy-api-dev.yml` — Builds and deploys SteelAxis.Api* to `<your-app-service-api>`
  - `.github/workflows/deploy-web-dev.yml` — Builds and deploys SteelAxis.Web* to `<your-app-service-web>`
  - `.github/workflows/ci-dev.yml` — dotnet format, build (warnings as errors), tests, Trivy FS scan with SARIF upload
  - `.github/workflows/codeql-csharp-dev.yml` — CodeQL for C#
  - `.github/dependabot.yml` — Weekly NuGet (root) and npm (/web) updates
- **PR reference**: #1 Add GitHub Actions CI/CD with Azure OIDC for dev branch

### Security and quality
- Trivy filesystem scan (CRITICAL, HIGH) with upload to GitHub code scanning
- CodeQL static analysis for C#
- dotnet format enforced, build warnings treated as errors
- Dependabot weekly updates for NuGet and npm
- Azure Key Vault integration for secrets management
- Microsoft Entra External ID (CIAM) authentication

### Copilot for pull requests
- Enable in repository settings:
  - Code review with GitHub Copilot
  - Pull request summaries
  - (Optional) Autofix with GitHub Copilot

## Repository layout

The solution follows a standard .NET multi-project structure:

```
SteelAxis/
├── .github/
│   └── workflows/           # CI/CD pipeline definitions
├── docs/                    # Comprehensive documentation
│   ├── authentication/      # CIAM and Entra setup guides
│   ├── azure-infrastructure/ # Azure deployment guides
│   ├── customer-portal/     # Customer portal documentation
│   └── en-1090-compliance/  # EN-1090 compliance specs
├── SteelAxis.Api/          # ASP.NET Core Web API
├── SteelAxis.Auth/         # Authentication models
├── SteelAxis.Data/         # EF Core DbContext
├── SteelAxis.Directory/    # Directory service
├── SteelAxis.Services/     # Business logic layer
├── SteelAxis.Shared/       # Common models and interfaces
├── SteelAxis.Tests/        # xUnit test project
├── SteelAxis.Web/          # Blazor Server UI
└── SteelAxis.sln           # Solution file
```

**Note**: SteelAxis.* projects are auto-detected by workflows:
- API: any `*SteelAxis.Api*.csproj`
- Web: any `*SteelAxis.Web*.csproj`

## Local development

### Environment variables

**For API (SteelAxis.Api)**:
```bash
AzureAdB2C__Authority=https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>
AzureAdB2C__ApiClientId=<YOUR_API_CLIENT_ID>
```

**For Web (SteelAxis.Web)**:
```bash
AzureAdB2C__Authority=https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>
AzureAdB2C__ClientId=<YOUR_SPA_CLIENT_ID>
AzureAdB2C__DefaultScopes=api://<YOUR_API_CLIENT_ID>/access_as_user
```

### Build and run locally

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the API
dotnet run --project SteelAxis.Api/SteelAxis.Api.csproj

# In a separate terminal, run the Web app
dotnet run --project SteelAxis.Web/SteelAxis.Web.csproj
```

### Development notes
- The web project uses Blazor Server with MudBlazor UI components
- API uses JWT Bearer authentication with Microsoft Identity Web
- Both projects support Azure Key Vault for configuration in deployed environments

## Deployment

### Automated deployment

**On push to `dev` branch**:
- API workflow builds and deploys `SteelAxis.Api*` to `<your-app-service-api>`
- Web workflow builds and deploys `SteelAxis.Web*` to `<your-app-service-web>`

**Prerequisites**:
- Repository secrets configured:
  - `AZURE_CLIENT_ID`
  - `AZURE_TENANT_ID`
  - `AZURE_SUBSCRIPTION_ID`
- OIDC federated credential exists for the dev branch
- Managed identity has `Website Contributor` role on both App Services
- Azure Key Vault configured with CIAM settings

### Manual deployment

```bash
# Publish API
dotnet publish SteelAxis.Api/SteelAxis.Api.csproj -c Release -o ./publish/api

# Publish Web
dotnet publish SteelAxis.Web/SteelAxis.Web.csproj -c Release -o ./publish/web

# Deploy using Azure CLI
az webapp deploy --resource-group <rg> --name <api-app> --src-path ./publish/api
az webapp deploy --resource-group <rg> --name <web-app> --src-path ./publish/web
```

## Authentication details

### Token configuration

Tokens issued by CIAM user flow (e.g., `B2C_1_susi`):
- **Authority**: `https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>` 
  - Uses /v2.0 OIDC metadata endpoint
- **Audience (API)**: `<YOUR_API_CLIENT_ID>`
- **Scope**: `api://<YOUR_API_CLIENT_ID>/access_as_user`
- **Known authorities for clients**: `["<your-tenant>.ciamlogin.com"]`

### Application registrations

1. **SteelAxis.Web (SPA Client)**
   - Client ID: `<YOUR_SPA_CLIENT_ID>`
   - Redirect URIs: Configure for your deployment URLs
   - API permissions: `access_as_user` scope on SteelAxis.Api

2. **SteelAxis.Api (API Resource)**
   - Client ID: `<YOUR_API_CLIENT_ID>`
   - Exposed scopes: `access_as_user`
   - Accepted token version: 2.0

For detailed setup instructions, see [AUTHENTICATION_QUICKSTART.md](AUTHENTICATION_QUICKSTART.md) and [ENTRA_EXTERNAL_ID_SETUP.md](ENTRA_EXTERNAL_ID_SETUP.md).

## Additional resources

- **[AUTHENTICATION_QUICKSTART.md](AUTHENTICATION_QUICKSTART.md)** - Quick guide to authentication setup
- **[CONFIGURATION.md](CONFIGURATION.md)** - Configuration reference
- **[ENTRA_EXTERNAL_ID_SETUP.md](ENTRA_EXTERNAL_ID_SETUP.md)** - Entra External ID configuration
- **[SECURITY_ADVISORY.md](SECURITY_ADVISORY.md)** - Security best practices
- **[docs/](docs/)** - Comprehensive documentation including EN-1090 compliance, customer portal, and Azure infrastructure guides

## Changelog

### Latest updates
- **#4**: Merged working branch into dev - Added complete .NET 8.0 solution structure with 8 projects
  - Scaffolded all projects following Manimp architecture pattern
  - Added comprehensive documentation (48K+ lines)
  - Integrated Blazor Server with MudBlazor UI
  - Set up xUnit test framework
  - Configured .gitignore for proper repository hygiene

- **#1**: Initial CI/CD setup with Azure OIDC
  - Added GitHub Actions workflows for deployment
  - Implemented quality and security scanning (CodeQL, Trivy)
  - Configured Dependabot for dependency updates
  - Set up OIDC authentication for secure Azure deployments

## Contributing

This is a private repository for steel fabrication management. For questions or contributions, please contact the repository maintainers.

## License

Proprietary - All rights reserved.
