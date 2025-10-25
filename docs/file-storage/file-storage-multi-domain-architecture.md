# Multi-Domain File Storage Architecture - Complete Plan

**Date:** 14 October 2025  
**Status:** Enhanced Architecture Design  
**Complexity:** Enterprise-Grade Multi-Tenant SaaS

---

## 🎯 Domain Architecture Overview

### Three-Domain Strategy

```
┌─────────────────────────────────────────────────────────────┐
│                  Multi-Domain Access Pattern                 │
└─────────────────────────────────────────────────────────────┘

1. Main Application (Internal Users)
   ├── URL: {tenant}.manimp.com
   ├── Auth: Required (company employees)
   ├── Access: Full system (ERP, Procurement, Inventory, etc.)
   └── Files: Upload, manage, internal sharing

2. Internal File Portal (Company Users)
   ├── URL: {tenant}.files.manimp.com
   ├── Auth: Required (same as main app)
   ├── Access: All company files (all projects)
   └── Features: Upload, organize, internal share

3. External Client Portal (Customers/Partners)
   ├── URL: {tenant}.docs.manimp.com
   ├── Auth: Optional (configurable per tenant)
   ├── Access: Project-specific files only
   └── Features: View/download approved docs
```

### Real-World Example

**ACME Steel Corp (Tenant):**
```
acme.manimp.com              → ERP system for ACME employees
acme.files.manimp.com        → File management for ACME employees
acme.docs.manimp.com         → Document portal for ACME's clients
```

**ACME's Client "BuildCo" accessing Project #789:**
```
acme.docs.manimp.com
  ↓ Login (if required by ACME admin)
  ↓ Show Project #789 files only
  ├── ✅ Phase 1: Planning (Completed) → Show drawings
  ├── ✅ Phase 2: Fabrication (Completed) → Show photos
  ├── 🔄 Phase 3: Assembly (In Progress) → Hide (not completed)
  └── ⏳ Phase 4: Delivery (Not Started) → Hide
```

---

## 🏗️ Complete Architecture Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                      DNS Layer (Wildcard)                       │
├────────────────────────────────────────────────────────────────┤
│  *.manimp.com       → Main App (tenant resolver)                │
│  *.files.manimp.com → File Portal (employee access)             │
│  *.docs.manimp.com  → Client Portal (external access)           │
└────────────────────────────────────────────────────────────────┘
                              ↓
┌────────────────────────────────────────────────────────────────┐
│              Subdomain Tenant Resolver Middleware               │
│  Extract: "acme" from host                                      │
│  Query: Tenants table → TenantId                                │
│  Inject: HttpContext.Items["TenantId"]                          │
└────────────────────────────────────────────────────────────────┘
                              ↓
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────┐      ┌──────────────┐     ┌──────────────┐
│ Main App     │      │ File Portal  │     │ Client Portal│
│ (Internal)   │      │ (Internal)   │     │ (External)   │
├──────────────┤      ├──────────────┤     ├──────────────┤
│ • ERP        │      │ • Browse all │     │ • Browse by  │
│ • Procure    │      │ • Upload     │     │   project    │
│ • Inventory  │      │ • Share      │     │ • View only  │
│ • Projects   │      │ • Organize   │     │ • Download   │
└──────────────┘      └──────────────┘     └──────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              ▼
┌────────────────────────────────────────────────────────────────┐
│              File Access Control Service                        │
│  Check:                                                         │
│  1. User role (Admin, Manager, User, Client)                   │
│  2. Project permissions                                         │
│  3. File sharing settings (role-based)                         │
│  4. External access enabled?                                    │
│  5. Project phase completed?                                    │
└────────────────────────────────────────────────────────────────┘
                              ↓
┌────────────────────────────────────────────────────────────────┐
│                    Azure Blob Storage                           │
│  tenant-{guid}/                                                 │
│  ├── projects/                                                  │
│  │   ├── project-123/                                           │
│  │   │   ├── phase-1-planning/ (completed, visible to client)  │
│  │   │   ├── phase-2-fabrication/ (completed, visible)         │
│  │   │   ├── phase-3-assembly/ (in progress, hidden)           │
│  │   │   └── phase-4-delivery/ (not started, hidden)           │
│  ├── internal/                                                  │
│  │   ├── hr/                                                    │
│  │   └── finance/                                               │
│  └── shared/                                                    │
└────────────────────────────────────────────────────────────────┘
```

---

## 📊 Database Schema Extensions

### 1. Tenant Settings (Admin Panel Configuration)

```sql
-- Add to Tenant table
ALTER TABLE Tenants ADD COLUMN FileSharingSettings NVARCHAR(MAX); -- JSON

-- Example FileSharingSettings JSON:
{
  "externalAccessEnabled": true,
  "requireExternalRegistration": false,  -- If false, anyone with link can access
  "defaultShareExpiry": null,            -- null = unlimited, or days (e.g., 30)
  "allowedRoles": {
    "Admin": {
      "canShareUnlimited": true,
      "canShareExternal": true,
      "maxShareDays": null
    },
    "Manager": {
      "canShareUnlimited": false,
      "canShareExternal": true,
      "maxShareDays": 90
    },
    "User": {
      "canShareUnlimited": false,
      "canShareExternal": false,
      "maxShareDays": 30
    }
  }
}
```

### 2. External Users Table

```csharp
namespace Manimp.Shared.Models;

/// <summary>
/// External users (clients/partners) with access to docs portal
/// </summary>
public class ExternalUser
{
    public Guid ExternalUserId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Tenant this external user belongs to
    /// </summary>
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Email address (used for login if registration required)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name
    /// </summary>
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Company name
    /// </summary>
    [MaxLength(200)]
    public string? Company { get; set; }
    
