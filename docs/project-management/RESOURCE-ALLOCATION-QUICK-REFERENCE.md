# Resource Allocation Feature - Quick Reference

**Feature Status**: ✅ 100% Complete (Week 1-2)  
**Last Updated**: October 20, 2025  

---

## 📁 Files Created/Modified

### UI Components (NEW)
```
Manimp.Web/
├── Services/
│   └── ResourceAllocationHttpService.cs (220 lines)
└── Components/
    ├── Pages/
    │   └── ResourceAllocation.razor (340 lines)
    └── Dialogs/
        ├── AllocateResourceDialog.razor (130 lines)
        ├── LogTimeDialog.razor (90 lines)
        └── EditAllocationDialog.razor (120 lines)
```

### Tests (NEW)
```
Manimp.Tests/
├── ResourceAllocationServiceTests.cs (550+ lines, 14 tests)
└── Manimp.Tests.csproj (project file)
```

### Previously Created (Week 1)
```
Manimp.Shared/Models/
├── ResourceAllocation.cs (4 domain models)
└── FeatureGating.cs (4 feature keys added)

Manimp.Data/
└── AppDbContext.cs (DbSets & configuration)

Manimp.Services/Implementation/
├── ResourceAllocationService.cs (12 methods)
└── IResourceAllocationService interface

Manimp.Api/
├── Controllers/ResourceAllocationController.cs (9 endpoints)
└── Program.cs (service registration)
```

---

## 🎯 Feature Overview

### What It Does
- Allocate resources (people, equipment, materials) to projects
- Track resource utilization across projects
- Log time worked on allocations
- Calculate costs and availability
- Detect allocation conflicts

### Who Uses It
- Project managers allocating resources
- Team members logging hours
- Finance tracking costs
- Executives monitoring utilization

### When It's Used
- Project planning phase (allocate resources)
- Daily/weekly (log time worked)
- Project reviews (check utilization)
- Resource planning (check availability)

---

## 🚀 Getting Started

### Access the Feature
```
URL: /projects/{projectId}/resources
Example: /projects/1/resources
```

### Main Page Layout
```
┌─────────────────────────────────────────┐
│  Resource Allocation                    │
│  Manage resources and track utilization │
│                          [Allocate]     │
├─────────────────────────────────────────┤
│  [Total Allocated] [Avg Util] [Hours]   │
│  [Est. Cost]                            │
├─────────────────────────────────────────┤
│  Resource Type | Status | % | Hours | $ │
│  ─────────────────────────────────────  │
│  Developer     | Active | 50| 40    |$4K│
│  Designer      | Active | 30| 25    |$2K│
│                     [⏱️] [✎] [🗑️]      │
└─────────────────────────────────────────┘
```

---

## 📊 Key Metrics

### Statistics Dashboard
- **Total Allocated**: Number of resources
- **Avg Utilization**: Average utilization %
- **Hours Logged**: Sum of actual hours worked
- **Est. Cost**: Total estimated cost

### Per-Resource Metrics
- **Allocated %**: Resource capacity used (0-100%)
- **Estimated Hours**: Planned hours
- **Actual Hours**: Hours logged
- **Utilization %**: (Actual / Estimated) × 100
- **Est. Cost**: Hourly Rate × Estimated Hours

---

## 🔄 Core Workflows

### Workflow 1: Allocate Resource
```
1. Click "Allocate Resource"
   ↓
2. Enter resource details
   - Type: Developer, Designer, Equipment, etc.
   - ID: Unique identifier
   - Name: Display name
   ↓
3. Set allocation details
   - Percentage: 0-100% (capacity)
   - Hourly Rate: Cost per hour
   - Estimated Hours: Expected total hours
   ↓
4. Set allocation period
   - Start Date: When allocated
   - End Date: When allocation ends
   ↓
5. Click "Allocate Resource"
   → Resource added to project
```

### Workflow 2: Log Time
```
1. Click timer icon (⏱️) on resource row
   ↓
2. Select log date
   ↓
3. Enter hours worked (0-24)
   ↓
4. Add optional description
   ↓
5. Click "Log Time"
   → ActualHours updated automatically
```

### Workflow 3: Update Allocation
```
1. Click edit icon (✎) on resource row
   ↓
2. Modify details
   - Allocation %
   - Hourly rate
   - Estimated hours
   - End date
   ↓
3. Click "Update Allocation"
   → Changes saved
```

### Workflow 4: Remove Resource
```
1. Click delete icon (🗑️) on resource row
   ↓
2. Confirm removal
   ↓
3. Resource removed from project
```

---

## 💾 Data Structure

### ProjectResource Entity
```csharp
{
  ProjectResourceId: int,          // Primary key
  CrmProjectId: int,               // Foreign key to project
  ResourceType: string,            // Developer, Designer, etc.
  ResourceId: string,              // External ID (EMP001)
  ResourceName: string,            // Display name
  AllocatedPercentage: decimal,    // 0-100%
  HourlyRate: decimal,             // Cost per hour
  EstimatedHours: decimal,         // Planned hours
  ActualHours: decimal,            // Hours logged
  AllocationStartDate: DateTime,   // Period start
  AllocationEndDate: DateTime,     // Period end
  Status: string,                  // Assigned/Active/Completed/Released
  Notes: string?,                  // Optional notes
  CreatedDate: DateTime,           // Audit
  LastModifiedDate: DateTime       // Audit
}
```

