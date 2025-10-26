# EN 1090 Data Hashing Implementation Plan

**Document Version:** 1.0  
**Date:** October 9, 2025  
**Status:** Planning Phase  
**Purpose:** Secure sensitive EN 1090 compliance data through cryptographic hashing

---

## Executive Summary

This document outlines a comprehensive plan for implementing cryptographic hashing of sensitive data within EN 1090 workflows. The goal is to protect proprietary manufacturing data, supplier information, and material traceability details while maintaining compliance verification capabilities.

### Key Objectives

1. **Protect Proprietary Data**: Hash sensitive manufacturing parameters, supplier pricing, and chemical compositions
2. **Maintain Compliance**: Ensure EN 1090 audit trails remain verifiable without exposing sensitive details
3. **Enable Verification**: Support proof-of-compliance without revealing underlying proprietary information
4. **Secure Multi-Tenant**: Prevent cross-tenant data correlation through proper salting strategies

---

## Table of Contents

1. [Data Classification](#data-classification)
2. [Hashing Strategy](#hashing-strategy)
3. [Data Retention & Immutability](#data-retention--immutability)
4. [Implementation Architecture](#implementation-architecture)
5. [Data-by-Domain Analysis](#data-by-domain-analysis)
6. [Security Considerations](#security-considerations)
7. [Database Schema Changes](#database-schema-changes)
8. [API Changes](#api-changes)
9. [Migration Strategy](#migration-strategy)
10. [Testing & Validation](#testing--validation)
11. [Compliance Impact](#compliance-impact)

---

## Data Classification

### Category 1: MUST HASH - Highly Sensitive Proprietary Data

**Material Chemical Composition** (`MaterialCertificate.ChemicalComposition`)
- Contains exact alloy formulas (trade secrets)
- Currently stored as JSON: `{"C": 0.18, "Mn": 1.45, "Si": 0.30, "P": 0.025, "S": 0.015}`
- **Risk**: Competitors could reverse-engineer proprietary steel grades
- **Hash Approach**: SHA-256 hash stored alongside original for verification

**Welding Parameters** (WeldingRecord)
- Heat input, travel speed, current/voltage combinations
- Fields: `HeatInput`, `WeldingCurrent`, `WeldingVoltage`, `TravelSpeed`
- **Risk**: Proprietary welding techniques could be copied
- **Hash Approach**: Composite hash of parameter set for verification

**Supplier Pricing** (PurchaseOrder, PriceQuote)
- Unit costs, total pricing, discounts
- Fields: `UnitPrice`, `TotalPrice`, `DiscountAmount`
- **Risk**: Business intelligence for competitors, contractual violations
- **Hash Approach**: SHA-256 with tenant-specific salt

**Consumable Batches** (WeldingRecord.ConsumableBatch)
- Proprietary filler material formulations
- **Risk**: Reverse engineering of welding consumable specs
- **Hash Approach**: SHA-256 hash for traceability without disclosure

### Category 2: CONSIDER HASHING - Moderately Sensitive

**Heat/Batch Numbers** (`ProfileInventory.MaterialBatch`, `MaterialCertificate.HeatNumber`)
- **Current Status**: Plain text required for EN 1090 traceability
- **Risk**: Could reveal supplier relationships and material sources
- **Approach**: Store plain text for compliance, add hash for anti-tampering
- **Note**: EN 1090 auditors MUST be able to verify actual heat numbers

**Supplier Names** (`Supplier.Name`)
- **Risk**: Reveals supply chain partners
- **Approach**: Display hash in public APIs, show real names only to authorized users
- **Note**: Required for EN 10204 certificates (cannot fully hash)

**Welder IDs** (WelderQualification.WelderId, WelderQualification.WelderName)
- **Risk**: Personal data (GDPR), could reveal workforce capabilities
- **Approach**: Hash for public reports, store plain for compliance records

**Mechanical Properties** (`MaterialCertificate.MechanicalProperties`)
- Yield strength, tensile strength, elongation, impact values
- **Risk**: Less critical than chemistry but still proprietary
- **Approach**: Hash for verification, store plain for quality control

### Category 3: DO NOT HASH - Compliance-Critical Plain Text

**Certificate Numbers** (`MaterialCertificate.CertificateNumber`)
- **Reason**: Required for EN 10204 certificate verification
- **Mitigation**: Access control only, audit logging

**Execution Class** (`ExecutionClass.ClassLevel`)
- **Reason**: Core compliance requirement, must be verifiable by authorities
- **Mitigation**: Read-only after initial determination

**NDT Results** (`NDTRecord.TestResult`)
- **Reason**: Safety-critical, must be human-readable for inspectors
- **Mitigation**: Immutable records with tamper-proof signatures

**Quality Inspection Results** (`QualityInspectionRecord.Result`)
- **Reason**: Required for FPC (Factory Production Control) audits
- **Mitigation**: Digital signatures instead of hashing

**WPS/WPQR Numbers** (`WeldingProcedure.WPSNumber`)
- **Reason**: Referenced in contracts and specifications
- **Mitigation**: Version control and access restrictions

---

## Hashing Strategy

### Algorithm Selection

**Primary Algorithm: SHA-256**
- Industry standard for data integrity
- FIPS 140-2 compliant
- 256-bit output (32 bytes) balances security and storage
- Supported natively in .NET via `System.Security.Cryptography`

**Alternative: HMAC-SHA256 (for keyed verification)**
- Use when verification requires secret key
- Prevents rainbow table attacks
- Key derivation from tenant ID + master secret

### Salting Strategy

**Tenant-Specific Salts**
```csharp
// Derived from tenant ID + global secret
string salt = DeriveKeyFromTenant(tenantId, globalSecret);
byte[] hash = SHA256(data + salt);
```

**Benefits**:
- Prevents cross-tenant correlation
- Same data in different tenants = different hashes
- No additional storage overhead (salt derived on-the-fly)

**Per-Record Salts (Optional Enhanced Security)**
```csharp
// For highest security fields (chemical composition)
string recordSalt = GenerateRandomSalt();
byte[] hash = SHA256(data + recordSalt + tenantSalt);
// Store recordSalt alongside hash
```

### Hash Storage Format

**Database Schema Pattern**:
```csharp
public class SecureData
{
    // Original data (encrypted or null if fully hashed)
    public string? PlainText { get; set; }
    
    // SHA-256 hash (hex string, 64 chars)
    [MaxLength(64)]
    public string Hash { get; set; }
    
    // Optional per-record salt (base64, ~24 chars)
    [MaxLength(50)]
    public string? Salt { get; set; }
    
    // Hash algorithm identifier (for future upgrades)
    [MaxLength(20)]
    public string HashAlgorithm { get; set; } = "SHA256";
    
    // Timestamp for audit trail
    public DateTime HashedUtc { get; set; }
}
```

---

## Data Retention & Immutability

### EN 1090 Compliance Requirements

**Critical Principle**: Once EN 1090 compliance data is recorded and verified, it **MUST NOT be edited or deleted**. This is a fundamental requirement for:
- Factory Production Control (FPC) audits
- CE marking traceability
- Legal liability protection
- Regulatory compliance verification

### Immutability Strategy

#### 1. Database-Level Immutability

**Approach**: Soft deletes + versioning + hash chain integrity

**Core Pattern**:
```csharp
public abstract class ImmutableEN1090Record
{
    // Primary key
    public Guid RecordId { get; set; } = Guid.NewGuid();
    
    // Immutability tracking
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public bool IsFinalized { get; set; } = false;
    public DateTime? FinalizedUtc { get; set; }
    public string? FinalizedByUserId { get; set; }
    
    // Hash of entire record for tamper detection
    public string RecordHash { get; set; } = string.Empty;
    
    // Soft delete (for corrections only, original retained)
    public bool IsSuperseded { get; set; } = false;
    public Guid? SupersededByRecordId { get; set; }
    public string? SupersessionReason { get; set; }
    public DateTime? SupersededUtc { get; set; }
    
    // Optimistic concurrency (prevents accidental overwrites)
    public byte[] RowVersion { get; set; } = [];
}
```

**Tables Requiring Immutability** (EN 1090 Critical):
- `MaterialCertificate` - Material traceability records
- `WeldingRecord` - Welding execution records
- `WelderQualification` - Welder certifications
- `NDTRecord` - Non-destructive testing results
- `QualityInspectionRecord` - Quality control inspections
- `NonConformanceReport` - NCR tracking
- `ExecutionClass` - Project execution class determination
- `WeldingProcedure` (WPQR) - Welding procedure qualifications

#### 2. Finalization Workflow

**State Machine**:
```
Draft → Verified → Finalized (IMMUTABLE)
  ↓        ↓
Edit     Edit
Allowed  Allowed
         (with approval)

After Finalization:
- No edits allowed
- No deletions allowed
- Corrections via supersession only
```

**Implementation**:
```csharp
public async Task<Result> FinalizeWeldingRecord(Guid recordId, string userId)
{
    var record = await _context.WeldingRecords.FindAsync(recordId);
    
    if (record == null)
        return Result.Fail("Record not found");
    
    if (record.IsFinalized)
        return Result.Fail("Record is already finalized and cannot be modified");
    
    // Validate record completeness
    var validation = ValidateWeldingRecordComplete(record);
    if (!validation.IsValid)
        return Result.Fail($"Record incomplete: {validation.Errors}");
    
    // Generate immutable hash of entire record
    record.RecordHash = _hashingService.HashObject(record);
    record.IsFinalized = true;
    record.FinalizedUtc = DateTime.UtcNow;
    record.FinalizedByUserId = userId;
    
    // Log finalization event
    await _auditService.LogAsync(new AuditEntry
    {
        Action = "RecordFinalized",
        EntityType = "WeldingRecord",
        EntityId = recordId.ToString(),
        UserId = userId,
        Details = $"Record finalized and made immutable. Hash: {record.RecordHash}"
    });
    
    await _context.SaveChangesAsync();
    
    return Result.Success();
}
```

#### 3. Edit Prevention Mechanisms

**Application-Level Guards**:
```csharp
public class ImmutableRecordInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return result;
        
        // Check for modifications to finalized records
        var modifiedFinalizedRecords = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified)
            .Where(e => e.Entity is ImmutableEN1090Record)
            .Where(e => ((ImmutableEN1090Record)e.Entity).IsFinalized)
            .ToList();
        
        if (modifiedFinalizedRecords.Any())
        {
            throw new ImmutableRecordViolationException(
                "Cannot modify finalized EN 1090 compliance records. " +
                "Use supersession workflow for corrections."
            );
        }
        
        return result;
    }
}

// Register in Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(new ImmutableRecordInterceptor());
});
```

**Database-Level Constraints** (SQL Server):
```sql
-- Create trigger to prevent updates to finalized records
CREATE TRIGGER TR_WeldingRecord_PreventFinalizedUpdate
ON WeldingRecord
INSTEAD OF UPDATE
AS
BEGIN
    -- Check if any finalized records are being updated
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        INNER JOIN deleted d ON i.WeldingRecordId = d.WeldingRecordId
        WHERE d.IsFinalized = 1
    )
    BEGIN
        RAISERROR('Cannot modify finalized welding records. Use supersession workflow.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    -- Allow updates to non-finalized records
    UPDATE t
    SET t.WeldId = i.WeldId,
        t.WeldingCurrent = i.WeldingCurrent,
        -- ... other fields
    FROM WeldingRecord t
    INNER JOIN inserted i ON t.WeldingRecordId = i.WeldingRecordId
    WHERE t.IsFinalized = 0;
END;
GO

-- Similar triggers for all immutable tables
CREATE TRIGGER TR_MaterialCertificate_PreventFinalizedUpdate ON MaterialCertificate INSTEAD OF UPDATE AS ...
CREATE TRIGGER TR_NDTRecord_PreventFinalizedUpdate ON NDTRecord INSTEAD OF UPDATE AS ...
CREATE TRIGGER TR_QualityInspectionRecord_PreventFinalizedUpdate ON QualityInspectionRecord INSTEAD OF UPDATE AS ...
```

#### 4. Supersession Workflow (Corrections)

**When Corrections Are Needed**:
```csharp
public async Task<Result<Guid>> SupersedeWeldingRecord(
    Guid originalRecordId,
    WeldingRecord correctedRecord,
    string reason,
    string userId)
{
    var original = await _context.WeldingRecords.FindAsync(originalRecordId);
    
    if (original == null)
        return Result<Guid>.Fail("Original record not found");
    
    if (!original.IsFinalized)
        return Result<Guid>.Fail("Only finalized records can be superseded");
    
    if (string.IsNullOrWhiteSpace(reason))
        return Result<Guid>.Fail("Supersession reason is required");
    
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Mark original as superseded (NOT deleted)
        original.IsSuperseded = true;
        original.SupersededByRecordId = correctedRecord.WeldingRecordId;
        original.SupersessionReason = reason;
        original.SupersededUtc = DateTime.UtcNow;
        
        // Create new record with corrected data
        correctedRecord.WeldingRecordId = Guid.NewGuid();
        correctedRecord.CreatedUtc = DateTime.UtcNow;
        correctedRecord.CreatedByUserId = userId;
        correctedRecord.Notes = $"Supersedes {originalRecordId}. Reason: {reason}";
        
        _context.WeldingRecords.Add(correctedRecord);
        
        // Create supersession audit trail
        await _auditService.LogAsync(new AuditEntry
        {
            Action = "RecordSuperseded",
            EntityType = "WeldingRecord",
            EntityId = originalRecordId.ToString(),
            UserId = userId,
            Details = $"Record superseded by {correctedRecord.WeldingRecordId}. Reason: {reason}"
        });
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return Result<Guid>.Success(correctedRecord.WeldingRecordId);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Result<Guid>.Fail($"Supersession failed: {ex.Message}");
    }
}
```

**Supersession Chain Tracking**:
```sql
-- View to see complete correction history
CREATE VIEW vw_WeldingRecordHistory AS
WITH RecordChain AS (
    -- Get all records
    SELECT 
        WeldingRecordId,
        WeldId,
        WeldingDate,
        IsFinalized,
        IsSuperseded,
        SupersededByRecordId,
        SupersessionReason,
        CreatedUtc,
        0 AS ChainDepth
    FROM WeldingRecord
    WHERE SupersededByRecordId IS NULL -- Current records
    
    UNION ALL
    
    -- Recursively get superseded records
    SELECT 
        w.WeldingRecordId,
        w.WeldId,
        w.WeldingDate,
        w.IsFinalized,
        w.IsSuperseded,
        w.SupersededByRecordId,
        w.SupersessionReason,
        w.CreatedUtc,
        rc.ChainDepth + 1
    FROM WeldingRecord w
    INNER JOIN RecordChain rc ON w.WeldingRecordId = rc.SupersededByRecordId
)
SELECT * FROM RecordChain;
```

### Data Retention Policies

#### Retention Requirements by Data Type

**Permanent Retention (Indefinite)**:
- Material certificates and traceability
- Welding procedure specifications (WPS/WPQR)
- Welder qualifications
- Execution class determinations
- Declaration of Performance (DoP) records
- CE marking documentation

**10-Year Minimum Retention** (EN 1090 Standard):
- Welding records
- NDT inspection records
- Quality control inspection records
- Non-conformance reports (NCRs)
- Audit records
- Material genealogy/lineage

**5-Year Retention**:
- Purchase orders (after project completion)
- Supplier quotations
- Procurement documentation

**Retention Implementation**:
```csharp
public class RetentionPolicy
{
    public string EntityType { get; set; } = string.Empty;
    public int RetentionYears { get; set; } // -1 = Permanent
    public bool AutoArchive { get; set; } = true;
    public DateTime CalculateExpiryDate { get; set; }
}

public static class EN1090RetentionPolicies
{
    public static readonly Dictionary<string, RetentionPolicy> Policies = new()
    {
        ["MaterialCertificate"] = new() { RetentionYears = -1 }, // Permanent
        ["WeldingProcedure"] = new() { RetentionYears = -1 },    // Permanent
        ["WeldingRecord"] = new() { RetentionYears = 10 },
        ["NDTRecord"] = new() { RetentionYears = 10 },
        ["QualityInspectionRecord"] = new() { RetentionYears = 10 },
        ["NonConformanceReport"] = new() { RetentionYears = 10 },
        ["PurchaseOrder"] = new() { RetentionYears = 5 },
        ["PriceQuote"] = new() { RetentionYears = 5 }
    };
}
```

#### Archival Strategy

**3-Tier Storage Architecture**:

1. **Hot Storage** (Active Database - 0-2 years)
   - Frequently accessed records
   - Full-text search enabled
   - Optimized indexes
   - Standard query performance

2. **Warm Storage** (Archive Database - 2-10 years)
   - Accessed occasionally for audits
   - Compressed storage
   - Slower but acceptable query times
   - Cost-optimized tier

3. **Cold Storage** (Blob Storage - 10+ years / Permanent)
   - Rarely accessed, long-term retention
   - Azure Blob Storage (Cool/Archive tier)
   - Encrypted at rest
   - Legal hold capabilities

**Archival Process**:
```csharp
public class ArchivalService : IArchivalService
{
    private readonly AppDbContext _context;
    private readonly BlobServiceClient _blobClient;
    private readonly ILogger<ArchivalService> _logger;
    
    public async Task ArchiveRecordsAsync(string entityType, DateTime cutoffDate)
    {
        var policy = EN1090RetentionPolicies.Policies[entityType];
        
        if (policy.RetentionYears == -1)
            return; // Permanent records not archived
        
        // Query records older than cutoff
        var recordsToArchive = entityType switch
        {
            "WeldingRecord" => await _context.WeldingRecords
                .Where(w => w.CreatedUtc < cutoffDate && w.IsFinalized)
                .ToListAsync(),
            "NDTRecord" => await _context.NDTRecords
                .Where(n => n.TestingDate < cutoffDate)
                .ToListAsync(),
            _ => throw new NotSupportedException($"Archival not configured for {entityType}")
        };
        
        foreach (var record in recordsToArchive)
        {
            // Serialize record to JSON with full fidelity
            var json = JsonSerializer.Serialize(record, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            });
            
            // Upload to blob storage
            var containerClient = _blobClient.GetBlobContainerClient("en1090-archive");
            var blobName = $"{entityType}/{record.RecordId}_{DateTime.UtcNow:yyyyMMdd}.json";
            var blobClient = containerClient.GetBlobClient(blobName);
            
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await blobClient.UploadAsync(stream, overwrite: false);
            
            // Set blob metadata
            await blobClient.SetMetadataAsync(new Dictionary<string, string>
            {
                ["EntityType"] = entityType,
                ["RecordId"] = record.RecordId.ToString(),
                ["ArchivedDate"] = DateTime.UtcNow.ToString("O"),
                ["RetentionPolicy"] = policy.RetentionYears.ToString(),
                ["RecordHash"] = _hashingService.HashObject(record)
            });
            
            // Mark record as archived in database (don't delete)
            record.IsArchived = true;
            record.ArchivedUtc = DateTime.UtcNow;
            record.ArchiveBlobUri = blobClient.Uri.ToString();
            
            _logger.LogInformation(
                "Archived {EntityType} record {RecordId} to {BlobUri}",
                entityType, record.RecordId, blobClient.Uri);
        }
        
        await _context.SaveChangesAsync();
    }
}
```

**Scheduled Archival Job**:
```csharp
public class ArchivalBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddYears(-2); // Archive records older than 2 years
                
                await _archivalService.ArchiveRecordsAsync("WeldingRecord", cutoffDate);
                await _archivalService.ArchiveRecordsAsync("NDTRecord", cutoffDate);
                await _archivalService.ArchiveRecordsAsync("QualityInspectionRecord", cutoffDate);
                
                _logger.LogInformation("Archival process completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Archival process failed");
            }
            
            // Run monthly
            await Task.Delay(TimeSpan.FromDays(30), stoppingToken);
        }
    }
}
```

#### Retrieval from Archive

**On-Demand Retrieval API**:
```csharp
[HttpGet("archived/{entityType}/{recordId}")]
[Authorize(Roles = "Admin,Auditor")]
public async Task<IActionResult> GetArchivedRecord(string entityType, Guid recordId)
{
    // Check if record is archived
    var metadata = await _context.Set<ArchiveMetadata>()
        .FirstOrDefaultAsync(m => m.EntityType == entityType && m.RecordId == recordId);
    
    if (metadata == null)
        return NotFound();
    
    if (!metadata.IsArchived)
        return BadRequest("Record is not archived");
    
    // Retrieve from blob storage
    var blobClient = new BlobClient(new Uri(metadata.ArchiveBlobUri));
    var response = await blobClient.DownloadContentAsync();
    
    var json = response.Value.Content.ToString();
    var record = JsonSerializer.Deserialize(json, Type.GetType(entityType));
    
    // Verify integrity via hash
    var storedHash = (await blobClient.GetPropertiesAsync()).Value.Metadata["RecordHash"];
    var currentHash = _hashingService.HashObject(record);
    
    if (storedHash != currentHash)
    {
        _logger.LogError("Archive integrity violation: Hash mismatch for {RecordId}", recordId);
        return StatusCode(500, "Archive integrity check failed");
    }
    
    // Log retrieval for audit
    await _auditService.LogAsync(new AuditEntry
    {
        Action = "ArchivedRecordRetrieved",
        EntityType = entityType,
        EntityId = recordId.ToString(),
        UserId = User.Identity?.Name
    });
    
    return Ok(record);
}
```

### Chain of Custody Tracking

**Blockchain-Style Hash Chaining** (for critical records):
```csharp
public class HashChain
{
    public Guid ChainId { get; set; }
    public int TenantId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    
    // Hash of current record
    public string CurrentHash { get; set; } = string.Empty;
    
    // Hash of previous record in chain (blockchain pattern)
    public string? PreviousHash { get; set; }
    
    // Hash of CurrentHash + PreviousHash (chain integrity)
    public string ChainHash { get; set; } = string.Empty;
    
    public DateTime ChainedUtc { get; set; }
    public string ChainedByUserId { get; set; } = string.Empty;
    
    // Sequence number in chain
    public long SequenceNumber { get; set; }
}

public async Task AddToHashChain(string entityType, Guid recordId, object record)
{
    var currentHash = _hashingService.HashObject(record);
    
    // Get previous hash in chain
    var previousChain = await _context.HashChains
        .Where(h => h.EntityType == entityType)
        .OrderByDescending(h => h.SequenceNumber)
        .FirstOrDefaultAsync();
    
    var chainEntry = new HashChain
    {
        TenantId = _tenantContext.TenantId,
        EntityType = entityType,
        RecordId = recordId,
        CurrentHash = currentHash,
        PreviousHash = previousChain?.ChainHash,
        SequenceNumber = (previousChain?.SequenceNumber ?? 0) + 1,
        ChainedUtc = DateTime.UtcNow,
        ChainedByUserId = _currentUser.UserId
    };
    
    // Create chain hash (like blockchain)
    chainEntry.ChainHash = _hashingService.HashComposite(
        currentHash,
        chainEntry.PreviousHash ?? "",
        chainEntry.SequenceNumber
    );
    
    _context.HashChains.Add(chainEntry);
    await _context.SaveChangesAsync();
}

// Verify entire chain integrity
public async Task<bool> VerifyHashChainIntegrity(string entityType)
{
    var chain = await _context.HashChains
        .Where(h => h.EntityType == entityType)
        .OrderBy(h => h.SequenceNumber)
        .ToListAsync();
    
    for (int i = 1; i < chain.Count; i++)
    {
        var expected = _hashingService.HashComposite(
            chain[i].CurrentHash,
            chain[i-1].ChainHash,
            chain[i].SequenceNumber
        );
        
        if (chain[i].ChainHash != expected)
        {
            _logger.LogError(
                "Hash chain integrity violation at sequence {Seq} for {Type}",
                chain[i].SequenceNumber,
                entityType);
            return false;
        }
    }
    
    return true;
}
```

### UI Indicators & User Experience

**Visual Indicators in UI**:
```cshtml
<!-- Finalized record badge -->
@if (record.IsFinalized)
{
    <MudChip Color="Color.Success" Icon="@Icons.Material.Filled.Lock" Size="Size.Small">
        Finalized - Cannot Edit
    </MudChip>
    <MudText Typo="Typo.caption" Color="Color.Secondary">
        Finalized on @record.FinalizedUtc?.ToString("yyyy-MM-dd HH:mm") by @record.FinalizedByUserName
    </MudText>
}

<!-- Superseded record warning -->
@if (record.IsSuperseded)
{
    <MudAlert Severity="Severity.Warning" Dense="true">
        This record has been superseded by a correction. 
        <MudLink Href="@($"/welding-records/{record.SupersededByRecordId}")">
            View current record
        </MudLink>
        <br/>
        <strong>Reason:</strong> @record.SupersessionReason
    </MudAlert>
}

<!-- Edit button disabled for finalized records -->
<MudButton 
    StartIcon="@Icons.Material.Filled.Edit" 
    Color="Color.Primary"
    Disabled="@record.IsFinalized"
    OnClick="@(() => EditRecord(record))">
    Edit
</MudButton>

@if (record.IsFinalized)
{
    <MudButton 
        StartIcon="@Icons.Material.Filled.ContentCopy" 
        Color="Color.Secondary"
        OnClick="@(() => SupersedeRecord(record))">
        Create Correction (Supersede)
    </MudButton>
}
```

**Finalization Confirmation Dialog**:
```cshtml
<MudDialog>
    <DialogContent>
        <MudAlert Severity="Severity.Warning" Dense="false">
            <MudText Typo="Typo.h6">⚠️ Warning: This action is irreversible</MudText>
            <MudText>
                You are about to finalize this @entityType record. Once finalized:
            </MudText>
            <ul>
                <li>The record <strong>cannot be edited</strong></li>
                <li>The record <strong>cannot be deleted</strong></li>
                <li>Corrections require creating a superseding record</li>
                <li>A cryptographic hash will be generated for integrity verification</li>
                <li>This action will be logged in the audit trail</li>
            </ul>
        </MudAlert>
        
        <MudTextField @bind-Value="finalizationNotes"
                      Label="Finalization Notes (Optional)"
                      Lines="3"
                      Variant="Variant.Outlined"
                      Class="mt-4" />
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Error" Variant="Variant.Filled" OnClick="ConfirmFinalize">
            Finalize Record
        </MudButton>
    </DialogActions>
</MudDialog>
```

---

## Implementation Architecture

### Service Layer Components

**1. IHashingService Interface**
```csharp
namespace Manimp.Services.Interfaces;

public interface IHashingService
{
    // Basic hashing
    string HashData(string data, string? salt = null);
    bool VerifyHash(string data, string hash, string? salt = null);
    
    // Tenant-scoped hashing
    string HashWithTenantSalt(int tenantId, string data);
    bool VerifyWithTenantSalt(int tenantId, string data, string hash);
    
    // Complex object hashing
    string HashObject<T>(T obj, string? salt = null);
    bool VerifyObject<T>(T obj, string hash, string? salt = null);
    
    // Composite field hashing (for multi-field verification)
    string HashComposite(params object[] fields);
    
    // Key derivation
    string DeriveTenantSalt(int tenantId);
}
```

**2. HashingService Implementation**
```csharp
public class HashingService : IHashingService
{
    private readonly IConfiguration _configuration;
    private readonly string _masterSecret;
    
    public HashingService(IConfiguration configuration)
    {
        _configuration = configuration;
        // Load from secure config (Azure Key Vault in production)
        _masterSecret = _configuration["Security:HashingMasterSecret"]
            ?? throw new InvalidOperationException("Master secret not configured");
    }
    
    public string HashData(string data, string? salt = null)
    {
        using var sha256 = SHA256.Create();
        var input = salt != null ? $"{data}{salt}" : data;
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    
    public string DeriveTenantSalt(int tenantId)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_masterSecret));
        var tenantBytes = BitConverter.GetBytes(tenantId);
        var derivedKey = hmac.ComputeHash(tenantBytes);
        return Convert.ToBase64String(derivedKey);
    }
    
    // ... other methods
}
```

**3. EN1090HashingService (Domain-Specific)**
```csharp
public interface IEN1090HashingService
{
    // Material certificate hashing
    string HashChemicalComposition(MaterialCertificate cert);
    bool VerifyChemicalComposition(MaterialCertificate cert, string hash);
    
    // Welding parameter hashing
    string HashWeldingParameters(WeldingRecord record);
    bool VerifyWeldingParameters(WeldingRecord record, string hash);
    
    // Batch traceability hashing (anti-tampering)
    string CreateTraceabilityHash(string heatNumber, string batchNumber, DateTime date);
    
    // Supplier pricing hashing
    string HashPurchaseOrderPricing(PurchaseOrder po);
}
```

### Service Registration

```csharp
// In Program.cs
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IEN1090HashingService, EN1090HashingService>();

// Production: Load master secret from Azure Key Vault
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = new Uri(builder.Configuration["KeyVault:Uri"]!);
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

---

## Data-by-Domain Analysis

### Domain 1: Material Traceability

**Tables**: `ProfileInventory`, `MaterialCertificate`

**Fields to Hash**:

| Field | Plain/Hash | Rationale |
|-------|-----------|-----------|
| `ChemicalComposition` | Hash only | Proprietary steel formula |
| `MechanicalProperties` | Plain + Hash | Need verification + compliance proof |
| `MaterialBatch` | Plain + Hash | EN 1090 requires plain, hash prevents tampering |
| `HeatNumber` | Plain + Hash | Same as MaterialBatch |
| `MillTestCertificateNumber` | Plain only | EN 10204 requirement |
| `ManufacturerName` | Plain only | Required for certificate verification |
| `UnitCost` | Hash only | Pricing is proprietary |

**New Schema Fields**:
```sql
ALTER TABLE ProfileInventory ADD COLUMN MaterialBatchHash VARCHAR(64);
ALTER TABLE MaterialCertificate ADD COLUMN ChemicalCompositionHash VARCHAR(64);
ALTER TABLE MaterialCertificate ADD COLUMN MechanicalPropertiesHash VARCHAR(64);
```

**Verification Flow**:
```csharp
// During material receiving
var inventory = new ProfileInventory { 
    MaterialBatch = "ABC123456",
    MaterialBatchHash = hashingService.HashWithTenantSalt(tenantId, "ABC123456")
};

// During audit
bool isValid = hashingService.VerifyWithTenantSalt(
    tenantId, 
    inventory.MaterialBatch, 
    inventory.MaterialBatchHash
);
```

### Domain 2: Welding Management

**Tables**: `WeldingRecord`, `WeldingProcedure`, `WelderQualification`

**Fields to Hash**:

| Field | Plain/Hash | Rationale |
|-------|-----------|-----------|
| `WeldingCurrent` | Plain + Hash | QC needs values, hash for IP protection |
| `WeldingVoltage` | Plain + Hash | Same as current |
| `TravelSpeed` | Plain + Hash | Proprietary technique |
| `HeatInput` | Plain + Hash | Derived value, critical for compliance |
| `ConsumableBatch` | Hash only | Proprietary filler material |
| `WelderId` | Hash for reports | GDPR consideration |
| `WelderName` | Hash for reports | GDPR consideration |

**Composite Hashing**:
```csharp
// Hash entire welding parameter set for verification
var parameterHash = hashingService.HashComposite(
    record.WeldingCurrent,
    record.WeldingVoltage,
    record.TravelSpeed,
    record.HeatInput,
    record.PreheatTemperature,
    record.InterpassTemperature
);

record.WeldingParametersHash = parameterHash;
```

**New Schema Fields**:
```sql
ALTER TABLE WeldingRecord ADD COLUMN WeldingParametersHash VARCHAR(64);
ALTER TABLE WeldingRecord ADD COLUMN ConsumableBatchHash VARCHAR(64);
ALTER TABLE WelderQualification ADD COLUMN WelderIdHash VARCHAR(64);
```

### Domain 3: Procurement & Sourcing

**Tables**: `PurchaseOrder`, `PurchaseOrderLine`, `PriceQuote`

**Fields to Hash**:

| Field | Plain/Hash | Rationale |
|-------|-----------|-----------|
| `UnitPrice` | Hash only | Pricing is highly sensitive |
| `TotalPrice` | Hash only | Derived from unit price |
| `DiscountAmount` | Hash only | Contractual sensitive data |
| `Supplier.Name` | Access-controlled plain | Required for PO processing |

**Verification Use Case**:
```csharp
// External API exposes pricing verification without revealing amounts
public async Task<bool> VerifyPurchaseOrderPricing(
    int poId, 
    string pricingHash
)
{
    var po = await _context.PurchaseOrders
        .Include(p => p.PurchaseOrderLines)
        .FirstOrDefaultAsync(p => p.PurchaseOrderId == poId);
    
    var calculatedHash = _hashingService.HashComposite(
        po.TotalPrice,
        po.OrderDate,
        po.SupplierId
    );
    
    return calculatedHash == pricingHash;
}
```

### Domain 4: Quality Control & NDT

**Tables**: `QualityInspectionRecord`, `NDTRecord`, `NonConformanceReport`

**Fields to Hash**:

| Field | Plain/Hash | Rationale |
|-------|-----------|-----------|
| `InspectionResult` | Plain + Digital Signature | Safety-critical, must be readable |
| `TestResult` | Plain + Digital Signature | EN 1090 compliance requirement |
| `DefectDescription` | Plain + Hash | May contain proprietary process info |
| `TesterName` | Hash for reports | GDPR consideration |

**Digital Signature Approach** (alternative to hashing for critical fields):
```csharp
public class SignedInspectionRecord
{
    public string InspectionResult { get; set; }
    public string InspectorUserId { get; set; }
    public DateTime InspectionDate { get; set; }
    
    // RSA signature of result + inspector + date
    public string DigitalSignature { get; set; }
    
    // Timestamp from trusted authority (optional)
    public string? TimestampToken { get; set; }
}
```

---

## Security Considerations

### Master Secret Management

**Development Environment**:
```json
// appsettings.Development.json (NOT committed to git)
{
  "Security": {
    "HashingMasterSecret": "dev-only-not-for-production-12345"
  }
}
```

**Production Environment**:
```csharp
// Load from Azure Key Vault
var keyVaultUri = new Uri(configuration["KeyVault:Uri"]);
var credential = new DefaultAzureCredential();
var secretClient = new SecretClient(keyVaultUri, credential);
var masterSecret = await secretClient.GetSecretAsync("hashing-master-secret");
```

**Secret Rotation Strategy**:
1. Generate new master secret
2. Dual-key period: Accept old + new hashes
3. Re-hash all data with new secret (background job)
4. Deprecate old secret after migration

### Access Control Integration

**Role-Based Hash Access**:
```csharp
[Authorize(Roles = "Admin,QualityManager")]
public async Task<IActionResult> GetPlainTextChemicalComposition(Guid certId)
{
    // Only authorized users can see plain text proprietary data
    var cert = await _context.MaterialCertificates.FindAsync(certId);
    return Ok(cert.ChemicalComposition); // Plain JSON
}

[Authorize(Roles = "Auditor,Customer")]
public async Task<IActionResult> VerifyChemicalComposition(
    Guid certId, 
    string expectedHash
)
{
    // External auditors verify without seeing sensitive data
    var cert = await _context.MaterialCertificates.FindAsync(certId);
    var actualHash = _hashingService.HashData(cert.ChemicalComposition);
    return Ok(new { IsValid = actualHash == expectedHash });
}
```

### Audit Logging

**Hash Verification Events**:
```csharp
public async Task<bool> VerifyAndLog(string data, string hash, string entityType, int entityId)
{
    var isValid = _hashingService.VerifyHash(data, hash);
    
    await _auditService.LogAsync(new AuditEntry
    {
        Action = "HashVerification",
        EntityType = entityType,
        EntityId = entityId,
        UserId = _currentUser.UserId,
        Result = isValid ? "Success" : "Failed",
        Timestamp = DateTime.UtcNow,
        IPAddress = _httpContext.Connection.RemoteIpAddress?.ToString()
    });
    
    return isValid;
}
```

---

## Database Schema Changes

### New Tables

**1. HashAuditLog** (track hash verification attempts)
```sql
CREATE TABLE HashAuditLog (
    AuditId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    EntityId VARCHAR(100) NOT NULL,
    HashType VARCHAR(50) NOT NULL,
    VerificationResult BIT NOT NULL,
    UserId VARCHAR(450) NULL,
    IPAddress VARCHAR(45) NULL,
    VerifiedUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_HashAuditLog_Tenant FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);

CREATE INDEX IX_HashAuditLog_Tenant_Date ON HashAuditLog(TenantId, VerifiedUtc);
CREATE INDEX IX_HashAuditLog_Entity ON HashAuditLog(EntityType, EntityId);
```

**2. HashChain** (blockchain-style integrity tracking)
```sql
CREATE TABLE HashChain (
    ChainId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId INT NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    RecordId UNIQUEIDENTIFIER NOT NULL,
    CurrentHash VARCHAR(64) NOT NULL,
    PreviousHash VARCHAR(64) NULL,
    ChainHash VARCHAR(64) NOT NULL,
    SequenceNumber BIGINT NOT NULL,
    ChainedUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChainedByUserId VARCHAR(450) NOT NULL,
    CONSTRAINT FK_HashChain_Tenant FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);

CREATE INDEX IX_HashChain_Entity ON HashChain(EntityType, SequenceNumber);
CREATE INDEX IX_HashChain_Record ON HashChain(RecordId);
CREATE UNIQUE INDEX IX_HashChain_Sequence ON HashChain(TenantId, EntityType, SequenceNumber);
```

**3. ArchiveMetadata** (track archived records)
```sql
CREATE TABLE ArchiveMetadata (
    ArchiveId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId INT NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    RecordId UNIQUEIDENTIFIER NOT NULL,
    ArchiveBlobUri VARCHAR(1000) NOT NULL,
    ArchiveHash VARCHAR(64) NOT NULL,
    ArchivedUtc DATETIME2 NOT NULL,
    ArchivedByUserId VARCHAR(450) NOT NULL,
    RetentionYears INT NOT NULL, -- -1 for permanent
    ExpiryDate DATE NULL, -- NULL for permanent retention
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedUtc DATETIME2 NULL,
    DeletionApprovalRef VARCHAR(200) NULL, -- Legal approval for deletion
    CONSTRAINT FK_ArchiveMetadata_Tenant FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);

CREATE INDEX IX_ArchiveMetadata_Entity ON ArchiveMetadata(EntityType, RecordId);
CREATE INDEX IX_ArchiveMetadata_Expiry ON ArchiveMetadata(ExpiryDate) WHERE IsDeleted = 0;
```

**4. RecordSupersession** (track corrections and supersessions)
```sql
CREATE TABLE RecordSupersession (
    SupersessionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId INT NOT NULL,
    EntityType VARCHAR(100) NOT NULL,
    OriginalRecordId UNIQUEIDENTIFIER NOT NULL,
    SupersedingRecordId UNIQUEIDENTIFIER NOT NULL,
    Reason VARCHAR(1000) NOT NULL,
    SupersededUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    SupersededByUserId VARCHAR(450) NOT NULL,
    ApprovalRequired BIT NOT NULL DEFAULT 1,
    ApprovedByUserId VARCHAR(450) NULL,
    ApprovedUtc DATETIME2 NULL,
    CONSTRAINT FK_RecordSupersession_Tenant FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId)
);

CREATE INDEX IX_RecordSupersession_Original ON RecordSupersession(OriginalRecordId);
CREATE INDEX IX_RecordSupersession_Superseding ON RecordSupersession(SupersedingRecordId);
```

### Column Additions

**ALL EN 1090 Critical Tables** (apply immutability pattern):
```sql
-- Immutability and finalization tracking
ALTER TABLE MaterialCertificate ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE MaterialCertificate ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE MaterialCertificate ADD FinalizedByUserId VARCHAR(450) NULL;
ALTER TABLE MaterialCertificate ADD RecordHash VARCHAR(64) NULL;

ALTER TABLE MaterialCertificate ADD IsSuperseded BIT NOT NULL DEFAULT 0;
ALTER TABLE MaterialCertificate ADD SupersededByRecordId UNIQUEIDENTIFIER NULL;
ALTER TABLE MaterialCertificate ADD SupersessionReason VARCHAR(1000) NULL;
ALTER TABLE MaterialCertificate ADD SupersededUtc DATETIME2 NULL;

ALTER TABLE MaterialCertificate ADD IsArchived BIT NOT NULL DEFAULT 0;
ALTER TABLE MaterialCertificate ADD ArchivedUtc DATETIME2 NULL;
ALTER TABLE MaterialCertificate ADD ArchiveBlobUri VARCHAR(1000) NULL;

-- Repeat for all immutable tables
ALTER TABLE WeldingRecord ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE WeldingRecord ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE WeldingRecord ADD FinalizedByUserId VARCHAR(450) NULL;
ALTER TABLE WeldingRecord ADD RecordHash VARCHAR(64) NULL;
ALTER TABLE WeldingRecord ADD IsSuperseded BIT NOT NULL DEFAULT 0;
ALTER TABLE WeldingRecord ADD SupersededByRecordId UNIQUEIDENTIFIER NULL;
ALTER TABLE WeldingRecord ADD SupersessionReason VARCHAR(1000) NULL;
ALTER TABLE WeldingRecord ADD SupersededUtc DATETIME2 NULL;
ALTER TABLE WeldingRecord ADD IsArchived BIT NOT NULL DEFAULT 0;
ALTER TABLE WeldingRecord ADD ArchivedUtc DATETIME2 NULL;
ALTER TABLE WeldingRecord ADD ArchiveBlobUri VARCHAR(1000) NULL;

ALTER TABLE NDTRecord ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE NDTRecord ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE NDTRecord ADD FinalizedByUserId VARCHAR(450) NULL;
ALTER TABLE NDTRecord ADD RecordHash VARCHAR(64) NULL;
ALTER TABLE NDTRecord ADD IsSuperseded BIT NOT NULL DEFAULT 0;
ALTER TABLE NDTRecord ADD SupersededByRecordId UNIQUEIDENTIFIER NULL;
ALTER TABLE NDTRecord ADD IsArchived BIT NOT NULL DEFAULT 0;

ALTER TABLE QualityInspectionRecord ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE QualityInspectionRecord ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE QualityInspectionRecord ADD RecordHash VARCHAR(64) NULL;
ALTER TABLE QualityInspectionRecord ADD IsSuperseded BIT NOT NULL DEFAULT 0;
ALTER TABLE QualityInspectionRecord ADD IsArchived BIT NOT NULL DEFAULT 0;

ALTER TABLE WelderQualification ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE WelderQualification ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE WelderQualification ADD RecordHash VARCHAR(64) NULL;
ALTER TABLE WelderQualification ADD IsArchived BIT NOT NULL DEFAULT 0;

ALTER TABLE NonConformanceReport ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE NonConformanceReport ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE NonConformanceReport ADD RecordHash VARCHAR(64) NULL;
ALTER TABLE NonConformanceReport ADD IsSuperseded BIT NOT NULL DEFAULT 0;

ALTER TABLE ExecutionClass ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE ExecutionClass ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE ExecutionClass ADD RecordHash VARCHAR(64) NULL;
```

**Hash Storage Columns**:
```sql
ALTER TABLE ProfileInventory ADD MaterialBatchHash VARCHAR(64) NULL;
ALTER TABLE ProfileInventory ADD UnitCostHash VARCHAR(64) NULL;
```

**MaterialCertificate**:
```sql
ALTER TABLE MaterialCertificate ADD ChemicalCompositionHash VARCHAR(64) NULL;
ALTER TABLE MaterialCertificate ADD MechanicalPropertiesHash VARCHAR(64) NULL;
ALTER TABLE MaterialCertificate ADD HashAlgorithm VARCHAR(20) NULL DEFAULT 'SHA256';
ALTER TABLE MaterialCertificate ADD HashedUtc DATETIME2 NULL;
```

**WeldingRecord**:
```sql
ALTER TABLE WeldingRecord ADD WeldingParametersHash VARCHAR(64) NULL;
ALTER TABLE WeldingRecord ADD ConsumableBatchHash VARCHAR(64) NULL;
```

**PurchaseOrder** & **PurchaseOrderLine**:
```sql
ALTER TABLE PurchaseOrder ADD TotalPriceHash VARCHAR(64) NULL;
ALTER TABLE PurchaseOrderLine ADD UnitPriceHash VARCHAR(64) NULL;
```

**WelderQualification**:
```sql
ALTER TABLE WelderQualification ADD WelderIdHash VARCHAR(64) NULL;
ALTER TABLE WelderQualification ADD WelderNameHash VARCHAR(64) NULL;
```

---

## API Changes

### New Endpoints

**1. Hash Verification API**
```csharp
[ApiController]
[Route("api/compliance/verification")]
public class ComplianceVerificationController : ControllerBase
{
    [HttpPost("verify-material-certificate")]
    [RequireFeature(FeatureKeys.EN1090Compliance)]
    public async Task<IActionResult> VerifyMaterialCertificate(
        [FromBody] MaterialCertificateVerificationRequest request
    )
    {
        var cert = await _context.MaterialCertificates
            .FindAsync(request.CertificateId);
        
        if (cert == null) return NotFound();
        
        var isValid = _hashingService.VerifyHash(
            cert.ChemicalComposition,
            request.ChemicalCompositionHash
        );
        
        return Ok(new { IsValid = isValid, VerifiedAt = DateTime.UtcNow });
    }
    
    [HttpPost("verify-welding-parameters")]
    public async Task<IActionResult> VerifyWeldingParameters(
        [FromBody] WeldingParametersVerificationRequest request
    )
    {
        var record = await _context.WeldingRecords.FindAsync(request.WeldingRecordId);
        if (record == null) return NotFound();
        
        var actualHash = _en1090HashingService.HashWeldingParameters(record);
        var isValid = actualHash == request.ExpectedHash;
        
        return Ok(new { IsValid = isValid });
    }
}
```

**2. Secure Data Export API** (hash-only mode)
```csharp
[HttpGet("material-certificates/export")]
[RequireFeature(FeatureKeys.EN1090Compliance)]
public async Task<IActionResult> ExportMaterialCertificates(
    [FromQuery] bool includeHashesOnly = false
)
{
    var certs = await _context.MaterialCertificates.ToListAsync();
    
    if (includeHashesOnly)
    {
        // Return only hashes for external verification
        var hashedData = certs.Select(c => new
        {
            c.CertificateId,
            c.CertificateNumber,
            ChemicalCompositionHash = c.ChemicalCompositionHash,
            MechanicalPropertiesHash = c.MechanicalPropertiesHash
        });
        return Ok(hashedData);
    }
    
    // Full data only for authorized internal users
    if (!User.IsInRole("Admin"))
        return Forbid();
    
    return Ok(certs);
}
```

---

## Migration Strategy

### Phase 1: Infrastructure (Week 1-2)

**Tasks**:
1. Implement `IHashingService` and `EN1090HashingService`
2. Add database columns for hash storage
3. Configure master secret in Azure Key Vault
4. Create EF migrations for new schema
5. Unit tests for hashing service

**Deliverables**:
- `Manimp.Services/Implementation/HashingService.cs`
- `Manimp.Services/Implementation/EN1090HashingService.cs`
- Database migration: `AddHashFieldsToEN1090Tables`
- Unit tests: 100% coverage of hashing service

### Phase 2: Backward-Compatible Implementation (Week 3-4)

**Approach**: Dual-storage period
```csharp
// Store both plain and hash during transition
public async Task UpdateMaterialCertificate(MaterialCertificate cert)
{
    // Store plain text (existing behavior)
    cert.ChemicalComposition = compositionJson;
    
    // NEW: Also store hash
    cert.ChemicalCompositionHash = _hashingService.HashData(compositionJson);
    cert.HashedUtc = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
}
```

**Tasks**:
1. Update all Create/Update operations to generate hashes
2. Add hash verification to critical read operations
3. Create background job to hash existing data
4. API versioning for hash-aware endpoints

**Deliverables**:
- Modified services with hash generation
- Background migration job
- API v2 with hash support
- Integration tests

### Phase 3: Hash-Only Mode (Week 5-6)

**Transition**: Remove plain text storage for sensitive fields
```csharp
// BEFORE (Phase 2):
cert.ChemicalComposition = compositionJson; // Plain text
cert.ChemicalCompositionHash = hash;

// AFTER (Phase 3):
cert.ChemicalComposition = null; // Removed
cert.ChemicalCompositionHash = hash; // Only hash stored
```

**Tasks**:
1. Update UI to show "verification available" instead of values
2. Remove plain text columns (or mark deprecated)
3. Update documentation
4. Train users on verification workflows

### Phase 4: Audit & Optimization (Week 7-8)

**Tasks**:
1. Performance testing of hash operations
2. Security audit of implementation
3. Optimize database indexes for hash lookups
4. Documentation finalization

---

## Testing & Validation

### Unit Tests

**HashingService Tests**:
```csharp
[Fact]
public void HashData_SameInput_ProducesSameHash()
{
    var service = new HashingService(_mockConfig);
    var data = "test data";
    
    var hash1 = service.HashData(data);
    var hash2 = service.HashData(data);
    
    Assert.Equal(hash1, hash2);
}

[Fact]
public void HashData_DifferentSalt_ProducesDifferentHash()
{
    var service = new HashingService(_mockConfig);
    var data = "test data";
    
    var hash1 = service.HashData(data, "salt1");
    var hash2 = service.HashData(data, "salt2");
    
    Assert.NotEqual(hash1, hash2);
}

[Fact]
public void VerifyHash_CorrectData_ReturnsTrue()
{
    var service = new HashingService(_mockConfig);
    var data = "test data";
    var hash = service.HashData(data);
    
    var isValid = service.VerifyHash(data, hash);
    
    Assert.True(isValid);
}
```

### Integration Tests

**EN1090 Workflow Tests**:
```csharp
[Fact]
public async Task CreateMaterialCertificate_AutomaticallyHashesChemicalComposition()
{
    var cert = new MaterialCertificate
    {
        ChemicalComposition = "{\"C\": 0.18, \"Mn\": 1.45}"
    };
    
    await _service.CreateMaterialCertificateAsync(cert);
    
    Assert.NotNull(cert.ChemicalCompositionHash);
    Assert.Equal(64, cert.ChemicalCompositionHash.Length); // SHA-256 hex
}

[Fact]
public async Task VerifyWeldingParameters_TamperedData_ReturnsFalse()
{
    var record = await CreateTestWeldingRecord();
    var originalHash = record.WeldingParametersHash;
    
    // Simulate tampering
    record.WeldingCurrent += 10;
    
    var isValid = _en1090HashingService.VerifyWeldingParameters(record, originalHash);
    
    Assert.False(isValid);
}
```

### Performance Tests

**Benchmark Targets**:
- Hash generation: < 1ms per operation
- Hash verification: < 1ms per operation
- Batch hashing (1000 records): < 5 seconds
- No measurable impact on page load times

---

## Compliance Impact

### EN 1090 Audit Compatibility

**Auditor Access Levels**:
1. **Internal Audits**: Full access to plain text + hashes
2. **Third-Party Audits**: Verification via hashes, selective plain text disclosure
3. **Regulatory Audits**: Full disclosure with proper authorization

**Audit Trail Requirements**:
```csharp
public class EN1090AuditPackage
{
    public Guid PackageId { get; set; }
    public int ProjectId { get; set; }
    
    // Plain text data for compliance verification
    public List<MaterialCertificate> Certificates { get; set; }
    public List<WeldingRecord> WeldingRecords { get; set; }
    
    // Corresponding hashes for tamper detection
    public Dictionary<string, string> VerificationHashes { get; set; }
    
    // Digital signature of entire package
    public string PackageSignature { get; set; }
    public DateTime GeneratedUtc { get; set; }
}
```

### GDPR Compliance

**Personal Data Hashing**:
- Welder names/IDs hashed for reports
- Inspector names hashed for public-facing documents
- Original data retained for employment records (separate access control)

**Right to be Forgotten**:
```csharp
public async Task AnonymizeWelderData(string welderId)
{
    // Replace real ID with hash
    var qualifications = await _context.WelderQualifications
        .Where(w => w.WelderId == welderId)
        .ToListAsync();
    
    foreach (var qual in qualifications)
    {
        qual.WelderIdHash = _hashingService.HashData(qual.WelderId);
        qual.WelderId = $"ANONYMIZED-{qual.WelderIdHash.Substring(0, 8)}";
        qual.WelderName = "ANONYMIZED";
    }
    
    await _context.SaveChangesAsync();
}
```

---

## Implementation Checklist

### Development Phase
- [ ] Create `IHashingService` interface
- [ ] Implement `HashingService` with SHA-256
- [ ] Implement `EN1090HashingService` with domain logic
- [ ] **Create `ImmutableEN1090Record` base class**
- [ ] **Implement `ImmutableRecordInterceptor` for EF Core**
- [ ] **Create finalization workflow service**
- [ ] **Create supersession workflow service**
- [ ] **Implement `IArchivalService` for data retention**
- [ ] Add database migrations for hash columns
- [ ] **Add database migrations for immutability columns (IsFinalized, IsSuperseded, etc.)**
- [ ] **Create database triggers to prevent finalized record updates**
- [ ] **Add HashChain table and chain tracking logic**
- [ ] Configure Azure Key Vault integration
- [ ] Write comprehensive unit tests (>90% coverage)
- [ ] **Add immutability workflow tests**
- [ ] Create integration tests for EN 1090 workflows
- [ ] **Test finalization → edit prevention → supersession workflow**
- [ ] **Test archival and retrieval workflows**
- [ ] Performance benchmarking

### Deployment Phase
- [ ] Deploy master secret to Azure Key Vault (production)
- [ ] **Configure Azure Blob Storage for archive tier**
- [ ] Run database migrations in staging environment
- [ ] **Deploy immutability triggers and constraints**
- [ ] Execute backward-compatible hash generation for existing data
- [ ] **Finalize all existing production records (one-time migration)**
- [ ] Monitor performance impact
- [ ] Train users on hash verification UI
- [ ] **Train users on finalization and supersession workflows**
- [ ] Update API documentation
- [ ] **Document retention policies and archival schedules**
- [ ] Security audit by third party

### Verification Phase
- [ ] Validate hash integrity for sample data
- [ ] **Verify finalized records cannot be edited**
- [ ] **Test supersession workflow with corrections**
- [ ] **Validate hash chain integrity**
- [ ] Test auditor access workflows
- [ ] Verify GDPR compliance
- [ ] Confirm EN 1090 audit compatibility
- [ ] **Demonstrate 10-year retention capability**
- [ ] **Test archive retrieval for 5+ year old records**
- [ ] Load testing with production data volumes
- [ ] Disaster recovery testing (hash regeneration)
- [ ] **Test archival migration and blob storage integrity**

---

## Future Enhancements

### Phase 2 Features (Future)

**Blockchain Integration**:
- Store critical hashes on immutable blockchain
- Timestamping for legal non-repudiation
- Cross-company verification (supply chain)

**Zero-Knowledge Proofs**:
- Prove compliance without revealing data
- "Prove welding parameters are within spec without showing values"

**Homomorphic Encryption**:
- Perform calculations on encrypted data
- Analytics on hashed chemical compositions

---

## Conclusion

This hashing implementation plan provides a comprehensive, phased approach to securing sensitive EN 1090 compliance data. By carefully classifying data, implementing industry-standard cryptographic techniques, and maintaining audit compatibility, Manimp will protect proprietary information while meeting all regulatory requirements.

### Key Success Metrics
- **Security**: 100% of proprietary data hashed or access-controlled
- **Performance**: < 5% overhead on existing operations
- **Compliance**: Pass EN 1090 audits with hash-based verification
- **User Experience**: Seamless verification workflows for authorized users

### Next Steps
1. Review and approve this plan with stakeholders
2. Begin Phase 1 implementation (Infrastructure)
3. Set up Azure Key Vault and development environment
4. Create detailed user stories for Phase 2 development

---

**Document Control**  
**Author**: AI Development Team  
**Reviewed By**: [Pending]  
**Approved By**: [Pending]  
**Next Review Date**: [After Phase 1 completion]