    /// <summary>
    /// Password hash (if registration required)
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// Which projects this user can access
    /// </summary>
    public List<Guid> AllowedProjectIds { get; set; } = new();
    
    /// <summary>
    /// Is this user active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When was this user created?
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginUtc { get; set; }
    
    /// <summary>
    /// Access expiry date (null = unlimited)
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }
}
```

### 3. File Sharing Permissions

```csharp
namespace Manimp.Shared.Models;

/// <summary>
/// Tracks file/folder sharing permissions
/// </summary>
public class FileShare
{
    public Guid FileShareId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Tenant this share belongs to
    /// </summary>
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Blob URI or folder path being shared
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ResourcePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Is this a folder share? (vs single file)
    /// </summary>
    public bool IsFolder { get; set; }
    
    /// <summary>
    /// Share type: Internal, External, Public
    /// </summary>
    [Required]
    public ShareType ShareType { get; set; }
    
    /// <summary>
    /// Short code for share link (e.g., "abc123")
    /// </summary>
    [MaxLength(50)]
    public string? ShareCode { get; set; }
    
    /// <summary>
    /// Who created this share?
    /// </summary>
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// User role of creator (for audit)
    /// </summary>
    [MaxLength(50)]
    public string CreatedByRole { get; set; } = string.Empty;
    
    /// <summary>
    /// Share expiry (null = unlimited, if allowed by role)
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Access count (analytics)
    /// </summary>
    public int AccessCount { get; set; }
    
    /// <summary>
    /// Last accessed timestamp
    /// </summary>
    public DateTime? LastAccessedUtc { get; set; }
    
    /// <summary>
    /// Is this share active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional: Restrict to specific external users
    /// </summary>
    public List<Guid> AllowedExternalUserIds { get; set; } = new();
    
    /// <summary>
    /// Optional: Password protection
    /// </summary>
    public string? PasswordHash { get; set; }
}

public enum ShareType
{
    Internal,      // Only company employees
    External,      // Registered external users
    Public,        // Anyone with link (no registration)
    ClientPortal   // Visible on {tenant}.docs.manimp.com
}
```

### 4. Project File Visibility

```csharp
namespace Manimp.Shared.Models;

/// <summary>
/// Controls which project files are visible to clients
/// </summary>
public class ProjectFileVisibility
{
    public Guid ProjectFileVisibilityId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Project this visibility rule applies to
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    /// <summary>
    /// File or folder path in blob storage
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ResourcePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Is this visible to external clients?
    /// </summary>
    public bool IsVisibleToClients { get; set; }
    
    /// <summary>
    /// Requires project phase completion?
    /// </summary>
    public Guid? RequiredPhaseId { get; set; }
    
    /// <summary>
    /// Marked for client access by user
    /// </summary>
    public string? MarkedByUserId { get; set; }
    
    /// <summary>
    /// When was this marked?
    /// </summary>
    public DateTime? MarkedUtc { get; set; }
}

/// <summary>
/// Project phases (matches your README workflow)
/// </summary>
public class ProjectPhase
{
    public Guid ProjectPhaseId { get; set; } = Guid.NewGuid();
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string PhaseName { get; set; } = string.Empty; // "Planning", "Fabrication", etc.
    
    public int PhaseOrder { get; set; } // 1, 2, 3...
    
    public PhaseStatus Status { get; set; }
    
    public DateTime? CompletedUtc { get; set; }
}

