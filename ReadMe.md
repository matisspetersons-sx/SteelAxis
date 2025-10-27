# SteelAxis

Multi-tenant steel fabrication management system built with Blazor (MudBlazor) web application and a secured ASP.NET Core API, authenticated by Microsoft Entra External ID for customers (CIAM). The project includes Azure-hosted infrastructure and GitHub Actions CI/CD with OpenID Connect (no publish profiles).

## ⚠️ Security Notice

**DO NOT store secrets, passwords, or sensitive credentials in this repository.**

- Use Azure Key Vault or GitHub Secrets for all sensitive configuration values
- Never commit actual tenant IDs, subscription IDs, client IDs, or client secrets
- This README uses placeholders like `<YOUR_TENANT_ID>` - replace them with your actual values only in secure configuration stores
- Review all commits to ensure no sensitive data is accidentally included

## Solution Structure

The solution is structured with 8 projects following a clean architecture pattern:

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
# Run all tests (recommended - runs tests from each test project)
dotnet test

# Or run tests for a specific test project
dotnet test SteelAxis.Tests/SteelAxis.Tests.csproj

# Run with specific configuration
dotnet test -c Release --verbosity normal
```

**Note:** When running tests in CI/CD or when troubleshooting, it's recommended to run tests on individual test project files rather than at the solution level. This avoids potential issues with VSTest argument handling and makes test execution more explicit.

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

## Current Status

**🚧 INITIAL DEVELOPMENT PHASE - NO FEATURES IMPLEMENTED**

### ✅ Completed Setup
- Complete .NET 8.0 solution structure (8 projects following clean architecture)
- Blazor Server web application foundation with MudBlazor 8.12.0
- ASP.NET Core Web API foundation
- Entity Framework Core integration configured
- xUnit test project structure
- Comprehensive development documentation

### ✅ Authentication (CIAM)
- Microsoft Entra External ID tenant configured
- User flow: B2C_1_susi (Sign up and sign in)
- Email sign-up enabled with verification
- App registrations completed:
  - Web client (SteelAxis.Web)
  - API resource (SteelAxis.Api)
  - API scope: `api://{client-id}/access_as_user`

### ✅ Azure Infrastructure
- **App Service (Web)**: `steelaxis-dev` - Blazor web application
- **App Service (API)**: `steelaxis-dev-api` - ASP.NET Core API
- **Key Vault**: `kv-Steelaxis-dev` - Stores authentication secrets
- **SQL Server**: Configured with elastic pool
- **GitHub Actions CI/CD**: Automated deployment pipelines

### ❌ Not Yet Implemented
- **No business features** - EN 1090 compliance, material management, etc.
- **No database models** - Beyond base entity classes
- **No service layer** - Business logic services not created
- **No API controllers** - Except health check endpoint
- **No UI components** - Beyond authentication pages

**SteelAxis is ready for feature development to begin.**

---

## Next Steps

Ready to start implementing features following the established patterns:

1. **Review Development Guidelines**
   - See `.github/copilot-instructions.md` for complete workflow
   - Understand API-first development approach
   - Follow standardized component patterns

2. **Implement EN 1090 Foundation**
   - Material certificate management
   - Material traceability system
   - Document management

3. **Build Quality Management**
   - Non-Conformance Reports (NCR)
   - Corrective Action Requests (CAR)
   - Quality control workflows

See `docs/README.md` for complete documentation and implementation order.

---

## Repository Layout

The solution follows a standard .NET multi-project structure:

```
SteelAxis/
├── .github/
│   ├── workflows/           # CI/CD pipeline definitions
│   ├── copilot-instructions.md      # Primary development guidelines
│   ├── copilot-ui-instructions.md   # UI component standards
│   └── copilot-model-instructions.md # Entity model patterns
├── docs/                    # Complete documentation
│   ├── DOCUMENTATION-TEMPLATE.md    # Feature documentation templates
│   ├── README.md                    # Documentation index
│   ├── authentication/              # Authentication setup guides
│   ├── azure-infrastructure/        # Azure deployment guides
│   ├── en-1090-compliance/          # EN 1090 specifications
│   └── mudblazor/                   # MudBlazor component reference
├── SteelAxis.Api/          # ASP.NET Core Web API (controllers, endpoints)
├── SteelAxis.Auth/         # Authentication models (ApplicationUser)
├── SteelAxis.Data/         # EF Core DbContext (tenant databases)
├── SteelAxis.Directory/    # Directory service (central tenant management)
├── SteelAxis.Services/     # Business logic layer (to be implemented)
├── SteelAxis.Shared/       # Common models and interfaces
├── SteelAxis.Tests/        # xUnit test project
├── SteelAxis.Web/          # Blazor Server UI application
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
  - Scaffolded all projects following clean architecture pattern
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
