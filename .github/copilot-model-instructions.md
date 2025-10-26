# GitHub Copilot Model Instructions for SteelAxis

**Project:** SteelAxis - Multi-tenant Steel Fabrication Management System  
**Framework:** Entity Framework Core 8.0  
**Last Updated:** October 27, 2025

---

## 🎯 Model Architecture Overview

### Design Principles
1. **Domain-Driven Design** - Models reflect business domain (EN 1090 compliance, steel fabrication)
2. **Multi-Tenancy** - Every tenant-scoped entity includes `TenantId`
3. **Audit Trail** - All entities track creation/modification metadata
4. **Immutability Support** - Critical compliance records use immutability flags + data hashing
5. **Nullable Reference Types** - Enabled project-wide for null safety
6. **Navigation Properties** - Use for relationship management
7. **Value Objects** - Use for complex types (addresses, measurements)

### Entity Categories
- **Directory Entities** - Central tenant management (stored in Directory database)
- **Tenant Entities** - Business data (stored in per-tenant databases)
- **Authentication Entities** - User identity (Entra External ID integration)
- **Shared Entities** - DTOs, view models, request/response objects

---

## � Development Workflow Requirements

### Planning & Approval Process
**CRITICAL:** Before creating or modifying any entity models:

1. **Create a Detailed Plan**
   - Define all entities to be created/modified
   - Specify properties, data types, and relationships
   - Identify navigation properties and foreign keys
   - List indexes and constraints
   - Plan EF Core configurations (Fluent API)
   - Plan database migrations
   - Present the plan to the user clearly

2. **Get User Confirmation**
   - Wait for explicit user approval before proceeding
   - Do NOT create entities or migrations without confirmation
   - If user requests schema changes, revise and confirm again

3. **Implement the Approved Plan**
   - Create entities following base entity patterns
   - Add EF Core configurations
   - Generate and test migrations
   - Report completion and any issues

### Documentation Requirements
**CRITICAL:** After completing any model work:

1. **Create/Update Feature Documentation Folder**
   - Every feature MUST have a dedicated folder in `/docs/[feature-name]/`
   - Add **DATABASE.md** to the feature folder:
     ```
     docs/
     └── [feature-name]/
         ├── README.md           # Main feature documentation
         ├── PLAN.md             # Implementation plan
         ├── DATABASE.md         # Database schema documentation (add this)
         └── ...
     ```
   - **DATABASE.md** should include:
     - Entity models with property descriptions
     - Table relationships and foreign keys
     - Indexes and constraints
     - Data validation rules
     - Migration scripts or notes
     - Sample data or seed data requirements
     - Multi-tenancy considerations (TenantId usage)
     - EN 1090 compliance fields (if applicable)

2. **Update Project README Files**
   - Document new entities and their purpose
   - Link to feature documentation folder
   - Note any migration requirements
   - Document relationships and constraints
   - Update DTO documentation

3. **Commit Documentation**
   - Commit DATABASE.md updates separately
   - Use clear commit messages: "docs: add database schema for [feature-name]"
   - Ensure all models are properly documented

---

## �📦 Base Entity Pattern

### Standard Base Entity

**Location:** `SteelAxis.Shared/Models/BaseEntity.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Shared.Models;

/// <summary>
/// Base entity with common properties for all tenant-scoped entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the tenant identifier (for multi-tenancy)
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Gets or sets when the entity was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets who created the entity
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string CreatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets when the entity was last modified (UTC)
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// Gets or sets who last modified the entity
    /// </summary>
    [MaxLength(256)]
    public string? ModifiedBy { get; set; }
    
    /// <summary>
    /// Gets or sets whether this entity is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Gets or sets when the entity was deleted (UTC)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets who deleted the entity
    /// </summary>
    [MaxLength(256)]
    public string? DeletedBy { get; set; }
}
```

### Immutable Base Entity (EN 1090 Compliance)