public enum PhaseStatus
{
    NotStarted,
    InProgress,
    Completed,
    OnHold
}
```

---

## 🔐 Enhanced Access Control Service

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Manimp.Data.Contexts;
using Manimp.Shared.Interfaces;
using Manimp.Shared.Models;

namespace Manimp.Services.Implementation;

/// <summary>
/// Controls file access based on role, project, and sharing settings
/// </summary>
public class FileAccessControlService : IFileAccessControlService
{
    private readonly AppDbContext _context;
    private readonly ITenantResolver _tenantResolver;
    private readonly ILogger<FileAccessControlService> _logger;

    public FileAccessControlService(
        AppDbContext context,
        ITenantResolver tenantResolver,
        ILogger<FileAccessControlService> logger)
    {
        _context = context;
        _tenantResolver = tenantResolver;
        _logger = logger;
    }

    /// <summary>
    /// Checks if user can access a file based on domain and role
    /// </summary>
    public async Task<FileAccessResult> CanAccessFileAsync(
        string resourcePath, 
        string userId, 
        string domain, 
        bool isExternalUser = false)
    {
        var tenantId = _tenantResolver.GetTenantId();

        // Domain-based access control
        if (domain.Contains(".docs."))
        {
            // External client portal
            return await CheckClientPortalAccessAsync(resourcePath, userId, tenantId, isExternalUser);
        }
        else if (domain.Contains(".files.") || !domain.Contains(".docs."))
        {
            // Internal file portal or main app
            return await CheckInternalAccessAsync(resourcePath, userId, tenantId);
        }

        return FileAccessResult.Denied("Invalid domain");
    }

    /// <summary>
    /// Checks if external user can access file on docs portal
    /// </summary>
    private async Task<FileAccessResult> CheckClientPortalAccessAsync(
        string resourcePath, 
        string userId, 
        string tenantId,
        bool isExternalUser)
    {
        // Check tenant settings: is external access enabled?
        var tenant = await _context.Tenants.FindAsync(Guid.Parse(tenantId));
        if (tenant == null)
            return FileAccessResult.Denied("Tenant not found");

        var settings = JsonSerializer.Deserialize<FileSharingSettings>(
            tenant.FileSharingSettings ?? "{}");

        if (!settings.ExternalAccessEnabled)
        {
            return FileAccessResult.Denied("External access disabled by administrator");
        }

        // If registration required, validate external user
        if (settings.RequireExternalRegistration && isExternalUser)
        {
            var externalUser = await _context.ExternalUsers
                .FirstOrDefaultAsync(eu => 
                    eu.ExternalUserId.ToString() == userId && 
                    eu.IsActive &&
                    (eu.ExpiresUtc == null || eu.ExpiresUtc > DateTime.UtcNow));

            if (externalUser == null)
                return FileAccessResult.Denied("User not registered or expired");

            // Check project access
            var projectId = ExtractProjectIdFromPath(resourcePath);
            if (projectId != null && !externalUser.AllowedProjectIds.Contains(projectId.Value))
            {
                return FileAccessResult.Denied("User not assigned to this project");
            }
        }

        // Check if file is marked for client visibility
        var visibility = await _context.ProjectFileVisibilities
            .Include(v => v.Project)
            .ThenInclude(p => p.Phases)
            .FirstOrDefaultAsync(v => v.ResourcePath == resourcePath);

        if (visibility == null || !visibility.IsVisibleToClients)
        {
            return FileAccessResult.Denied("File not marked for client access");
        }

        // Check if required project phase is completed
        if (visibility.RequiredPhaseId != null)
        {
            var phase = await _context.ProjectPhases
                .FirstOrDefaultAsync(p => p.ProjectPhaseId == visibility.RequiredPhaseId);

            if (phase == null || phase.Status != PhaseStatus.Completed)
            {
                return FileAccessResult.Denied($"Waiting for phase '{phase?.PhaseName}' completion");
            }
        }

        _logger.LogInformation(
            "Client portal access granted: {ResourcePath} for user {UserId}",
            resourcePath, userId);

        return FileAccessResult.Allowed();
    }

    /// <summary>
    /// Checks if internal user can access file
    /// </summary>
    private async Task<FileAccessResult> CheckInternalAccessAsync(
        string resourcePath, 
        string userId, 
        string tenantId)
    {
        // Internal users (employees) have access to all company files
        // Role-based restrictions can be added here if needed
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return FileAccessResult.Denied("User not found");

        // Optional: Check role-based folder restrictions
        // e.g., "hr/" folder only for HR role

        _logger.LogInformation(
            "Internal access granted: {ResourcePath} for user {UserId}",
            resourcePath, userId);

        return FileAccessResult.Allowed();
    }

    /// <summary>
    /// Validates if user can create share link based on role
    /// </summary>
    public async Task<SharePermissionResult> CanCreateShareAsync(
        string userId, 
        string userRole, 
        int? requestedDays)
    {
        var tenantId = _tenantResolver.GetTenantId();
        var tenant = await _context.Tenants.FindAsync(Guid.Parse(tenantId));
        
        var settings = JsonSerializer.Deserialize<FileSharingSettings>(
            tenant.FileSharingSettings ?? "{}");

        var roleSettings = settings.AllowedRoles?.GetValueOrDefault(userRole);
        if (roleSettings == null)
        {
            return SharePermissionResult.Denied("Role not configured for sharing");
        }

        // Check if unlimited sharing allowed
        if (requestedDays == null)
        {
            if (!roleSettings.CanShareUnlimited)
            {
                return SharePermissionResult.Denied(
                    $"Your role allows max {roleSettings.MaxShareDays} days sharing");
            }
        }
        else
        {
            // Check if requested days exceed role limit
            if (roleSettings.MaxShareDays.HasValue && 
                requestedDays > roleSettings.MaxShareDays)
            {
                return SharePermissionResult.Denied(
                    $"Your role allows max {roleSettings.MaxShareDays} days");
            }
        }

        return SharePermissionResult.Allowed(
            canShareExternal: roleSettings.CanShareExternal,
            maxDays: roleSettings.MaxShareDays);
    }

    private Guid? ExtractProjectIdFromPath(string path)
    {
        // Extract project ID from path like: tenant-{guid}/projects/project-{id}/...
        var match = Regex.Match(path, @"projects/project-([a-f0-9\-]+)");
        if (match.Success && Guid.TryParse(match.Groups[1].Value, out var projectId))
        {
            return projectId;
        }
        return null;
    }
}

public class FileAccessResult
{
    public bool IsAllowed { get; set; }
    public string? DenialReason { get; set; }

    public static FileAccessResult Allowed() => new() { IsAllowed = true };
    public static FileAccessResult Denied(string reason) => 
        new() { IsAllowed = false, DenialReason = reason };
}

public class SharePermissionResult
{
    public bool CanShare { get; set; }
    public bool CanShareExternal { get; set; }
    public int? MaxDays { get; set; }
    public string? DenialReason { get; set; }

    public static SharePermissionResult Allowed(bool canShareExternal, int? maxDays) => 
        new() 
        { 
            CanShare = true, 
            CanShareExternal = canShareExternal,
            MaxDays = maxDays 
        };
    
    public static SharePermissionResult Denied(string reason) => 
        new() { CanShare = false, DenialReason = reason };
}

public class FileSharingSettings
{
    public bool ExternalAccessEnabled { get; set; }
    public bool RequireExternalRegistration { get; set; }
    public int? DefaultShareExpiry { get; set; }
    public Dictionary<string, RoleShareSettings>? AllowedRoles { get; set; }
}

public class RoleShareSettings
{
    public bool CanShareUnlimited { get; set; }
    public bool CanShareExternal { get; set; }
    public int? MaxShareDays { get; set; }
}
```

---

## 🎨 Admin Panel UI - Tenant Settings

