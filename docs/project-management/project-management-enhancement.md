# Enhanced Project Management System - Implementation Plan

**Date**: October 14, 2025  
**Status**: Planning Phase  
**Target Tier**: Enterprise (with Professional subset)

## Overview

This document outlines the comprehensive enhancement of Manimp's project management capabilities, transforming it from basic project tracking into a full-featured project management system with financial tracking, milestone management, resource allocation, and advanced reporting.

## Current State Analysis

### Existing CrmProject Model
- ✅ Basic project information (name, code, description)
- ✅ Customer association
- ✅ Date tracking (start, planned delivery, actual delivery)
- ✅ Status tracking (Planning, In Progress, etc.)
- ✅ Delivery address and rules
- ✅ Basic project value field
- ✅ EN 1090 compliance tracking
- ✅ Assembly and delivery associations

### Gaps & Enhancement Opportunities
- ❌ No budget vs actual cost tracking
- ❌ No milestone/phase management
- ❌ No project document management
- ❌ No resource allocation tracking
- ❌ No project timeline visualization
- ❌ No contract management
- ❌ No project manager assignment
- ❌ No priority/risk assessment
- ❌ Limited financial reporting
- ❌ No change order tracking

## Enhanced Features

### 1. Financial Management
**Tier**: Professional (Basic), Enterprise (Advanced)

#### Fields to Add:
- `BudgetAmount` (decimal) - Approved project budget
- `ActualCost` (decimal) - Running total of actual costs
- `EstimatedCost` (decimal) - Initial cost estimate
- `PaymentTerms` (string, 500 chars) - Payment schedule and terms
- `ContractNumber` (string, 100 chars) - Legal contract reference
- `ProfitMargin` (decimal) - Calculated profit margin percentage
- `InvoicedAmount` (decimal) - Total amount invoiced to customer
- `ReceivedAmount` (decimal) - Total payment received

#### Features:
- Budget vs Actual variance tracking with alerts
- Cost breakdown by category (materials, labor, outsourcing)
- Automatic cost rollup from inventory usage, procurement, and labor
- Payment milestone tracking
- Invoice generation and payment tracking
- Profit margin analysis and forecasting

### 2. Milestone & Phase Management
**Tier**: Professional

