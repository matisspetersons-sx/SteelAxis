# Project Management Phase 2 - Implementation Plan

**Created**: October 20, 2025  
**Status**: Planning  
**Target Features**: Resource Allocation, Task Dependencies, Budget Tracking, Status Dashboard

---

## 🎯 Overview

This document outlines the implementation plan for four critical project management features that will transform Manimp into a comprehensive project management platform.

### Features in Scope
1. **Resource Allocation Interface** - Assign and track team members, equipment, and materials
2. **Task Dependencies** - Define and visualize task relationships and critical paths
3. **Project Budget Tracking** - Comprehensive financial monitoring with variance analysis
4. **Project Status Dashboard** - Real-time overview of all project metrics

---

## 📊 Feature 1: Resource Allocation Interface

### Business Value
- Optimize team utilization
- Prevent resource conflicts
- Track labor costs accurately
- Forecast resource needs
- Improve project scheduling

### Database Schema

#### New Tables

**ProjectResource**
```csharp
public class ProjectResource
{
    public int ProjectResourceId { get; set; }
    public int CrmProjectId { get; set; }
    public string ResourceType { get; set; } // Team, Equipment, Material
    public string ResourceId { get; set; } // UserId, EquipmentId, or MaterialId
    public string ResourceName { get; set; }
    public DateTime AllocationStartDate { get; set; }
    public DateTime AllocationEndDate { get; set; }
    public decimal AllocatedPercentage { get; set; } // 0-100% for partial allocation
    public decimal? HourlyRate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public string Status { get; set; } // Assigned, Active, Completed, Released
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
    public ICollection<ResourceTimeLog> TimeLogs { get; set; }
}

public class ResourceTimeLog
{
    public int TimeLogId { get; set; }
    public int ProjectResourceId { get; set; }
    public DateTime LogDate { get; set; }
    public decimal HoursWorked { get; set; }
    public string? TaskDescription { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
    
    // Navigation
    public ProjectResource ProjectResource { get; set; }
}

public class ResourceAvailability
{
    public int AvailabilityId { get; set; }
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }
    public string ResourceName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AvailablePercentage { get; set; } // Available capacity
    public string? Reason { get; set; } // Vacation, Training, etc.
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
}
```

### API Endpoints

```csharp
// POST /api/projects/{projectId}/resources
// Assign resource to project
public record AssignResourceRequest(
    string ResourceType,
    string ResourceId,
    string ResourceName,
    DateTime StartDate,
    DateTime EndDate,
    decimal AllocatedPercentage,
    decimal? HourlyRate,
    decimal? EstimatedHours
);

// GET /api/projects/{projectId}/resources
// Get all resources assigned to project

// PUT /api/projects/{projectId}/resources/{resourceId}
// Update resource allocation

// DELETE /api/projects/{projectId}/resources/{resourceId}
// Remove resource from project

// GET /api/resources/availability?startDate={date}&endDate={date}
// Check resource availability across all projects

// POST /api/projects/{projectId}/resources/{resourceId}/time-log
// Log time worked by resource

// GET /api/projects/{projectId}/resources/utilization
// Get resource utilization report
```

### UI Components

**Pages**
- `/projects/{id}/resources` - Main resource allocation page
- `/resources/calendar` - Resource availability calendar view
- `/resources/utilization` - Team utilization dashboard

**Components**
```razor
@* ResourceAllocationPanel.razor *@
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Project Resources</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudButton Color="Color.Primary" OnClick="OpenAssignDialog">
                Assign Resource
            </MudButton>
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudTable Items="@Resources" Hover="true">
            <HeaderContent>
                <MudTh>Resource</MudTh>
                <MudTh>Type</MudTh>
                <MudTh>Allocation</MudTh>
                <MudTh>Date Range</MudTh>
                <MudTh>Estimated Hours</MudTh>
                <MudTh>Actual Hours</MudTh>
                <MudTh>Cost</MudTh>
                <MudTh>Actions</MudTh>
            </HeaderContent>
            <RowTemplate>
                <!-- Resource row details -->
            </RowTemplate>
        </MudTable>
    </MudCardContent>
</MudCard>

@* ResourceAvailabilityCalendar.razor *@
@* Gantt-style calendar showing resource allocations *@

@* ResourceUtilizationChart.razor *@
@* Bar chart showing team member utilization percentages *@
```

### Implementation Steps

1. **Database** (2 days)
   - Create migration for ProjectResource, ResourceTimeLog, ResourceAvailability
   - Add indexes on CrmProjectId, ResourceId, dates
   - Configure relationships in AppDbContext

2. **Backend Services** (3 days)
   - `IResourceAllocationService` interface
   - Implement allocation logic with conflict detection
   - Implement availability checking
   - Create time logging service
   - Add utilization calculation methods

3. **API Controllers** (2 days)
   - `ResourceAllocationController` with full CRUD
   - Add feature gate `FeatureKeys.ResourceAllocation`
   - Implement validation and business rules