```razor
@page "/admin/file-settings"
@using Manimp.Shared.Models
@using Manimp.Shared.Interfaces
@inject ITenantService TenantService
@inject ISnackbar Snackbar

<PageTitle>File Sharing Settings</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">File Sharing & External Access Settings</MudText>
    
    <MudPaper Elevation="2" Class="pa-4 mb-4">
        <MudText Typo="Typo.h6" Class="mb-2">Client Portal ({_tenant?.Subdomain}.docs.manimp.com)</MudText>
        
        <MudSwitch @bind-Value="_settings.ExternalAccessEnabled" 
                  Color="Color.Primary"
                  Label="Enable External Client Access" />
        
        @if (_settings.ExternalAccessEnabled)
        {
            <MudSwitch @bind-Value="_settings.RequireExternalRegistration" 
                      Color="Color.Primary"
                      Label="Require Client Email Registration"
                      Class="mt-2" />
            
            <MudAlert Severity="Severity.Info" Class="mt-2">
                @if (_settings.RequireExternalRegistration)
                {
                    <MudText>Clients must register with email to access documents.</MudText>
                }
                else
                {
                    <MudText>Anyone with project link can view documents (no registration).</MudText>
                }
            </MudAlert>
        }
    </MudPaper>
    
    <MudPaper Elevation="2" Class="pa-4 mb-4">
        <MudText Typo="Typo.h6" Class="mb-2">Default Share Link Expiry</MudText>
        
        <MudRadioGroup @bind-Value="_expiryType">
            <MudRadio Value="@("unlimited")" Color="Color.Primary">
                Unlimited (links never expire)
            </MudRadio>
            <MudRadio Value="@("limited")" Color="Color.Primary">
                Limited time (specify days)
            </MudRadio>
        </MudRadioGroup>
        
        @if (_expiryType == "limited")
        {
            <MudNumericField @bind-Value="_settings.DefaultShareExpiry" 
                           Label="Default Expiry (days)" 
                           Min="1" 
                           Max="3650"
                           Class="mt-2" />
        }
    </MudPaper>
    
    <MudPaper Elevation="2" Class="pa-4 mb-4">
        <MudText Typo="Typo.h6" Class="mb-4">Role-Based Sharing Permissions</MudText>
        
        <MudSimpleTable Dense="true" Hover="true">
            <thead>
                <tr>
                    <th>Role</th>
                    <th>Can Share Unlimited?</th>
                    <th>Can Share External?</th>
                    <th>Max Days</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var role in _roles)
                {
                    var roleSettings = _settings.AllowedRoles![role];
                    <tr>
                        <td><strong>@role</strong></td>
                        <td>
                            <MudSwitch @bind-Value="roleSettings.CanShareUnlimited" 
                                     Color="Color.Primary" 
                                     Dense="true" />
                        </td>
                        <td>
                            <MudSwitch @bind-Value="roleSettings.CanShareExternal" 
                                     Color="Color.Primary" 
                                     Dense="true" />
                        </td>
                        <td>
                            @if (!roleSettings.CanShareUnlimited)
                            {
                                <MudNumericField @bind-Value="roleSettings.MaxShareDays" 
                                               Min="1" 
                                               Max="3650" 
                                               Variant="Variant.Outlined"
                                               Margin="Margin.Dense" />
                            }
                            else
                            {
                                <MudText Color="Color.Secondary">—</MudText>
                            }
                        </td>
                    </tr>
                }
            </tbody>
        </MudSimpleTable>
    </MudPaper>
    
    <MudButton Variant="Variant.Filled" 
              Color="Color.Primary" 
              Size="Size.Large"
              StartIcon="@Icons.Material.Filled.Save"
              OnClick="SaveSettings">
        Save Settings
    </MudButton>
</MudContainer>

@code {
    private Tenant? _tenant;
    private FileSharingSettings _settings = new()
    {
        AllowedRoles = new Dictionary<string, RoleShareSettings>
        {
            ["Admin"] = new() { CanShareUnlimited = true, CanShareExternal = true },
            ["Manager"] = new() { CanShareUnlimited = false, CanShareExternal = true, MaxShareDays = 90 },
            ["User"] = new() { CanShareUnlimited = false, CanShareExternal = false, MaxShareDays = 30 }
        }
    };
    
    private string[] _roles = { "Admin", "Manager", "User" };
    private string _expiryType = "unlimited";

    protected override async Task OnInitializedAsync()
    {
        _tenant = await TenantService.GetCurrentTenantAsync();
        
        if (!string.IsNullOrEmpty(_tenant.FileSharingSettings))
        {
            _settings = JsonSerializer.Deserialize<FileSharingSettings>(_tenant.FileSharingSettings)!;
        }
        
        _expiryType = _settings.DefaultShareExpiry.HasValue ? "limited" : "unlimited";
    }

    private async Task SaveSettings()
    {
        if (_expiryType == "unlimited")
        {
            _settings.DefaultShareExpiry = null;
        }
        
        _tenant!.FileSharingSettings = JsonSerializer.Serialize(_settings);
        
        var result = await TenantService.UpdateTenantAsync(_tenant);
        
        if (result.Success)
        {
            Snackbar.Add("File sharing settings saved successfully", Severity.Success);
        }
        else
        {
            Snackbar.Add($"Failed to save settings: {result.Message}", Severity.Error);
        }
    }
}
```

---

## 🌐 Client Portal UI ({tenant}.docs.manimp.com)

