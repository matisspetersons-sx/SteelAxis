# EN 1090 Compliance - Complete Implementation Guide

**Last Updated:** October 14, 2025  
**Status:** Ready for Implementation  
**Standard:** EN 1090-1:2009+A1:2011 (Execution of steel structures and aluminium structures)

---

## 🎯 Overview

EN 1090 is the European standard for fabrication and assembly of steel and aluminium structures. This guide consolidates all compliance requirements into a single actionable reference.

### Key Requirements
- **Traceability:** Full material and process tracking
- **Document Control:** Version management with immutable records
- **Quality Management:** NCR system with corrective actions
- **Certification:** Material certificates (EN 10204 3.1/3.2)
- **Data Integrity:** SHA-256 hashing for audit trails

---

## 📋 Core Implementation Areas

### 1. Material Traceability

**Database Schema:**
```sql
CREATE TABLE MaterialCertificates (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    LotNumber NVARCHAR(100) NOT NULL,
    MaterialGrade NVARCHAR(50) NOT NULL,
    CertificateType NVARCHAR(10) NOT NULL, -- '3.1', '3.2'
    Supplier NVARCHAR(200) NOT NULL,
    CertificateNumber NVARCHAR(100) NOT NULL,
    TestDate DATETIME2 NOT NULL,
    ChemicalComposition NVARCHAR(MAX), -- JSON
    MechanicalProperties NVARCHAR(MAX), -- JSON
    CertificateFileUrl NVARCHAR(500),
    DataHash NVARCHAR(64), -- SHA-256
    IsImmutable BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_MaterialCertificates_LotNumber 
    ON MaterialCertificates(TenantId, LotNumber);
```

**Service Implementation:**
```csharp
public class MaterialTraceabilityService : IMaterialTraceabilityService
{
    public async Task<MaterialCertificate> RegisterCertificateAsync(
        Guid tenantId, 
        string lotNumber, 
        CertificateData data)
    {
        var certificate = new MaterialCertificate
        {
            TenantId = tenantId,
            LotNumber = lotNumber,
            MaterialGrade = data.Grade,
            CertificateType = data.Type,
            ChemicalComposition = JsonSerializer.Serialize(data.Chemistry),
            MechanicalProperties = JsonSerializer.Serialize(data.Mechanics)
        };
        
        // Generate immutable hash
        certificate.DataHash = GenerateHash(certificate);
        certificate.IsImmutable = true;
        
        await _context.MaterialCertificates.AddAsync(certificate);
        await _context.SaveChangesAsync();
        
        return certificate;
    }
    
    private string GenerateHash(MaterialCertificate cert)
    {
        var data = $"{cert.LotNumber}|{cert.MaterialGrade}|{cert.CertificateNumber}|{cert.TestDate:O}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }
}
```

---

### 2. Welding Documentation (WPS/WPQR)

**Database Schema:**
```sql
CREATE TABLE WeldingProcedures (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    WPSNumber NVARCHAR(50) NOT NULL,
    ProcessType NVARCHAR(50) NOT NULL, -- 'MIG', 'TIG', 'SMAW'
    BaseMaterial NVARCHAR(100) NOT NULL,
    FillerMaterial NVARCHAR(100) NOT NULL,
    JointType NVARCHAR(50) NOT NULL,
    WeldingPosition NVARCHAR(20) NOT NULL, -- 'PA', 'PB', 'PC', 'PD'
    PreheatingTemp INT,
    InterpassTemp INT,
    ApprovedBy NVARCHAR(200),
    ApprovedDate DATETIME2,
    DataHash NVARCHAR(64),
    IsImmutable BIT DEFAULT 0
);

CREATE TABLE WelderQualifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    WelderNumber NVARCHAR(50) NOT NULL,
    WelderName NVARCHAR(200) NOT NULL,
    WPSId UNIQUEIDENTIFIER NOT NULL,
    QualificationDate DATETIME2 NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    CertificateNumber NVARCHAR(100),
    TestResults NVARCHAR(MAX), -- JSON
    DataHash NVARCHAR(64),
    IsImmutable BIT DEFAULT 0,
    FOREIGN KEY (WPSId) REFERENCES WeldingProcedures(Id)
);
```