4. **UI Components** (4 days)
   - Build resource allocation panel
   - Create assignment dialog with conflict warnings
   - Build availability calendar view
   - Create utilization dashboard
   - Add time logging interface

5. **Testing & Documentation** (2 days)
   - Unit tests for allocation logic
   - Integration tests for API
   - Update user documentation

**Total Estimate**: 13 days

---

## 🔗 Feature 2: Task Dependencies

### Business Value
- Identify critical path automatically
- Prevent scheduling conflicts
- Understand task relationships
- Optimize project timeline
- Calculate project delays impact

### Database Schema

#### New Tables

**ProjectTask**
```csharp
public class ProjectTask
{
    public int TaskId { get; set; }
    public int CrmProjectId { get; set; }
    public int? MilestoneId { get; set; } // Optional link to milestone
    public string TaskName { get; set; }
    public string? Description { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int DurationDays { get; set; }
    public string Status { get; set; } // NotStarted, InProgress, Completed, Blocked
    public string Priority { get; set; } // Low, Medium, High, Critical
    public string? AssignedTo { get; set; } // UserId
    public decimal PercentComplete { get; set; } // 0-100
    public bool IsOnCriticalPath { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
    public ProjectMilestone? Milestone { get; set; }
    public ICollection<TaskDependency> DependenciesFrom { get; set; }
    public ICollection<TaskDependency> DependenciesTo { get; set; }
}

public class TaskDependency
{
    public int DependencyId { get; set; }
    public int PredecessorTaskId { get; set; }
    public int SuccessorTaskId { get; set; }
    public string DependencyType { get; set; } // FinishToStart, StartToStart, FinishToFinish, StartToFinish
    public int LagDays { get; set; } // Positive = lag, Negative = lead
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
    
    // Navigation
    public ProjectTask PredecessorTask { get; set; }
    public ProjectTask SuccessorTask { get; set; }
}

public class CriticalPathAnalysis
{
    public int AnalysisId { get; set; }
    public int CrmProjectId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public string CriticalPathTaskIds { get; set; } // JSON array of task IDs
    public decimal TotalProjectDuration { get; set; }
    public DateTime ProjectEarliestFinish { get; set; }
    public DateTime ProjectLatestFinish { get; set; }
    public decimal TotalSlackDays { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

### API Endpoints

```csharp
// POST /api/projects/{projectId}/tasks
// Create new task
public record CreateTaskRequest(
    string TaskName,
    string? Description,
    DateTime PlannedStartDate,
    DateTime PlannedEndDate,
    string Priority,
    string? AssignedTo,
    int? MilestoneId
);

// PUT /api/projects/{projectId}/tasks/{taskId}
// Update task

// POST /api/projects/{projectId}/tasks/{taskId}/dependencies
// Add task dependency
public record AddDependencyRequest(
    int PredecessorTaskId,
    int SuccessorTaskId,
    string DependencyType,
    int LagDays
);

// DELETE /api/projects/{projectId}/tasks/{taskId}/dependencies/{dependencyId}
// Remove dependency

// GET /api/projects/{projectId}/tasks/gantt
// Get Gantt chart data

// POST /api/projects/{projectId}/tasks/critical-path
// Calculate critical path
public record CriticalPathResponse(
    List<int> CriticalTaskIds,
    decimal TotalDuration,
    DateTime EarliestFinish,
    Dictionary<int, TaskScheduleInfo> TaskSchedules
);

// GET /api/projects/{projectId}/tasks/{taskId}/impact
// Calculate impact of task delay on project
```

### UI Components

**Pages**
- `/projects/{id}/tasks` - Task list and Gantt chart
- `/projects/{id}/tasks/dependencies` - Dependency diagram view

**Components**
```razor
@* GanttChart.razor *@
<div class="gantt-container">
    <div class="gantt-timeline">
        @* Date headers *@
    </div>
    <div class="gantt-tasks">
        @foreach (var task in Tasks)
        {
            <div class="gantt-task @(task.IsOnCriticalPath ? "critical" : "")"
                 style="left: @GetTaskPosition(task); width: @GetTaskWidth(task)">
                @task.TaskName
            </div>
            @* Dependency arrows *@
        }
    </div>
</div>

@* TaskDependencyGraph.razor *@
@* Network diagram showing task relationships *@

