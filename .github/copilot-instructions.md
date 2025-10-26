# GitHub Copilot Instructions for SteelAxis

**Project:** SteelAxis - Multi-tenant Steel Fabrication Management System  
**Last Updated:** October 26, 2025  
**Technology Stack:** .NET 8.0, Blazor Server, MudBlazor 8.12.0, ASP.NET Core Web API, Microsoft Entra External ID (CIAM), Entity Framework Core 8.0, Azure Infrastructure

---

## 🎯 Project Overview

SteelAxis is a **multi-tenant steel fabrication management system** built with Blazor Server (MudBlazor) for the web application and a secured ASP.NET Core API. The system is authenticated by **Microsoft Entra External ID** for customers (CIAM) and deployed on **Azure** with GitHub Actions CI/CD using OpenID Connect.

### Core Business Domain
- **Steel Fabrication Management** for multiple tenants (fabrication companies)
- **EN 1090 Compliance** - European standard for steel structure execution (material traceability, quality control, documentation)
- **Multi-tenant Architecture** - Database per tenant pattern with central directory
- **Customer Portal** - Secure document sharing with external customers
- **Material & Inventory Management** - Tracking steel profiles, sheets, and remnants
- **Project Management** - Task dependencies, critical path analysis, resource planning
- **Quality Control** - NCR (Non-Conformance Reports), welding procedures, NDT requirements

---

## 🔄 Development Workflow Requirements

### Planning & Approval Process
**CRITICAL:** Before implementing any feature, fix, or significant change:

1. **Create a Detailed Plan**
   - Break down the work into clear, actionable steps
   - Identify all files that will be created or modified
   - **Plan API endpoints** - Every feature MUST include corresponding API endpoints
   - **Plan data models** - Define DTOs, request/response objects
   - **Plan UI components** - Use standardized, reusable component patterns
   - List dependencies and potential risks
   - Estimate time/complexity
   - Present the plan to the user in clear, structured format

2. **Get User Confirmation**
   - Wait for explicit user approval before proceeding
   - Do NOT start implementation without confirmation
   - If user requests changes to the plan, revise and confirm again

3. **Implement the Approved Plan**
   - Follow the approved plan systematically
   - Keep user informed of progress for multi-step work
   - Handle errors gracefully and report issues immediately

### Documentation Requirements
**CRITICAL:** After completing any work:

1. **Create Feature Documentation Folder**
   - Every feature MUST have a dedicated folder in `/docs/[feature-name]/`
   - Structure:
     ```
     docs/
     └── [feature-name]/
         ├── README.md           # Main feature documentation
         ├── PLAN.md             # Implementation plan (created first)
         ├── API-SPEC.md         # API endpoints and contracts
         ├── DATABASE.md         # Database schema and migrations
         └── IMPLEMENTATION.md   # Implementation details and decisions
     ```
   - **PLAN.md** - Created BEFORE implementation:
     - Feature description and business value
     - User stories and acceptance criteria
     - Technical approach and architecture
     - API endpoints to be created
     - Database schema changes
     - UI components and pages
     - Dependencies and risks
     - Implementation steps
   - **README.md** - Updated AFTER implementation:
     - What: Feature description and capabilities
     - Why: Business value and use cases
     - How: Usage instructions and examples
     - API documentation links
     - Screenshots or diagrams
     - Configuration requirements
     - Known limitations

2. **Update Project README Files**
   - Update project root README.md with new features/changes
   - Link to feature documentation folder
   - Document any new configuration requirements
   - Update setup instructions if needed
   - Note any breaking changes or migration requirements

3. **Commit Documentation**
   - Commit PLAN.md before starting implementation
   - Commit README and documentation updates separately after completion
   - Use clear commit messages: "docs: add [feature-name] implementation plan" or "docs: complete [feature-name] documentation"
   - Ensure all documentation is current before considering work complete

**Example Workflow:**
```
1. User requests: "Add material certificate upload feature"
2. Agent creates plan, presents to user
3. User approves plan
4. Agent implements feature
5. Agent updates README.md and docs/en-1090-compliance/
6. Agent commits documentation updates
7. Work is complete
```

---

## 🌐 API Development Requirements

### API-First Approach
**CRITICAL:** Every feature MUST include API implementation

**Why API-First:**
- Future mobile app support
- Third-party integrations
- Microservices readiness
- Clear separation of concerns
- Testability

### API Development Pattern

When creating ANY feature, always implement in this order:

**1. Data Models (SteelAxis.Shared)**
```csharp
// Entity model
public class MaterialCertificate : ImmutableBaseEntity { ... }

// DTOs
public record MaterialCertificateDto { ... }
public record CreateMaterialCertificateRequest { ... }
public record UpdateMaterialCertificateRequest { ... }
```

**2. Service Interface & Implementation (SteelAxis.Services)**
```csharp
public interface IMaterialCertificateService
{
    Task<List<MaterialCertificateDto>> GetAllAsync(Guid tenantId);
    Task<MaterialCertificateDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<MaterialCertificateDto> CreateAsync(Guid tenantId, CreateMaterialCertificateRequest request);
    Task UpdateAsync(Guid tenantId, Guid id, UpdateMaterialCertificateRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
```

**3. API Controller (SteelAxis.Api)**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialCertificatesController : ControllerBase
{
    private readonly IMaterialCertificateService _service;
    private readonly ITenantService _tenantService;
    
    [HttpGet]
    [ProducesResponseType(typeof(List<MaterialCertificateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaterialCertificateDto>>> GetAll()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var items = await _service.GetAllAsync(tenantId);
        return Ok(items);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(MaterialCertificateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MaterialCertificateDto>> Create([FromBody] CreateMaterialCertificateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var tenantId = _tenantService.GetCurrentTenantId();
        var result = await _service.CreateAsync(tenantId, request);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    // GET by ID, PUT, DELETE endpoints...
}
```

**4. HTTP Service (SteelAxis.Web/Services)**
```csharp
public interface IMaterialCertificateHttpService
{
    Task<List<MaterialCertificateDto>> GetAllAsync();
    Task<MaterialCertificateDto?> GetByIdAsync(Guid id);
    Task<MaterialCertificateDto> CreateAsync(CreateMaterialCertificateRequest request);
    Task UpdateAsync(Guid id, UpdateMaterialCertificateRequest request);
    Task DeleteAsync(Guid id);
}

public class MaterialCertificateHttpService : IMaterialCertificateHttpService
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<MaterialCertificateDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/materialcertificates");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MaterialCertificateDto>>() ?? new();
    }
    
    // Other methods...
}
```

**5. UI Components (SteelAxis.Web/Components)**
- Use standardized dialog/form patterns (see UI Instructions)
- Call HTTP service, not database directly

### API Endpoint Naming Convention

```
GET    /api/materialcertificates           - Get all
GET    /api/materialcertificates/{id}      - Get by ID
POST   /api/materialcertificates           - Create
PUT    /api/materialcertificates/{id}      - Update
DELETE /api/materialcertificates/{id}      - Delete
GET    /api/materialcertificates/search    - Search with filters
```

### Standard API Response Pattern

```csharp
// Success responses
return Ok(data);                          // 200 OK
return CreatedAtAction(...);              // 201 Created
return NoContent();                       // 204 No Content

// Error responses
return BadRequest(ModelState);            // 400 Bad Request
return NotFound();                        // 404 Not Found
return Unauthorized();                    // 401 Unauthorized
return Forbid();                          // 403 Forbidden
return StatusCode(500, "Error message"); // 500 Internal Server Error
```

**Never skip API implementation** - Even if only building UI initially, create the full API stack.

---

## 🏗️ Solution Architecture
```

---

## � API Development Requirements

### API-First Approach
**CRITICAL:** Every feature MUST include API implementation

**Why API-First:**
- Future mobile app support
- Third-party integrations
- Microservices readiness
- Clear separation of concerns
- Testability

### API Development Pattern

When creating ANY feature, always implement in this order:

**1. Data Models (SteelAxis.Shared)**
```csharp
// Entity model
public class MaterialCertificate : ImmutableBaseEntity { ... }

// DTOs
public record MaterialCertificateDto { ... }
public record CreateMaterialCertificateRequest { ... }
public record UpdateMaterialCertificateRequest { ... }
```

**2. Service Interface & Implementation (SteelAxis.Services)**
```csharp
public interface IMaterialCertificateService
{
    Task<List<MaterialCertificateDto>> GetAllAsync(Guid tenantId);
    Task<MaterialCertificateDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<MaterialCertificateDto> CreateAsync(Guid tenantId, CreateMaterialCertificateRequest request);
    Task UpdateAsync(Guid tenantId, Guid id, UpdateMaterialCertificateRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
```

**3. API Controller (SteelAxis.Api)**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialCertificatesController : ControllerBase
{
    private readonly IMaterialCertificateService _service;
    private readonly ITenantService _tenantService;
    
    [HttpGet]
    [ProducesResponseType(typeof(List<MaterialCertificateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaterialCertificateDto>>> GetAll()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var items = await _service.GetAllAsync(tenantId);
        return Ok(items);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(MaterialCertificateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MaterialCertificateDto>> Create([FromBody] CreateMaterialCertificateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var tenantId = _tenantService.GetCurrentTenantId();
        var result = await _service.CreateAsync(tenantId, request);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    // GET by ID, PUT, DELETE endpoints...
}
```

**4. HTTP Service (SteelAxis.Web/Services)**
```csharp
public interface IMaterialCertificateHttpService
{
    Task<List<MaterialCertificateDto>> GetAllAsync();
    Task<MaterialCertificateDto?> GetByIdAsync(Guid id);
    Task<MaterialCertificateDto> CreateAsync(CreateMaterialCertificateRequest request);
    Task UpdateAsync(Guid id, UpdateMaterialCertificateRequest request);
    Task DeleteAsync(Guid id);
}

public class MaterialCertificateHttpService : IMaterialCertificateHttpService
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<MaterialCertificateDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/materialcertificates");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MaterialCertificateDto>>() ?? new();
    }
    
    // Other methods...
}
```

**5. UI Components (SteelAxis.Web/Components)**
- Use standardized dialog/form patterns (see UI Instructions)
- Call HTTP service, not database directly

### API Endpoint Naming Convention

```
GET    /api/materialcertificates           - Get all
GET    /api/materialcertificates/{id}      - Get by ID
POST   /api/materialcertificates           - Create
PUT    /api/materialcertificates/{id}      - Update
DELETE /api/materialcertificates/{id}      - Delete
GET    /api/materialcertificates/search    - Search with filters
```

### Standard API Response Pattern

```csharp
// Success responses
return Ok(data);                          // 200 OK
return CreatedAtAction(...);              // 201 Created
return NoContent();                       // 204 No Content

// Error responses
return BadRequest(ModelState);            // 400 Bad Request
return NotFound();                        // 404 Not Found
return Unauthorized();                    // 401 Unauthorized
return Forbid();                          // 403 Forbidden
return StatusCode(500, "Error message"); // 500 Internal Server Error
```

**Never skip API implementation** - Even if only building UI initially, create the full API stack.

---

## �🏗️ Solution Architecture

### Project Structure (Manimp Pattern)
```
SteelAxis/
├── SteelAxis.Shared/          # Common models and interfaces
├── SteelAxis.Auth/            # Authentication models (ApplicationUser)
├── SteelAxis.Directory/       # Central directory service (tenant management)
├── SteelAxis.Data/            # Tenant database contexts (to be created)
├── SteelAxis.Services/        # Business logic services
├── SteelAxis.Api/             # Web API endpoints (JWT Bearer auth)
├── SteelAxis.Web/             # Blazor Server web app (OIDC auth)
└── SteelAxis.Tests/           # Unit and integration tests
```

### Multi-Tenancy Implementation

**Pattern:** Database per tenant with central directory
- **Central Directory Database:** Stores tenant metadata, connection strings, user-tenant mappings
- **Tenant Databases:** Separate SQL Server database for each tenant's data (projects, materials, documents)
- **Tenant Resolution:** Based on user authentication claims from Entra External ID
- **Data Isolation:** Complete separation - no shared tables between tenants

**Key Services to Implement:**
```csharp
// ITenantService - Resolve current tenant from authenticated user
public interface ITenantService
{
    Guid GetCurrentTenantId();
    string GetTenantConnectionString(Guid tenantId);
    Task<Tenant> GetTenantAsync(Guid tenantId);
}

// ITenantDbContextFactory - Create tenant-specific DbContext
public interface ITenantDbContextFactory<TContext> where TContext : DbContext
{
    TContext CreateDbContext();
}
```

**Database Context Pattern:**
```csharp
public class TenantDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;
    
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantService tenantService,
        IConfiguration configuration) : base(options)
    {
        _tenantService = tenantService;
        _configuration = configuration;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var connectionString = _tenantService.GetTenantConnectionString(tenantId);
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
```

**Service Lifetime:** Use `Scoped` lifetime for `IDbContextFactory<TenantDbContext>` to ensure tenant context is resolved per request.

---

## 🔐 Authentication & Authorization

### Microsoft Entra External ID (CIAM) Setup

**Architecture:**
- **SteelAxis.Web (Blazor Server):** OpenID Connect (OIDC) authentication
- **SteelAxis.Api (Web API):** JWT Bearer token authentication
- **Token Flow:** Web app acquires token on behalf of user, passes to API

**Web App Configuration (`Program.cs`):**
```csharp
// Authentication with Entra External ID
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // Require authentication by default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add Razor Components with Server interactivity
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

// Add Controllers for Microsoft Identity UI
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
```

**API Configuration (`Program.cs`):**
```csharp
// JWT Bearer authentication for API
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

// Authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => 
        policy.RequireRole("User", "Admin"));
});
```

**Configuration Pattern (`appsettings.json`):**
```json
{
  "AzureAdB2C": {
    "Instance": "https://{tenant-name}.ciamlogin.com/",
    "Domain": "{tenant-name}.onmicrosoft.com",
    "TenantId": "{tenant-id}",
    "ClientId": "{client-id}",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "Scopes": "api://{api-client-id}/access_as_user"
  }
}
```

**Security Best Practices:**
- **NEVER** commit secrets, tenant IDs, or client IDs to repository
- Use **Azure Key Vault** for production secrets
- Use **GitHub Secrets** for CI/CD secrets
- Use placeholders like `<YOUR_TENANT_ID>` in documentation

---

## � API Development Requirements

### API-First Approach
**CRITICAL:** Every feature MUST include API implementation

**Why API-First:**
- Future mobile app support
- Third-party integrations
- Microservices readiness
- Clear separation of concerns
- Testability

### API Development Pattern

When creating ANY feature, always implement in this order:

**1. Data Models (SteelAxis.Shared)**
```csharp
// Entity model
public class MaterialCertificate : ImmutableBaseEntity { ... }

// DTOs
public record MaterialCertificateDto { ... }
public record CreateMaterialCertificateRequest { ... }
public record UpdateMaterialCertificateRequest { ... }
```

**2. Service Interface & Implementation (SteelAxis.Services)**
```csharp
public interface IMaterialCertificateService
{
    Task<List<MaterialCertificateDto>> GetAllAsync(Guid tenantId);
    Task<MaterialCertificateDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<MaterialCertificateDto> CreateAsync(Guid tenantId, CreateMaterialCertificateRequest request);
    Task UpdateAsync(Guid tenantId, Guid id, UpdateMaterialCertificateRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
```

**3. API Controller (SteelAxis.Api)**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialCertificatesController : ControllerBase
{
    private readonly IMaterialCertificateService _service;
    private readonly ITenantService _tenantService;
    
    [HttpGet]
    [ProducesResponseType(typeof(List<MaterialCertificateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaterialCertificateDto>>> GetAll()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var items = await _service.GetAllAsync(tenantId);
        return Ok(items);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(MaterialCertificateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MaterialCertificateDto>> Create([FromBody] CreateMaterialCertificateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var tenantId = _tenantService.GetCurrentTenantId();
        var result = await _service.CreateAsync(tenantId, request);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    // GET by ID, PUT, DELETE endpoints...
}
```

**4. HTTP Service (SteelAxis.Web/Services)**
```csharp
public interface IMaterialCertificateHttpService
{
    Task<List<MaterialCertificateDto>> GetAllAsync();
    Task<MaterialCertificateDto?> GetByIdAsync(Guid id);
    Task<MaterialCertificateDto> CreateAsync(CreateMaterialCertificateRequest request);
    Task UpdateAsync(Guid id, UpdateMaterialCertificateRequest request);
    Task DeleteAsync(Guid id);
}

public class MaterialCertificateHttpService : IMaterialCertificateHttpService
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<MaterialCertificateDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/materialcertificates");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MaterialCertificateDto>>() ?? new();
    }
    
    // Other methods...
}
```

**5. UI Components (SteelAxis.Web/Components)**
- Use standardized dialog/form patterns (see UI Instructions)
- Call HTTP service, not database directly

### API Endpoint Naming Convention

```
GET    /api/materialcertificates           - Get all
GET    /api/materialcertificates/{id}      - Get by ID
POST   /api/materialcertificates           - Create
PUT    /api/materialcertificates/{id}      - Update
DELETE /api/materialcertificates/{id}      - Delete
GET    /api/materialcertificates/search    - Search with filters
```

### Standard API Response Pattern

```csharp
// Success responses
return Ok(data);                          // 200 OK
return CreatedAtAction(...);              // 201 Created
return NoContent();                       // 204 No Content

// Error responses
return BadRequest(ModelState);            // 400 Bad Request
return NotFound();                        // 404 Not Found
return Unauthorized();                    // 401 Unauthorized
return Forbid();                          // 403 Forbidden
return StatusCode(500, "Error message"); // 500 Internal Server Error
```

**Never skip API implementation** - Even if only building UI initially, create the full API stack.

---

## �🎨 MudBlazor UI Framework

### Component Usage Patterns

**Installation:**
```xml
<PackageReference Include="MudBlazor" Version="8.12.0" />
```

**Setup in `Program.cs`:**
```csharp
builder.Services.AddMudServices();
```

**Setup in `_Imports.razor`:**
```razor
@using MudBlazor
```

**Setup in `App.razor` or Layout:**
```razor
<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

### Standard Component Patterns

**Data Tables with MudDataGrid:**
```razor
<MudDataGrid T="MaterialCertificate" 
             Items="@certificates" 
             Filterable="true" 
             SortMode="SortMode.Multiple"
             Groupable="true">
    <Columns>
        <PropertyColumn Property="x => x.LotNumber" Title="Lot Number" />
        <PropertyColumn Property="x => x.MaterialGrade" Title="Grade" />
        <PropertyColumn Property="x => x.CertificateType" Title="Type" />
        <PropertyColumn Property="x => x.Supplier" Title="Supplier" />
        <TemplateColumn Title="Actions">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                               Size="Size.Small" 
                               OnClick="@(() => EditCertificate(context.Item))" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                               Size="Size.Small" 
                               Color="Color.Error"
                               OnClick="@(() => DeleteCertificate(context.Item))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>
```

**Forms with Validation:**
```razor
<EditForm Model="@model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <MudCard>
        <MudCardContent>
            <MudTextField @bind-Value="model.LotNumber" 
                          Label="Lot Number" 
                          Required="true"
                          For="@(() => model.LotNumber)" />
            
            <MudSelect @bind-Value="model.MaterialGrade" 
                       Label="Material Grade" 
                       Required="true"
                       For="@(() => model.MaterialGrade)">
                <MudSelectItem Value="@("S355")">S355</MudSelectItem>
                <MudSelectItem Value="@("S275")">S275</MudSelectItem>
            </MudSelect>
            
            <MudDatePicker @bind-Date="model.TestDate" 
                           Label="Test Date" 
                           For="@(() => model.TestDate)" />
        </MudCardContent>
        <MudCardActions>
            <MudButton ButtonType="ButtonType.Submit" 
                       Variant="Variant.Filled" 
                       Color="Color.Primary">
                Save
            </MudButton>
            <MudButton OnClick="Cancel" 
                       Variant="Variant.Text">
                Cancel
            </MudButton>
        </MudCardActions>
    </MudCard>
</EditForm>
```

**Dialogs:**
```razor
@inject IDialogService DialogService

// Open dialog
private async Task OpenDialog()
{
    var parameters = new DialogParameters<EditCertificateDialog>();
    parameters.Add(x => x.CertificateId, certificateId);
    
    var options = new DialogOptions { 
        CloseButton = true, 
        MaxWidth = MaxWidth.Medium, 
        FullWidth = true 
    };
    
    var dialog = await DialogService.ShowAsync<EditCertificateDialog>(
        "Edit Certificate", parameters, options);
    var result = await dialog.Result;
    
    if (!result.Canceled)
    {
        await LoadCertificates();
    }
}
```

**Loading State:**
```razor
@if (_loading)
{
    <MudProgressLinear Color="Color.Primary" Indeterminate="true" />
}
else
{
    <!-- Content -->
}
```

**Notifications:**
```razor
@inject ISnackbar Snackbar

private void ShowSuccess(string message)
{
    Snackbar.Add(message, Severity.Success);
}

private void ShowError(string message)
{
    Snackbar.Add(message, Severity.Error);
}
```

---

## 🏭 EN 1090 Compliance Implementation

### Core Requirements

**Material Traceability:**
- Track material certificates (EN 10204 Type 3.1/3.2)
- Lot number tracking throughout fabrication
- Chemical composition and mechanical properties
- Supplier documentation

**Quality Control:**
- Non-Conformance Reports (NCR)
- Corrective Action Requests (CAR)
- Welding Procedure Specifications (WPS)
- Welding Procedure Qualification Records (WPQR)
- Non-Destructive Testing (NDT) records

**Documentation:**
- Declaration of Performance (DoP)
- CE Marking requirements
- Manufacturing dossiers
- Audit trails

**Data Integrity:**
- SHA-256 hashing for immutable records
- Version control for documents
- Audit logging for all changes

### Database Schema Examples

**Material Certificates:**
```csharp
public class MaterialCertificate
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string MaterialGrade { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty; // "3.1", "3.2"
    public string Supplier { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public string ChemicalComposition { get; set; } = "{}"; // JSON
    public string MechanicalProperties { get; set; } = "{}"; // JSON
    public string? CertificateFileUrl { get; set; }
    public string? DataHash { get; set; } // SHA-256
    public bool IsImmutable { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
```

**NCR (Non-Conformance Report):**
```csharp
public class NonConformanceReport
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string NCRNumber { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string Description { get; set; } = string.Empty;
    public NCRSeverity Severity { get; set; }
    public NCRStatus Status { get; set; }
    public DateTime DetectedDate { get; set; }
    public string DetectedBy { get; set; } = string.Empty;
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public DateTime? TargetClosureDate { get; set; }
    public DateTime? ActualClosureDate { get; set; }
    public string? VerifiedBy { get; set; }
    public List<NCRAttachment> Attachments { get; set; } = new();
}

public enum NCRSeverity { Minor, Major, Critical }
public enum NCRStatus { Open, InProgress, UnderReview, Closed, Rejected }
```

### Service Pattern for EN 1090

```csharp
public interface IMaterialTraceabilityService
{
    Task<MaterialCertificate> RegisterCertificateAsync(Guid tenantId, CertificateData data);
    Task<List<MaterialCertificate>> GetCertificatesByLotNumberAsync(Guid tenantId, string lotNumber);
    Task<bool> ValidateCertificateIntegrityAsync(Guid certificateId);
    Task<string> GenerateDataHashAsync(MaterialCertificate certificate);
}

public class MaterialTraceabilityService : IMaterialTraceabilityService
{
    private readonly TenantDbContext _context;
    
    public async Task<string> GenerateDataHashAsync(MaterialCertificate cert)
    {
        var data = $"{cert.LotNumber}|{cert.MaterialGrade}|{cert.CertificateNumber}|{cert.TestDate:O}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }
}
```

---

## 📦 Entity Framework Core Patterns

### DbContext Configuration

**Service Registration:**
```csharp
// For multi-tenancy, use Scoped lifetime
builder.Services.AddDbContextFactory<TenantDbContext>(
    options => options.UseSqlServer(), 
    ServiceLifetime.Scoped);

// Register tenant service
builder.Services.AddScoped<ITenantService, TenantService>();
```

**Blazor Component Usage:**
```csharp
@inject IDbContextFactory<TenantDbContext> DbFactory
@implements IAsyncDisposable

@code {
    private TenantDbContext _context = default!;
    
    protected override void OnInitialized()
    {
        _context = DbFactory.CreateDbContext();
    }
    
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
```

### Query Patterns

**With Filtering and Sorting:**
```csharp
var certificates = await _context.MaterialCertificates
    .Where(c => c.TenantId == tenantId)
    .Where(c => c.LotNumber.Contains(searchTerm))
    .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();
```

**With Includes:**
```csharp
var project = await _context.Projects
    .Include(p => p.Materials)
    .Include(p => p.Tasks)
    .FirstOrDefaultAsync(p => p.Id == projectId && p.TenantId == tenantId);
```

---

## 🔧 API Development Patterns

### Controller Structure

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require JWT authentication
public class MaterialCertificatesController : ControllerBase
{
    private readonly IMaterialTraceabilityService _service;
    private readonly ITenantService _tenantService;
    
    public MaterialCertificatesController(
        IMaterialTraceabilityService service,
        ITenantService tenantService)
    {
        _service = service;
        _tenantService = tenantService;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<MaterialCertificateDto>>> GetAll()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var certificates = await _service.GetAllAsync(tenantId);
        return Ok(certificates);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MaterialCertificateDto>> Create(
        [FromBody] CreateCertificateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var tenantId = _tenantService.GetCurrentTenantId();
        var certificate = await _service.CreateAsync(tenantId, request);
        
        return CreatedAtAction(
            nameof(GetById), 
            new { id = certificate.Id }, 
            certificate);
    }
}
```

### HTTP Service Pattern (Blazor → API)

```csharp
public interface IMaterialCertificateHttpService
{
    Task<List<MaterialCertificateDto>> GetAllAsync();
    Task<MaterialCertificateDto> GetByIdAsync(Guid id);
    Task<MaterialCertificateDto> CreateAsync(CreateCertificateRequest request);
    Task UpdateAsync(Guid id, UpdateCertificateRequest request);
    Task DeleteAsync(Guid id);
}

public class MaterialCertificateHttpService : IMaterialCertificateHttpService
{
    private readonly HttpClient _httpClient;
    
    public MaterialCertificateHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<MaterialCertificateDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/materialcertificates");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MaterialCertificateDto>>() 
            ?? new();
    }
    
    public async Task<MaterialCertificateDto> CreateAsync(CreateCertificateRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/materialcertificates", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MaterialCertificateDto>() 
            ?? throw new InvalidOperationException();
    }
}
```

---

## 📝 Coding Standards & Best Practices

### C# Conventions

1. **Use nullable reference types** - Enable in project files
2. **Use `async/await`** - For all I/O operations
3. **Use dependency injection** - Constructor injection preferred
4. **Use records for DTOs** - Immutable data transfer objects
5. **Use `nameof()`** - For property names in error messages
6. **Use string interpolation** - `$"{variable}"` instead of concatenation
7. **Use expression-bodied members** - When it improves readability

### Naming Conventions

- **Classes/Interfaces:** PascalCase (`MaterialCertificate`, `IMaterialService`)
- **Methods:** PascalCase (`GetAllAsync`, `CreateCertificate`)
- **Properties:** PascalCase (`LotNumber`, `TenantId`)
- **Private fields:** `_camelCase` (`_context`, `_service`)
- **Local variables:** camelCase (`certificate`, `tenantId`)
- **Constants:** PascalCase (`MaxFileSize`, `DefaultPageSize`)

### Error Handling

```csharp
public async Task<Result<MaterialCertificate>> CreateCertificateAsync(...)
{
    try
    {
        // Validate
        if (string.IsNullOrWhiteSpace(data.LotNumber))
            return Result<MaterialCertificate>.Failure("Lot number is required");
        
        // Create
        var certificate = new MaterialCertificate { ... };
        await _context.MaterialCertificates.AddAsync(certificate);
        await _context.SaveChangesAsync();
        
        return Result<MaterialCertificate>.Success(certificate);
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error creating certificate");
        return Result<MaterialCertificate>.Failure("Database error occurred");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating certificate");
        return Result<MaterialCertificate>.Failure("An error occurred");
    }
}
```

### Validation Patterns

**Data Annotations:**
```csharp
public class CreateCertificateRequest
{
    [Required(ErrorMessage = "Lot number is required")]
    [StringLength(100, MinimumLength = 3)]
    public string LotNumber { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^(S235|S275|S355|S420|S460)$", 
        ErrorMessage = "Invalid material grade")]
    public string MaterialGrade { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^(3\.1|3\.2)$", 
        ErrorMessage = "Certificate type must be 3.1 or 3.2")]
    public string CertificateType { get; set; } = string.Empty;
}
```

---

## 🧪 Testing Guidelines

### Unit Test Pattern

```csharp
public class MaterialTraceabilityServiceTests
{
    private readonly Mock<TenantDbContext> _contextMock;
    private readonly MaterialTraceabilityService _service;
    
    public MaterialTraceabilityServiceTests()
    {
        _contextMock = new Mock<TenantDbContext>();
        _service = new MaterialTraceabilityService(_contextMock.Object);
    }
    
    [Fact]
    public async Task CreateCertificate_ValidData_ReturnsCertificate()
    {
        // Arrange
        var request = new CertificateData
        {
            LotNumber = "LOT12345",
            MaterialGrade = "S355",
            CertificateType = "3.1"
        };
        
        // Act
        var result = await _service.RegisterCertificateAsync(Guid.NewGuid(), request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("LOT12345", result.LotNumber);
        Assert.NotNull(result.DataHash);
    }
}
```

---

## 🚀 Azure Deployment & CI/CD

### GitHub Actions Workflow Pattern

**Uses OpenID Connect (no publish profiles):**
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --configuration Release --no-restore
      
      - name: Test
        run: dotnet test --no-build --verbosity normal
      
      - name: Publish Web
        run: dotnet publish SteelAxis.Web/SteelAxis.Web.csproj -c Release -o ./publish-web
      
      - name: Publish API
        run: dotnet publish SteelAxis.Api/SteelAxis.Api.csproj -c Release -o ./publish-api
      
      - name: Login to Azure
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: steelaxis-dev
          package: ./publish-web
      
      - name: Deploy API
        uses: azure/webapps-deploy@v2
        with:
          app-name: steelaxis-dev-api
          package: ./publish-api
```

---

## 📚 Documentation References

### Internal Documentation Structure
```
docs/
├── INDEX.md                           # Documentation overview
├── authentication/                    # Auth implementation guides
├── azure-infrastructure/              # Azure setup & deployment
├── en-1090-compliance/               # EN 1090 implementation
│   ├── EN-1090-COMPLETE-GUIDE.md    # Complete compliance guide
│   ├── en-1090-requirements.md       # Database schemas & APIs
│   └── en-1090-ncr-management.md     # NCR workflow
├── customer-portal/                   # Customer portal feature
│   ├── START-HERE.md                 # Implementation plan
│   └── IMPLEMENTATION-PLAN.md        # Detailed specifications
├── file-storage/                      # File management architecture
├── inventory/                         # Inventory management
├── project-management/                # PM features
└── security/                          # Security guidelines
```

### When to Reference Documentation
- **Authentication implementation:** See `docs/authentication/`
- **EN 1090 compliance features:** See `docs/en-1090-compliance/EN-1090-COMPLETE-GUIDE.md`
- **Multi-tenant patterns:** See `docs/INDEX.md` for references
- **Customer portal:** See `docs/customer-portal/START-HERE.md`

---

## 🎯 Code Generation Guidelines

### When Generating Code:

1. **Always follow multi-tenant pattern** - Include `TenantId` in all tenant-scoped entities
2. **Use dependency injection** - Constructor injection for all services
3. **Include proper error handling** - Try-catch with logging
4. **Add XML documentation** - For public methods and classes
5. **Use async/await** - For all database and HTTP operations
6. **Follow MudBlazor patterns** - For all UI components
7. **Include validation** - Both client-side and server-side
8. **Add audit fields** - `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`
9. **Implement proper disposal** - For DbContext and HttpClient
10. **Write unit tests** - For business logic

### Code Template Example

```csharp
/// <summary>
/// Service for managing material certificates in compliance with EN 10204
/// </summary>
public class MaterialTraceabilityService : IMaterialTraceabilityService
{
    private readonly IDbContextFactory<TenantDbContext> _contextFactory;
    private readonly ITenantService _tenantService;
    private readonly ILogger<MaterialTraceabilityService> _logger;
    
    public MaterialTraceabilityService(
        IDbContextFactory<TenantDbContext> contextFactory,
        ITenantService tenantService,
        ILogger<MaterialTraceabilityService> logger)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
        _logger = logger;
    }
    
    /// <summary>
    /// Registers a new material certificate with data integrity hash
    /// </summary>
    /// <param name="data">Certificate data including lot number and test results</param>
    /// <returns>Created certificate with generated hash</returns>
    /// <exception cref="ArgumentNullException">When data is null</exception>
    /// <exception cref="ValidationException">When data validation fails</exception>
    public async Task<MaterialCertificate> RegisterCertificateAsync(CertificateData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var tenantId = _tenantService.GetCurrentTenantId();
            
            var certificate = new MaterialCertificate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                LotNumber = data.LotNumber,
                MaterialGrade = data.Grade,
                CertificateType = data.Type,
                Supplier = data.Supplier,
                TestDate = data.TestDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _tenantService.GetCurrentUserId()
            };
            
            // Generate immutable hash for data integrity
            certificate.DataHash = await GenerateDataHashAsync(certificate);
            certificate.IsImmutable = true;
            
            await context.MaterialCertificates.AddAsync(certificate);
            await context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Certificate {CertificateId} registered for lot {LotNumber}", 
                certificate.Id, certificate.LotNumber);
            
            return certificate;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error registering certificate");
            throw new InvalidOperationException("Failed to register certificate", ex);
        }
    }
    
    private async Task<string> GenerateDataHashAsync(MaterialCertificate cert)
    {
        var data = $"{cert.LotNumber}|{cert.MaterialGrade}|{cert.CertificateNumber}|{cert.TestDate:O}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }
}
```

---

## 🔍 Common Patterns to Use

### Result Pattern for Error Handling
```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
```

### Repository Pattern (if needed)
```csharp
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(Guid tenantId);
    Task<T?> GetByIdAsync(Guid tenantId, Guid id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid tenantId, Guid id);
}
```

---

## 🛡️ Security Reminders

- **NEVER** store secrets in code or appsettings.json
- Use **Azure Key Vault** for production secrets
- Use **GitHub Secrets** for CI/CD
- Always validate user input on **both client and server**
- Use **parameterized queries** (EF Core does this automatically)
- Implement **rate limiting** for APIs
- Use **HTTPS** everywhere
- Validate **tenant isolation** in all queries
- Log **security events** (auth failures, unauthorized access)
- Implement **audit trails** for sensitive operations

---

## 📞 Support & Questions

For questions about:
- **Architecture decisions:** Review `/docs/INDEX.md`
- **EN 1090 implementation:** See `/docs/en-1090-compliance/`
- **Authentication:** See `/docs/authentication/`
- **Azure deployment:** See `/docs/azure-infrastructure/`

---

**Remember:** This is a multi-tenant steel fabrication system with strict compliance requirements. Always prioritize data isolation, security, and audit trail capabilities in your code generation.