---

### 3. Non-Conformance Reports (NCR)

**Workflow:**
```
1. Create NCR → 2. Review → 3. Disposition → 4. Corrective Action → 5. Verify → 6. Close
```

**Database Schema:**
```sql
CREATE TABLE NonConformanceReports (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    NCRNumber NVARCHAR(50) NOT NULL,
    ProjectId UNIQUEIDENTIFIER,
    DetectedDate DATETIME2 NOT NULL,
    DetectedBy NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Severity NVARCHAR(20) NOT NULL, -- 'Critical', 'Major', 'Minor'
    Status NVARCHAR(20) NOT NULL, -- 'Open', 'InReview', 'ActionRequired', 'Closed'
    RootCause NVARCHAR(MAX),
    Disposition NVARCHAR(20), -- 'Rework', 'Repair', 'UseAsIs', 'Scrap'
    CorrectiveAction NVARCHAR(MAX),
    VerifiedBy NVARCHAR(200),
    VerifiedDate DATETIME2,
    ClosedBy NVARCHAR(200),
    ClosedDate DATETIME2,
    DataHash NVARCHAR(64),
    IsImmutable BIT DEFAULT 0
);
```

**Service Implementation:**
```csharp
public class NCRService : INCRService
{
    public async Task<NCR> CreateNCRAsync(Guid tenantId, NCRCreateDto dto)
    {
        var ncr = new NonConformanceReport
        {
            TenantId = tenantId,
            NCRNumber = await GenerateNCRNumberAsync(tenantId),
            DetectedDate = DateTime.UtcNow,
            DetectedBy = dto.DetectedBy,
            Description = dto.Description,
            Severity = dto.Severity,
            Status = "Open"
        };
        
        await _context.NonConformanceReports.AddAsync(ncr);
        await _context.SaveChangesAsync();
        
        // Log audit trail
        await _auditService.LogAsync(tenantId, "NCR_CREATED", ncr.Id);
        
        return ncr;
    }
    
    public async Task<NCR> DispositionNCRAsync(Guid ncrId, string disposition, string action)
    {
        var ncr = await _context.NonConformanceReports.FindAsync(ncrId);
        
        // Make immutable before disposition
        ncr.DataHash = GenerateHash(ncr);
        ncr.IsImmutable = true;
        
        ncr.Disposition = disposition;
        ncr.CorrectiveAction = action;
        ncr.Status = "ActionRequired";
        
        await _context.SaveChangesAsync();
        await _auditService.LogAsync(ncr.TenantId, "NCR_DISPOSITION", ncrId);
        
        return ncr;
    }
}
```

---

### 4. Immutable Audit Trails

**Implementation:**
```csharp
public abstract class AuditableEntity
{
    public string DataHash { get; set; }
    public DateTime? HashGeneratedAt { get; set; }
    public bool IsImmutable { get; set; }
    
    public void MakeImmutable()
    {
        if (IsImmutable)
            throw new InvalidOperationException("Entity is already immutable");
            
        DataHash = GenerateHash();
        HashGeneratedAt = DateTime.UtcNow;
        IsImmutable = true;
    }
    
    protected virtual string GenerateHash()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }
    
    public bool VerifyHash()
    {
        if (!IsImmutable) return true;
        return DataHash == GenerateHash();
    }
}
```

**Audit Log Table:**
```sql
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    UserId NVARCHAR(100) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    OldValues NVARCHAR(MAX), -- JSON
    NewValues NVARCHAR(MAX), -- JSON
    DataHash NVARCHAR(64),
    PreviousHash NVARCHAR(64), -- Blockchain-style linking
    INDEX IX_AuditLogs_Entity (TenantId, EntityType, EntityId),
    INDEX IX_AuditLogs_Timestamp (TenantId, Timestamp DESC)
);
```