```razor
@page "/client-portal"
@page "/client-portal/project/{ProjectId:guid}"
@using Manimp.Shared.Interfaces
@using Manimp.Shared.Models
@inject IFileAccessControlService AccessControl
@inject IProjectService ProjectService
@inject IFileStorageService FileStorage
@inject NavigationManager Navigation

<PageTitle>@_tenant?.Name - Project Documents</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <!-- Branding Header -->
    <MudPaper Elevation="2" Class="pa-4 mb-4">
        <MudStack Row="true" Justify="Justify.SpaceBetween" AlignItems="AlignItems.Center">
            <MudStack Spacing="1">
                <MudText Typo="Typo.h4">@_tenant?.Name</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">Project Document Portal</MudText>
            </MudStack>
            
            @if (_requiresLogin && !_isLoggedIn)
            {
                <MudButton Variant="Variant.Filled" 
                          Color="Color.Primary"
                          OnClick="ShowLoginDialog">
                    Login
                </MudButton>
            }
            else if (_isLoggedIn)
            {
                <MudStack Spacing="1">
                    <MudText Typo="Typo.body2">
                        <MudIcon Icon="@Icons.Material.Filled.Person" Size="Size.Small" />
                        @_externalUser?.Name
                    </MudText>
                    <MudButton Variant="Variant.Text" 
                              Size="Size.Small"
                              OnClick="Logout">
                        Logout
                    </MudButton>
                </MudStack>
            }
        </MudStack>
    </MudPaper>
    
    @if (!_requiresLogin || _isLoggedIn)
    {
        <!-- Project Selection -->
        @if (ProjectId == null)
        {
            <MudText Typo="Typo.h5" Class="mb-4">Your Projects</MudText>
            
            <MudGrid>
                @foreach (var project in _projects)
                {
                    <MudItem xs="12" sm="6" md="4">
                        <MudCard>
                            <MudCardHeader>
                                <MudText Typo="Typo.h6">@project.Name</MudText>
                            </MudCardHeader>
                            <MudCardContent>
                                <MudText Typo="Typo.body2">@project.Description</MudText>
                                
                                <!-- Phase Progress -->
                                <MudText Typo="Typo.caption" Class="mt-2">Progress:</MudText>
                                @foreach (var phase in project.Phases.OrderBy(p => p.PhaseOrder))
                                {
                                    <MudChip Size="Size.Small" 
                                            Color="@GetPhaseColor(phase.Status)"
                                            Class="ma-1">
                                        @phase.PhaseName
                                    </MudChip>
                                }
                            </MudCardContent>
                            <MudCardActions>
                                <MudButton Variant="Variant.Text" 
                                          Color="Color.Primary"
                                          Href="@($"/client-portal/project/{project.ProjectId}")">
                                    View Documents
                                </MudButton>
                            </MudCardActions>
                        </MudCard>
                    </MudItem>
                }
            </MudGrid>
        }
        else
        {
            <!-- Project Document Browser -->
            <MudBreadcrumbs Items="_breadcrumbs" Class="mb-4" />
            
            <MudText Typo="Typo.h5" Class="mb-2">@_currentProject?.Name - Documents</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-4">
                @_currentProject?.Description
            </MudText>
            
            <!-- Phase Filter -->
            <MudChipSet @bind-SelectedChip="_selectedPhase" Filter="true" Class="mb-4">
                <MudChip Value="@("all")" Color="Color.Primary">All Phases</MudChip>
                @foreach (var phase in _currentProject?.Phases.OrderBy(p => p.PhaseOrder) ?? Enumerable.Empty<ProjectPhase>())
                {
                    @if (phase.Status == PhaseStatus.Completed)
                    {
                        <MudChip Value="@phase.ProjectPhaseId" Color="Color.Success">
                            ✓ @phase.PhaseName
                        </MudChip>
                    }
                }
            </MudChipSet>
            
            <!-- File List -->
            <MudTable Items="_files" Hover="true" Dense="true">
                <HeaderContent>
                    <MudTh>Document</MudTh>
                    <MudTh>Phase</MudTh>
                    <MudTh>Type</MudTh>
                    <MudTh>Size</MudTh>
                    <MudTh>Uploaded</MudTh>
                    <MudTh>Actions</MudTh>
                </HeaderContent>
                <RowTemplate>
                    <MudTd DataLabel="Document">
                        <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
                            <MudIcon Icon="@GetFileIcon(context.Name)" Color="Color.Primary" />
                            <MudText>@context.Name</MudText>
                        </MudStack>
                    </MudTd>
                    <MudTd DataLabel="Phase">
                        <MudChip Size="Size.Small" Color="Color.Success">
                            @context.PhaseName
                        </MudChip>
                    </MudTd>
                    <MudTd DataLabel="Type">
                        <MudText Typo="Typo.body2">@context.FileType</MudText>
                    </MudTd>
                    <MudTd DataLabel="Size">
                        <MudText Typo="Typo.body2">@FormatFileSize(context.SizeBytes)</MudText>
                    </MudTd>
                    <MudTd DataLabel="Uploaded">
                        <MudText Typo="Typo.body2">@context.UploadedUtc.ToLocalTime().ToString("MMM dd, yyyy")</MudText>
                    </MudTd>
                    <MudTd DataLabel="Actions">
                        <MudStack Row="true" Spacing="1">
                            <MudIconButton Icon="@Icons.Material.Filled.Visibility" 
                                         Size="Size.Small"
                                         OnClick="@(() => PreviewFile(context))"
                                         Title="Preview" />
                            <MudIconButton Icon="@Icons.Material.Filled.Download" 
                                         Size="Size.Small"
                                         OnClick="@(() => DownloadFile(context))"
                                         Title="Download" />
                        </MudStack>
                    </MudTd>
                </RowTemplate>
            </MudTable>
        }
    }
    else
    {
        <MudAlert Severity="Severity.Info" Class="mt-4">
            <MudText>Please login to access project documents.</MudText>
        </MudAlert>
    }
</MudContainer>

@code {
    [Parameter] public Guid? ProjectId { get; set; }
    
    private Tenant? _tenant;
    private ExternalUser? _externalUser;
    private bool _requiresLogin;
    private bool _isLoggedIn;
    private List<Project> _projects = new();
    private Project? _currentProject;
    private List<ClientPortalFile> _files = new();
    private MudChip? _selectedPhase;
    private List<BreadcrumbItem> _breadcrumbs = new();

    protected override async Task OnInitializedAsync()
    {
        // Check if external registration required
        _tenant = await TenantService.GetCurrentTenantAsync();
        var settings = JsonSerializer.Deserialize<FileSharingSettings>(_tenant.FileSharingSettings ?? "{}");
        _requiresLogin = settings.RequireExternalRegistration;
        
        // Check if user already logged in (from cookie/session)
        _isLoggedIn = await CheckExternalUserSession();
        
        if (!_requiresLogin || _isLoggedIn)
        {
            await LoadProjectsAsync();
        }
    }

    private async Task LoadProjectsAsync()
    {
        if (_isLoggedIn && _externalUser != null)
        {
            // Load only projects this external user can access
            _projects = await ProjectService.GetProjectsByIdsAsync(_externalUser.AllowedProjectIds);
        }
        else
        {
            // Public access: load all projects with client-visible files
            _projects = await ProjectService.GetProjectsWithClientFilesAsync();
        }
        
        if (ProjectId.HasValue)
        {
            _currentProject = _projects.FirstOrDefault(p => p.ProjectId == ProjectId);
            await LoadProjectFilesAsync();
        }
    }

    private async Task LoadProjectFilesAsync()
    {
        // Load files marked for client visibility
        var visibilities = await ProjectService.GetClientVisibleFilesAsync(ProjectId!.Value);
        
        _files = visibilities
            .Where(v => v.RequiredPhaseId == null || 
                       v.Project.Phases.Any(p => p.ProjectPhaseId == v.RequiredPhaseId && 
                                                 p.Status == PhaseStatus.Completed))
            .Select(v => new ClientPortalFile
            {
                Name = Path.GetFileName(v.ResourcePath),
                ResourcePath = v.ResourcePath,
                PhaseName = v.Project.Phases
                    .FirstOrDefault(p => p.ProjectPhaseId == v.RequiredPhaseId)?.PhaseName ?? "General",
                // ... other properties
            })
            .ToList();
    }

    private Color GetPhaseColor(PhaseStatus status) => status switch
    {
        PhaseStatus.Completed => Color.Success,
        PhaseStatus.InProgress => Color.Warning,
        _ => Color.Default
    };

    private string GetFileIcon(string fileName) => Path.GetExtension(fileName).ToLower() switch
    {
        ".pdf" => Icons.Material.Filled.PictureAsPdf,
        ".jpg" or ".png" => Icons.Material.Filled.Image,
        ".dwg" or ".dxf" => Icons.Material.Filled.Architecture,
        _ => Icons.Material.Filled.InsertDriveFile
    };

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private record ClientPortalFile
    {
        public string Name { get; set; } = string.Empty;
        public string ResourcePath { get; set; } = string.Empty;
        public string PhaseName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime UploadedUtc { get; set; }
    }
}
```