@* TaskForm.razor *@
<MudDialog>
    <DialogContent>
        <MudTextField @bind-Value="Task.TaskName" Label="Task Name" Required />
        <MudDatePicker @bind-Date="Task.PlannedStartDate" Label="Start Date" />
        <MudDatePicker @bind-Date="Task.PlannedEndDate" Label="End Date" />
        <MudSelect @bind-Value="Task.Priority" Label="Priority">
            <MudSelectItem Value="@("Low")">Low</MudSelectItem>
            <MudSelectItem Value="@("Medium")">Medium</MudSelectItem>
            <MudSelectItem Value="@("High")">High</MudSelectItem>
            <MudSelectItem Value="@("Critical")">Critical</MudSelectItem>
        </MudSelect>
        
        <MudText Typo="Typo.h6" Class="mt-4">Dependencies</MudText>
        <MudSelect @bind-Value="SelectedPredecessor" Label="Predecessor Task">
            @foreach (var t in AvailableTasks)
            {
                <MudSelectItem Value="@t.TaskId">@t.TaskName</MudSelectItem>
            }
        </MudSelect>
        <MudSelect @bind-Value="DependencyType" Label="Dependency Type">
            <MudSelectItem Value="@("FinishToStart")">Finish to Start</MudSelectItem>
            <MudSelectItem Value="@("StartToStart")">Start to Start</MudSelectItem>
        </MudSelect>
    </DialogContent>
</MudDialog>

