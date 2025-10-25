# SteelAxis

Multi-tenant steel fabrication management system built with Blazor (MudBlazor) web application and a secured ASP.NET Core API, authenticated by Microsoft Entra External ID for customers (CIAM). The project includes Azure-hosted infrastructure and GitHub Actions CI/CD with OpenID Connect.

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

### Build the solution
```bash
dotnet restore
dotnet build
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

### Run tests
```bash
dotnet test
```

## What's been accomplished

Identity (CIAM)
- CIAM tenant: `<your-tenant>.onmicrosoft.com`
- User flow: `<YOUR_USER_FLOW>` (e.g., `B2C_1_susi` for Sign up and sign in)
- Any email sign-up enabled via Local accounts (email verification)
- App registrations in CIAM:
  - SPA client (SteelAxis.Web): `<YOUR_SPA_CLIENT_ID>`
  - API resource (SteelAxis.Api): `<YOUR_API_CLIENT_ID>`
  - Scope exposed: `api://<YOUR_API_CLIENT_ID>/access_as_user`
- Authority (for apps): `https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>`

Azure resources
- App Service (Web): `<your-app-service-web>`
- App Service (API): `<your-app-service-api>`
- Key Vault: `<your-key-vault>` (stores CIAM config)
  - AzureAdB2C--Authority = `https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>`
  - AzureAdB2C--ClientId = `<YOUR_SPA_CLIENT_ID>`
  - AzureAdB2C--ApiClientId = `<YOUR_API_CLIENT_ID>`
  - AzureAdB2C--DefaultScopes = `api://<YOUR_API_CLIENT_ID>/access_as_user`

CI/CD (GitHub Actions with Azure OIDC)
- Branch: dev
- OIDC federated credential subject: `repo:<your-github-org>/<your-repo>:ref:refs/heads/dev`
- Repo secrets required:
  - AZURE_CLIENT_ID = `<YOUR_MANAGED_IDENTITY_CLIENT_ID>` (the managed identity/service principal client ID for OIDC)
  - AZURE_TENANT_ID = `<YOUR_TENANT_ID>`
  - AZURE_SUBSCRIPTION_ID = `<YOUR_SUBSCRIPTION_ID>`
- Workflows added:
  - .github/workflows/deploy-api-dev.yml — builds and deploys SteelAxis.Api* to `<your-app-service-api>`
  - .github/workflows/deploy-web-dev.yml — builds and deploys SteelAxis.Web* to `<your-app-service-web>`
  - .github/workflows/ci-dev.yml — dotnet format, build (warnings as errors), tests, Trivy FS scan with SARIF upload
  - .github/workflows/codeql-csharp-dev.yml — CodeQL for C#
  - .github/dependabot.yml — weekly NuGet (root) and npm (/web) updates
- PR reference: #1 Add GitHub Actions CI/CD with Azure OIDC for dev branch

Security and quality
- Trivy filesystem scan (CRITICAL,HIGH) with upload to GitHub code scanning
- CodeQL static analysis for C#
- dotnet format enforced, build warnings treated as errors
- Dependabot weekly updates for NuGet and npm

Copilot for pull requests
- Enable in repository settings:
  - Code review with GitHub Copilot
  - Pull request summaries
  - (Optional) Autofix with GitHub Copilot

## Repository layout

- SteelAxis.* projects are auto-detected by workflows:
  - API: any `*SteelAxis.Api*.csproj`
  - Web: any `*SteelAxis.Web*.csproj`
- Typical structure:
  - src/SteelAxis.Api/SteelAxis.Api.csproj
  - src/SteelAxis.Web/SteelAxis.Web.csproj

## Local development

Environment variables (API)
```
AzureAdB2C__Authority=https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>
AzureAdB2C__ApiClientId=<YOUR_API_CLIENT_ID>
```

Build and run
```
dotnet restore
dotnet build
dotnet run --project src/SteelAxis.Api/SteelAxis.Api.csproj
dotnet run --project src/SteelAxis.Web/SteelAxis.Web.csproj
```

Blazor + MudBlazor
- The web project is Blazor (MudBlazor UI). Publish output is deployed to `<your-app-service-web>`.

## Deployment

On push to dev:
- API workflow builds/publishes SteelAxis.Api* and deploys to `<your-app-service-api>`
- Web workflow builds/publishes SteelAxis.Web* and deploys to `<your-app-service-web>`

Requirements
- Repo secrets set (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID)
- OIDC federated credential exists for dev and the identity has Website Contributor on both App Services

## Authentication details

Tokens issued by CIAM user flow (e.g., `B2C_1_susi`):
- Authority: `https://<your-tenant>.ciamlogin.com/<your-tenant>.onmicrosoft.com/<YOUR_USER_FLOW>` (use /v2.0 OIDC metadata)
- Audience (API): `<YOUR_API_CLIENT_ID>`
- Scope: `api://<YOUR_API_CLIENT_ID>/access_as_user`
- knownAuthorities for clients: `["<your-tenant>.ciamlogin.com"]`

## Changelog

- #1: CI/CD with Azure OIDC, quality/security workflows, Dependabot, and documentation updates.