---

## 🔧 Enhanced Subdomain Tenant Resolver

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Manimp.Directory.Data;
using Manimp.Shared.Interfaces;

namespace Manimp.Services.Implementation;

/// <summary>
/// Enhanced subdomain resolver supporting multiple domain patterns:
/// - {tenant}.manimp.com (main app)
/// - {tenant}.files.manimp.com (internal file portal)
/// - {tenant}.docs.manimp.com (external client portal)
/// </summary>
public class MultiDomainTenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DirectoryDbContext _directoryContext;
    private readonly ILogger<MultiDomainTenantResolver> _logger;
    private string? _cachedTenantId;
    private DomainType? _cachedDomainType;

    public MultiDomainTenantResolver(
        IHttpContextAccessor httpContextAccessor,
        DirectoryDbContext directoryContext,
        ILogger<MultiDomainTenantResolver> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _directoryContext = directoryContext;
        _logger = logger;
    }

    public string GetTenantId()
    {
        if (_cachedTenantId != null)
            return _cachedTenantId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        // Extract subdomain and determine domain type
        var host = httpContext.Request.Host.Host;
        var (subdomain, domainType) = ParseHost(host);
        
        _cachedDomainType = domainType;

        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenantId = ResolveTenantFromSubdomainAsync(subdomain).Result;
            
            if (!string.IsNullOrEmpty(tenantId))
            {
                _cachedTenantId = tenantId;
                
                // Store domain type in HttpContext for access control
                httpContext.Items["DomainType"] = domainType;
                httpContext.Items["TenantId"] = tenantId;
                
                _logger.LogInformation(
                    "Resolved tenant {TenantId} from subdomain {Subdomain} on domain {DomainType}",
                    tenantId, subdomain, domainType);
                
                return tenantId;
            }
        }

        // Fallback strategies
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            _cachedTenantId = headerValue.ToString();
            return _cachedTenantId;
        }

        var claimTenantId = httpContext.User.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrEmpty(claimTenantId))
        {
            _cachedTenantId = claimTenantId;
            return _cachedTenantId;
        }

        throw new UnauthorizedAccessException("Could not resolve tenant ID");
    }

    public async Task<string> GetTenantIdAsync()
    {
        return await Task.FromResult(GetTenantId());
    }

    /// <summary>
    /// Parses host to extract subdomain and determine domain type
    /// </summary>
    private (string? subdomain, DomainType domainType) ParseHost(string host)
    {
        // Examples:
        // acme.manimp.com → ("acme", MainApp)
        // acme.files.manimp.com → ("acme", FilePortal)
        // acme.docs.manimp.com → ("acme", ClientPortal)
        // localhost:5000 → (null, MainApp)

        if (host.Contains("localhost") || host.Contains("127.0.0.1"))
        {
            return (null, DomainType.MainApp);
        }

        var parts = host.Split('.');
        
        if (parts.Length >= 3)
        {
            var subdomain = parts[0];
            var secondLevel = parts[1];

            var domainType = secondLevel.ToLower() switch
            {
                "files" => DomainType.FilePortal,
                "docs" => DomainType.ClientPortal,
                _ => DomainType.MainApp
            };

            return (subdomain, domainType);
        }

        return (null, DomainType.MainApp);
    }

    private async Task<string?> ResolveTenantFromSubdomainAsync(string subdomain)
    {
        // Try GUID format first
        if (Guid.TryParse(subdomain.Replace("tenant-", ""), out var tenantGuid))
        {
            return tenantGuid.ToString();
        }

        // Try vanity subdomain lookup
        var tenant = await _directoryContext.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);

        if (tenant != null)
        {
            return tenant.TenantId.ToString();
        }

        _logger.LogWarning("Could not resolve tenant from subdomain: {Subdomain}", subdomain);
        return null;
    }

    public DomainType GetDomainType()
    {
        if (_cachedDomainType.HasValue)
            return _cachedDomainType.Value;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.ContainsKey("DomainType") == true)
        {
            return (DomainType)httpContext.Items["DomainType"]!;
        }

        var host = httpContext?.Request.Host.Host ?? "";
        return ParseHost(host).domainType;
    }
}