### ResourceTimeLog Entity
```csharp
{
  ResourceTimeLogId: int,
  ProjectResourceId: int,          // Foreign key
  HoursWorked: decimal,            // Hours logged
  LogDate: DateTime,               // When worked
  Description: string?,            // What done
  CreatedDate: DateTime,           // Audit
  LastModifiedDate: DateTime       // Audit
}
```

---

## 🧪 Testing

### Run All Tests
```bash
dotnet test Manimp.Tests/Manimp.Tests.csproj -v normal
```

### Test Categories
- **Allocation Tests** (4): Creation, validation, retrieval
- **Conflict Detection** (2): Overlapping vs. non-overlapping
- **Time Logging** (3): Creation, updates, retrieval
- **Utilization Calc** (2): Percentage calculation, edge cases
- **Availability** (2): Multi-project, no allocations
- **Cost Calc** (1): Hourly rate × estimated hours

### Test Results Expected
```
14 tests passed in ~500ms
✓ Allocation success cases
✓ Conflict detection working
✓ Time accumulation correct
✓ Utilization math accurate
✓ Availability calculated right
✓ Cost calculations correct
```

---

## 🔌 API Endpoints

### Base Route
```
/api/projects/{projectId}/resources
```

### Endpoints
```
POST   /                      → Allocate resource
GET    /                      → List project resources
GET    /{resourceId}          → Get single resource
PUT    /{resourceId}          → Update resource
DELETE /{resourceId}          → Remove resource

POST   /{resourceId}/time-logs        → Log time
GET    /{resourceId}/time-logs        → Get time logs
GET    /availability?type=...&id=...  → Check availability
GET    /utilization                   → Get project utilization
```

### Response Format
```csharp
// Success
{
  "success": true,
  "message": "Operation successful",
  "data": { /* entity */ }
}

// Error
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

---

## 🎨 UI Components

### ResourceAllocation.razor
- Main page showing all allocated resources
- Statistics dashboard (4 cards)
- Data table with inline actions
- Integration with dialogs
- Snackbar notifications

### Dialogs
- **AllocateResourceDialog**: New resource allocation form
- **LogTimeDialog**: Time entry form
- **EditAllocationDialog**: Modification form

### Features
- Real-time calculations
- Date validation
- Numeric input validation
- Error messages
- Success feedback

---

## 🛡️ Security

### Feature Gating
All endpoints protected by `[RequireFeature(FeatureKeys.ResourceAllocation)]`

### Authorization
All endpoints require `[Authorize]` attribute

### Access Control
- Only authenticated users can access
- Feature must be enabled for tenant
- Tier-based access via feature gates

---

## 📈 Calculations

### Utilization Percentage
```
Utilization % = (ActualHours / EstimatedHours) × 100

Examples:
- 20 actual / 40 estimated = 50%
- 45 actual / 40 estimated = 112.5% (over-allocated)
- 0 actual / 40 estimated = 0%
```

### Resource Availability
```
Allocated % = Sum of all allocation percentages for resource

Available % = 100 - Allocated %

Examples:
- 50% allocated = 50% available
- 80% allocated = 20% available
- 100% allocated = 0% available
```

### Estimated Cost
```
Est. Cost = Hourly Rate × Estimated Hours
Act. Cost = Hourly Rate × Actual Hours

Examples:
- $100/hr × 40 hours = $4,000
- $80/hr × 30 hours = $2,400
```

### Conflict Detection
```
Allocations conflict if:
- Same resource type AND
- Same resource ID AND
- Date ranges overlap (start < otherEnd AND end > otherStart)
```

---

## 🐛 Common Issues & Solutions

### Issue: Resource not showing in dropdown
**Solution**: Resource must exist and be active in the system

### Issue: Conflict warning on allocation
**Solution**: Change resource ID, change dates, or release conflicting allocation

### Issue: Hours not accumulating
**Solution**: Check if LogDate is within allocation period

### Issue: Costs not calculating
**Solution**: Verify hourly rate and estimated hours are set

### Issue: Utilization shows 0%
**Solution**: Ensure EstimatedHours > 0; log time to increase ActualHours

---

## 📚 Related Documentation

- [PHASE2-IMPLEMENTATION-PLAN.md](./PHASE2-IMPLEMENTATION-PLAN.md) - Full roadmap
- [WEEK1-SUMMARY.md](./WEEK1-SUMMARY.md) - Implementation summary
- [RESOURCE-ALLOCATION-IMPLEMENTATION.md](./RESOURCE-ALLOCATION-IMPLEMENTATION.md) - Technical details
- [UI-TESTING-IMPLEMENTATION.md](./UI-TESTING-IMPLEMENTATION.md) - UI & test details

---

## 📞 Support

**Questions?** Check the related documentation above or code comments.

**Issues?** File a bug with:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots if applicable

**Feature Requests?** Suggest in the "What's Next" section of roadmap.

---

*Last Updated: October 20, 2025*  
*Version: 1.0*  
*Status: Production Ready*
