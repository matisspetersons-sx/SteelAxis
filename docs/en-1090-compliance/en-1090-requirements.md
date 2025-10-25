# EN 1090 Compliance Requirements for Manimp Platform

**Document Version:** 1.0  
**Last Updated:** September 18, 2025  
**Scope:** Complete EN 1090 implementation requirements for steel construction manufacturing

---

## Table of Contents

1. [Overview](#overview)
2. [Execution Classes](#execution-classes)
3. [Material Requirements](#material-requirements)
4. [Traceability Requirements](#traceability-requirements)
5. [Quality Management System](#quality-management-system)
6. [Welding Requirements](#welding-requirements)
7. [Non-Destructive Testing (NDT)](#non-destructive-testing-ndt)
8. [Documentation Requirements](#documentation-requirements)
9. [CE Marking and Declaration of Performance](#ce-marking-and-declaration-of-performance)
10. [Factory Production Control (FPC)](#factory-production-control-fpc)
11. [Implementation Checklist](#implementation-checklist)
12. [Database Schema Requirements](#database-schema-requirements)
13. [User Interface Requirements](#user-interface-requirements)
14. [API Requirements](#api-requirements)
15. [Validation Rules](#validation-rules)

---

## Overview

EN 1090 is the European standard for the execution of steel structures and aluminium structures. It defines requirements for conformity assessment of structural components and covers the entire manufacturing process from material procurement to final delivery.

### Key Standards:
- **EN 1090-1:** Requirements for conformity assessment of structural components
- **EN 1090-2:** Technical requirements for steel structures
- **EN 1090-3:** Technical requirements for aluminium structures

### Regulatory Framework:
- **Construction Products Regulation (CPR) 305/2011**
- **CE Marking Requirements**
- **Declaration of Performance (DoP)**
- **Assessment and Verification of Constancy of Performance (AVCP)**

---

## Execution Classes

EN 1090-2 defines four execution classes (EXC) based on the consequences of failure and structural complexity.

### EXC1 - Low Consequence Class
**Characteristics:**
- Simple structures with low risk of failure consequences
- Basic quality requirements
- Minimal inspection and testing requirements

**Examples:**
- Simple industrial buildings
- Agricultural structures
- Basic warehouse structures

**Manimp Implementation:**
- Available in **Basic Tier** subscription
- Simplified quality control workflows
- Basic material traceability
- Standard documentation requirements

**Required Features:**
```
✓ Basic material tracking
✓ Simple project management
✓ Standard documentation
✓ Basic quality records
```

### EXC2 - Medium Consequence Class
**Characteristics:**
- Structures where failure could result in moderate consequences
- Enhanced quality requirements
- Regular inspection and testing

**Examples:**
- Office buildings
- Commercial structures
- Multi-story residential buildings

**Manimp Implementation:**
- Available in **Basic Tier** subscription (enhanced features)
- Enhanced quality control workflows
- Material batch tracking mandatory
- Certificate validation required

**Required Features:**
```
✓ Material batch/heat number tracking
✓ EN 10204 3.1 certificate management
✓ Enhanced quality control workflows
✓ Regular inspection scheduling
✓ Non-compliance tracking
```

### EXC3 - High Consequence Class
**Characteristics:**
- Structures where failure could result in serious consequences
- Comprehensive quality requirements
- Extensive inspection and testing

**Examples:**
- Bridges
- Industrial facilities
- High-rise buildings
- Stadiums and public venues

**Manimp Implementation:**
- Available in **Professional Tier** subscription
- Comprehensive traceability systems
- Advanced quality management
- Complete documentation control

**Required Features:**
```
✓ Complete material genealogy
✓ EN 10204 3.1 certificates mandatory
✓ Comprehensive weld tracking
✓ NDT scheduling and results
✓ Advanced quality control
✓ Supplier qualification
```

### EXC4 - Very High Consequence Class
**Characteristics:**
- Structures where failure could result in very serious consequences
- Most stringent quality requirements
- Comprehensive testing and documentation

**Examples:**
- Petrochemical plants
- Nuclear facilities
- Seismic zones (high seismicity)
- Critical infrastructure

**Manimp Implementation:**
- Available in **Enterprise Tier** subscription only
- Maximum traceability requirements
- Complete quality documentation
- Advanced supplier management

**Required Features:**
```
✓ Complete supply chain traceability
✓ EN 10204 3.2 certificates mandatory
✓ Country of origin tracking
✓ Advanced NDT requirements
✓ Complete welding documentation
✓ Supplier audit trails
✓ Environmental condition tracking
```

---

## Material Requirements

### Material Categories
EN 1090 specifies requirements for different material categories used in steel construction.

#### Structural Steel Products
**EN 10025 Series - Hot rolled products:**
- EN 10025-1: General technical delivery conditions
- EN 10025-2: Non-alloy structural steels (S235, S275, S355, S450)
- EN 10025-3: Normalized/normalized rolled weldable fine grain structural steels
- EN 10025-4: Thermomechanical rolled weldable fine grain structural steels
- EN 10025-5: Weather resistant structural steels
- EN 10025-6: Flat products of high yield strength structural steels

**Database Implementation:**
```sql
-- Steel Grades table with EN standards
CREATE TABLE SteelGrades (
    SteelGradeId INT IDENTITY(1,1) PRIMARY KEY,
    StandardReference NVARCHAR(50) NOT NULL, -- e.g., 'EN 10025-2'
    GradeDesignation NVARCHAR(20) NOT NULL,  -- e.g., 'S355JR'
    YieldStrength INT NOT NULL,              -- N/mm²
    TensileStrength INT NOT NULL,            -- N/mm²
    ElongationMin DECIMAL(4,2),              -- %
    CharpyImpactEnergy INT,                  -- J at test temperature
    TestTemperature INT,                     -- °C
    CarbonEquivalent DECIMAL(4,2),           -- CEV
    IsWeatherResistant BIT DEFAULT 0,
    ApplicationNotes NVARCHAR(500)
);
```

#### Material Test Certificates
**EN 10204 Certificate Types:**

| Type | Description | Third Party | Manimp Tier Requirement |
|------|-------------|-------------|-------------------------|
| 2.1  | Test report based on routine tests | No | Basic (Optional) |
| 2.2  | Test report based on routine tests | Yes | Professional |
| 3.1  | Inspection certificate based on specific tests | Yes | Professional (EXC3) |
| 3.2  | Inspection certificate based on specific tests | Yes | Enterprise (EXC4) |

**Database Implementation:**
```sql
-- Material certificates tracking
CREATE TABLE MaterialCertificates (
    CertificateId INT IDENTITY(1,1) PRIMARY KEY,
    ProfileInventoryId INT NOT NULL,
    CertificateNumber NVARCHAR(100) NOT NULL,
    CertificateType NVARCHAR(10) NOT NULL, -- '2.1', '2.2', '3.1', '3.2'
    IssuingBody NVARCHAR(200) NOT NULL,
    IssueDate DATE NOT NULL,
    ExpiryDate DATE,
    DocumentPath NVARCHAR(500),
    ChemicalComposition NVARCHAR(1000), -- JSON format
    MechanicalProperties NVARCHAR(1000), -- JSON format
    TestResults NVARCHAR(2000),          -- JSON format
    IsValid BIT DEFAULT 1,
    CONSTRAINT FK_MaterialCertificates_ProfileInventory 
        FOREIGN KEY (ProfileInventoryId) REFERENCES ProfileInventories(ProfileInventoryId)
);
```

---

## Traceability Requirements

### Material Traceability
Complete traceability from raw material to finished product is essential for EN 1090 compliance.

#### Heat/Batch Tracking
**Requirements by Execution Class:**
- **EXC1:** Basic lot identification
- **EXC2:** Heat/batch number tracking
- **EXC3:** Complete heat genealogy with test results
- **EXC4:** Full supply chain traceability including country of origin

**Database Implementation:**
```sql
-- Enhanced ProfileInventories with traceability
ALTER TABLE ProfileInventories ADD (
    MaterialBatch NVARCHAR(100),              -- Heat/batch number
    CountryOfOrigin NVARCHAR(100),            -- Required for EXC4
    ManufacturingRoute NVARCHAR(50),          -- 'Hot Rolled', 'Cold Formed'
    SurfaceCondition NVARCHAR(50),            -- 'As Rolled', 'Shot Blasted'
    MaterialStandard NVARCHAR(50),            -- e.g., 'EN 10025-2'
    TraceabilityLevel INT NOT NULL DEFAULT 1, -- 1-4 based on EXC class
    TraceabilityNotes NVARCHAR(2000)
);

-- Material genealogy tracking
CREATE TABLE MaterialGenealogy (
    GenealogyId INT IDENTITY(1,1) PRIMARY KEY,
    ProfileInventoryId INT NOT NULL,
    ParentMaterialId INT,                     -- For processed materials
    ProcessStep NVARCHAR(100),                -- 'Rolling', 'Cutting', 'Forming'
    ProcessDate DATETIME2 NOT NULL,
    ProcessedBy NVARCHAR(100),
    QualityCheckId INT,                       -- Reference to quality check
    Notes NVARCHAR(500),
    CONSTRAINT FK_MaterialGenealogy_ProfileInventory 
        FOREIGN KEY (ProfileInventoryId) REFERENCES ProfileInventories(ProfileInventoryId)
);
```

### Supplier Qualification
Suppliers must be qualified and monitored for EN 1090 compliance.

**Database Implementation:**
```sql
-- Enhanced supplier tracking
CREATE TABLE SupplierQualifications (
    SupplierQualificationId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL,
    QualificationType NVARCHAR(50) NOT NULL,  -- 'EN 1090-1', 'ISO 9001'
    CertificateNumber NVARCHAR(100),
    CertifyingBody NVARCHAR(200),
    IssueDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    ScopeOfCertification NVARCHAR(1000),
    DocumentPath NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- Supplier audit records
CREATE TABLE SupplierAudits (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL,
    AuditDate DATE NOT NULL,
    AuditorName NVARCHAR(100) NOT NULL,
    AuditType NVARCHAR(50),                   -- 'Initial', 'Surveillance', 'Re-certification'
    AuditResult NVARCHAR(20),                 -- 'Pass', 'Conditional', 'Fail'
    FindingsCount INT DEFAULT 0,
    CorrectiveActionsRequired INT DEFAULT 0,
    NextAuditDue DATE,
    ReportPath NVARCHAR(500),
    Notes NVARCHAR(1000)
);
```

---

## Quality Management System

### Quality Control Framework
EN 1090 requires a comprehensive quality management system covering all aspects of production.

#### Quality Control Points
**Mandatory inspection points:**
1. **Incoming material inspection**
2. **Cutting and preparation checks**
3. **Fit-up and assembly verification**
4. **Welding process monitoring**
5. **Post-weld inspection**
6. **Surface treatment verification**
7. **Final dimensional checks**
8. **Packaging and dispatch verification**

**Database Implementation:**
```sql
-- Quality control checkpoints
CREATE TABLE QualityControlCheckpoints (
    CheckpointId INT IDENTITY(1,1) PRIMARY KEY,
    AssemblyId INT NOT NULL,
    CheckpointType NVARCHAR(100) NOT NULL,
    ExecutionClass NVARCHAR(10) NOT NULL,
    IsRequired BIT NOT NULL DEFAULT 1,
    SequenceOrder INT NOT NULL,
    Description NVARCHAR(500),
    StandardReference NVARCHAR(100),          -- EN 1090 section reference
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Quality inspection records
CREATE TABLE QualityInspectionRecords (
    InspectionId INT IDENTITY(1,1) PRIMARY KEY,
    CheckpointId INT NOT NULL,
    AssemblyId INT NOT NULL,
    InspectorName NVARCHAR(100) NOT NULL,
    InspectionDate DATETIME2 NOT NULL,
    InspectionResult NVARCHAR(20) NOT NULL,   -- 'Pass', 'Fail', 'Conditional'
    DefectsFound INT DEFAULT 0,
    CorrectiveActionRequired BIT DEFAULT 0,
    InspectionNotes NVARCHAR(2000),
    DocumentationPath NVARCHAR(500),
    ApprovedBy NVARCHAR(100),
    ApprovedDate DATETIME2,
    CONSTRAINT FK_QualityInspection_Checkpoint 
        FOREIGN KEY (CheckpointId) REFERENCES QualityControlCheckpoints(CheckpointId)
);
```

### Non-Conformance Management
Comprehensive system for handling non-conformances as required by EN 1090.

**Database Implementation:**
```sql
-- Non-conformance reports (NCR)
CREATE TABLE NonConformanceReports (
    NCRId INT IDENTITY(1,1) PRIMARY KEY,
    NCRNumber NVARCHAR(50) NOT NULL UNIQUE,
    AssemblyId INT,
    MaterialId INT,
    ProcessId INT,
    DetectedBy NVARCHAR(100) NOT NULL,
    DetectedDate DATETIME2 NOT NULL,
    NonConformanceType NVARCHAR(100) NOT NULL,
    SeverityLevel NVARCHAR(20) NOT NULL,      -- 'Critical', 'Major', 'Minor'
    Description NVARCHAR(2000) NOT NULL,
    RootCauseAnalysis NVARCHAR(2000),
    ImmediateAction NVARCHAR(1000),
    CorrectiveAction NVARCHAR(2000),
    PreventiveAction NVARCHAR(2000),
    ResponsiblePerson NVARCHAR(100),
    TargetCloseDate DATE,
    ActualCloseDate DATE,
    Status NVARCHAR(20) DEFAULT 'Open',       -- 'Open', 'In Progress', 'Closed', 'Verified'
    VerifiedBy NVARCHAR(100),
    VerificationDate DATETIME2,
    CustomerNotified BIT DEFAULT 0,
    CustomerNotificationDate DATETIME2,
    Cost DECIMAL(10,2),
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
```

---

## Welding Requirements

### Welding Procedure Specifications (WPS)
EN 1090-2 requires qualified welding procedures for all welding operations.

#### WPS Requirements by Execution Class:
- **EXC1:** Basic WPS for standard joints
- **EXC2:** Qualified WPS for all production joints
- **EXC3:** Qualified WPS with extended qualification range
- **EXC4:** Qualified WPS with full documentation and traceability

**Database Implementation:**
```sql
-- Welding Procedure Specifications
CREATE TABLE WeldingProcedureSpecs (
    WPSId INT IDENTITY(1,1) PRIMARY KEY,
    WPSNumber NVARCHAR(50) NOT NULL UNIQUE,
    WelderQualificationId INT,
    BaseMaterialGroup NVARCHAR(50) NOT NULL,
    WeldingProcess NVARCHAR(50) NOT NULL,     -- 'GMAW', 'SMAW', 'SAW', etc.
    ConsumableType NVARCHAR(100),
    JointType NVARCHAR(50),
    WeldingPosition NVARCHAR(20),             -- 'PA', 'PB', 'PC', 'PD', etc.
    PreheatingTemp INT,                       -- °C
    InterpassTemp INT,                        -- °C
    PostWeldHeatTreatment NVARCHAR(200),
    QualificationStandard NVARCHAR(50),       -- 'EN ISO 15614-1'
    QualificationDate DATE NOT NULL,
    ValidUntil DATE,
    ApprovedBy NVARCHAR(100) NOT NULL,
    ExecutionClassApplicable NVARCHAR(20),    -- 'EXC1,EXC2,EXC3,EXC4'
    DocumentPath NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Welder qualifications
CREATE TABLE WelderQualifications (
    WelderQualificationId INT IDENTITY(1,1) PRIMARY KEY,
    WelderName NVARCHAR(100) NOT NULL,
    WelderNumber NVARCHAR(50) NOT NULL UNIQUE,
    QualificationStandard NVARCHAR(50),       -- 'EN ISO 9606-1'
    WeldingProcess NVARCHAR(50) NOT NULL,
    MaterialGroup NVARCHAR(50) NOT NULL,
    ThicknessRange NVARCHAR(50),              -- '3-15mm'
    DiameterRange NVARCHAR(50),               -- For pipe welding
    WeldingPosition NVARCHAR(50),
    QualificationDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    TestingBody NVARCHAR(200),
    CertificateNumber NVARCHAR(100),
    CertificatePath NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- Welding records for production
CREATE TABLE WeldingRecords (
    WeldingRecordId INT IDENTITY(1,1) PRIMARY KEY,
    AssemblyId INT NOT NULL,
    WeldId NVARCHAR(50) NOT NULL,             -- Unique weld identifier
    WPSId INT NOT NULL,
    WelderQualificationId INT NOT NULL,
    WeldingDate DATE NOT NULL,
    WeldingStartTime TIME,
    WeldingEndTime TIME,
    PreheatingApplied BIT DEFAULT 0,
    PreheatingTemp INT,
    InterpassTemp INT,
    AmbientTemp INT,
    Humidity DECIMAL(5,2),                    -- %
    WeatherConditions NVARCHAR(100),
    ConsumableBatch NVARCHAR(100),
    WeldingParameters NVARCHAR(1000),         -- JSON format for voltage, current, etc.
    QualityCheckPassed BIT,
    InspectorName NVARCHAR(100),
    InspectionDate DATE,
    DefectsNoted NVARCHAR(1000),
    CorrectiveActionTaken NVARCHAR(1000),
    Notes NVARCHAR(1000),
    CONSTRAINT FK_WeldingRecords_WPS 
        FOREIGN KEY (WPSId) REFERENCES WeldingProcedureSpecs(WPSId)
);
```

---

## Non-Destructive Testing (NDT)

### NDT Requirements by Execution Class
EN 1090-2 specifies different levels of NDT based on execution class and joint significance.

#### Testing Requirements:
- **EXC1:** Visual testing (VT) only for most applications
- **EXC2:** VT + limited penetrant testing (PT) or magnetic particle testing (MT)
- **EXC3:** VT + PT/MT + limited radiographic testing (RT) or ultrasonic testing (UT)
- **EXC4:** Comprehensive NDT including VT, PT/MT, RT/UT as required

**Database Implementation:**
```sql
-- NDT requirements matrix
CREATE TABLE NDTRequirements (
    NDTRequirementId INT IDENTITY(1,1) PRIMARY KEY,
    ExecutionClass NVARCHAR(10) NOT NULL,
    JointCategory NVARCHAR(50) NOT NULL,      -- 'A', 'B', 'C', 'D' per EN 1090-2
    WeldType NVARCHAR(50),                    -- 'Butt', 'Fillet', 'Partial penetration'
    TestingMethod NVARCHAR(50) NOT NULL,      -- 'VT', 'PT', 'MT', 'RT', 'UT'
    TestingExtent NVARCHAR(50),               -- 'All', '100%', '20%', '10%'
    StandardReference NVARCHAR(100),          -- EN ISO reference
    IsRequired BIT NOT NULL DEFAULT 1
);

-- NDT execution records
CREATE TABLE NDTRecords (
    NDTRecordId INT IDENTITY(1,1) PRIMARY KEY,
    WeldingRecordId INT NOT NULL,
    NDTRequirementId INT NOT NULL,
    TestingMethod NVARCHAR(50) NOT NULL,
    TestingDate DATE NOT NULL,
    TesterName NVARCHAR(100) NOT NULL,
    TesterQualification NVARCHAR(100),        -- Level II, Level III
    Equipment NVARCHAR(200),
    TestingStandard NVARCHAR(100),
    TestResult NVARCHAR(20) NOT NULL,         -- 'Pass', 'Fail', 'Retest Required'
    DefectType NVARCHAR(100),
    DefectSize NVARCHAR(100),
    DefectLocation NVARCHAR(200),
    AcceptanceCriteria NVARCHAR(500),
    ResultsDocumentPath NVARCHAR(500),
    RepairRequired BIT DEFAULT 0,
    RepairCompleted BIT DEFAULT 0,
    RetestRequired BIT DEFAULT 0,
    RetestCompleted BIT DEFAULT 0,
    FinalResult NVARCHAR(20),
    ApprovedBy NVARCHAR(100),
    ApprovalDate DATE,
    Notes NVARCHAR(1000),
    CONSTRAINT FK_NDTRecords_WeldingRecord 
        FOREIGN KEY (WeldingRecordId) REFERENCES WeldingRecords(WeldingRecordId)
);
```

---

## Documentation Requirements

### Essential Documentation per EN 1090
Comprehensive documentation requirements vary by execution class.

#### Document Categories:
1. **Quality Plan** - Project-specific quality requirements
2. **Inspection and Test Plan (ITP)** - Detailed testing schedule
3. **Material Certificates** - All material documentation
4. **Welding Documentation** - WPS, welder certificates, welding records
5. **NDT Records** - All testing documentation
6. **Non-Conformance Reports** - Quality issues and resolutions
7. **Final Inspection Report** - Project completion documentation
8. **Declaration of Performance (DoP)** - CE marking documentation

**Database Implementation:**
```sql
-- Document management system
CREATE TABLE ProjectDocuments (
    DocumentId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    DocumentType NVARCHAR(100) NOT NULL,
    DocumentNumber NVARCHAR(100),
    DocumentTitle NVARCHAR(200) NOT NULL,
    DocumentVersion NVARCHAR(20) DEFAULT '1.0',
    FilePath NVARCHAR(500),
    FileSize BIGINT,
    MimeType NVARCHAR(100),
    CreatedBy NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME2,
    ApprovedBy NVARCHAR(100),
    ApprovalDate DATE,
    ExpiryDate DATE,
    ExecutionClass NVARCHAR(10),
    IsControlled BIT DEFAULT 1,               -- Controlled document
    IsMandatory BIT DEFAULT 0,                -- Required by EN 1090
    DocumentStatus NVARCHAR(20) DEFAULT 'Draft', -- 'Draft', 'Review', 'Approved', 'Obsolete'
    AccessLevel NVARCHAR(20) DEFAULT 'Internal', -- 'Public', 'Internal', 'Confidential'
    ChecksumHash NVARCHAR(100),               -- File integrity verification
    Tags NVARCHAR(500)                        -- Searchable tags
);

-- Document relationships and dependencies
CREATE TABLE DocumentRelationships (
    RelationshipId INT IDENTITY(1,1) PRIMARY KEY,
    ParentDocumentId INT NOT NULL,
    ChildDocumentId INT NOT NULL,
    RelationshipType NVARCHAR(50) NOT NULL,   -- 'Supersedes', 'References', 'Supports'
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_DocumentRelationships_Parent 
        FOREIGN KEY (ParentDocumentId) REFERENCES ProjectDocuments(DocumentId),
    CONSTRAINT FK_DocumentRelationships_Child 
        FOREIGN KEY (ChildDocumentId) REFERENCES ProjectDocuments(DocumentId)
);
```

---

## CE Marking and Declaration of Performance

### CE Marking Requirements
All structural steel products covered by EN 1090 must have CE marking when placed on the European market.

#### DoP Requirements:
1. **Unique identification** of the construction product
2. **Intended use(s)** as foreseen by the manufacturer
3. **Name and address** of the manufacturer
4. **Authorized representative** (if applicable)
5. **AVCP system** applied
6. **Harmonized standard** applied
7. **Notified body** (if applicable)
8. **Essential characteristics** and performance levels
9. **Performance declaration date**
10. **Manufacturer signature**

**Database Implementation:**
```sql
-- Declaration of Performance (DoP)
CREATE TABLE DeclarationsOfPerformance (
    DoPId INT IDENTITY(1,1) PRIMARY KEY,
    DoPNumber NVARCHAR(100) NOT NULL UNIQUE,
    ProjectId INT NOT NULL,
    ProductIdentification NVARCHAR(200) NOT NULL,
    IntendedUse NVARCHAR(500) NOT NULL,
    ManufacturerName NVARCHAR(200) NOT NULL,
    ManufacturerAddress NVARCHAR(500) NOT NULL,
    AuthorizedRepresentative NVARCHAR(200),
    AVCPSystem NVARCHAR(10) NOT NULL,         -- '1+', '2+', '3', '4'
    HarmonizedStandard NVARCHAR(100) DEFAULT 'EN 1090-1+A1:2011',
    NotifiedBodyNumber NVARCHAR(20),
    NotifiedBodyName NVARCHAR(200),
    EssentialCharacteristics NVARCHAR(2000),  -- JSON format
    PerformanceDeclarationDate DATE NOT NULL,
    ValidFrom DATE NOT NULL,
    ValidUntil DATE,
    SignedBy NVARCHAR(100) NOT NULL,
    SignatureDate DATE NOT NULL,
    DocumentPath NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- CE marking labels
CREATE TABLE CEMarkingLabels (
    CEMarkId INT IDENTITY(1,1) PRIMARY KEY,
    AssemblyId INT NOT NULL,
    DoPId INT NOT NULL,
    CEMarkingNumber NVARCHAR(100) NOT NULL,
    ProductType NVARCHAR(100) NOT NULL,
    ExecutionClass NVARCHAR(10) NOT NULL,
    ManufacturingYear INT NOT NULL,
    LabelAppliedBy NVARCHAR(100) NOT NULL,
    LabelAppliedDate DATE NOT NULL,
    LabelLocation NVARCHAR(200),              -- Physical location on product
    QRCodeData NVARCHAR(500),                 -- For digital verification
    IsVisible BIT DEFAULT 1,
    PhotoDocumentPath NVARCHAR(500),          -- Photo of applied label
    CONSTRAINT FK_CEMarking_DoP 
        FOREIGN KEY (DoPId) REFERENCES DeclarationsOfPerformance(DoPId)
);
```

---

## Factory Production Control (FPC)

### FPC System Requirements
EN 1090-1 requires a Factory Production Control system to ensure consistent product quality.

#### FPC Elements:
1. **Organization structure** and responsibilities
2. **Documentation control** system
3. **Control of incoming materials**
4. **Control of production equipment**
5. **Control of production process**
6. **Testing and inspection procedures**
7. **Non-conforming product control**
8. **Corrective and preventive actions**
9. **Internal quality audits**
10. **Management review**

**Database Implementation:**
```sql
-- FPC control procedures
CREATE TABLE FPCProcedures (
    FPCProcedureId INT IDENTITY(1,1) PRIMARY KEY,
    ProcedureNumber NVARCHAR(50) NOT NULL UNIQUE,
    ProcedureTitle NVARCHAR(200) NOT NULL,
    ProcedureType NVARCHAR(100) NOT NULL,     -- Type of FPC element
    ApplicableExecutionClasses NVARCHAR(20),  -- 'EXC1,EXC2,EXC3,EXC4'
    ResponsibleRole NVARCHAR(100) NOT NULL,
    ProcedureDescription NVARCHAR(2000),
    ControlMethod NVARCHAR(500),
    AcceptanceCriteria NVARCHAR(1000),
    RecordingRequirements NVARCHAR(500),
    DocumentPath NVARCHAR(500),
    Version NVARCHAR(20) DEFAULT '1.0',
    EffectiveDate DATE NOT NULL,
    ReviewDate DATE,
    ApprovedBy NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1
);

-- FPC audit records
CREATE TABLE FPCAudits (
    FPCAuditId INT IDENTITY(1,1) PRIMARY KEY,
    AuditNumber NVARCHAR(50) NOT NULL UNIQUE,
    AuditType NVARCHAR(50) NOT NULL,          -- 'Internal', 'External', 'Customer'
    AuditDate DATE NOT NULL,
    AuditorName NVARCHAR(100) NOT NULL,
    AuditorQualification NVARCHAR(200),
    AuditScope NVARCHAR(1000),                -- Areas/processes audited
    FindingsCount INT DEFAULT 0,
    MajorNonconformancesCount INT DEFAULT 0,
    MinorNonconformancesCount INT DEFAULT 0,
    ObservationsCount INT DEFAULT 0,
    AuditResult NVARCHAR(20),                 -- 'Satisfactory', 'Conditional', 'Unsatisfactory'
    CorrectiveActionsRequired INT DEFAULT 0,
    FollowUpAuditRequired BIT DEFAULT 0,
    NextAuditDue DATE,
    ReportPath NVARCHAR(500),
    CompletedBy NVARCHAR(100),
    CompletionDate DATE,
    Notes NVARCHAR(1000)
);
```

---

## Implementation Checklist

### Phase 1: Foundation (Months 1-2)
- [ ] **Database Schema Implementation**
  - [ ] Create all EN 1090 related tables
  - [ ] Implement foreign key relationships
  - [ ] Add appropriate indexes for performance
  - [ ] Create data validation constraints
  
- [ ] **Basic Material Traceability**
  - [ ] Material batch/heat number tracking
  - [ ] Certificate management (EN 10204 types)
  - [ ] Country of origin tracking (EXC4)
  - [ ] Supplier qualification records

- [ ] **Execution Class Framework**
  - [ ] Project execution class assignment
  - [ ] Subscription tier validation
  - [ ] Feature access control by execution class
  - [ ] Monthly project limits enforcement

### Phase 2: Quality Management (Months 3-4)
- [ ] **Quality Control System**
  - [ ] Quality checkpoint definitions
  - [ ] Inspection record management
  - [ ] Non-conformance report system
  - [ ] Corrective action tracking

- [ ] **Welding Management**
  - [ ] WPS management system
  - [ ] Welder qualification tracking
  - [ ] Welding record management
  - [ ] Welding parameter validation

- [ ] **NDT Integration**
  - [ ] NDT requirement matrix
  - [ ] Testing schedule management
  - [ ] Test result recording
  - [ ] Defect tracking and resolution

### Phase 3: Documentation & Compliance (Months 5-6)
- [ ] **Document Management**
  - [ ] Controlled document system
  - [ ] Document approval workflows
  - [ ] Version control and change management
  - [ ] Document search and retrieval

- [ ] **CE Marking System**
  - [ ] Declaration of Performance generation
  - [ ] CE marking label management
  - [ ] Product identification system
  - [ ] Digital verification capabilities

- [ ] **FPC Implementation**
  - [ ] FPC procedure management
  - [ ] Internal audit system
  - [ ] Performance monitoring
  - [ ] Management review processes

---

## Database Schema Requirements

### Core Tables Summary

```sql
-- Essential EN 1090 tables
1. SteelGrades                      -- Material specifications
2. MaterialCertificates             -- EN 10204 certificates
3. MaterialGenealogy               -- Material traceability chain
4. SupplierQualifications          -- Supplier certifications
5. SupplierAudits                  -- Supplier audit records
6. QualityControlCheckpoints       -- Inspection points
7. QualityInspectionRecords        -- Inspection results
8. NonConformanceReports           -- NCR management
9. WeldingProcedureSpecs          -- WPS management
10. WelderQualifications          -- Welder certificates
11. WeldingRecords                -- Production welding records
12. NDTRequirements               -- Testing requirements
13. NDTRecords                    -- Testing results
14. ProjectDocuments              -- Document management
15. DocumentRelationships         -- Document dependencies
16. DeclarationsOfPerformance     -- CE marking DoP
17. CEMarkingLabels              -- Physical CE marks
18. FPCProcedures                -- Factory production control
19. FPCAudits                    -- FPC audit records
```

### Data Relationships

```sql
-- Key relationships
Projects 1:* AssemblyProgress
AssemblyProgress 1:* QualityInspectionRecords
AssemblyProgress 1:* WeldingRecords
WeldingRecords 1:* NDTRecords
ProfileInventories 1:* MaterialCertificates
ProfileInventories 1:* MaterialGenealogy
Suppliers 1:* SupplierQualifications
Projects 1:* DeclarationsOfPerformance
Assemblies 1:* CEMarkingLabels
```

---

## User Interface Requirements

### Dashboard Requirements

#### Executive Dashboard
- **EN 1090 Compliance Status** by project and execution class
- **Certificate Expiry Alerts** for materials and qualifications
- **Non-Conformance Trends** with severity breakdown
- **Audit Status Overview** with upcoming audit notifications

#### Quality Manager Dashboard
- **Active Quality Control Points** requiring attention
- **NDT Scheduling Calendar** with resource allocation
- **Welder Qualification Status** with expiry tracking
- **Document Control Status** with pending approvals

#### Production Dashboard
- **Assembly Progress Status** with EN 1090 checkpoints
- **Material Availability** with traceability status
- **Welding Schedule** with qualified welder assignments
- **Quality Hold Items** requiring corrective action

### Form Requirements

#### Material Entry Forms
```
✓ Material specification with steel grade validation
✓ Certificate upload with type verification (2.1, 2.2, 3.1, 3.2)
✓ Batch/heat number entry with format validation
✓ Country of origin selection (required for EXC4)
✓ Traceability level assignment based on execution class
```

#### Quality Control Forms
```
✓ Inspection checkpoint selection from predefined list
✓ Inspector qualification verification
✓ Pass/fail result with defect recording capability
✓ Photo upload for visual documentation
✓ Digital signature for inspector approval
```

#### Welding Forms
```
✓ WPS selection with validation against material combination
✓ Welder qualification verification
✓ Welding parameter entry with range validation
✓ Environmental condition recording
✓ Quality check results with defect notation
```

---

## API Requirements

### RESTful API Endpoints

#### Material Traceability APIs
```http
GET /api/materials/{id}/traceability
GET /api/materials/certificates/{certificateId}
POST /api/materials/{id}/certificates
PUT /api/materials/{id}/traceability
DELETE /api/materials/certificates/{certificateId}
```

#### Quality Management APIs
```http
GET /api/quality/checkpoints/project/{projectId}
POST /api/quality/inspections
PUT /api/quality/inspections/{inspectionId}
GET /api/quality/ncr
POST /api/quality/ncr
PUT /api/quality/ncr/{ncrId}/close
```

#### Welding Management APIs
```http
GET /api/welding/wps/active
GET /api/welding/welders/qualified/{processType}
POST /api/welding/records
GET /api/welding/records/assembly/{assemblyId}
PUT /api/welding/records/{recordId}/inspection
```

#### Document Management APIs
```http
GET /api/documents/project/{projectId}
POST /api/documents/upload
GET /api/documents/{documentId}/download
PUT /api/documents/{documentId}/approve
DELETE /api/documents/{documentId}
```

#### CE Marking APIs
```http
POST /api/ce-marking/dop
GET /api/ce-marking/dop/{dopId}
POST /api/ce-marking/labels
GET /api/ce-marking/verify/{markingNumber}
```

---

## Validation Rules

### Material Validation Rules

#### Certificate Type Validation by Execution Class:
```csharp
public static class EN1090ValidationRules
{
    public static bool ValidateCertificateType(ExecutionClass execClass, string certificateType)
    {
        return execClass switch
        {
            ExecutionClass.EXC1 => true, // Any certificate type acceptable
            ExecutionClass.EXC2 => certificateType is "2.2" or "3.1" or "3.2",
            ExecutionClass.EXC3 => certificateType is "3.1" or "3.2",
            ExecutionClass.EXC4 => certificateType == "3.2",
            _ => false
        };
    }
    
    public static bool ValidateBatchTrackingRequired(ExecutionClass execClass)
    {
        return execClass >= ExecutionClass.EXC2;
    }
    
    public static bool ValidateCountryOfOriginRequired(ExecutionClass execClass)
    {
        return execClass == ExecutionClass.EXC4;
    }
}
```

#### NDT Requirements Validation:
```csharp
public static class NDTValidationRules
{
    public static List<string> GetRequiredNDTMethods(ExecutionClass execClass, string jointCategory)
    {
        return execClass switch
        {
            ExecutionClass.EXC1 => new List<string> { "VT" },
            ExecutionClass.EXC2 => jointCategory == "A" 
                ? new List<string> { "VT", "PT" } 
                : new List<string> { "VT" },
            ExecutionClass.EXC3 => jointCategory switch
            {
                "A" => new List<string> { "VT", "PT", "RT" },
                "B" => new List<string> { "VT", "PT" },
                _ => new List<string> { "VT" }
            },
            ExecutionClass.EXC4 => jointCategory switch
            {
                "A" => new List<string> { "VT", "PT", "RT", "UT" },
                "B" => new List<string> { "VT", "PT", "RT" },
                "C" => new List<string> { "VT", "PT" },
                _ => new List<string> { "VT" }
            },
            _ => new List<string>()
        };
    }
}
```

### Quality Validation Rules

#### Welder Qualification Validation:
```csharp
public static class WelderValidationRules
{
    public static bool ValidateWelderQualification(WelderQualification qualification, 
        WeldingProcedureSpec wps, DateTime weldingDate)
    {
        // Check if qualification is current
        if (qualification.ExpiryDate < weldingDate)
            return false;
            
        // Check if welding process matches
        if (qualification.WeldingProcess != wps.WeldingProcess)
            return false;
            
        // Check if material group is covered
        if (!qualification.MaterialGroup.Contains(wps.BaseMaterialGroup))
            return false;
            
        // Check if welding position is qualified
        if (!qualification.WeldingPosition.Contains(wps.WeldingPosition))
            return false;
            
        return true;
    }
}
```

---

## Conclusion

This comprehensive EN 1090 implementation guide provides the complete framework for ensuring your Manimp platform meets all European steel construction standards. The implementation should be done in phases, starting with the foundational elements and progressing to the advanced compliance features.

### Key Success Factors:
1. **Proper execution class implementation** with subscription tier alignment
2. **Complete material traceability** from supplier to final product
3. **Comprehensive quality management** with automated validation
4. **Robust documentation system** with controlled access and approval workflows
5. **Integrated CE marking process** for regulatory compliance

### Next Steps:
1. **Review and approve** this requirements document
2. **Begin Phase 1 implementation** with database schema and basic traceability
3. **Establish quality management processes** in Phase 2
4. **Complete documentation and compliance systems** in Phase 3

This implementation will position Manimp as the leading EN 1090 compliant steel construction management platform in the European market.

---

*This document should be updated as EN 1090 standards evolve and new requirements are published.*