public enum DomainType
{
    MainApp,       // {tenant}.manimp.com
    FilePortal,    // {tenant}.files.manimp.com
    ClientPortal   // {tenant}.docs.manimp.com
}
```

---

## 📋 Database Migrations

### Migration 1: Add File Sharing Settings

```bash
cd Manimp.Directory
dotnet ef migrations add AddFileSharingSettings
```

```csharp
public partial class AddFileSharingSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FileSharingSettings",
            table: "Tenants",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FileSharingSettings",
            table: "Tenants");
    }
}
```

### Migration 2: Add External Users

```bash
cd Manimp.Data
dotnet ef migrations add AddExternalUsers
```

```csharp
public partial class AddExternalUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExternalUsers",
            columns: table => new
            {
                ExternalUserId = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Email = table.Column<string>(maxLength: 255, nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Company = table.Column<string>(maxLength: 200, nullable: true),
                PasswordHash = table.Column<string>(nullable: true),
                AllowedProjectIds = table.Column<string>(nullable: false), // JSON array
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedUtc = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                LastLoginUtc = table.Column<DateTime>(nullable: true),
                ExpiresUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExternalUsers", x => x.ExternalUserId);
                table.ForeignKey(
                    name: "FK_ExternalUsers_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "TenantId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExternalUsers_TenantId_Email",
            table: "ExternalUsers",
            columns: new[] { "TenantId", "Email" },
            unique: true);
    }
}
```

### Migration 3: Add Project File Visibility

```bash
cd Manimp.Data
dotnet ef migrations add AddProjectFileVisibility
```

```csharp
public partial class AddProjectFileVisibility : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProjectFileVisibilities",
            columns: table => new
            {
                ProjectFileVisibilityId = table.Column<Guid>(nullable: false),
                ProjectId = table.Column<Guid>(nullable: false),
                ResourcePath = table.Column<string>(maxLength: 500, nullable: false),
                IsVisibleToClients = table.Column<bool>(nullable: false, defaultValue: false),
                RequiredPhaseId = table.Column<Guid>(nullable: true),
                MarkedByUserId = table.Column<string>(nullable: true),
                MarkedUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectFileVisibilities", x => x.ProjectFileVisibilityId);
                table.ForeignKey(
                    name: "FK_ProjectFileVisibilities_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "ProjectId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectFileVisibilities_ProjectId_ResourcePath",
            table: "ProjectFileVisibilities",
            columns: new[] { "ProjectId", "ResourcePath" },
            unique: true);
    }
}
```

---

## 🚀 DNS Configuration

### Wildcard DNS Records

| Type | Name | Value | TTL | Purpose |
|------|------|-------|-----|---------|
| `CNAME` | `*.manimp.com` | `app-manimp-prod.azurewebsites.net` | 300 | Main app |
| `CNAME` | `*.files.manimp.com` | `app-manimp-prod.azurewebsites.net` | 300 | File portal |
| `CNAME` | `*.docs.manimp.com` | `app-manimp-prod.azurewebsites.net` | 300 | Client portal |

**Result:**
- `acme.manimp.com` → App Service
- `acme.files.manimp.com` → App Service (same)
- `acme.docs.manimp.com` → App Service (same)
- All resolved by `MultiDomainTenantResolver`

---

## 🔐 SSL Certificates

### Option 1: Azure Managed Certificates (Recommended)

```
1. Azure App Service → Custom Domains
2. Add custom domains:
   - *.manimp.com
   - *.files.manimp.com
   - *.docs.manimp.com
3. Azure App Service → Certificates → Add Managed Certificate
4. Select wildcard domains
5. Azure automatically provisions & renews SSL
6. Done! Free with App Service
```

### Option 2: Azure Key Vault Certificates

```
1. Purchase wildcard cert from DigiCert/GoDaddy
2. Upload to Azure Key Vault
3. Grant App Service access to Key Vault
4. Bind certificate to custom domains
5. Manual renewal required (yearly)
```

### Option 3: Let's Encrypt Wildcard

```bash
# Install certbot
brew install certbot

# Get wildcard certificates
sudo certbot certonly --manual \
  -d "*.manimp.com" \
  -d "*.files.manimp.com" \
  -d "*.docs.manimp.com" \
  --preferred-challenges dns-01