@* CriticalPathHighlight.razor *@
@* Visual indicator for critical path tasks *@
```

### Implementation Steps

1. **Database** (2 days)
   - Create migration for ProjectTask, TaskDependency, CriticalPathAnalysis
   - Add indexes for performance
   - Configure self-referencing relationships

2. **Backend Services** (5 days)
   - `ITaskDependencyService` interface
   - Implement critical path calculation (CPM algorithm)
   - Create dependency validation (prevent circular dependencies)
   - Implement schedule calculation with dependencies
   - Add impact analysis for task delays

3. **API Controllers** (2 days)
   - `ProjectTaskController` with full CRUD
   - Add dependency management endpoints
   - Implement critical path calculation endpoint
   - Add feature gate `FeatureKeys.TaskDependencies`

4. **UI Components** (6 days)
   - Build interactive Gantt chart component
   - Create dependency graph visualization
   - Build task creation/edit dialog
   - Add dependency management UI
   - Implement critical path highlighting
   - Create impact analysis view

5. **Testing & Documentation** (2 days)
   - Unit tests for CPM algorithm
   - Test circular dependency detection
   - Integration tests for API
   - Update user documentation

**Total Estimate**: 17 days

---

## 💰 Feature 3: Project Budget Tracking

### Business Value
- Monitor project profitability in real-time
- Identify cost overruns early
- Track variance against budget
- Generate financial reports
- Improve cost estimation for future projects

### Database Schema

#### Extend Existing CrmProject
```csharp
// Add to CrmProject class
public decimal? BudgetAmount { get; set; }
public decimal? EstimatedCost { get; set; }
public decimal ActualCost { get; set; } // Calculated
public decimal? InvoicedAmount { get; set; }
public decimal? ReceivedAmount { get; set; }
public decimal? ProfitMargin { get; set; } // Calculated
public DateTime? BudgetApprovedDate { get; set; }
public string? BudgetApprovedBy { get; set; }
```

#### New Tables

**ProjectCostItem**
```csharp
public class ProjectCostItem
{
    public int CostItemId { get; set; }
    public int CrmProjectId { get; set; }
    public string CostCategory { get; set; } // Material, Labor, Equipment, Outsourcing, Overhead
    public string CostType { get; set; } // More specific: Steel, Welding, Paint, etc.
    public string Description { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string? Reference { get; set; } // PO number, invoice number, time sheet
    public string? Supplier { get; set; }
    public bool IsBudgeted { get; set; }
    public string Status { get; set; } // Planned, Committed, Invoiced, Paid
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}

public class BudgetVarianceAlert
{
    public int AlertId { get; set; }
    public int CrmProjectId { get; set; }
    public string AlertType { get; set; } // OverBudget, AtRisk, CostSpike
    public string Category { get; set; }
    public decimal ThresholdPercentage { get; set; }
    public decimal CurrentVariancePercentage { get; set; }
    public decimal VarianceAmount { get; set; }
    public DateTime DetectedDate { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}

public class ProjectBudgetSnapshot
{
    public int SnapshotId { get; set; }
    public int CrmProjectId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualCost { get; set; }
    public decimal PercentComplete { get; set; }
    public decimal EarnedValue { get; set; } // % complete * budget
    public decimal CostVariance { get; set; } // EV - Actual Cost
    public decimal ScheduleVariance { get; set; }
    public decimal CostPerformanceIndex { get; set; } // EV / Actual Cost
    public decimal SchedulePerformanceIndex { get; set; }
    public decimal EstimateAtCompletion { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

### API Endpoints

```csharp
// POST /api/projects/{projectId}/budget
// Set or update project budget
public record SetBudgetRequest(
    decimal BudgetAmount,
    decimal EstimatedCost,
    Dictionary<string, decimal> CategoryBreakdown
);

// POST /api/projects/{projectId}/costs
// Add cost item
public record AddCostRequest(
    string CostCategory,
    string CostType,
    string Description,
    decimal PlannedAmount,
    decimal ActualAmount,
    DateTime PlannedDate,
    DateTime? ActualDate,
    string? Reference
);

// GET /api/projects/{projectId}/budget/summary
// Get budget summary
public record BudgetSummaryResponse(
    decimal BudgetAmount,
    decimal ActualCost,
    decimal Variance,
    decimal VariancePercentage,
    Dictionary<string, CategoryCost> BreakdownByCategory,
    List<BudgetVarianceAlert> ActiveAlerts
);

// GET /api/projects/{projectId}/budget/trend
// Get cost trend over time

// GET /api/projects/{projectId}/budget/earned-value
// Get earned value management metrics
public record EarnedValueResponse(
    decimal PlannedValue,
    decimal EarnedValue,
    decimal ActualCost,
    decimal CostVariance,
    decimal ScheduleVariance,
    decimal CPI,
    decimal SPI,
    decimal EstimateAtCompletion,
    decimal EstimateToComplete
);

// POST /api/projects/{projectId}/budget/snapshot
// Create budget snapshot for historical tracking

// GET /api/projects/{projectId}/budget/forecast
// Forecast final project cost based on current trends
```

### UI Components

**Pages**
- `/projects/{id}/budget` - Main budget tracking dashboard
- `/projects/{id}/costs` - Detailed cost breakdown
- `/projects/{id}/budget/analysis` - Earned value analysis

**Components**
```razor
@* BudgetDashboard.razor *@
<MudGrid>
    <MudItem xs="12" md="3">
        <MudCard>
            <MudCardContent>
                <MudText Typo="Typo.h6">Total Budget</MudText>
                <MudText Typo="Typo.h3" Color="Color.Primary">
                    @BudgetSummary.BudgetAmount.ToString("C")
                </MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
    <MudItem xs="12" md="3">
        <MudCard>
            <MudCardContent>
                <MudText Typo="Typo.h6">Actual Cost</MudText>
                <MudText Typo="Typo.h3" Color="@GetCostColor()">
                    @BudgetSummary.ActualCost.ToString("C")
                </MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
    <MudItem xs="12" md="3">
        <MudCard>
            <MudCardContent>
                <MudText Typo="Typo.h6">Variance</MudText>
                <MudText Typo="Typo.h3" Color="@GetVarianceColor()">
                    @BudgetSummary.Variance.ToString("C")
                </MudText>
                <MudText Typo="Typo.body2">
                    @BudgetSummary.VariancePercentage.ToString("P1")
                </MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
    <MudItem xs="12" md="3">
        <MudCard>
            <MudCardContent>
                <MudText Typo="Typo.h6">Forecast</MudText>
                <MudText Typo="Typo.h3">
                    @ForecastedTotal.ToString("C")
                </MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>

@* Budget vs Actual Chart *@
<MudChart ChartType="ChartType.Bar" ChartData="@BudgetChartData" />

@* Cost Breakdown by Category *@
<MudTable Items="@CategoryBreakdown">
    <HeaderContent>
        <MudTh>Category</MudTh>
        <MudTh>Budgeted</MudTh>
        <MudTh>Actual</MudTh>
        <MudTh>Variance</MudTh>
        <MudTh>% of Budget</MudTh>
    </HeaderContent>
</MudTable>

@* BudgetVarianceAlerts.razor *@
<MudAlert Severity="Severity.Warning" Variant="Variant.Filled">
    <MudText>Material costs are 15% over budget!</MudText>
    <MudText Typo="Typo.body2">Current: $45,000 | Budget: $39,000</MudText>
</MudAlert>

@* CostTrendChart.razor *@
@* Line chart showing cost accumulation over time *@

@* EarnedValueMetrics.razor *@
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Earned Value Analysis</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudGrid>
            <MudItem xs="6">
                <MudText>Cost Performance Index (CPI)</MudText>
                <MudText Typo="Typo.h4" Color="@GetCpiColor()">
                    @CPI.ToString("F2")
                </MudText>
                <MudText Typo="Typo.body2">
                    @(CPI > 1 ? "Under Budget" : "Over Budget")
                </MudText>
            </MudItem>
            <MudItem xs="6">
                <MudText>Schedule Performance Index (SPI)</MudText>
                <MudText Typo="Typo.h4" Color="@GetSpiColor()">
                    @SPI.ToString("F2")
                </MudText>
                <MudText Typo="Typo.body2">
                    @(SPI > 1 ? "Ahead of Schedule" : "Behind Schedule")
                </MudText>
            </MudItem>
        </MudGrid>
    </MudCardContent>
</MudCard>
```

### Implementation Steps

1. **Database** (2 days)
   - Create migration for ProjectCostItem, BudgetVarianceAlert, ProjectBudgetSnapshot
   - Add budget fields to CrmProject
   - Add indexes and constraints

2. **Backend Services** (5 days)
   - `IBudgetTrackingService` interface
   - Implement cost aggregation from inventory, procurement, labor
   - Create variance calculation and alert system
   - Implement earned value management calculations
   - Add budget forecasting algorithm
   - Create snapshot service for historical tracking

3. **API Controllers** (2 days)
   - `BudgetController` with all endpoints
   - Add feature gate `FeatureKeys.BudgetTracking`
   - Implement validation and business rules

4. **UI Components** (5 days)
   - Build budget dashboard with metrics cards
   - Create cost breakdown tables
   - Build budget vs actual charts
   - Implement cost trend visualization
   - Create earned value metrics display
   - Add alert notifications
   - Build cost entry forms

5. **Integration** (2 days)
   - Connect to inventory usage tracking
   - Link to procurement purchase orders
   - Integrate with labor time logs
   - Create automated cost capture

6. **Testing & Documentation** (2 days)
   - Unit tests for calculations
   - Test alert thresholds
   - Integration tests for API
   - Update user documentation

**Total Estimate**: 18 days

---

## 📊 Feature 4: Project Status Dashboard

### Business Value
- Single-pane view of all project health metrics
- Quick identification of at-risk projects
- Executive-level reporting
- Real-time project portfolio overview
- Data-driven decision making

### Dashboard Metrics

#### Key Performance Indicators (KPIs)
1. **Schedule Health**
   - On-time delivery percentage
   - Days ahead/behind schedule
   - Milestone completion rate
   - Critical path status

2. **Budget Health**
   - Budget variance ($ and %)
   - Cost performance index (CPI)
   - Burn rate
   - Forecast at completion

3. **Resource Health**
   - Team utilization %
   - Resource conflicts
   - Overtime hours
   - Resource availability

4. **Quality & Risk**
   - Open issues count
   - NCR (Non-Conformance Reports) count
   - Risk exposure score
   - Quality inspection pass rate

5. **Progress Metrics**
   - Overall % complete
   - Tasks completed vs total
   - Deliverables completed
   - Customer approvals received

### Database Schema

#### New Tables

**ProjectStatusSnapshot**
```csharp
public class ProjectStatusSnapshot
{
    public int SnapshotId { get; set; }
    public int CrmProjectId { get; set; }
    public DateTime SnapshotDate { get; set; }
    
    // Schedule Health
    public string ScheduleStatus { get; set; } // OnTrack, AtRisk, Delayed
    public int DaysAheadBehind { get; set; }
    public decimal MilestoneCompletionRate { get; set; }
    public bool IsOnCriticalPath { get; set; }
    
    // Budget Health
    public string BudgetStatus { get; set; } // OnBudget, AtRisk, OverBudget
    public decimal BudgetVariance { get; set; }
    public decimal BudgetVariancePercentage { get; set; }
    public decimal CostPerformanceIndex { get; set; }
    
    // Resource Health
    public decimal TeamUtilization { get; set; }
    public int ResourceConflictCount { get; set; }
    public decimal OvertimeHours { get; set; }
    
    // Quality & Risk
    public int OpenIssuesCount { get; set; }
    public int OpenNcrCount { get; set; }
    public decimal RiskScore { get; set; }
    public decimal QualityScore { get; set; }
    
    // Progress
    public decimal PercentComplete { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public int DeliverablesCompleted { get; set; }
    public int TotalDeliverables { get; set; }
    
    public string? Notes { get; set; }
    public string CreatedBy { get; set; }
}

public class ProjectHealthScore
{
    public int HealthScoreId { get; set; }
    public int CrmProjectId { get; set; }
    public DateTime CalculatedDate { get; set; }
    public decimal OverallScore { get; set; } // 0-100
    public decimal ScheduleScore { get; set; }
    public decimal BudgetScore { get; set; }
    public decimal ResourceScore { get; set; }
    public decimal QualityScore { get; set; }
    public string HealthRating { get; set; } // Excellent, Good, Fair, Poor, Critical
    public string? Recommendations { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}

public class ProjectRiskRegister
{
    public int RiskId { get; set; }
    public int CrmProjectId { get; set; }
    public string RiskCategory { get; set; } // Schedule, Budget, Quality, Resource, External
    public string RiskDescription { get; set; }
    public string Impact { get; set; } // Low, Medium, High, Critical
    public string Probability { get; set; } // Low, Medium, High
    public decimal RiskScore { get; set; } // Calculated: Impact x Probability
    public string MitigationStrategy { get; set; }
    public string Status { get; set; } // Identified, Mitigating, Resolved, Occurred
    public string? Owner { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string CreatedBy { get; set; }
    
    // Navigation
    public CrmProject Project { get; set; }
}
```

### API Endpoints

```csharp
// GET /api/projects/{projectId}/status
// Get current project status
public record ProjectStatusResponse(
    string ProjectName,
    string OverallHealth,
    decimal HealthScore,
    ScheduleHealth Schedule,
    BudgetHealth Budget,
    ResourceHealth Resources,
    QualityHealth Quality,
    ProgressMetrics Progress,
    List<RiskItem> TopRisks,
    List<string> Recommendations
);

// GET /api/projects/dashboard
// Get dashboard for all projects
public record DashboardResponse(
    int TotalProjects,
    int OnTrack,
    int AtRisk,
    int Delayed,
    List<ProjectSummary> Projects,
    List<AlertItem> Alerts,
    Dictionary<string, int> ProjectsByStatus
);

// GET /api/projects/{projectId}/health-history
// Get health score trend over time

// POST /api/projects/{projectId}/status/snapshot
// Create status snapshot

// GET /api/projects/{projectId}/risks
// Get risk register for project

// POST /api/projects/{projectId}/risks
// Add new risk
public record AddRiskRequest(
    string RiskCategory,
    string RiskDescription,
    string Impact,
    string Probability,
    string MitigationStrategy,
    string Owner
);

// GET /api/projects/portfolio-health
// Get portfolio-level health metrics
```

### UI Components

**Pages**
- `/projects/dashboard` - Main project portfolio dashboard
- `/projects/{id}/status` - Individual project status page
- `/dashboard/executive` - Executive summary dashboard

**Components**
```razor
@* ProjectPortfolioDashboard.razor *@
<MudGrid>
    <MudItem xs="12">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h5">Project Portfolio Overview</MudText>
                </CardHeaderContent>
                <CardHeaderActions>
                    <MudButton Color="Color.Primary" Href="/projects/create">
                        New Project
                    </MudButton>
                </CardHeaderActions>
            </MudCardHeader>
            <MudCardContent>
                <MudGrid>
                    <MudItem xs="3">
                        <MudCard Elevation="0" Class="status-card">
                            <MudCardContent>
                                <MudText Typo="Typo.h3" Color="Color.Success">
                                    @OnTrackCount
                                </MudText>
                                <MudText>On Track</MudText>
                            </MudCardContent>
                        </MudCard>
                    </MudItem>
                    <MudItem xs="3">
                        <MudCard Elevation="0" Class="status-card">
                            <MudCardContent>
                                <MudText Typo="Typo.h3" Color="Color.Warning">
                                    @AtRiskCount
                                </MudText>
                                <MudText>At Risk</MudText>
                            </MudCardContent>
                        </MudCard>
                    </MudItem>
                    <MudItem xs="3">
                        <MudCard Elevation="0" Class="status-card">
                            <MudCardContent>
                                <MudText Typo="Typo.h3" Color="Color.Error">
                                    @DelayedCount
                                </MudText>
                                <MudText>Delayed</MudText>
                            </MudCardContent>
                        </MudCard>
                    </MudItem>
                    <MudItem xs="3">
                        <MudCard Elevation="0" Class="status-card">
                            <MudCardContent>
                                <MudText Typo="Typo.h3" Color="Color.Info">
                                    @TotalProjects
                                </MudText>
                                <MudText>Total Projects</MudText>
                            </MudCardContent>
                        </MudCard>
                    </MudItem>
                </MudGrid>
            </MudCardContent>
        </MudCard>
    </MudItem>

    <MudItem xs="12">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">Active Projects</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                <MudTable Items="@Projects" Hover="true" OnRowClick="NavigateToProject">
                    <HeaderContent>
                        <MudTh>Project</MudTh>
                        <MudTh>Customer</MudTh>
                        <MudTh>Health</MudTh>
                        <MudTh>Progress</MudTh>
                        <MudTh>Schedule</MudTh>
                        <MudTh>Budget</MudTh>
                        <MudTh>Due Date</MudTh>
                        <MudTh>Actions</MudTh>
                    </HeaderContent>
                    <RowTemplate>
                        <MudTd>@context.ProjectName</MudTd>
                        <MudTd>@context.CustomerName</MudTd>
                        <MudTd>
                            <ProjectHealthBadge HealthScore="@context.HealthScore" />
                        </MudTd>
                        <MudTd>
                            <MudProgressLinear Value="@context.PercentComplete" 
                                              Color="@GetProgressColor(context)" />
                            <MudText Typo="Typo.caption">
                                @context.PercentComplete.ToString("F0")%
                            </MudText>
                        </MudTd>
                        <MudTd>
                            <MudChip Size="Size.Small" 
                                    Color="@GetScheduleStatusColor(context.ScheduleStatus)">
                                @context.ScheduleStatus
                            </MudChip>
                        </MudTd>
                        <MudTd>
                            <MudChip Size="Size.Small" 
                                    Color="@GetBudgetStatusColor(context.BudgetStatus)">
                                @context.BudgetVariance.ToString("C0")
                            </MudChip>
                        </MudTd>
                        <MudTd>@context.PlannedDeliveryDate.ToShortDateString()</MudTd>
                        <MudTd>
                            <MudIconButton Icon="@Icons.Material.Filled.Visibility" 
                                          Size="Size.Small"
                                          Href="@($"/projects/{context.ProjectId}/status")" />
                        </MudTd>
                    </RowTemplate>
                </MudTable>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>

@* ProjectHealthBadge.razor *@
<MudChip Color="@GetHealthColor()" Size="Size.Small">
    @HealthRating
</MudChip>

@* ProjectStatusCard.razor *@
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h5">@ProjectName</MudText>
            <ProjectHealthBadge HealthScore="@HealthScore" />
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudGrid>
            <MudItem xs="6">
                <MudText Typo="Typo.h6">Schedule</MudText>
                <MudProgressCircular Value="@ScheduleScore" Color="@GetScheduleColor()">
                    @ScheduleScore%
                </MudProgressCircular>
            </MudItem>
            <MudItem xs="6">
                <MudText Typo="Typo.h6">Budget</MudText>
                <MudProgressCircular Value="@BudgetScore" Color="@GetBudgetColor()">
                    @BudgetScore%
                </MudProgressCircular>
            </MudItem>
        </MudGrid>
        
        <MudDivider Class="my-4" />
        
        <MudText Typo="Typo.h6">Key Metrics</MudText>
        <MudSimpleTable Dense="true">
            <tbody>
                <tr>
                    <td>Progress</td>
                    <td>@PercentComplete%</td>
                </tr>
                <tr>
                    <td>Days Ahead/Behind</td>
                    <td class="@(DaysAheadBehind >= 0 ? "text-success" : "text-error")">
                        @DaysAheadBehind days
                    </td>
                </tr>
                <tr>
                    <td>Budget Variance</td>
                    <td class="@(BudgetVariance >= 0 ? "text-success" : "text-error")">
                        @BudgetVariance.ToString("C0")
                    </td>
                </tr>
                <tr>
                    <td>Team Utilization</td>
                    <td>@TeamUtilization%</td>
                </tr>
            </tbody>
        </MudSimpleTable>
    </MudCardContent>
</MudCard>

@* HealthTrendChart.razor *@
<MudChart ChartType="ChartType.Line" 
          ChartSeries="@HealthTrendSeries" 
          XAxisLabels="@DateLabels"
          ChartOptions="@ChartOptions" />

@* RiskRegisterTable.razor *@
<MudTable Items="@Risks" Dense="true">
    <HeaderContent>
        <MudTh>Risk</MudTh>
        <MudTh>Category</MudTh>
        <MudTh>Impact</MudTh>
        <MudTh>Probability</MudTh>
        <MudTh>Score</MudTh>
        <MudTh>Status</MudTh>
        <MudTh>Owner</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.RiskDescription</MudTd>
        <MudTd>@context.RiskCategory</MudTd>
        <MudTd>
            <MudChip Size="Size.Small" Color="@GetImpactColor(context.Impact)">
                @context.Impact
            </MudChip>
        </MudTd>
        <MudTd>@context.Probability</MudTd>
        <MudTd>
            <MudText Color="@GetRiskScoreColor(context.RiskScore)">
                @context.RiskScore
            </MudText>
        </MudTd>
        <MudTd>@context.Status</MudTd>
        <MudTd>@context.Owner</MudTd>
    </RowTemplate>
</MudTable>

@* ExecutiveDashboard.razor *@
<MudGrid>
    <MudItem xs="12" md="6">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">Portfolio Health Distribution</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                <MudChart ChartType="ChartType.Donut" ChartData="@HealthDistribution" />
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <MudItem xs="12" md="6">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">Budget Performance</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                <MudChart ChartType="ChartType.Bar" 
                         ChartSeries="@BudgetPerformanceSeries" />
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>
```

### Implementation Steps

1. **Database** (2 days)
   - Create migration for ProjectStatusSnapshot, ProjectHealthScore, ProjectRiskRegister
   - Add indexes for performance
   - Configure relationships

2. **Backend Services** (6 days)
   - `IProjectDashboardService` interface
   - Implement health score calculation algorithm
   - Create status snapshot service
   - Build risk scoring logic
   - Implement portfolio aggregation
   - Create recommendation engine
   - Add alerting service for at-risk projects

3. **API Controllers** (2 days)
   - `ProjectDashboardController` with all endpoints
   - Add feature gate `FeatureKeys.ProjectDashboard`
   - Implement caching for performance

4. **UI Components** (6 days)
   - Build portfolio dashboard page
   - Create project status card components
   - Build health score visualization
   - Implement trend charts
   - Create risk register table
   - Build executive dashboard
   - Add real-time updates (SignalR)

5. **Integration** (2 days)
   - Connect to all other project management features
   - Set up automated snapshot creation (daily)
   - Integrate with alerting system

6. **Testing & Documentation** (2 days)
   - Unit tests for health calculations
   - Performance testing for dashboard loads
   - Integration tests for API
   - Update user documentation

**Total Estimate**: 20 days

---

## 📅 Implementation Timeline

### Phase 2A: Foundation (Weeks 1-3)
**Duration**: 3 weeks

**Week 1-2: Resource Allocation (13 days)**
- Database schema and migrations
- Backend services
- API controllers
- UI components

**Week 3: Testing & Documentation**
- Complete resource allocation testing
- User documentation
- Begin task dependencies planning

### Phase 2B: Dependencies & Budget (Weeks 4-7)
**Duration**: 4 weeks

**Week 4-5: Task Dependencies (17 days)**
- Database schema
- CPM algorithm implementation
- Dependency validation
- Gantt chart UI

**Week 6-7: Budget Tracking (18 days)**
- Database schema
- Budget calculation services
- EVM implementation
- Budget dashboard UI

### Phase 2C: Dashboard & Integration (Weeks 8-10)
**Duration**: 3 weeks

**Week 8-9: Status Dashboard (20 days)**
- Database schema
- Health score algorithms
- Dashboard UI components
- Portfolio views

**Week 10: Integration & Polish**
- Cross-feature integration
- Performance optimization
- End-to-end testing
- Documentation finalization

### Total Phase 2 Timeline
**Total Duration**: 10 weeks (2.5 months)  
**Total Effort**: 68 days (distributed across team)

---

## 🎯 Success Criteria

### Resource Allocation
- [ ] Can assign team members to projects with allocation %
- [ ] Conflict detection works across projects
- [ ] Utilization reports show accurate data
- [ ] Time logging captures actual hours

### Task Dependencies
- [ ] Can create tasks with predecessors/successors
- [ ] Critical path calculates correctly
- [ ] Gantt chart displays accurately
- [ ] Dependency changes update schedules automatically

### Budget Tracking
- [ ] Budget vs actual updates in real-time
- [ ] Variance alerts trigger at thresholds
- [ ] EVM metrics calculate correctly
- [ ] Cost forecasting provides accurate estimates

### Status Dashboard
- [ ] Portfolio view shows all projects
- [ ] Health scores update automatically
- [ ] Risk register tracks top risks
- [ ] Executive dashboard provides summary metrics

---

## 🔐 Feature Gating

All features will be gated appropriately:

```csharp
// Add to FeatureKeys.cs
public static class FeatureKeys
{
    // ... existing features ...
    
    // Phase 2 Features
    public const string ResourceAllocation = "ResourceAllocation";
    public const string TaskDependencies = "TaskDependencies";
    public const string BudgetTracking = "BudgetTracking";
    public const string ProjectDashboard = "ProjectDashboard";
}
```

### Plan Association
- **Starter**: None of these features
- **Professional**: ResourceAllocation, TaskDependencies, BudgetTracking (basic)
- **Enterprise**: All features with advanced analytics

---

## 📚 Documentation Requirements

For each feature, create:

1. **User Guide**
   - Feature overview
   - Step-by-step tutorials
   - Best practices
   - Common scenarios

2. **API Documentation**
   - Endpoint specifications
   - Request/response examples
   - Error codes
   - Rate limits

3. **Developer Guide**
   - Architecture overview
   - Database schema
   - Service interfaces
   - Extension points

4. **Admin Guide**
   - Feature configuration
   - Performance tuning
   - Monitoring
   - Troubleshooting

---

## 🚀 Next Steps

1. **Week 1**: Begin Resource Allocation implementation
   - Create database migration
   - Implement core services
   - Set up API endpoints

2. **Review Points**: End of each feature implementation
   - Demo to stakeholders
   - Gather feedback
   - Adjust priorities if needed

3. **Continuous**: Documentation
   - Update docs as features are completed
   - Create video tutorials
   - Build example workflows

---

## 📊 Progress Tracking

Track implementation progress in project management tool:

- [ ] **Resource Allocation**: 0% (Not Started)
  - [ ] Database (0/2 days)
  - [ ] Backend (0/3 days)
  - [ ] API (0/2 days)
  - [ ] UI (0/4 days)
  - [ ] Testing (0/2 days)

- [ ] **Task Dependencies**: 0% (Not Started)
  - [ ] Database (0/2 days)
  - [ ] Backend (0/5 days)
  - [ ] API (0/2 days)
  - [ ] UI (0/6 days)
  - [ ] Testing (0/2 days)

- [ ] **Budget Tracking**: 0% (Not Started)
  - [ ] Database (0/2 days)
  - [ ] Backend (0/5 days)
  - [ ] API (0/2 days)
  - [ ] UI (0/5 days)
  - [ ] Integration (0/2 days)
  - [ ] Testing (0/2 days)

- [ ] **Status Dashboard**: 0% (Not Started)
  - [ ] Database (0/2 days)
  - [ ] Backend (0/6 days)
  - [ ] API (0/2 days)
  - [ ] UI (0/6 days)
  - [ ] Integration (0/2 days)
  - [ ] Testing (0/2 days)

---

**Let's build amazing project management features!** 🚀
