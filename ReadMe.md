# SteelAxis

Blazor (MudBlazor) web application with a secured ASP.NET Core API, authenticated by Microsoft Entra External ID for customers (CIAM). The project includes Azure-hosted infrastructure and GitHub Actions CI/CD with OpenID Connect (no publish profiles).

## What’s been accomplished

Identity (CIAM)
- CIAM tenant: steelaxistenants.onmicrosoft.com
- User flow: B2C_1_susi (Sign up and sign in)
- Any email sign-up enabled via Local accounts (email verification)
- App registrations in CIAM:
  - SPA client (SteelAxis.Web): c18d34dc-20da-408c-bfaa-d61760a88957
  - API resource (SteelAxis.Api): 3b6c5177-6c96-46ce-a15a-818f1738dc7d
  - Scope exposed: api://3b6c5177-6c96-46ce-a15a-818f1738dc7d/access_as_user
- Authority (for apps): https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi

Azure resources
- App Service (Web): steelaxis-dev
- App Service (API): steelaxis-dev-api
- Key Vault: kv-Steelaxis-dev (stores CIAM config)
  - AzureAdB2C--Authority = https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi
  - AzureAdB2C--ClientId = c18d34dc-20da-408c-bfaa-d61760a88957
  - AzureAdB2C--ApiClientId = 3b6c5177-6c96-46ce-a15a-818f1738dc7d
  - AzureAdB2C--DefaultScopes = api://3b6c5177-6c96-46ce-a15a-818f1738dc7d/access_as_user

CI/CD (GitHub Actions with Azure OIDC)
- Branch: dev
- OIDC federated credential subject: repo:matisspetersons-sx/SteelAxis:ref:refs/heads/dev
- Repo secrets required:
  - AZURE_CLIENT_ID
  - AZURE_TENANT_ID = c0353f5d-dcc8-4dc6-9d6c-a89377d04251
  - AZURE_SUBSCRIPTION_ID = 31a2fcb1-5cca-4c0e-9782-57054e8232f9
- Workflows added:
  - .github/workflows/deploy-api-dev.yml — builds and deploys SteelAxis.Api* to steelaxis-dev-api
  - .github/workflows/deploy-web-dev.yml — builds and deploys SteelAxis.Web* to steelaxis-dev
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
AzureAdB2C__Authority=https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi
AzureAdB2C__ApiClientId=3b6c5177-6c96-46ce-a15a-818f1738dc7d
```

Build and run
```
dotnet restore
dotnet build
dotnet run --project src/SteelAxis.Api/SteelAxis.Api.csproj
dotnet run --project src/SteelAxis.Web/SteelAxis.Web.csproj
```

Blazor + MudBlazor
- The web project is Blazor (MudBlazor UI). Publish output is deployed to steelaxis-dev.

## Deployment

On push to dev:
- API workflow builds/publishes SteelAxis.Api* and deploys to steelaxis-dev-api
- Web workflow builds/publishes SteelAxis.Web* and deploys to steelaxis-dev

Requirements
- Repo secrets set (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID)
- OIDC federated credential exists for dev and the identity has Website Contributor on both App Services

## Authentication details

Tokens issued by CIAM user flow B2C_1_susi:
- Authority: https://steelaxistenants.ciamlogin.com/steelaxistenants.onmicrosoft.com/B2C_1_susi (use /v2.0 OIDC metadata)
- Audience (API): 3b6c5177-6c96-46ce-a15a-818f1738dc7d
- Scope: api://3b6c5177-6c96-46ce-a15a-818f1738dc7d/access_as_user
- knownAuthorities for clients: ["steelaxistenants.ciamlogin.com"]

## Changelog

- #1: CI/CD with Azure OIDC, quality/security workflows, Dependabot, and documentation updates.
