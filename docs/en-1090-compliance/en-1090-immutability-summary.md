# EN 1090 Data Immutability & Retention - Quick Reference

**Document Version:** 1.0  
**Date:** October 9, 2025  
**Related Document:** `en-1090-data-hashing-plan.md`

---

## Core Principle

**Once EN 1090 compliance data is finalized, it CANNOT be edited or deleted.** This is mandatory for:
- Factory Production Control (FPC) audits
- CE marking traceability  
- Legal liability protection
- Regulatory compliance verification

---

## Immutable Record Lifecycle

```
┌─────────┐      ┌──────────┐      ┌───────────┐
│  Draft  │─────▶│ Verified │─────▶│ Finalized │
│ (Edit)  │      │ (Edit*)  │      │  (LOCKED) │
└─────────┘      └──────────┘      └───────────┘
                                          │
                                          ▼
                                    ┌──────────────┐
                                    │  Superseded  │
                                    │ (Correction) │
                                    └──────────────┘
                                          │
                                          ▼
                                    New Draft Created
                                    (Original retained)
```

\* Verified state edits may require approval

---

## Tables with Immutability

**Critical EN 1090 Tables** (8 tables):
1. `MaterialCertificate` - Material traceability
2. `WeldingRecord` - Welding execution records  
3. `WelderQualification` - Welder certifications
4. `NDTRecord` - Non-destructive testing results
5. `QualityInspectionRecord` - Quality inspections
6. `NonConformanceReport` - NCR tracking
7. `ExecutionClass` - Project execution class
8. `WeldingProcedure` - WPS/WPQR qualifications

---

## Database Schema Pattern

```sql
-- Add to ALL EN 1090 tables
ALTER TABLE [TableName] ADD IsFinalized BIT NOT NULL DEFAULT 0;
ALTER TABLE [TableName] ADD FinalizedUtc DATETIME2 NULL;
ALTER TABLE [TableName] ADD FinalizedByUserId VARCHAR(450) NULL;
ALTER TABLE [TableName] ADD RecordHash VARCHAR(64) NULL;

ALTER TABLE [TableName] ADD IsSuperseded BIT NOT NULL DEFAULT 0;
ALTER TABLE [TableName] ADD SupersededByRecordId UNIQUEIDENTIFIER NULL;
ALTER TABLE [TableName] ADD SupersessionReason VARCHAR(1000) NULL;
ALTER TABLE [TableName] ADD SupersededUtc DATETIME2 NULL;

ALTER TABLE [TableName] ADD IsArchived BIT NOT NULL DEFAULT 0;
ALTER TABLE [TableName] ADD ArchivedUtc DATETIME2 NULL;
ALTER TABLE [TableName] ADD ArchiveBlobUri VARCHAR(1000) NULL;
```

---

## Protection Mechanisms

### 1. EF Core Interceptor
```csharp
public class ImmutableRecordInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(...)
    {
        // Throws exception if finalized record is modified
        var finalizedChanges = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified)
            .Where(e => e.Entity is ImmutableEN1090Record)
            .Where(e => ((ImmutableEN1090Record)e.Entity).IsFinalized);
        
        if (finalizedChanges.Any())
            throw new ImmutableRecordViolationException();
    }
}
```

### 2. Database Trigger
```sql
CREATE TRIGGER TR_WeldingRecord_PreventFinalizedUpdate
ON WeldingRecord
INSTEAD OF UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM inserted i 
               INNER JOIN deleted d ON i.WeldingRecordId = d.WeldingRecordId
               WHERE d.IsFinalized = 1)
    BEGIN
        RAISERROR('Cannot modify finalized welding records', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
```