```csharp
namespace SteelAxis.Shared.Models;

/// <summary>
/// Base entity for EN 1090 compliance records that require immutability
/// </summary>
public abstract class ImmutableBaseEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets whether this record is immutable (locked from changes)
    /// </summary>
    public bool IsImmutable { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the SHA-256 hash of critical data for integrity verification
    /// </summary>
    [MaxLength(64)]
    public string? DataHash { get; set; }
    
    /// <summary>
    /// Gets or sets when the record was made immutable
    /// </summary>
    public DateTime? LockedAt { get; set; }
    
    /// <summary>
    /// Gets or sets who locked the record
    /// </summary>
    [MaxLength(256)]
    public string? LockedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the version number for change tracking
    /// </summary>
    public int Version { get; set; } = 1;
}
```

---

## 🏭 EN 1090 Compliance Entities

### Material Certificate

**Purpose:** Track material test certificates (EN 10204 Type 3.1/3.2) for steel traceability

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteelAxis.Shared.Models;

/// <summary>
/// Material certificate for EN 10204 compliance (Type 3.1 or 3.2)
/// </summary>
[Table("MaterialCertificates")]
public class MaterialCertificate : ImmutableBaseEntity
{
    /// <summary>
    /// Lot number or heat number from supplier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LotNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Material grade (e.g., S355, S275)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MaterialGrade { get; set; } = string.Empty;
    
    /// <summary>
    /// Certificate type (3.1 = Mill Certificate, 3.2 = Inspection Certificate)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string CertificateType { get; set; } = string.Empty; // "3.1" or "3.2"
    
    /// <summary>
    /// Supplier name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Supplier { get; set; } = string.Empty;
    
    /// <summary>
    /// Certificate number from supplier
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of material testing
    /// </summary>
    public DateTime TestDate { get; set; }
    
    /// <summary>
    /// Chemical composition as JSON (e.g., {"C": 0.18, "Mn": 1.4, "Si": 0.5})
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string ChemicalComposition { get; set; } = "{}";
    
    /// <summary>
    /// Mechanical properties as JSON (e.g., {"YieldStrength": 355, "TensileStrength": 490})
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string MechanicalProperties { get; set; } = "{}";
    
    /// <summary>
    /// URL to certificate file in Azure Blob Storage
    /// </summary>
    [MaxLength(2048)]
    public string? CertificateFileUrl { get; set; }
    
    /// <summary>
    /// Material thickness in mm (for plates/sheets)
    /// </summary>
    public decimal? Thickness { get; set; }
    
    /// <summary>
    /// Material width in mm (for plates/sheets)
    /// </summary>
    public decimal? Width { get; set; }
    
    /// <summary>
    /// Material length in mm (for plates/sheets)
    /// </summary>
    public decimal? Length { get; set; }
    
    /// <summary>
    /// Quantity received
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Unit of measure (e.g., "kg", "m", "pieces")
    /// </summary>
    [MaxLength(20)]
    public string Unit { get; set; } = "kg";
    
    /// <summary>
    /// Purchase order number
    /// </summary>
    [MaxLength(100)]
    public string? PurchaseOrderNumber { get; set; }
    
    /// <summary>
    /// Delivery date
    /// </summary>
    public DateTime? DeliveryDate { get; set; }
    
    /// <summary>
    /// Notes or remarks
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Projects using this material
    /// </summary>
    public virtual ICollection<ProjectMaterial> ProjectMaterials { get; set; } = new List<ProjectMaterial>();
}
```

### Non-Conformance Report (NCR)

**Purpose:** Track quality issues and corrective actions

```csharp
namespace SteelAxis.Shared.Models;