---

### 5. Document Version Control

**Database Schema:**
```sql
CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    DocumentNumber NVARCHAR(50) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    DocumentType NVARCHAR(50) NOT NULL,
    CurrentVersionId UNIQUEIDENTIFIER,
    Status NVARCHAR(20) NOT NULL
);

CREATE TABLE DocumentVersions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    VersionNumber NVARCHAR(20) NOT NULL,
    RevisionDate DATETIME2 NOT NULL,
    Author NVARCHAR(200) NOT NULL,
    ReviewedBy NVARCHAR(200),
    ApprovedBy NVARCHAR(200),
    ApprovedDate DATETIME2,
    Changes NVARCHAR(MAX),
    FileUrl NVARCHAR(500),
    DataHash NVARCHAR(64),
    IsSuperseded BIT DEFAULT 0,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id)
);
```

---

## 🚀 Implementation Phases

### Phase 1: Core Traceability (Week 1-2)
```bash
# Database migrations
cd SteelAxis.Data
dotnet ef migrations add AddMaterialCertificates
dotnet ef migrations add AddWeldingProcedures
dotnet ef database update
```

**Tasks:**
- [ ] Create MaterialCertificates table
- [ ] Create WeldingProcedures table
- [ ] Create WelderQualifications table
- [ ] Implement MaterialTraceabilityService
- [ ] Add lot number validation
- [ ] Create certificate upload UI

### Phase 2: NCR System (Week 3-4)
**Tasks:**
- [ ] Create NonConformanceReports table
- [ ] Implement NCRService with workflow
- [ ] Create NCR form UI
- [ ] Add disposition workflow
- [ ] Implement corrective action tracking
- [ ] Create NCR dashboard

### Phase 3: Immutability & Audit (Week 5-6)
**Tasks:**
- [ ] Add SHA-256 hashing to entities
- [ ] Create AuditLogs table
- [ ] Implement audit trail service
- [ ] Add hash verification endpoints
- [ ] Create audit log viewer UI
- [ ] Implement blockchain-style linking

### Phase 4: Document Control (Week 7-8)
**Tasks:**
- [ ] Create Documents & DocumentVersions tables
- [ ] Implement version control service
- [ ] Add approval workflow
- [ ] Create document upload UI
- [ ] Implement superseded document handling
- [ ] Add version comparison view

---

## 📊 Compliance Checklist

### Material Traceability
- [ ] Lot numbers tracked for all materials
- [ ] Certificates stored (EN 10204 3.1/3.2)
- [ ] Chemical composition recorded
- [ ] Mechanical properties documented
- [ ] Supplier information maintained
- [ ] Immutable hash generated for each certificate

### Welding Documentation
- [ ] WPS documented for all welding processes
- [ ] WPQR maintained for all welders
- [ ] Qualification expiry dates tracked
- [ ] Test results recorded
- [ ] Approval signatures captured
- [ ] Welding position codes documented

### Non-Conformance Management
- [ ] NCR system implemented
- [ ] Root cause analysis documented
- [ ] Dispositions tracked (Rework/Repair/UseAsIs/Scrap)
- [ ] Corrective actions recorded
- [ ] Verification process in place
- [ ] Closed NCRs archived immutably

### Audit & Traceability
- [ ] All changes logged in audit trail
- [ ] SHA-256 hashing for critical records
- [ ] Hash verification available
- [ ] Blockchain-style linking (optional)
- [ ] Audit reports available
- [ ] 10-year retention policy

### Document Control
- [ ] Version control system in place
- [ ] Approval workflow implemented
- [ ] Superseded documents flagged
- [ ] Document history maintained
- [ ] Digital signatures (optional)
- [ ] Access control by role

---

## 💻 Code Examples

