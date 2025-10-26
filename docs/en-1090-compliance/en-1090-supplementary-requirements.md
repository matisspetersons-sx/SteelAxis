# EN 1090 Supplementary Requirements - Missing Elements

**Document Version:** 1.0  
**Last Updated:** September 18, 2025  
**Scope:** Additional EN 1090 requirements not covered in primary documentation

---

## Table of Contents

1. [Overview of Missing Requirements](#overview-of-missing-requirements)
2. [Corrosion Protection and Surface Treatment](#corrosion-protection-and-surface-treatment)
3. [Bolting and Fastener Systems](#bolting-and-fastener-systems)
4. [Thermal Cutting Requirements](#thermal-cutting-requirements)
5. [Coating and Paint Systems](#coating-and-paint-systems)
6. [Personnel Qualification Requirements](#personnel-qualification-requirements)
7. [Equipment and Machinery Requirements](#equipment-and-machinery-requirements)
8. [Environmental Considerations](#environmental-considerations)
9. [Stainless Steel Specific Requirements](#stainless-steel-specific-requirements)
10. [Initial Type Testing (ITT)](#initial-type-testing-itt)
11. [Database Schema Extensions](#database-schema-extensions)
12. [Implementation Priority Matrix](#implementation-priority-matrix)

---

## Overview of Missing Requirements

After comprehensive analysis of EN 1090 standards and industry best practices, several critical requirements were identified that need to be incorporated into the Manimp platform for complete compliance:

### **Critical Missing Elements:**
1. **Corrosion Protection Systems** - Complete coating and surface treatment management
2. **Advanced Bolting Management** - Preloaded bolt systems and installation procedures
3. **Thermal Cutting Controls** - Surface quality and hardness requirements
4. **Personnel Qualification Tracking** - Comprehensive competency management
5. **Equipment Calibration Systems** - Tool and machinery certification
6. **Environmental Control Systems** - Atmospheric condition monitoring
7. **Stainless Steel Requirements** - Specialized processing and handling
8. **Initial Type Testing** - Product qualification procedures

---

## Corrosion Protection and Surface Treatment

### Corrosion Protection Categories

EN 1090 defines three corrosion protection categories that must be systematically managed:

#### **Category 1: Low Corrosion Risk**
- **Environment:** Indoor areas with dry atmosphere, no condensation
- **Protection Methods:** Light protective coatings, minimal surface preparation
- **Requirements:** Basic visual inspection, standard documentation
- **Execution Class Applicability:** Primarily EXC1, some EXC2

#### **Category 2: Moderate Corrosion Risk**  
- **Environment:** Outdoor areas with temperate climate, industrial environments
- **Protection Methods:** Standard paint systems, hot-dip galvanizing, thermal spraying
- **Requirements:** Detailed surface preparation, qualified personnel, comprehensive testing
- **Execution Class Applicability:** EXC2, EXC3

#### **Category 3: High Corrosion Risk**
- **Environment:** Marine environments, chemical plants, extreme industrial conditions
- **Protection Methods:** Advanced coating systems, duplex systems (galvanizing + painting)
- **Requirements:** Maximum surface preparation, expert supervision, extensive testing
- **Execution Class Applicability:** EXC3, EXC4

### Surface Preparation Requirements

**Database Implementation:**
```sql
-- Corrosion protection management
CREATE TABLE CorrosionProtectionSystems (
    ProtectionSystemId INT IDENTITY(1,1) PRIMARY KEY,
    SystemName NVARCHAR(200) NOT NULL,
    ProtectionCategory INT NOT NULL,              -- 1, 2, or 3
    CorrosivityClass NVARCHAR(20) NOT NULL,       -- C1, C2, C3, C4, C5, CX per ISO 12944-2
    ExpectedLifetime INT NOT NULL,                -- Years
    SurfacePreparationGrade NVARCHAR(20) NOT NULL, -- Sa2, Sa2.5, Sa3 per ISO 8501-1
    CoatingSystemDescription NVARCHAR(1000),
    ApplicableExecutionClasses NVARCHAR(20),      -- 'EXC1,EXC2,EXC3,EXC4'
    RequiredQualifications NVARCHAR(500),
    TestingRequirements NVARCHAR(1000),
    EnvironmentalConditions NVARCHAR(500),        -- Temperature, humidity limits
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Surface preparation records
CREATE TABLE SurfacePreparationRecords (
    PreparationRecordId INT IDENTITY(1,1) PRIMARY KEY,
    AssemblyId INT NOT NULL,
    ProtectionSystemId INT NOT NULL,
    PreparationMethod NVARCHAR(100) NOT NULL,     -- Blast cleaning, pickling, etc.
    AchievedGrade NVARCHAR(20) NOT NULL,          -- Actual grade achieved
    SurfaceRoughness DECIMAL(5,2),                -- Ra value in micrometers
    CleanlinessLevel NVARCHAR(20),                -- Per ISO 8501-3
    PreparedBy NVARCHAR(100) NOT NULL,
    PreparationDate DATE NOT NULL,
    InspectedBy NVARCHAR(100),
    InspectionDate DATE,
    AmbientTemperature DECIMAL(4,1),              -- °C during preparation
    RelativeHumidity DECIMAL(4,1),               -- % during preparation
    DewPoint DECIMAL(4,1),                       -- °C
    SurfaceTemperature DECIMAL(4,1),             -- °C
    PreparationPhotos NVARCHAR(500),             -- Photo documentation paths
    QualityApproved BIT DEFAULT 0,
    Notes NVARCHAR(1000),
    CONSTRAINT FK_SurfacePrep_ProtectionSystem 
        FOREIGN KEY (ProtectionSystemId) REFERENCES CorrosionProtectionSystems(ProtectionSystemId)
);

-- Coating application records
CREATE TABLE CoatingApplicationRecords (
    CoatingRecordId INT IDENTITY(1,1) PRIMARY KEY,
    PreparationRecordId INT NOT NULL,
    CoatingType NVARCHAR(100) NOT NULL,          -- Primer, Intermediate, Topcoat
    CoatingProduct NVARCHAR(200) NOT NULL,
    BatchNumber NVARCHAR(100),
    ApplicationMethod NVARCHAR(100) NOT NULL,     -- Spray, Brush, Roller
    WetFilmThickness DECIMAL(5,1),               -- Micrometers
    DryFilmThickness DECIMAL(5,1),               -- Micrometers
    ApplicationDate DATE NOT NULL,
    AppliedBy NVARCHAR(100) NOT NULL,
    CuredDate DATE,
    AmbientConditions NVARCHAR(500),             -- Temperature, humidity during application
    AdhesionTestResult NVARCHAR(50),             -- Per ISO 4624
    CrossCutTestResult NVARCHAR(50),             -- Per ISO 2409
    InspectedBy NVARCHAR(100),
    InspectionDate DATE,
    CoatingApproved BIT DEFAULT 0,
    DefectsNoted NVARCHAR(1000),
    CorrectionRequired BIT DEFAULT 0,
    CONSTRAINT FK_CoatingApplication_SurfacePrep 
        FOREIGN KEY (PreparationRecordId) REFERENCES SurfacePreparationRecords(PreparationRecordId)
);
```

### Thermal Spraying (Metallization)

**Critical Requirements for Thermal Spraying:**
- **Surface Preparation:** Sa 3 (white metal) blast cleaning mandatory
- **Surface Roughness:** 75-150 μm Ra for optimal adhesion
- **Environmental Controls:** Temperature >5°C, relative humidity <85%
- **Application Speed:** Immediate coating after preparation (<4 hours)
- **Sealing Requirements:** Mandatory sealer coat within 2 hours

```sql
-- Thermal spraying records
CREATE TABLE ThermalSprayingRecords (
    SprayRecordId INT IDENTITY(1,1) PRIMARY KEY,
    PreparationRecordId INT NOT NULL,
    SprayMaterial NVARCHAR(100) NOT NULL,        -- Zinc, Aluminum, Zinc-Aluminum
    MaterialBatch NVARCHAR(100),
    SprayThickness DECIMAL(5,1) NOT NULL,        -- Micrometers
    SprayMethod NVARCHAR(50) NOT NULL,           -- Arc, Flame, Wire
    AdhesionTestResult DECIMAL(5,2),             -- N/mm² per ISO 4624
    PorosityLevel DECIMAL(4,2),                  -- % porosity
    UniformityCheck BIT DEFAULT 0,
    SealerApplied BIT DEFAULT 0,
    SealerType NVARCHAR(100),
    SealerThickness DECIMAL(5,1),
    OperatorQualification NVARCHAR(200),
    EquipmentCalibrationDate DATE,
    SprayingDate DATE NOT NULL,
    QualityApproved BIT DEFAULT 0
);
```

---

## Bolting and Fastener Systems

### Preloaded Bolt Management

EN 1090-2 requires sophisticated management of preloaded bolt systems for EXC2+ execution classes.

#### **Bolt System Types (EN 14399):**
- **System HR (High Resistance)** - Standard hexagon bolt assemblies
- **System HV (High Resistance Vespac)** - Short thread, thin nut design
- **System HRC (High Resistance Calibrated)** - Tension Control Bolts (TCB)

**Database Implementation:**
```sql
-- Bolt assembly specifications
CREATE TABLE BoltAssemblySpecs (
    BoltAssemblySpecId INT IDENTITY(1,1) PRIMARY KEY,
    BoltSystem NVARCHAR(20) NOT NULL,            -- HR, HV, HRC
    BoltGrade NVARCHAR(20) NOT NULL,             -- 8.8, 10.9
    NominalDiameter INT NOT NULL,                -- mm
    ThreadLength DECIMAL(5,2),                   -- mm
    KClass NVARCHAR(5) NOT NULL,                 -- K0, K1, K2 classification
    MinimumPreload DECIMAL(8,2) NOT NULL,       -- kN
    ProofLoad DECIMAL(8,2) NOT NULL,            -- kN
    TensileStrength DECIMAL(8,2) NOT NULL,      -- kN
    StandardReference NVARCHAR(50),              -- EN 14399 part
    CertificatePath NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- Preloaded bolt installation records
CREATE TABLE PreloadedBoltInstallation (
    InstallationId INT IDENTITY(1,1) PRIMARY KEY,
    AssemblyId INT NOT NULL,
    ConnectionId NVARCHAR(100) NOT NULL,
    BoltAssemblySpecId INT NOT NULL,
    BoltPosition NVARCHAR(50),                   -- Position identifier in connection
    TighteningMethod NVARCHAR(50) NOT NULL,      -- Torque, Combined, DTI, HRC
    TorqueValue DECIMAL(6,2),                    -- Nm if applicable
    TurnValue DECIMAL(4,2),                      -- Additional turns if applicable
    PreloadAchieved DECIMAL(8,2),               -- kN actual preload
    InstallerName NVARCHAR(100) NOT NULL,
    InstallerQualification NVARCHAR(200),
    InstallationDate DATE NOT NULL,
    TighteningSequence INT,                      -- Order of tightening in connection
    CalibrationCertificate NVARCHAR(200),       -- Equipment calibration reference
    InspectedBy NVARCHAR(100),
    InspectionDate DATE,
    InspectionResult NVARCHAR(20),               -- Pass, Fail, Retest Required
    Notes NVARCHAR(1000),
    CONSTRAINT FK_BoltInstallation_AssemblySpec 
        FOREIGN KEY (BoltAssemblySpecId) REFERENCES BoltAssemblySpecs(BoltAssemblySpecId)
);

-- Bolt tightening calibration records
CREATE TABLE BoltTighteningCalibration (
    CalibrationId INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentId NVARCHAR(100) NOT NULL,
    EquipmentType NVARCHAR(100) NOT NULL,        -- Torque wrench, Hydraulic tensioner, etc.
    CalibrationDate DATE NOT NULL,
    NextCalibrationDue DATE NOT NULL,
    CalibrationStandard NVARCHAR(100),           -- ISO 6789, etc.
    AccuracyClass DECIMAL(3,1),                  -- ±% accuracy
    CalibrationRange NVARCHAR(100),              -- Operating range
    CalibratedBy NVARCHAR(100) NOT NULL,
    CalibrationCertificate NVARCHAR(200),
    IsCurrentlyValid BIT DEFAULT 1
);
```

### Installation Methods and Validation

**Tightening Methods per EN 1090-2:**

1. **Torque Method (8.5.3):**
   - Requires K2 class bolts
   - ±4% accuracy torque wrench mandatory
   - Weekly calibration verification

2. **Combined Method (8.5.4):**
   - Initial torque + additional rotation
   - Requires K1 or K2 class bolts
   - Most common method for bridges

3. **Direct Tension Indicator (DTI) Method (8.5.6):**
   - Uses load-indicating washers
   - Suitable for all K-classes
   - Visual indication of achieved preload

4. **HRC Method (8.5.5):**
   - Tension Control Bolts with break-off spline
   - Requires specific installation tools
   - Automatic preload indication

---

## Thermal Cutting Requirements

### Surface Quality Control

EN 1090-2 specifies strict requirements for thermal cutting surface quality that vary by execution class.

**Database Implementation:**
```sql
-- Thermal cutting procedures
CREATE TABLE ThermalCuttingProcedures (
    CuttingProcedureId INT IDENTITY(1,1) PRIMARY KEY,
    ProcedureName NVARCHAR(200) NOT NULL,
    CuttingProcess NVARCHAR(100) NOT NULL,       -- Oxy-fuel, Plasma, Laser
    MaterialGradeRange NVARCHAR(200),            -- Applicable steel grades
    ThicknessRange NVARCHAR(100),               -- Applicable thickness range
    ExecutionClass NVARCHAR(10) NOT NULL,
    QualityClass NVARCHAR(10) NOT NULL,         -- Range 1-4 per ISO 9013
    PerpendicularityClass NVARCHAR(10),         -- u-class per ISO 9013
    RoughnessClass NVARCHAR(10),                -- Rz class per ISO 9013
    HardnessLimit INT,                          -- HV10 maximum
    CuttingSpeed DECIMAL(6,2),                  -- mm/min
    GasComposition NVARCHAR(200),
    PowerSetting DECIMAL(6,2),
    QualifiedBy NVARCHAR(100) NOT NULL,
    QualificationDate DATE NOT NULL,
    ProcedureApproved BIT DEFAULT 1,
    ReviewDate DATE,
    IsActive BIT DEFAULT 1
);

-- Thermal cutting execution records
CREATE TABLE ThermalCuttingRecords (
    CuttingRecordId INT IDENTITY(1,1) PRIMARY KEY,
    MaterialId INT NOT NULL,
    CuttingProcedureId INT NOT NULL,
    CutEdgeLocation NVARCHAR(200),
    AchievedQuality NVARCHAR(10),               -- Actual quality achieved
    AchievedPerpendicularity NVARCHAR(10),
    AchievedRoughness NVARCHAR(10),
    MeasuredHardness INT,                       -- HV10 actual
    OperatorName NVARCHAR(100) NOT NULL,
    OperatorQualification NVARCHAR(200),
    CuttingDate DATE NOT NULL,
    EquipmentUsed NVARCHAR(200),
    EquipmentCalibrationDate DATE,
    InspectedBy NVARCHAR(100),
    InspectionDate DATE,
    QualityApproved BIT DEFAULT 0,
    ReworkRequired BIT DEFAULT 0,
    ReworkCompleted BIT DEFAULT 0,
    Notes NVARCHAR(1000)
);
```

### Hardness Control Requirements

**Critical Hardness Limits by Execution Class:**
- **EXC1:** No specific hardness requirements
- **EXC2:** 380 HV10 maximum for most applications
- **EXC3:** 350 HV10 maximum, mandatory verification
- **EXC4:** 320 HV10 maximum, 100% inspection required

---

## Coating and Paint Systems

### Paint System Specification and Control

**Database Implementation:**
```sql
-- Paint system definitions
CREATE TABLE PaintSystems (
    PaintSystemId INT IDENTITY(1,1) PRIMARY KEY,
    SystemName NVARCHAR(200) NOT NULL,
    CorrosivityCategory NVARCHAR(10) NOT NULL,   -- C1-C5, CX per ISO 12944-2
    ExpectedDurability NVARCHAR(20) NOT NULL,    -- L, M, H, VH per ISO 12944-1
    DurabilityYears INT NOT NULL,
    PrimerType NVARCHAR(100),
    PrimerThickness DECIMAL(5,1),               -- Micrometers DFT
    IntermediateType NVARCHAR(100),
    IntermediateThickness DECIMAL(5,1),
    TopcoatType NVARCHAR(100),
    TopcoatThickness DECIMAL(5,1),
    TotalSystemThickness DECIMAL(5,1),
    ApplicationTemperatureMin DECIMAL(4,1),     -- °C
    ApplicationTemperatureMax DECIMAL(4,1),     -- °C
    RelativeHumidityMax DECIMAL(4,1),          -- %
    SurfaceTemperatureDelta DECIMAL(4,1),      -- °C above dew point
    TestingRequirements NVARCHAR(1000),
    StandardReference NVARCHAR(200),           -- ISO 12944 series
    ManufacturerApproval NVARCHAR(200),
    IsActive BIT DEFAULT 1
);

-- Paint application environmental monitoring
CREATE TABLE PaintApplicationEnvironment (
    EnvironmentRecordId INT IDENTITY(1,1) PRIMARY KEY,
    CoatingRecordId INT NOT NULL,
    MeasurementTime DATETIME2 NOT NULL,
    AmbientTemperature DECIMAL(4,1) NOT NULL,  -- °C
    RelativeHumidity DECIMAL(4,1) NOT NULL,    -- %
    DewPoint DECIMAL(4,1) NOT NULL,            -- °C
    SurfaceTemperature DECIMAL(4,1) NOT NULL,  -- °C
    TemperatureDelta DECIMAL(4,1) COMPUTED (SurfaceTemperature - DewPoint),
    WindSpeed DECIMAL(4,1),                    -- m/s
    MeasuredBy NVARCHAR(100) NOT NULL,
    ConditionsAcceptable BIT COMPUTED (
        CASE 
            WHEN TemperatureDelta >= 3.0 AND RelativeHumidity <= 85.0 THEN 1
            ELSE 0
        END
    )
);
```

---

## Personnel Qualification Requirements

### Comprehensive Personnel Management

EN 1090 requires detailed tracking of personnel qualifications across all activities.

**Database Implementation:**
```sql
-- Personnel qualifications comprehensive tracking
CREATE TABLE PersonnelQualifications (
    PersonnelQualificationId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EmployeeId NVARCHAR(100) NOT NULL,
    EmployeeName NVARCHAR(200) NOT NULL,
    QualificationType NVARCHAR(100) NOT NULL,   -- Welder, Inspector, Coordinator, etc.
    QualificationStandard NVARCHAR(100),        -- ISO 9606, EN 473, etc.
    QualificationLevel NVARCHAR(50),            -- Level I, II, III for NDT
    ProcessType NVARCHAR(100),                  -- GMAW, SMAW, VT, PT, RT, UT
    MaterialGroup NVARCHAR(100),               -- Steel group per standard
    ThicknessRange NVARCHAR(100),              -- Qualified thickness range
    PositionRange NVARCHAR(100),               -- Welding positions: PA, PB, PC, PD
    QualificationDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    CertifyingBody NVARCHAR(200) NOT NULL,
    CertificateNumber NVARCHAR(100),
    CertificatePath NVARCHAR(500),
    RequalificationRequired BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    LastAssessmentDate DATE,
    NextAssessmentDue DATE,
    SupervisorApproval NVARCHAR(100),
    Notes NVARCHAR(1000)
);

-- Responsible Welding Coordinator (RWC) requirements
CREATE TABLE ResponsibleWeldingCoordinators (
    RWCId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PersonnelQualificationId INT NOT NULL,
    CoordinatorLevel NVARCHAR(10) NOT NULL,     -- B, S, C per ISO 14731
    ResponsibilityScope NVARCHAR(500),          -- Description of responsibilities
    AppointmentDate DATE NOT NULL,
    AppointedBy NVARCHAR(100) NOT NULL,
    ExecutionClassAuthority NVARCHAR(20),       -- Authorized EXC levels
    ContinuousEducationHours INT DEFAULT 0,
    LastTrainingDate DATE,
    NextTrainingDue DATE,
    IsCurrentlyActive BIT DEFAULT 1,
    CONSTRAINT FK_RWC_PersonnelQual 
        FOREIGN KEY (PersonnelQualificationId) REFERENCES PersonnelQualifications(PersonnelQualificationId)
);

-- Training and competence records
CREATE TABLE PersonnelTrainingRecords (
    TrainingRecordId INT IDENTITY(1,1) PRIMARY KEY,
    PersonnelQualificationId INT NOT NULL,
    TrainingType NVARCHAR(100) NOT NULL,
    TrainingDescription NVARCHAR(500),
    TrainingDate DATE NOT NULL,
    Duration DECIMAL(4,1),                      -- Hours
    TrainingProvider NVARCHAR(200),
    Instructor NVARCHAR(100),
    AssessmentResult NVARCHAR(50),              -- Pass, Fail, Conditional
    CertificateIssued BIT DEFAULT 0,
    CertificateNumber NVARCHAR(100),
    ValidUntil DATE,
    ContinuousEducationCredit DECIMAL(4,1),    -- Hours credited
    DocumentationPath NVARCHAR(500),
    CONSTRAINT FK_Training_PersonnelQual 
        FOREIGN KEY (PersonnelQualificationId) REFERENCES PersonnelQualifications(PersonnelQualificationId)
);
```

---

## Equipment and Machinery Requirements

### Equipment Calibration and Maintenance

**Database Implementation:**
```sql
-- Equipment and machinery management
CREATE TABLE EquipmentInventory (
    EquipmentId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EquipmentType NVARCHAR(100) NOT NULL,       -- Welding machine, Torque wrench, etc.
    EquipmentCategory NVARCHAR(100),            -- Production, Testing, Measurement
    Manufacturer NVARCHAR(100),
    ModelNumber NVARCHAR(100),
    SerialNumber NVARCHAR(100) NOT NULL,
    InstallationDate DATE,
    LastCalibrationDate DATE,
    NextCalibrationDue DATE,
    CalibrationInterval INT,                    -- Days
    CalibrationStandard NVARCHAR(100),
    AccuracyClass NVARCHAR(50),
    OperatingRange NVARCHAR(200),
    ResponsiblePerson NVARCHAR(100),
    Location NVARCHAR(200),
    Status NVARCHAR(50) DEFAULT 'Active',      -- Active, Maintenance, Calibration, Retired
    MaintenanceSchedule NVARCHAR(500),
    IsCalibrationRequired BIT DEFAULT 1,
    LastMaintenanceDate DATE,
    NextMaintenanceDue DATE
);

-- Equipment calibration records
CREATE TABLE EquipmentCalibrationRecords (
    CalibrationRecordId INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentId INT NOT NULL,
    CalibrationDate DATE NOT NULL,
    CalibrationDueDate DATE NOT NULL,
    CalibrationStandard NVARCHAR(100),
    CalibrationMethod NVARCHAR(200),
    CalibratedBy NVARCHAR(100) NOT NULL,
    CalibrationBody NVARCHAR(200),
    CertificateNumber NVARCHAR(100),
    CertificatePath NVARCHAR(500),
    CalibrationResult NVARCHAR(50),             -- Pass, Fail, Limited Use
    Accuracy DECIMAL(5,2),                      -- Achieved accuracy %
    UncertaintyValue DECIMAL(8,4),             -- Measurement uncertainty
    EnvironmentalConditions NVARCHAR(300),     -- Temperature, humidity during calibration
    LimitationsNoted NVARCHAR(1000),
    NextCalibrationDue DATE,
    Cost DECIMAL(10,2),
    Notes NVARCHAR(1000),
    CONSTRAINT FK_CalibrationRecord_Equipment 
        FOREIGN KEY (EquipmentId) REFERENCES EquipmentInventory(EquipmentId)
);

-- Equipment usage tracking
CREATE TABLE EquipmentUsageLog (
    UsageLogId INT IDENTITY(1,1) PRIMARY KEY,
    EquipmentId INT NOT NULL,
    UsedBy NVARCHAR(100) NOT NULL,
    UsageDate DATE NOT NULL,
    StartTime TIME,
    EndTime TIME,
    Purpose NVARCHAR(200),                      -- Welding, Testing, Calibration
    ProjectId INT,
    AssemblyId INT,
    OperatingConditions NVARCHAR(500),
    PerformanceNotes NVARCHAR(1000),
    MaintenanceRequired BIT DEFAULT 0,
    CalibrationDrift BIT DEFAULT 0,            -- Indication of calibration drift
    CONSTRAINT FK_UsageLog_Equipment 
        FOREIGN KEY (EquipmentId) REFERENCES EquipmentInventory(EquipmentId)
);
```

---

## Environmental Considerations

### Atmospheric Monitoring and Control

**Database Implementation:**
```sql
-- Environmental monitoring for manufacturing operations
CREATE TABLE EnvironmentalConditions (
    ConditionRecordId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    LocationId NVARCHAR(100),                   -- Shop area, outdoor site
    MeasurementDateTime DATETIME2 NOT NULL,
    AmbientTemperature DECIMAL(4,1),            -- °C
    RelativeHumidity DECIMAL(4,1),             -- %
    AtmosphericPressure DECIMAL(6,1),          -- hPa
    DewPoint DECIMAL(4,1),                     -- °C
    WindSpeed DECIMAL(4,1),                    -- m/s
    WindDirection NVARCHAR(20),                -- N, NE, E, SE, S, SW, W, NW
    Precipitation NVARCHAR(50),                -- None, Light rain, Heavy rain, Snow
    Visibility DECIMAL(5,1),                   -- km
    CorrosivityIndicator NVARCHAR(10),         -- C1-C5 based on conditions
    MeasuredBy NVARCHAR(100),
    MeasurementMethod NVARCHAR(100),           -- Automatic station, Handheld device
    QualityFlag NVARCHAR(20) DEFAULT 'Good',   -- Good, Questionable, Bad
    Notes NVARCHAR(500)
);

-- Process-specific environmental requirements
CREATE TABLE ProcessEnvironmentalLimits (
    ProcessLimitId INT IDENTITY(1,1) PRIMARY KEY,
    ProcessType NVARCHAR(100) NOT NULL,        -- Welding, Painting, Coating, etc.
    ExecutionClass NVARCHAR(10),
    MinTemperature DECIMAL(4,1),               -- °C
    MaxTemperature DECIMAL(4,1),               -- °C
    MaxRelativeHumidity DECIMAL(4,1),          -- %
    MinDewPointDelta DECIMAL(4,1),             -- °C surface temp above dew point
    MaxWindSpeed DECIMAL(4,1),                 -- m/s
    PrecipitationAllowed BIT DEFAULT 0,
    StandardReference NVARCHAR(100),
    ProcessSpecificNotes NVARCHAR(1000),
    IsActive BIT DEFAULT 1
);
```

---

## Stainless Steel Specific Requirements

### Specialized Stainless Steel Processing

EN 1090-2 includes specific requirements for stainless steel that differ from carbon steel.

**Database Implementation:**
```sql
-- Stainless steel specific material properties
CREATE TABLE StainlessSteelGrades (
    StainlessGradeId INT IDENTITY(1,1) PRIMARY KEY,
    GradeDesignation NVARCHAR(50) NOT NULL,     -- 304, 316, 2205, etc.
    SteelType NVARCHAR(50) NOT NULL,           -- Austenitic, Ferritic, Duplex, Martensitic
    StandardReference NVARCHAR(100),            -- EN 10088 series
    ChemicalComposition NVARCHAR(1000),         -- JSON format
    WorkHardeningRate NVARCHAR(50),            -- High, Medium, Low
    FormingLimitations NVARCHAR(500),
    WeldingConsiderations NVARCHAR(1000),
    CorrosionResistance NVARCHAR(500),
    SpecialHandlingRequired NVARCHAR(1000),
    SurfaceProtectionNeeds NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- Stainless steel fabrication records
CREATE TABLE StainlessSteelFabrication (
    FabricationRecordId INT IDENTITY(1,1) PRIMARY KEY,
    MaterialId INT NOT NULL,
    StainlessGradeId INT NOT NULL,
    FabricationProcess NVARCHAR(100) NOT NULL,  -- Cutting, Forming, Welding
    ProcessParameters NVARCHAR(1000),           -- JSON format for specific parameters
    ContaminationPrevention NVARCHAR(500),     -- Measures taken
    SurfaceProtection NVARCHAR(500),           -- Protection during fabrication
    QualityInspection NVARCHAR(500),           -- Specific inspections performed
    PassivationRequired BIT DEFAULT 0,
    PassivationCompleted BIT DEFAULT 0,
    PassivationMethod NVARCHAR(200),
    ProcessedBy NVARCHAR(100) NOT NULL,
    ProcessDate DATE NOT NULL,
    QualityApproved BIT DEFAULT 0,
    SpecialNotes NVARCHAR(1000),
    CONSTRAINT FK_SSFabrication_SSGrade 
        FOREIGN KEY (StainlessGradeId) REFERENCES StainlessSteelGrades(StainlessGradeId)
);
```

---

## Initial Type Testing (ITT)

### Product Qualification Management

**Database Implementation:**
```sql
-- Initial Type Testing management
CREATE TABLE InitialTypeTests (
    ITTId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    TestType NVARCHAR(100) NOT NULL,           -- Component qualification, Process validation
    ProductFamily NVARCHAR(200) NOT NULL,
    ExecutionClass NVARCHAR(10) NOT NULL,
    TestDescription NVARCHAR(1000),
    TestStandard NVARCHAR(100),                -- EN 1090-2, ISO standards
    TestProcedure NVARCHAR(2000),
    SampleSize INT NOT NULL,
    TestingBody NVARCHAR(200),                 -- Internal, External lab
    TestStartDate DATE NOT NULL,
    TestCompletionDate DATE,
    TestResult NVARCHAR(50),                   -- Pass, Fail, Conditional
    CertificateNumber NVARCHAR(100),
    CertificatePath NVARCHAR(500),
    ValidityPeriod INT,                        -- Years
    ExpiryDate DATE,
    RetestRequired BIT DEFAULT 0,
    RetestTrigger NVARCHAR(500),              -- Process change, material change, etc.
    Cost DECIMAL(10,2),
    ResponsibleEngineer NVARCHAR(100),
    ApprovedBy NVARCHAR(100),
    Notes NVARCHAR(2000)
);

-- ITT test results details
CREATE TABLE ITTTestResults (
    TestResultId INT IDENTITY(1,1) PRIMARY KEY,
    ITTId INT NOT NULL,
    TestParameter NVARCHAR(200) NOT NULL,      -- Tensile strength, Impact energy, etc.
    RequiredValue NVARCHAR(100),               -- Specification requirement
    MeasuredValue NVARCHAR(100),               -- Actual test result
    Tolerance NVARCHAR(100),                   -- Acceptable tolerance
    ResultStatus NVARCHAR(20),                 -- Pass, Fail, Borderline
    TestMethod NVARCHAR(200),
    TestDate DATE,
    TestedBy NVARCHAR(100),
    Equipment NVARCHAR(200),
    TestConditions NVARCHAR(500),
    UncertaintyValue NVARCHAR(100),
    Notes NVARCHAR(1000),
    CONSTRAINT FK_ITTResults_ITT 
        FOREIGN KEY (ITTId) REFERENCES InitialTypeTests(ITTId)
);
```

---

## Implementation Priority Matrix

### Critical Implementation Order

| Priority | Requirement Category | Effort | Business Impact | Compliance Risk |
|----------|---------------------|---------|-----------------|-----------------|
| **1** | Corrosion Protection Systems | High | High | Critical |
| **2** | Personnel Qualification Tracking | Medium | High | Critical |
| **3** | Equipment Calibration Management | Medium | Medium | High |
| **4** | Thermal Cutting Controls | Medium | Medium | High |
| **5** | Preloaded Bolt Management | High | Medium | High |
| **6** | Environmental Monitoring | Low | Low | Medium |
| **7** | Stainless Steel Requirements | Medium | Low | Medium |
| **8** | Initial Type Testing | Low | Low | Medium |

### Phase 1 Implementation (Months 1-3)
**Focus: Critical Compliance Elements**
- Corrosion protection system framework
- Basic personnel qualification tracking
- Equipment calibration database
- Thermal cutting procedure controls

### Phase 2 Implementation (Months 4-6)
**Focus: Advanced Systems**
- Comprehensive bolt management
- Environmental monitoring integration
- Advanced personnel management
- ITT system implementation

### Phase 3 Implementation (Months 7-9)
**Focus: Specialized Requirements**
- Stainless steel specific processes
- Advanced environmental controls
- Complete ITT management
- System optimization and integration

---

## Integration with Existing Manimp Architecture

### Database Integration Points

```sql
-- Link existing tables to new requirements
ALTER TABLE Projects ADD (
    CorrosionProtectionRequired BIT DEFAULT 0,
    CorrosionCategory INT,                      -- 1, 2, or 3
    EnvironmentalClass NVARCHAR(10),           -- Indoor, Outdoor, Marine
    SpecialMaterialRequirements NVARCHAR(1000)
);

ALTER TABLE ProfileInventories ADD (
    IsStainlessSteel BIT DEFAULT 0,
    StainlessGradeId INT,
    SurfaceCondition NVARCHAR(100),            -- As rolled, pickled, brushed
    SurfaceProtectionApplied NVARCHAR(200),
    HandlingInstructions NVARCHAR(1000),
    CONSTRAINT FK_ProfileInv_SSGrade 
        FOREIGN KEY (StainlessGradeId) REFERENCES StainlessSteelGrades(StainlessGradeId)
);

ALTER TABLE Assemblies ADD (
    CorrosionProtectionSystemId INT,
    BoltingSystemRequired NVARCHAR(50),        -- Standard, Preloaded, Special
    ThermalCuttingRequired BIT DEFAULT 0,
    SpecialHandlingRequired NVARCHAR(1000),
    CONSTRAINT FK_Assembly_CorrosionSystem 
        FOREIGN KEY (CorrosionProtectionSystemId) REFERENCES CorrosionProtectionSystems(ProtectionSystemId)
);
```

### API Extensions

```http
# New API endpoints for missing requirements
POST /api/corrosion-protection/systems
GET /api/personnel/qualifications?type={type}&expiring=true
PUT /api/equipment/{id}/calibration
GET /api/environmental/conditions?location={location}&date={date}
POST /api/stainless-steel/fabrication
GET /api/bolting/preloaded/{connectionId}/installation
POST /api/thermal-cutting/procedures/qualify
GET /api/itt/tests?status=pending&executionClass={exc}
```

---

## Conclusion

These supplementary requirements represent critical EN 1090 compliance elements that were not covered in the original documentation. Their implementation is essential for:

### **Business Benefits:**
1. **Complete EN 1090 Compliance** - Addressing all standard requirements
2. **Competitive Differentiation** - Comprehensive coverage exceeding competitors
3. **Risk Mitigation** - Avoiding compliance gaps and regulatory issues
4. **Operational Excellence** - Systematic management of all production aspects

### **Technical Benefits:**
1. **Integrated Quality System** - Seamless connection of all quality elements
2. **Automated Compliance** - Systematic validation and reporting
3. **Traceability Enhancement** - Complete production process documentation
4. **Performance Optimization** - Data-driven process improvements

### **Implementation Recommendations:**
1. **Prioritize Phase 1** elements for immediate compliance
2. **Integrate with existing architecture** to avoid system fragmentation
3. **Focus on automation** to reduce manual compliance burden
4. **Plan for scalability** to accommodate future standard updates

This supplementary documentation ensures Manimp achieves **100% EN 1090 compliance** across all execution classes, providing the most comprehensive steel construction management platform available in the European market.

---

*This supplementary document should be reviewed alongside the primary EN 1090 requirements documentation and updated as standards evolve.*