#### New Model: ProjectMilestone
```csharp
public class ProjectMilestone
{
    public int MilestoneId { get; set; }
    public int CrmProjectId { get; set; }
    public string Name { get; set; } // e.g., "Design Approval", "Fabrication Complete"
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string Status { get; set; } // Pending, InProgress, Completed, Delayed
    public int SequenceOrder { get; set; }
    public decimal? PaymentPercentage { get; set; } // % of project value tied to this milestone
    public bool IsPaymentMilestone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

#### Features:
- Visual timeline/Gantt chart of milestones
- Milestone dependency tracking
- Payment milestone integration
- Automatic status updates based on assembly progress
- Delay alerts and critical path highlighting
- Milestone completion checklists

### 3. Project Document Management
**Tier**: Professional

#### New Model: ProjectDocument
```csharp
public class ProjectDocument
{
    public int DocumentId { get; set; }
    public int CrmProjectId { get; set; }
    public string DocumentType { get; set; } // Contract, Drawing, Specification, Certificate, Photo
    public string FileName { get; set; }
    public string? Description { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public byte[] FileContent { get; set; }
    public string? DocumentNumber { get; set; } // Drawing number, spec number, etc.
    public int? Revision { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedUtc { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

#### Features:
- Centralized document repository per project
- Document categorization (contracts, drawings, specs, photos)
- Version control with revision tracking
- Document search and filtering
- Access control and audit trail
- Drawing number and revision management
- PDF viewer integration

### 4. Cost Tracking System
**Tier**: Professional

#### New Model: ProjectCost
```csharp
public class ProjectCost
{
    public int CostId { get; set; }
    public int CrmProjectId { get; set; }
    public string CostType { get; set; } // Material, Labor, Equipment, Outsourcing, Other
    public string Category { get; set; } // More specific categorization
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime CostDate { get; set; }
    public string? Reference { get; set; } // PO number, invoice number, etc.
    public bool IsBudgeted { get; set; }
    public string? Notes { get; set; }
    public string RecordedBy { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

#### Features:
- Detailed cost entry and categorization
- Link costs to purchase orders and invoices
- Automatic cost aggregation from inventory usage
- Budget vs actual cost comparison by category
- Cost trend analysis and forecasting
- Export to accounting systems

### 5. Resource & Team Management
**Tier**: Enterprise

#### Fields to Add:
- `ProjectManager` (string, 200 chars) - Assigned project manager
- `TeamMembers` (string, 1000 chars) - Comma-separated team members
- `EstimatedHours` (decimal) - Estimated labor hours
- `ActualHours` (decimal) - Tracked actual hours

#### Features:
- Project manager assignment and responsibility tracking
- Team member allocation
- Labor hour tracking and utilization
- Resource conflict detection
- Capacity planning integration
- Time tracking per assembly/task

### 6. Risk & Priority Management
**Tier**: Enterprise

#### Fields to Add:
- `Priority` (string, 20 chars) - Low, Medium, High, Critical
- `RiskLevel` (string, 20 chars) - Low, Medium, High
- `RiskNotes` (string, 2000 chars) - Risk assessment and mitigation plans
- `CompletionPercentage` (decimal) - Overall project completion (0-100)

#### Features:
- Priority-based project sorting and alerts
- Risk assessment and mitigation tracking
- Automatic completion percentage calculation
- Project health dashboard
- Early warning system for at-risk projects

### 7. Change Order Management
**Tier**: Enterprise

#### New Model: ProjectChangeOrder
```csharp
public class ProjectChangeOrder
{
    public int ChangeOrderId { get; set; }
    public int CrmProjectId { get; set; }
    public string ChangeOrderNumber { get; set; }
    public DateTime RequestDate { get; set; }
    public string RequestedBy { get; set; }
    public string Description { get; set; }
    public string Reason { get; set; }
    public decimal? CostImpact { get; set; }
    public int? ScheduleImpact { get; set; } // Days
    public string Status { get; set; } // Pending, Approved, Rejected, Implemented
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

#### Features:
- Formal change order request workflow
- Cost and schedule impact analysis
- Approval workflow and audit trail
- Automatic budget/schedule adjustments after approval
- Change order history and reporting

## Database Schema Changes

### Migration: AddEnhancedProjectManagement

#### CrmProjects Table Updates:
```sql
ALTER TABLE CrmProjects ADD
    BudgetAmount DECIMAL(18,2) NULL,
    ActualCost DECIMAL(18,2) NULL DEFAULT 0,
    EstimatedCost DECIMAL(18,2) NULL,
    PaymentTerms NVARCHAR(500) NULL,
    ContractNumber NVARCHAR(100) NULL,
    ProfitMargin DECIMAL(5,2) NULL,
    InvoicedAmount DECIMAL(18,2) NULL DEFAULT 0,
    ReceivedAmount DECIMAL(18,2) NULL DEFAULT 0,
    ProjectManager NVARCHAR(200) NULL,
    TeamMembers NVARCHAR(1000) NULL,
    EstimatedHours DECIMAL(10,2) NULL,
    ActualHours DECIMAL(10,2) NULL DEFAULT 0,
    Priority NVARCHAR(20) NULL DEFAULT 'Medium',
    RiskLevel NVARCHAR(20) NULL DEFAULT 'Low',
    RiskNotes NVARCHAR(2000) NULL,
    CompletionPercentage DECIMAL(5,2) NULL DEFAULT 0
```

#### New Tables:
```sql
-- Project Milestones
CREATE TABLE ProjectMilestones (
    MilestoneId INT PRIMARY KEY IDENTITY(1,1),
    CrmProjectId INT NOT NULL FOREIGN KEY REFERENCES CrmProjects(CrmProjectId) ON DELETE CASCADE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    PlannedDate DATETIME2 NOT NULL,
    ActualDate DATETIME2,
    Status NVARCHAR(20) NOT NULL,
    SequenceOrder INT NOT NULL,
    PaymentPercentage DECIMAL(5,2),
    IsPaymentMilestone BIT NOT NULL DEFAULT 0,
    Notes NVARCHAR(2000),
    CreatedUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Milestone_Status CHECK (Status IN ('Pending', 'InProgress', 'Completed', 'Delayed'))
);

CREATE INDEX IX_ProjectMilestones_CrmProjectId ON ProjectMilestones(CrmProjectId);
CREATE INDEX IX_ProjectMilestones_PlannedDate ON ProjectMilestones(PlannedDate);
CREATE INDEX IX_ProjectMilestones_Status ON ProjectMilestones(Status);

-- Project Documents
CREATE TABLE ProjectDocuments (
    DocumentId INT PRIMARY KEY IDENTITY(1,1),
    CrmProjectId INT NOT NULL FOREIGN KEY REFERENCES CrmProjects(CrmProjectId) ON DELETE CASCADE,
    DocumentType NVARCHAR(50) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000),
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(200),
    FileContent VARBINARY(MAX) NOT NULL,
    DocumentNumber NVARCHAR(100),
    Revision INT,
    DocumentDate DATETIME2,
    UploadedBy NVARCHAR(200) NOT NULL,
    UploadedUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_Document_Type CHECK (DocumentType IN ('Contract', 'Drawing', 'Specification', 'Certificate', 'Photo', 'Other'))
);

CREATE INDEX IX_ProjectDocuments_CrmProjectId ON ProjectDocuments(CrmProjectId);
CREATE INDEX IX_ProjectDocuments_DocumentType ON ProjectDocuments(DocumentType);
CREATE INDEX IX_ProjectDocuments_DocumentNumber ON ProjectDocuments(DocumentNumber);

-- Project Costs
CREATE TABLE ProjectCosts (
    CostId INT PRIMARY KEY IDENTITY(1,1),
    CrmProjectId INT NOT NULL FOREIGN KEY REFERENCES CrmProjects(CrmProjectId) ON DELETE CASCADE,
    CostType NVARCHAR(50) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CostDate DATETIME2 NOT NULL,
    Reference NVARCHAR(200),
    IsBudgeted BIT NOT NULL DEFAULT 0,
    Notes NVARCHAR(1000),
    RecordedBy NVARCHAR(200) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Cost_Type CHECK (CostType IN ('Material', 'Labor', 'Equipment', 'Outsourcing', 'Other'))
);

CREATE INDEX IX_ProjectCosts_CrmProjectId ON ProjectCosts(CrmProjectId);
CREATE INDEX IX_ProjectCosts_CostType ON ProjectCosts(CostType);
CREATE INDEX IX_ProjectCosts_CostDate ON ProjectCosts(CostDate);

-- Change Orders
CREATE TABLE ProjectChangeOrders (
    ChangeOrderId INT PRIMARY KEY IDENTITY(1,1),
    CrmProjectId INT NOT NULL FOREIGN KEY REFERENCES CrmProjects(CrmProjectId) ON DELETE CASCADE,
    ChangeOrderNumber NVARCHAR(50) NOT NULL,
    RequestDate DATETIME2 NOT NULL,
    RequestedBy NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NOT NULL,
    Reason NVARCHAR(2000) NOT NULL,
    CostImpact DECIMAL(18,2),
    ScheduleImpact INT,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ApprovalDate DATETIME2,
    ApprovedBy NVARCHAR(200),
    Notes NVARCHAR(2000),
    CreatedUtc DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_ChangeOrder_Status CHECK (Status IN ('Pending', 'Approved', 'Rejected', 'Implemented'))
);

CREATE UNIQUE INDEX IX_ProjectChangeOrders_Number ON ProjectChangeOrders(ChangeOrderNumber);
CREATE INDEX IX_ProjectChangeOrders_CrmProjectId ON ProjectChangeOrders(CrmProjectId);
CREATE INDEX IX_ProjectChangeOrders_Status ON ProjectChangeOrders(Status);
```

## Service Layer Implementation

### CrmService Extensions

```csharp
// Milestone Management
Task<List<ProjectMilestone>> GetProjectMilestonesAsync(int projectId);
Task<ProjectMilestone> CreateMilestoneAsync(ProjectMilestone milestone);
Task<ProjectMilestone> UpdateMilestoneAsync(ProjectMilestone milestone);
Task DeleteMilestoneAsync(int milestoneId);
Task<bool> UpdateMilestoneStatusAsync(int milestoneId, string status, DateTime? actualDate);

// Cost Management
Task<List<ProjectCost>> GetProjectCostsAsync(int projectId);
Task<ProjectCost> AddProjectCostAsync(ProjectCost cost);
Task<decimal> GetProjectTotalCostAsync(int projectId);
Task<Dictionary<string, decimal>> GetCostBreakdownAsync(int projectId);
Task UpdateProjectActualCostAsync(int projectId); // Recalculate from ProjectCosts

// Document Management
Task<List<ProjectDocument>> GetProjectDocumentsAsync(int projectId);
Task<ProjectDocument> UploadDocumentAsync(ProjectDocument document);
Task<ProjectDocument?> GetDocumentByIdAsync(int documentId);
Task DeleteDocumentAsync(int documentId);
Task<List<ProjectDocument>> GetDocumentsByTypeAsync(int projectId, string documentType);

// Change Order Management
Task<List<ProjectChangeOrder>> GetProjectChangeOrdersAsync(int projectId);
Task<ProjectChangeOrder> CreateChangeOrderAsync(ProjectChangeOrder changeOrder);
Task<ProjectChangeOrder> UpdateChangeOrderAsync(ProjectChangeOrder changeOrder);
Task<bool> ApproveChangeOrderAsync(int changeOrderId, string approvedBy);
Task<bool> RejectChangeOrderAsync(int changeOrderId, string reason);

// Financial Reporting
Task<ProjectFinancialSummary> GetFinancialSummaryAsync(int projectId);
Task<bool> UpdateProjectBudgetAsync(int projectId, decimal budgetAmount);
Task<bool> RecordPaymentAsync(int projectId, decimal amount, string reference);

// Progress Tracking
Task<bool> UpdateCompletionPercentageAsync(int projectId); // Auto-calculate
Task<ProjectHealthReport> GetProjectHealthReportAsync(int projectId);
```

## API Controller Endpoints

### CrmController Extensions

```csharp
// Milestones
[HttpGet("projects/{projectId}/milestones")]
Task<IActionResult> GetMilestones(int projectId);

[HttpPost("projects/{projectId}/milestones")]
Task<IActionResult> CreateMilestone(int projectId, ProjectMilestone milestone);

[HttpPut("milestones/{id}")]
Task<IActionResult> UpdateMilestone(int id, ProjectMilestone milestone);

[HttpDelete("milestones/{id}")]
Task<IActionResult> DeleteMilestone(int id);

[HttpPatch("milestones/{id}/status")]
Task<IActionResult> UpdateMilestoneStatus(int id, MilestoneStatusUpdateDto dto);

// Costs
[HttpGet("projects/{projectId}/costs")]
Task<IActionResult> GetCosts(int projectId);

[HttpPost("projects/{projectId}/costs")]
Task<IActionResult> AddCost(int projectId, ProjectCost cost);

[HttpGet("projects/{projectId}/costs/summary")]
Task<IActionResult> GetCostSummary(int projectId);

[HttpGet("projects/{projectId}/costs/breakdown")]
Task<IActionResult> GetCostBreakdown(int projectId);

// Documents
[HttpGet("projects/{projectId}/documents")]
Task<IActionResult> GetDocuments(int projectId);

[HttpPost("projects/{projectId}/documents")]
Task<IActionResult> UploadDocument(int projectId, [FromForm] IFormFile file, [FromForm] string documentType);

[HttpGet("documents/{id}")]
Task<IActionResult> GetDocument(int id);

[HttpGet("documents/{id}/download")]
Task<IActionResult> DownloadDocument(int id);

[HttpDelete("documents/{id}")]
Task<IActionResult> DeleteDocument(int id);

// Change Orders
[HttpGet("projects/{projectId}/change-orders")]
Task<IActionResult> GetChangeOrders(int projectId);

[HttpPost("projects/{projectId}/change-orders")]
Task<IActionResult> CreateChangeOrder(int projectId, ProjectChangeOrder changeOrder);

[HttpPut("change-orders/{id}")]
Task<IActionResult> UpdateChangeOrder(int id, ProjectChangeOrder changeOrder);

[HttpPost("change-orders/{id}/approve")]
Task<IActionResult> ApproveChangeOrder(int id, ApprovalDto approval);

[HttpPost("change-orders/{id}/reject")]
Task<IActionResult> RejectChangeOrder(int id, RejectionDto rejection);

// Financial
[HttpGet("projects/{projectId}/financial-summary")]
Task<IActionResult> GetFinancialSummary(int projectId);

[HttpPatch("projects/{projectId}/budget")]
Task<IActionResult> UpdateBudget(int projectId, BudgetUpdateDto dto);

[HttpPost("projects/{projectId}/payments")]
Task<IActionResult> RecordPayment(int projectId, PaymentDto payment);

// Health & Progress
[HttpGet("projects/{projectId}/health")]
Task<IActionResult> GetProjectHealth(int projectId);

[HttpPatch("projects/{projectId}/completion")]
Task<IActionResult> UpdateCompletion(int projectId);
```

## UI Implementation

### Page: ProjectManagement.razor
**Route**: `/projects` or `/project-management`

#### Layout Structure:
```
┌─────────────────────────────────────────────────────────┐
│ Project Management                         [+ New Project] │
├─────────────────────────────────────────────────────────┤
│ 📊 Stats Cards Row                                      │
│ [Active Projects] [Delayed] [Budget Status] [Completion]│
├─────────────────────────────────────────────────────────┤
│ 🔍 Search & Filters                                     │
│ [Search] [Status▾] [Priority▾] [Manager▾] [Export]     │
├─────────────────────────────────────────────────────────┤
│ Projects Table                                          │
│ Name │ Customer │ Status │ Progress │ Budget │ Actions  │
│ ─────┼──────────┼────────┼──────────┼────────┼─────────│
│ Proj1│ Acme Corp│ Active │ ▓▓▓░░ 60%│ $50k   │ [👁][✎]│
│ Proj2│ ABC Inc  │ Delayed│ ▓▓░░░ 40%│ $75k   │ [👁][✎]│
└─────────────────────────────────────────────────────────┘
```

### Page: ProjectDetails.razor
**Route**: `/projects/{id}`

#### Tabbed Interface:
1. **Overview Tab**
   - Project header (name, code, customer, manager)
   - Key metrics cards (budget vs actual, completion, timeline status)
   - Status timeline visualization
   - Quick actions (edit, generate report, close project)

2. **Financial Tab**
   - Budget summary (estimated, budgeted, actual, variance)
   - Cost breakdown chart (pie/bar chart by category)
   - Recent costs table with add cost button
   - Payment tracking (invoiced vs received)
   - Profit margin indicators

3. **Milestones Tab**
   - Timeline/Gantt visualization
   - Milestone list with status indicators
   - Add/edit milestone dialogs
   - Payment milestone indicators
   - Critical path highlighting

4. **Documents Tab**
   - Document repository with categorization
   - Upload interface (drag & drop)
   - Document preview (PDF viewer)
   - Version history
   - Search and filter by type/date

5. **Team & Resources Tab**
   - Project manager assignment
   - Team member list
   - Hour tracking (estimated vs actual)
   - Resource allocation chart
   - Workload visualization

6. **Change Orders Tab**
   - Change order list
   - Create change order form
   - Approval workflow
   - Cost/schedule impact summary
   - Change order history

7. **Progress Tab**
   - Overall completion percentage
   - Assembly progress integration
   - Quality checkpoints status
   - Recent activity timeline
   - Next actions/pending items

### Dialogs

#### 1. ProjectDialog
- Create/edit project with all core fields
- Customer selection dropdown
- Financial fields (budget, payment terms, contract)
- Project manager assignment
- Priority and risk assessment
- Save & Close or Save & Add Milestones

#### 2. MilestoneDialog
- Milestone name and description
- Planned date picker
- Payment milestone toggle
- Payment percentage (if payment milestone)
- Sequence order
- Notes field

#### 3. CostDialog
- Cost type dropdown (Material, Labor, Equipment, etc.)
- Category input
- Amount and date
- Reference field (PO, invoice number)
- Budget toggle
- Description and notes

#### 4. DocumentUploadDialog
- File upload control
- Document type selection
- Document number/revision fields
- Document date
- Description/notes
- Preview before upload

#### 5. ChangeOrderDialog
- Change order number (auto-generated)
- Description and reason
- Cost impact calculation
- Schedule impact (days)
- Attachment upload
- Submit for approval

## Feature Gating

### New Feature Keys:
```csharp
// Professional Tier
public const string EnhancedProjectManagement = "enhanced_project_management";
public const string ProjectFinancialTracking = "project_financial_tracking";
public const string ProjectMilestones = "project_milestones";
public const string ProjectDocuments = "project_documents";
public const string ProjectCostTracking = "project_cost_tracking";

// Enterprise Tier
public const string ProjectResourceManagement = "project_resource_management";
public const string ProjectRiskManagement = "project_risk_management";
public const string ProjectChangeOrders = "project_change_orders";
public const string AdvancedProjectReporting = "advanced_project_reporting";
```

### Plan Feature Assignments:
- **Professional**: Enhanced project management, financial tracking, milestones, documents, cost tracking
- **Enterprise**: All Professional + resource management, risk management, change orders, advanced reporting

## HTTP Service: ProjectManagementHttpService

```csharp
public class ProjectManagementHttpService
{
    // Projects
    Task<List<CrmProject>> GetProjectsAsync(string? filter = null);
    Task<CrmProject?> GetProjectDetailsAsync(int projectId);
    Task<(bool Success, string Message, CrmProject? Data)> CreateProjectAsync(CrmProject project);
    Task<(bool Success, string Message)> UpdateProjectAsync(CrmProject project);
    Task<(bool Success, string Message)> DeleteProjectAsync(int projectId);
    
    // Milestones
    Task<List<ProjectMilestone>> GetMilestonesAsync(int projectId);
    Task<(bool Success, string Message)> CreateMilestoneAsync(ProjectMilestone milestone);
    Task<(bool Success, string Message)> UpdateMilestoneStatusAsync(int milestoneId, string status);
    
    // Costs
    Task<List<ProjectCost>> GetCostsAsync(int projectId);
    Task<Dictionary<string, decimal>> GetCostBreakdownAsync(int projectId);
    Task<(bool Success, string Message)> AddCostAsync(ProjectCost cost);
    
    // Documents
    Task<List<ProjectDocument>> GetDocumentsAsync(int projectId);
    Task<(bool Success, string Message)> UploadDocumentAsync(int projectId, MultipartFormDataContent content);
    Task<byte[]?> DownloadDocumentAsync(int documentId);
    
    // Change Orders
    Task<List<ProjectChangeOrder>> GetChangeOrdersAsync(int projectId);
    Task<(bool Success, string Message)> CreateChangeOrderAsync(ProjectChangeOrder changeOrder);
    Task<(bool Success, string Message)> ApproveChangeOrderAsync(int changeOrderId, string approvedBy);
    
    // Financial
    Task<ProjectFinancialSummary?> GetFinancialSummaryAsync(int projectId);
    Task<(bool Success, string Message)> RecordPaymentAsync(int projectId, decimal amount, string reference);
}
```

## Mock Services (Demo Mode)

### MockProjectManagementService
Provide sample data for demonstration:
- 5-10 demo projects with varied status, priority, and completion
- Sample milestones for each project
- Mock costs and budget data
- Sample documents (PDF metadata only)
- Mock change orders with various statuses
- Realistic financial summaries

## Reporting & Analytics

### Built-in Reports:
1. **Project Portfolio Dashboard**
   - All projects summary
   - Status distribution
   - Budget utilization
   - Timeline adherence

2. **Financial Performance Report**
   - Budget vs actual comparison
   - Profit margin analysis
   - Cost overrun identification
   - Payment status tracking

3. **Project Health Report**
   - On-time/delayed projects
   - Risk assessment summary
   - Resource utilization
   - Completion forecasting

4. **Cost Analysis Report**
   - Cost breakdown by type
   - Trend analysis
   - Variance reporting
   - Budget forecast

### Export Formats:
- PDF (formatted reports with charts)
- Excel (detailed data exports)
- CSV (raw data for external analysis)

## Integration Points

### Existing Manimp Modules:
1. **Inventory Module**
   - Material costs automatically added to ProjectCosts
   - Usage tracking linked to projects
   - Material traceability in project context

2. **Procurement Module**
   - Purchase order costs auto-populate ProjectCosts
   - PO delivery dates linked to project milestones
   - Supplier invoices linked to project financial tracking

3. **EN 1090 Compliance**
   - Compliance checkpoints as milestones
   - Certificate documents auto-linked
   - Execution class impacts project risk level

4. **Assembly Progress**
   - Assembly completion feeds project completion percentage
   - Manufacturing delays trigger milestone updates
   - Quality issues create change orders

5. **Dashboard Module**
   - Project metrics in operations dashboard
   - Real-time project health indicators
   - Integration with KPI tracking

## Implementation Phases

### Phase 1: Foundation (Week 1)
- ✅ Create implementation plan document
- Database schema design and migration
- Extend CrmProject model
- Create new entity models

### Phase 2: Backend Services (Week 1-2)
- Extend CrmService with project management methods
- Implement ProjectManagementService
- Create API endpoints in CrmController
- Unit tests for service layer

### Phase 3: HTTP Services (Week 2)
- Implement ProjectManagementHttpService
- Create mock service for demo mode
- Integration testing

### Phase 4: UI - Core Pages (Week 2-3)
- ProjectManagement.razor list page
- ProjectDetails.razor with tabs
- Stats and summary components
- Search and filter functionality

### Phase 5: UI - Dialogs (Week 3)
- ProjectDialog (create/edit)
- MilestoneDialog
- CostDialog
- DocumentUploadDialog
- ChangeOrderDialog

### Phase 6: Financial Features (Week 3-4)
- Financial tracking implementation
- Budget vs actual reporting
- Payment tracking
- Cost breakdown charts

### Phase 7: Advanced Features (Week 4)
- Change order workflow
- Resource management
- Risk assessment
- Timeline visualization

### Phase 8: Reporting & Polish (Week 4-5)
- Report generation
- Export functionality
- Performance optimization
- UI polish and UX improvements

### Phase 9: Testing & Documentation (Week 5)
- End-to-end testing
- User acceptance testing
- Update README.md
- Create user guide

## Success Metrics

### Functional Metrics:
- ✅ All CRUD operations working for projects, milestones, costs, documents
- ✅ Financial tracking accurately reflects budget vs actual
- ✅ Integration with existing modules seamless
- ✅ Real-time updates and calculations
- ✅ Feature gating properly enforced

### Performance Metrics:
- Project list loads in < 2 seconds
- Project details loads in < 1 second
- Document uploads < 5 seconds for files up to 10MB
- Report generation < 3 seconds
- No UI lag or blocking operations

### User Experience Metrics:
- Intuitive navigation and workflows
- Clear visual indicators (status, progress, alerts)
- Responsive design (mobile-friendly)
- Helpful error messages and validation
- Consistent with existing Manimp UI patterns

## Risk Mitigation

### Technical Risks:
- **Risk**: Database migration complexity
  - **Mitigation**: Thorough testing in dev environment, rollback plan
- **Risk**: Performance with large file uploads
  - **Mitigation**: Implement chunked uploads, file size limits
- **Risk**: Integration breaking existing features
  - **Mitigation**: Comprehensive regression testing, feature flags

### Business Risks:
- **Risk**: Feature complexity overwhelming users
  - **Mitigation**: Progressive disclosure, tooltips, optional advanced features
- **Risk**: Mobile usability challenges
  - **Mitigation**: Mobile-first design, responsive testing

## Future Enhancements (Post-Launch)

1. **AI-Powered Insights**
   - Project risk prediction
   - Cost overrun forecasting
   - Resource optimization suggestions

2. **Mobile App**
   - Dedicated mobile experience
   - Offline mode for site work
   - Photo capture and upload

3. **External Integrations**
   - QuickBooks/Xero integration
   - Microsoft Project integration
   - Email/calendar sync

4. **Advanced Scheduling**
   - Critical path method (CPM)
   - Resource leveling
   - What-if scenario analysis

5. **Client Portal**
   - Customer view of project status
   - Document sharing
   - Milestone approval workflow

## Conclusion

This comprehensive project management enhancement will transform Manimp into a complete project lifecycle management platform, providing metal fabricators with industry-leading tools for financial tracking, scheduling, resource management, and compliance. The phased approach ensures stable delivery while the enterprise features provide a clear upsell path for existing customers.

---
**Next Steps**: Begin Phase 1 implementation with database schema design and model creation.