### 3. API Authorization
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateWeldingRecord(Guid id, WeldingRecord record)
{
    var existing = await _context.WeldingRecords.FindAsync(id);
    
    if (existing.IsFinalized)
        return BadRequest("Cannot edit finalized record. Use supersession workflow.");
    
    // ... update logic
}
```

---

## Finalization Workflow

### Code Example
```csharp
public async Task<Result> FinalizeWeldingRecord(Guid recordId, string userId)
{
    var record = await _context.WeldingRecords.FindAsync(recordId);
    
    // Validation
    if (record.IsFinalized)
        return Result.Fail("Already finalized");
    
    // Generate immutable hash
    record.RecordHash = _hashingService.HashObject(record);
    record.IsFinalized = true;
    record.FinalizedUtc = DateTime.UtcNow;
    record.FinalizedByUserId = userId;
    
    // Add to hash chain (blockchain pattern)
    await _hashingService.AddToHashChain("WeldingRecord", recordId, record);
    
    await _context.SaveChangesAsync();
    
    return Result.Success();
}
```

### UI Component
```cshtml
@if (record.IsFinalized)
{
    <MudChip Color="Color.Success" Icon="@Icons.Material.Filled.Lock">
        Finalized - Cannot Edit
    </MudChip>
    <MudText Typo="Typo.caption">
        Finalized on @record.FinalizedUtc by @record.FinalizedByUserName
    </MudText>
    
    <!-- Edit button disabled -->
    <MudButton Disabled="true" StartIcon="@Icons.Material.Filled.Edit">
        Edit
    </MudButton>
    
    <!-- Supersession button enabled -->
    <MudButton Color="Color.Secondary" OnClick="@SupersedeRecord">
        Create Correction
    </MudButton>
}
```

---

## Supersession Workflow (Corrections)

### When to Use
- Data entry error discovered after finalization
- Measurement correction needed
- Certificate information updated by supplier
- Welder qualification status changed

### Process
1. Original record marked `IsSuperseded = true` (NOT deleted)
2. New record created with corrected data
3. Link established: `SupersededByRecordId`
4. Reason documented: `SupersessionReason`
5. Both records retained forever in audit trail

### Code Example
```csharp
public async Task<Result<Guid>> SupersedeWeldingRecord(
    Guid originalId, 
    WeldingRecord correctedRecord,
    string reason,
    string userId)
{
    var original = await _context.WeldingRecords.FindAsync(originalId);
    
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    // Mark original as superseded
    original.IsSuperseded = true;
    original.SupersededByRecordId = correctedRecord.WeldingRecordId;
    original.SupersessionReason = reason;
    original.SupersededUtc = DateTime.UtcNow;
    
    // Create new corrected record
    correctedRecord.WeldingRecordId = Guid.NewGuid();
    correctedRecord.Notes = $"Supersedes {originalId}. Reason: {reason}";
    _context.WeldingRecords.Add(correctedRecord);
    
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
    
    return Result<Guid>.Success(correctedRecord.WeldingRecordId);
}
```

---

## Data Retention Policies

### Retention Requirements

| Data Type | Retention Period | Archive After | Delete After |
|-----------|------------------|---------------|--------------|
| Material Certificates | **Permanent** | 2 years | Never |
| WPS/WPQR | **Permanent** | 2 years | Never |
| Welding Records | **10 years** | 2 years | 10 years |
| NDT Records | **10 years** | 2 years | 10 years |
| Quality Inspections | **10 years** | 2 years | 10 years |
| NCRs | **10 years** | 2 years | 10 years |
| Purchase Orders | **5 years** | 2 years | 5 years |
| Price Quotes | **5 years** | 2 years | 5 years |

### Storage Tiers

**Hot Storage (0-2 years)**:
- SQL Server database
- Fast queries
- Full-text search
- $$$$ cost

**Warm Storage (2-10 years)**:
- Archive database
- Compressed storage
- Acceptable query times
- $$ cost

**Cold Storage (10+ years / Permanent)**:
- Azure Blob Storage (Archive tier)
- Rarely accessed
- Legal hold capable
- $ cost

---

## Archival Process

### Automatic Monthly Job
```csharp
// Background service runs monthly
public async Task ArchiveOldRecords()
{
    var cutoffDate = DateTime.UtcNow.AddYears(-2);
    
    var recordsToArchive = await _context.WeldingRecords
        .Where(w => w.CreatedUtc < cutoffDate && w.IsFinalized)
        .ToListAsync();
    
    foreach (var record in recordsToArchive)
    {
        // Serialize to JSON
        var json = JsonSerializer.Serialize(record);
        
        // Upload to Azure Blob Storage
        var blobClient = _blobService.GetBlobClient($"welding-records/{record.WeldingRecordId}.json");
        await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        
        // Set metadata
        await blobClient.SetMetadataAsync(new Dictionary<string, string>
        {
            ["RecordHash"] = _hashingService.HashObject(record),
            ["ArchivedDate"] = DateTime.UtcNow.ToString("O"),
            ["RetentionYears"] = "10"
        });
        
        // Mark as archived in database (don't delete row)
        record.IsArchived = true;
        record.ArchivedUtc = DateTime.UtcNow;
        record.ArchiveBlobUri = blobClient.Uri.ToString();
    }
    
    await _context.SaveChangesAsync();
}
```

### Retrieval API
```csharp
[HttpGet("archived/{recordId}")]
[Authorize(Roles = "Admin,Auditor")]
public async Task<IActionResult> GetArchivedRecord(Guid recordId)
{
    var metadata = await _context.ArchiveMetadata
        .FirstOrDefaultAsync(m => m.RecordId == recordId);
    
    // Download from blob storage
    var blobClient = new BlobClient(new Uri(metadata.ArchiveBlobUri));
    var content = await blobClient.DownloadContentAsync();
    var json = content.Value.Content.ToString();
    
    // Verify integrity
    var record = JsonSerializer.Deserialize<WeldingRecord>(json);
    var storedHash = (await blobClient.GetPropertiesAsync()).Value.Metadata["RecordHash"];
    var currentHash = _hashingService.HashObject(record);
    
    if (storedHash != currentHash)
        return StatusCode(500, "Archive integrity check failed");
    
    return Ok(record);
}
```

---

## Hash Chain (Blockchain Pattern)

### Purpose
Detect tampering across record sequence, similar to blockchain

### Implementation
```csharp
public class HashChain
{
    public string CurrentHash { get; set; }        // Hash of this record
    public string PreviousHash { get; set; }       // Hash of previous record
    public string ChainHash { get; set; }          // Hash(CurrentHash + PreviousHash)
    public long SequenceNumber { get; set; }
}