/// <summary>
/// Non-Conformance Report for quality management
/// </summary>
[Table("NonConformanceReports")]
public class NonConformanceReport : ImmutableBaseEntity
{
    /// <summary>
    /// Unique NCR number (e.g., NCR-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string NCRNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Related project ID
    /// </summary>
    [Required]
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Description of non-conformance
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Severity level
    /// </summary>
    public NCRSeverity Severity { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    public NCRStatus Status { get; set; }
    
    /// <summary>
    /// When the issue was detected
    /// </summary>
    public DateTime DetectedDate { get; set; }
    
    /// <summary>
    /// Who detected the issue
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string DetectedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Area where issue was detected (e.g., "Welding", "Assembly", "Inspection")
    /// </summary>
    [MaxLength(100)]
    public string? DetectionArea { get; set; }
    
    /// <summary>
    /// Root cause analysis
    /// </summary>
    [MaxLength(2000)]
    public string? RootCause { get; set; }
    
    /// <summary>
    /// Corrective action taken
    /// </summary>
    [MaxLength(2000)]
    public string? CorrectiveAction { get; set; }
    
    /// <summary>
    /// Preventive action to avoid recurrence
    /// </summary>
    [MaxLength(2000)]
    public string? PreventiveAction { get; set; }
    
    /// <summary>
    /// Target date for closure
    /// </summary>
    public DateTime? TargetClosureDate { get; set; }
    
    /// <summary>
    /// Actual closure date
    /// </summary>
    public DateTime? ActualClosureDate { get; set; }
    
    /// <summary>
    /// Who verified the closure
    /// </summary>
    [MaxLength(256)]
    public string? VerifiedBy { get; set; }
    
    /// <summary>
    /// Verification notes
    /// </summary>
    [MaxLength(1000)]
    public string? VerificationNotes { get; set; }
    
    /// <summary>
    /// Estimated cost impact
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostImpact { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Related project
    /// </summary>
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }
    
    /// <summary>
    /// Attached files (photos, documents)
    /// </summary>
    public virtual ICollection<NCRAttachment> Attachments { get; set; } = new List<NCRAttachment>();
}

/// <summary>
/// NCR severity levels
/// </summary>
public enum NCRSeverity
{
    Minor = 1,
    Major = 2,
    Critical = 3
}

/// <summary>
/// NCR status workflow
/// </summary>
public enum NCRStatus
{
    Open = 1,
    InProgress = 2,
    UnderReview = 3,
    Closed = 4,
    Rejected = 5
}

/// <summary>
/// Attachment for NCR
/// </summary>
[Table("NCRAttachments")]
public class NCRAttachment : BaseEntity
{
    [Required]
    public Guid NCRId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2048)]
    public string FileUrl { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FileType { get; set; }
    
    public long FileSize { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [ForeignKey(nameof(NCRId))]
    public virtual NonConformanceReport? NCR { get; set; }
}
```

### Welding Procedure Specification (WPS)

```csharp
namespace SteelAxis.Shared.Models;

/// <summary>
/// Welding Procedure Specification for EN 1090 compliance
/// </summary>
[Table("WeldingProcedures")]
public class WeldingProcedureSpecification : ImmutableBaseEntity
{
    /// <summary>
    /// Unique WPS number
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WPSNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Base material 1
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string BaseMaterial1 { get; set; } = string.Empty;
    
    /// <summary>
    /// Base material 2 (for dissimilar welds)
    /// </summary>
    [MaxLength(100)]
    public string? BaseMaterial2 { get; set; }
    
    /// <summary>
    /// Welding process (e.g., "GMAW", "SMAW", "FCAW")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WeldingProcess { get; set; } = string.Empty;
    
    /// <summary>
    /// Filler material specification
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FillerMaterial { get; set; } = string.Empty;
    
    /// <summary>
    /// Shielding gas (for GMAW/GTAW)
    /// </summary>
    [MaxLength(100)]
    public string? ShieldingGas { get; set; }
    
    /// <summary>
    /// Welding position (e.g., "PA", "PB", "PC", "PD")
    /// </summary>
    [MaxLength(20)]
    public string? WeldingPosition { get; set; }
    
    /// <summary>
    /// Preheat temperature (°C)
    /// </summary>
    public int? PreheatTemperature { get; set; }
    
    /// <summary>
    /// Interpass temperature (°C)
    /// </summary>
    public int? InterpassTemperature { get; set; }
    
    /// <summary>
    /// Current range (A)
    /// </summary>
    [MaxLength(50)]
    public string? CurrentRange { get; set; }
    
    /// <summary>
    /// Voltage range (V)
    /// </summary>
    [MaxLength(50)]
    public string? VoltageRange { get; set; }
    
    /// <summary>
    /// Travel speed range (cm/min)
    /// </summary>
    [MaxLength(50)]
    public string? TravelSpeedRange { get; set; }
    
    /// <summary>
    /// WPQR number (supporting qualification record)
    /// </summary>
    [MaxLength(50)]
    public string? WPQRNumber { get; set; }
    
    /// <summary>
    /// Qualified welder name
    /// </summary>
    [MaxLength(256)]
    public string? QualifiedWelder { get; set; }
    
    /// <summary>
    /// Qualification date
    /// </summary>
    public DateTime? QualificationDate { get; set; }
    
    /// <summary>
    /// Expiry date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// URL to WPS document
    /// </summary>
    [MaxLength(2048)]
    public string? DocumentUrl { get; set; }
    
    /// <summary>
    /// Special remarks
    /// </summary>
    [MaxLength(2000)]
    public string? Remarks { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Welds using this WPS
    /// </summary>
    public virtual ICollection<WeldRecord> WeldRecords { get; set; } = new List<WeldRecord>();
}

/// <summary>
/// Individual weld record for traceability
/// </summary>
[Table("WeldRecords")]
public class WeldRecord : ImmutableBaseEntity
{
    [Required]
    public Guid ProjectId { get; set; }
    
    [Required]
    public Guid WPSId { get; set; }
    
    /// <summary>
    /// Weld identification number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string WeldNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Welder name/badge number
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string WelderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of welding
    /// </summary>
    public DateTime WeldDate { get; set; }
    
    /// <summary>
    /// Visual inspection result
    /// </summary>
    public InspectionResult? VisualInspection { get; set; }
    
    /// <summary>
    /// NDT method (e.g., "UT", "RT", "MT", "PT")
    /// </summary>
    [MaxLength(50)]
    public string? NDTMethod { get; set; }
    
    /// <summary>
    /// NDT result
    /// </summary>
    public InspectionResult? NDTResult { get; set; }
    
    /// <summary>
    /// Inspector name
    /// </summary>
    [MaxLength(256)]
    public string? InspectorName { get; set; }
    
    /// <summary>
    /// Inspection date
    /// </summary>
    public DateTime? InspectionDate { get; set; }
    
    /// <summary>
    /// Remarks
    /// </summary>
    [MaxLength(1000)]
    public string? Remarks { get; set; }
    
    // Navigation Properties
    
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }
    
    [ForeignKey(nameof(WPSId))]
    public virtual WeldingProcedureSpecification? WPS { get; set; }
}

public enum InspectionResult
{
    Pending = 0,
    Passed = 1,
    Failed = 2,
    Repaired = 3
}
```

---

## 📊 Project Management Entities

### Project

```csharp
namespace SteelAxis.Shared.Models;

/// <summary>
/// Steel fabrication project
/// </summary>
[Table("Projects")]
public class Project : BaseEntity
{
    /// <summary>
    /// Project number (unique within tenant)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ProjectNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Project name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Client/customer name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>
    /// Project address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    /// <summary>
    /// EN 1090 Execution Class (EXC1, EXC2, EXC3, EXC4)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string ExecutionClass { get; set; } = "EXC2";
    
    /// <summary>
    /// Project status
    /// </summary>
    public ProjectStatus Status { get; set; }
    
    /// <summary>
    /// Start date
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Planned completion date
    /// </summary>
    public DateTime? PlannedCompletionDate { get; set; }
    
    /// <summary>
    /// Actual completion date
    /// </summary>
    public DateTime? ActualCompletionDate { get; set; }
    
    /// <summary>
    /// Contract value
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ContractValue { get; set; }
    
    /// <summary>
    /// Total steel weight (tonnes)
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal? TotalWeight { get; set; }
    
    /// <summary>
    /// Project manager
    /// </summary>
    [MaxLength(256)]
    public string? ProjectManager { get; set; }
    
    /// <summary>
    /// Description
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    // Navigation Properties
    
    public virtual ICollection<ProjectMaterial> Materials { get; set; } = new List<ProjectMaterial>();
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<NonConformanceReport> NCRs { get; set; } = new List<NonConformanceReport>();
    public virtual ICollection<WeldRecord> WeldRecords { get; set; } = new List<WeldRecord>();
    public virtual ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
}

public enum ProjectStatus
{
    Draft = 0,
    Planning = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>
/// Link between projects and material certificates
/// </summary>
[Table("ProjectMaterials")]
public class ProjectMaterial : BaseEntity
{
    [Required]
    public Guid ProjectId { get; set; }
    
    [Required]
    public Guid MaterialCertificateId { get; set; }
    
    /// <summary>
    /// Quantity used from this certificate
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal QuantityUsed { get; set; }
    
    /// <summary>
    /// Component/assembly where used
    /// </summary>
    [MaxLength(200)]
    public string? ComponentReference { get; set; }
    
    // Navigation Properties
    
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }
    
    [ForeignKey(nameof(MaterialCertificateId))]
    public virtual MaterialCertificate? MaterialCertificate { get; set; }
}
```

### Project Task

```csharp
namespace SteelAxis.Shared.Models;

/// <summary>
/// Task/activity within a project
/// </summary>
[Table("ProjectTasks")]
public class ProjectTask : BaseEntity
{
    [Required]
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Task name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Task status
    /// </summary>
    public TaskStatus Status { get; set; }
    
    /// <summary>
    /// Planned start date
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }
    
    /// <summary>
    /// Planned end date
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }
    
    /// <summary>
    /// Actual start date
    /// </summary>
    public DateTime? ActualStartDate { get; set; }
    
    /// <summary>
    /// Actual end date
    /// </summary>
    public DateTime? ActualEndDate { get; set; }
    
    /// <summary>
    /// Assigned to
    /// </summary>
    [MaxLength(256)]
    public string? AssignedTo { get; set; }
    
    /// <summary>
    /// Percentage complete (0-100)
    /// </summary>
    public int PercentComplete { get; set; } = 0;
    
    /// <summary>
    /// Parent task ID (for subtasks)
    /// </summary>
    public Guid? ParentTaskId { get; set; }
    
    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }
    
    // Navigation Properties
    
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }
    
    [ForeignKey(nameof(ParentTaskId))]
    public virtual ProjectTask? ParentTask { get; set; }
    
    public virtual ICollection<ProjectTask> SubTasks { get; set; } = new List<ProjectTask>();
}