### Verify Material Certificate
```csharp
public async Task<bool> VerifyCertificateAsync(Guid certificateId)
{
    var cert = await _context.MaterialCertificates.FindAsync(certificateId);
    if (cert == null || !cert.IsImmutable) return false;
    
    var currentHash = GenerateHash(cert);
    return cert.DataHash == currentHash;
}
```

### Create NCR with Auto-numbering
```csharp
private async Task<string> GenerateNCRNumberAsync(Guid tenantId)
{
    var year = DateTime.UtcNow.Year;
    var count = await _context.NonConformanceReports
        .CountAsync(n => n.TenantId == tenantId && n.DetectedDate.Year == year);
    
    return $"NCR-{year}-{(count + 1):D4}"; // NCR-2025-0001
}
```

### Audit Trail Query
```csharp
public async Task<List<AuditLog>> GetAuditTrailAsync(
    Guid tenantId, 
    string entityType, 
    Guid entityId)
{
    return await _context.AuditLogs
        .Where(a => a.TenantId == tenantId 
                 && a.EntityType == entityType 
                 && a.EntityId == entityId)
        .OrderBy(a => a.Timestamp)
        .ToListAsync();
}
```

---

## 🔗 API Endpoints

```csharp
// Material Certificates
POST   /api/materials/certificates
GET    /api/materials/certificates/{id}
GET    /api/materials/certificates/lot/{lotNumber}
POST   /api/materials/certificates/{id}/verify

// NCR Management
POST   /api/ncr
GET    /api/ncr/{id}
GET    /api/ncr/open
PUT    /api/ncr/{id}/disposition
PUT    /api/ncr/{id}/close

// Welding
POST   /api/welding/procedures
GET    /api/welding/procedures/{id}
POST   /api/welding/qualifications
GET    /api/welding/qualifications/welder/{welderNumber}

// Audit
GET    /api/audit/{entityType}/{entityId}
GET    /api/audit/verify/{entityId}
```

---

## 📈 Reporting Requirements

### Required Reports
1. **Material Traceability Report** - All materials used in project
2. **NCR Summary** - Open and closed NCRs by period
3. **Welder Qualification Status** - Expiring qualifications
4. **Document Status Report** - Pending approvals
5. **Audit Trail Report** - Changes by user/date

### Export Formats
- PDF (for regulatory submission)
- Excel (for analysis)
- JSON (for integration)

---

## 🎓 Training Requirements

### Personnel Training
- [ ] Quality managers: Full EN 1090 standard
- [ ] Production staff: Traceability procedures
- [ ] Welders: WPS requirements
- [ ] Inspectors: NCR system
- [ ] Administrators: System usage

### Documentation
- [ ] User manual for NCR system
- [ ] Traceability procedure document
- [ ] WPS/WPQR templates
- [ ] Audit trail guide

---

## 💰 Certification Benefits

✅ **Market Access:** Required for EU structural steel projects  
✅ **Quality Assurance:** Systematic quality management  
✅ **Legal Compliance:** Meet regulatory requirements  
✅ **Competitive Advantage:** Certification differentiates you  
✅ **Risk Reduction:** Documented processes reduce liability  
✅ **Customer Confidence:** Demonstrates commitment to quality

---

## 🔗 Related Documentation

- **[en-1090-requirements.md](./en-1090-requirements.md)** - Full standard text
- **[en-1090-ncr-management.md](./en-1090-ncr-management.md)** - Detailed NCR workflows
- **[en-1090-quick-reference.md](./en-1090-quick-reference.md)** - Quick lookup
- **[../inventory/inventory-lot-number-improvements.md](../inventory/inventory-lot-number-improvements.md)** - Lot tracking integration

---

## ✅ Next Steps

1. **Week 1-2:** Implement material traceability
2. **Week 3-4:** Build NCR system
3. **Week 5-6:** Add audit trails with hashing
4. **Week 7-8:** Document version control
5. **Week 9-10:** Testing and certification prep

---

**Total Implementation Time:** 10 weeks  
**Complexity:** High  
**Business Value:** Critical for EU market access

---

**Quality by design!** ⚙️✅