// Verify entire chain
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
            return false; // Chain broken!
    }
    
    return true;
}
```

---

## User Experience

### Finalization Dialog
```
⚠️ Warning: This action is irreversible

You are about to finalize this welding record. Once finalized:
• The record CANNOT be edited
• The record CANNOT be deleted  
• Corrections require creating a superseding record
• A cryptographic hash will be generated
• This action will be logged in the audit trail

[Cancel]  [Finalize Record]
```

### Record View Badges
- 🔒 **Finalized** - Record is locked, cannot edit
- ⚠️ **Superseded** - Record has been corrected, view current version
- 📦 **Archived** - Record stored in long-term archive

---

## Compliance Verification

### EN 1090 Audit Trail
```csharp
// Generate audit package for regulatory inspection
public async Task<EN1090AuditPackage> GenerateAuditPackage(int projectId)
{
    var package = new EN1090AuditPackage
    {
        ProjectId = projectId,
        GeneratedUtc = DateTime.UtcNow,
        
        // Include all finalized records
        MaterialCertificates = await GetFinalizedCertificates(projectId),
        WeldingRecords = await GetFinalizedWeldingRecords(projectId),
        NDTRecords = await GetFinalizedNDTRecords(projectId),
        
        // Include verification hashes
        VerificationHashes = GenerateVerificationHashes(projectId)
    };
    
    // Sign entire package
    package.PackageSignature = _cryptoService.SignData(
        JsonSerializer.Serialize(package)
    );
    
    return package;
}
```

### GDPR "Right to be Forgotten"
```csharp
// Anonymize personal data while preserving compliance records
public async Task AnonymizeWelderData(string welderId)
{
    var qualifications = await _context.WelderQualifications
        .Where(w => w.WelderId == welderId)
        .ToListAsync();
    
    foreach (var qual in qualifications)
    {
        // Hash the ID for verification
        qual.WelderIdHash = _hashingService.HashData(qual.WelderId);
        
        // Replace with anonymized placeholder
        qual.WelderId = $"ANON-{qual.WelderIdHash.Substring(0, 8)}";
        qual.WelderName = "ANONYMIZED";
        
        // Compliance records retained, personal data removed
    }
    
    await _context.SaveChangesAsync();
}
```

---

## Key Takeaways

✅ **Immutability is enforced at 3 levels**: Application, EF Core, Database triggers  
✅ **Corrections use supersession**: Original record retained, new record created  
✅ **Hash chains detect tampering**: Blockchain-style integrity verification  
✅ **Retention policies are automatic**: 10-year minimum for compliance data  
✅ **Archives use Azure Blob Storage**: Cost-effective long-term storage  
✅ **EN 1090 audit compatible**: Complete traceability with immutable records  

---

## Next Steps

1. Review full plan: `docs/en-1090-data-hashing-plan.md`
2. Implement `ImmutableEN1090Record` base class
3. Add immutability columns to database
4. Create finalization and supersession workflows
5. Set up Azure Blob Storage for archival
6. Test finalization → edit prevention → supersession cycle

**Critical**: Test with actual EN 1090 audit scenario before production deployment!