public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Cancelled = 4
}
```

---

## 🔐 Directory & Tenant Entities

### Tenant (Directory Database)

**Location:** `SteelAxis.Directory/Entities/Tenant.cs`

```csharp
namespace SteelAxis.Directory.Entities;

/// <summary>
/// Tenant entity stored in central directory database
/// </summary>
[Table("Tenants")]
public class Tenant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Tenant name (company name)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique tenant identifier (subdomain or URL slug)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TenantIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection string to tenant database (encrypted)
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Subscription tier (Basic, Professional, Enterprise)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SubscriptionTier { get; set; } = "Basic";
    
    /// <summary>
    /// Is tenant active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Maximum users allowed
    /// </summary>
    public int MaxUsers { get; set; } = 10;
    
    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Azure Blob Storage container name for this tenant
    /// </summary>
    [MaxLength(200)]
    public string? StorageContainerName { get; set; }
}
```

---

## 📋 Data Transfer Objects (DTOs)

### DTO Pattern

```csharp
namespace SteelAxis.Shared.DTOs;

/// <summary>
/// DTO for material certificate list view
/// </summary>
public record MaterialCertificateDto
{
    public Guid Id { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public string MaterialGrade { get; init; } = string.Empty;
    public string CertificateType { get; init; } = string.Empty;
    public string Supplier { get; init; } = string.Empty;
    public DateTime TestDate { get; init; }
    public bool IsImmutable { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating material certificate
/// </summary>
public record CreateMaterialCertificateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string LotNumber { get; init; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^(S235|S275|S355|S420|S460)$")]
    public string MaterialGrade { get; init; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^(3\.1|3\.2)$")]
    public string CertificateType { get; init; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Supplier { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CertificateNumber { get; init; } = string.Empty;
    
    [Required]
    public DateTime TestDate { get; init; }
    
    public decimal Quantity { get; init; }
    
    public string? Notes { get; init; }
}
```

---

## 🎯 Entity Framework Configuration

### DbContext Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using SteelAxis.Shared.Models;

namespace SteelAxis.Data;

public class TenantDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }
    
    // DbSets
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<MaterialCertificate> MaterialCertificates => Set<MaterialCertificate>();
    public DbSet<NonConformanceReport> NonConformanceReports => Set<NonConformanceReport>();
    public DbSet<WeldingProcedureSpecification> WeldingProcedures => Set<WeldingProcedureSpecification>();
    public DbSet<WeldRecord> WeldRecords => Set<WeldRecord>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        
        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }
    
    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }
}
```

### Entity Configuration Example

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteelAxis.Shared.Models;

namespace SteelAxis.Data.Configurations;

/// <summary>
/// EF Core configuration for MaterialCertificate entity
/// </summary>
public class MaterialCertificateConfiguration : IEntityTypeConfiguration<MaterialCertificate>
{
    public void Configure(EntityTypeBuilder<MaterialCertificate> builder)
    {
        // Table name
        builder.ToTable("MaterialCertificates");
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Indexes
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.LotNumber);
        builder.HasIndex(e => new { e.TenantId, e.LotNumber });
        
        // Required properties
        builder.Property(e => e.LotNumber).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MaterialGrade).IsRequired().HasMaxLength(50);
        builder.Property(e => e.CertificateType).IsRequired().HasMaxLength(10);
        
        // Precision for decimals
        builder.Property(e => e.Quantity).HasPrecision(18, 3);
        builder.Property(e => e.Thickness).HasPrecision(10, 2);
        
        // JSON columns
        builder.Property(e => e.ChemicalComposition).HasColumnType("nvarchar(max)");
        builder.Property(e => e.MechanicalProperties).HasColumnType("nvarchar(max)");
        
        // Relationships
        builder.HasMany(e => e.ProjectMaterials)
            .WithOne(pm => pm.MaterialCertificate)
            .HasForeignKey(pm => pm.MaterialCertificateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## ✅ Validation & Business Rules

### Data Annotations

```csharp
// Always use data annotations for basic validation
[Required(ErrorMessage = "Lot number is required")]
[StringLength(100, MinimumLength = 3, ErrorMessage = "Lot number must be 3-100 characters")]
public string LotNumber { get; set; } = string.Empty;

[Range(0.01, 999999.99, ErrorMessage = "Quantity must be greater than 0")]
public decimal Quantity { get; set; }

[RegularExpression(@"^NCR-\d{4}-\d{3}$", ErrorMessage = "NCR number format: NCR-YYYY-NNN")]
public string NCRNumber { get; set; } = string.Empty;

[EmailAddress(ErrorMessage = "Invalid email format")]
public string? ContactEmail { get; set; }

[Url(ErrorMessage = "Invalid URL format")]
public string? WebsiteUrl { get; set; }
```

---

## 🎯 Code Generation Checklist

When generating entity models, always include:

1. ✅ **Inherit from BaseEntity or ImmutableBaseEntity** - For audit trail and multi-tenancy
2. ✅ **XML documentation** - For all public properties
3. ✅ **Data annotations** - Required, MaxLength, validation attributes
4. ✅ **Navigation properties** - Virtual for lazy loading support
5. ✅ **Table attribute** - Explicit table name
6. ✅ **Proper foreign keys** - Include FK properties and ForeignKey attribute
7. ✅ **Nullable reference types** - Use `?` for optional properties
8. ✅ **Default values** - Initialize collections, set sensible defaults
9. ✅ **Enum types** - Use for status fields, severity levels
10. ✅ **Precision for decimals** - Specify Column(TypeName) or configure in Fluent API

---

**Remember:** All tenant-scoped entities MUST include `TenantId` and inherit from `BaseEntity`. EN 1090 compliance records MUST inherit from `ImmutableBaseEntity` and support data hashing!
