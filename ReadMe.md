# SteelAxis

Multi-tenant steel fabrication management system.

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

## Quick Start

### Build the solution
```bash
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

## Technology Stack

- .NET 8.0
- ASP.NET Core
- Blazor Server
- MudBlazor 8.12.0
- Entity Framework Core 8.0
- SQL Server
- xUnit

See the main README.md in the solution root for complete documentation.

Bootstrapping repository. Initial PR will add solution/projects (Api, Services, Shared, Web), feature gating, Azure Key Vault + Azure AD B2C integration, CI/CD (GitHub Actions), branching (working → dev → main → prod), and documentation.