# Follow DNS TXT record instructions
# Upload certificates to Azure App Service
```

---

## 📊 Feature Matrix

| Feature | Main App | File Portal | Client Portal |
|---------|----------|-------------|---------------|
| **URL** | `{tenant}.manimp.com` | `{tenant}.files.manimp.com` | `{tenant}.docs.manimp.com` |
| **Auth Required** | ✅ Yes (employees) | ✅ Yes (employees) | ⚠️ Configurable |
| **User Type** | Internal | Internal | External |
| **Full ERP** | ✅ Yes | ❌ No | ❌ No |
| **File Upload** | ✅ Yes | ✅ Yes | ❌ No |
| **File Download** | ✅ Yes | ✅ Yes | ✅ Yes |
| **All Projects** | ✅ Yes | ✅ Yes | ❌ Assigned only |
| **Phase Filtering** | ✅ Yes | ✅ Yes | ✅ Auto (completed) |
| **Share Links** | ✅ Yes | ✅ Yes | ❌ No |
| **Admin Panel** | ✅ Yes | ✅ Yes | ❌ No |

---

## 🎯 Implementation Checklist

### Phase 1: Database & Models (Week 1)
- [ ] Add `FileSharingSettings` column to Tenants
- [ ] Create `ExternalUser` model and table
- [ ] Create `ProjectFileVisibility` model and table
- [ ] Create `FileShare` model and table
- [ ] Run migrations: `dotnet ef database update`

### Phase 2: Access Control (Week 2)
- [ ] Implement `MultiDomainTenantResolver`
- [ ] Implement `FileAccessControlService`
- [ ] Add domain detection middleware
- [ ] Test role-based sharing permissions

### Phase 3: Admin Panel (Week 3)
- [ ] Create file settings page (`/admin/file-settings`)
- [ ] Create external user management page
- [ ] Create project file marking UI
- [ ] Test settings persistence

### Phase 4: Client Portal (Week 4)
- [ ] Create client portal UI (`{tenant}.docs.manimp.com`)
- [ ] Implement optional login flow
- [ ] Add phase-based filtering
- [ ] Test external user access

### Phase 5: DNS & SSL (Week 5)
- [ ] Create Azure DNS zone
- [ ] Configure wildcard DNS records in Azure
- [ ] Update domain registrar name servers
- [ ] Enable Azure Managed Certificates for wildcard domains
- [ ] Verify SSL binding for all three domain patterns
- [ ] Test all three domains
- [ ] Monitor DNS propagation and logs

### Phase 6: Testing & Launch (Week 6)
- [ ] Test internal file sharing
- [ ] Test external client access (with/without registration)
- [ ] Test role-based limits
- [ ] Test project phase visibility
- [ ] User training & documentation
- [ ] Production deployment

---

## 📈 User Flows

### Flow 1: Admin Configures File Sharing

```
1. Admin logs into acme.manimp.com
2. Goes to Admin → File Settings
3. Enables external client access
4. Requires email registration: ON
5. Sets default share expiry: 30 days
6. Configures role permissions:
   - Admin: Unlimited sharing
   - Manager: 90 days max
   - User: 30 days max
7. Saves settings
```

### Flow 2: Manager Invites External Client

```
1. Manager logs into acme.manimp.com
2. Goes to Clients → Add External User
3. Enters:
   - Email: john@buildco.com
   - Name: John Doe
   - Company: BuildCo
   - Allowed Projects: [Project #789]
   - Expiry: 1 year
4. System sends email to john@buildco.com with:
   - Portal link: acme.docs.manimp.com
   - Temp password
5. John clicks link, logs in, sees only Project #789
```

### Flow 3: Project Manager Marks Files for Clients

```
1. PM logs into acme.manimp.com
2. Goes to Project #789 → Files tab
3. Right-clicks "Phase 1 Drawings" folder
4. Selects "Share with Clients"
5. Dialog:
   - Visible to clients: ✓
   - Required phase: Phase 1 - Planning
   - Phase status: ✅ Completed
6. Clicks "Save"
7. Files now visible on acme.docs.manimp.com for Project #789
```

### Flow 4: External Client Accesses Documents

```
1. John (BuildCo) visits acme.docs.manimp.com
2. Logs in with email + password
3. Sees: "Your Projects"
   - Project #789: Steel Bridge Fabrication
4. Clicks "View Documents"
5. Sees files grouped by phase:
   ✅ Phase 1: Planning (Completed) → 12 files
   ✅ Phase 2: Fabrication (Completed) → 8 files
   🔄 Phase 3: Assembly (In Progress) → Hidden
   ⏳ Phase 4: Delivery (Not Started) → Hidden
6. Clicks on drawing PDF → Opens in browser
7. Downloads files as needed
```

---

## 🎉 Summary

**You now have a complete enterprise-grade multi-domain file storage architecture!**

### ✅ What You Get:

1. **Three Domains:**
   - `{tenant}.manimp.com` - Full ERP for employees
   - `{tenant}.files.manimp.com` - File portal for employees
   - `{tenant}.docs.manimp.com` - Client portal for customers

2. **Role-Based Sharing:**
   - Admin: Unlimited sharing
   - Manager: 90 days max
   - User: 30 days max
   - Configurable per tenant

3. **External Access Control:**
   - Optional email registration
   - Project-based permissions
   - Phase completion gating
   - Automatic expiry

4. **Phase-Based Visibility:**
   - Files hidden until phase completed
   - Clients see only completed work
   - Automatic filtering by project

### 📊 Estimated Effort:

- **Development:** 6 weeks (160 hours)
- **Testing:** 2 weeks (40 hours)
- **Deployment:** 1 week (20 hours)
- **Total:** 9 weeks

### 💰 Cost:

- Azure Blob Storage: $100/month (1TB)
- DNS (Cloudflare): Free
- SSL Certificates: Free (Cloudflare)
- **Total:** ~$100/month

---

**Next Steps:**
1. Review this architecture with your team
2. Confirm domain names available
3. Start with Phase 1 (Database & Models)
4. Let me know when ready to proceed! 🚀
