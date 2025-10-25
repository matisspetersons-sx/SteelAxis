# Customer Portal Implementation - Quick Reference

## 🎯 At a Glance

**What's Done:** Backend service (600+ lines) + API controller (156 lines)  
**What's Needed:** HTTP service + 5 Razor pages + 3 dialogs = ~850 lines UI code  
**Timeline:** 15-20 hours  
**Entry Point:** Start with Task 1 (HTTP Service) - blocker for everything else

---

## 📍 File Locations

### Backend (Already Complete)
```
✅ Manimp.Shared/Interfaces/ICustomerPortalService.cs          (interface)
✅ Manimp.Services/Implementation/CustomerPortalService.cs     (600+ lines)
✅ Manimp.Api/Controllers/CustomerPortalController.cs          (156 lines)
✅ Manimp.Shared/Models/CustomerPortalAccess.cs                (model)
```

### Frontend (To Create)
```
⏳ Manimp.Web/Services/CustomerPortalHttpService.cs            (150-200 lines)
⏳ Manimp.Web/Components/Pages/CustomerPortalManagement.razor  (120-150 lines)
⏳ Manimp.Web/Components/Dialogs/GrantAccessDialog.razor       (80-100 lines)
⏳ Manimp.Web/Components/Dialogs/AccessDetailsDialog.razor     (100-120 lines)
⏳ Manimp.Web/Components/Dialogs/ExtendAccessDialog.razor      (60-80 lines)
⏳ Manimp.Web/Components/Pages/CustomerPortalDashboard.razor   (150-180 lines)
⏳ Manimp.Web/Components/CustomerPortalDocumentViewer.razor    (60-80 lines)
⏳ Manimp.Web/Components/Pages/CustomerPortalAnalytics.razor   (120-150 lines)
⏳ Manimp.Web/Program.cs                                        (modify - add DI)
⏳ Manimp.Web/Components/Layout/NavMenu.razor                  (modify - add menu)
```

---

## 🔗 API Endpoints (Already Implemented)

```
POST   /api/customportal/grant-access              → Grant access
GET    /api/customportal/access/{token}            → Get access info
GET    /api/customportal/documents/{token}         → List documents
GET    /api/customportal/download/{token}/{type}/{id}  → Download document
POST   /api/customportal/extend-access            → Extend expiry
POST   /api/customportal/revoke-access            → Revoke access
GET    /api/customportal/analytics                → Get analytics
```

---

## 🛠️ Task Order (Sequential - Dependencies Matter!)

| # | Task | Hours | Depends On | File Count |
|---|------|-------|-----------|-----------|
| 1 | HTTP Service + DI | 1-2 | None | 1 file (+1 modify) |
| 2 | Admin Pages & Dialogs | 4-6 | Task 1 | 4 files |
| 3 | Customer Portal Pages | 3-4 | Task 1 | 2 files |
| 4 | Analytics Dashboard | 2-3 | Task 1 | 1 file |
| 5 | Navigation Integration | 1 | Tasks 2,3,4 | 1 file (modify) |
| 6 | Testing & Polish | 3-4 | All | N/A |

**Total:** 15-20 hours, 11 new files (8 create, 3 modify)

---

## 💡 Key Decisions Made

1. **Architecture Pattern:** HTTP service wrapper (already proven in ProductionPlanningHttpService)
2. **Admin vs. Customer:** Separate pages for different user types
3. **Document Types:** Three supported: DoP, Material Certificate, Quality Report
4. **Access Control:** Token-based (URL contains token for customers)
5. **Analytics:** Simple time-series for now (export to CSV Phase 2)
6. **Charts:** Use MudBlazor Chart components (already in use)

---

## 🔧 Code Template Examples

### 1. HTTP Service Method
```csharp
public async Task<CustomerPortalAccess?> GrantCustomerAccessAsync(
    int projectId, string email, string name, string[] docTypes, int days, string userId)
{
    try
    {
        var request = new { projectId, customerEmail = email, customerName = name, 
            documentTypes = docTypes, accessDurationDays = days, grantedByUserId = userId };
        var response = await _httpClient.PostAsJsonAsync("api/customportal/grant-access", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerPortalAccess>();
    }
    catch { return null; }
}
```

### 2. Dialog Component (Razor)
```razor
@if (showDialog)
{
    <MudDialog>
        <DialogContent>
            <MudTextField @bind-Value="model.CustomerEmail" Label="Email" Required />
            <MudButton OnClick="Save" Color="Color.Primary">Save</MudButton>
        </DialogContent>
    </MudDialog>
}
```

### 3. Main Page Pattern
```razor
@page "/customer-portal-management"
@inject CustomerPortalHttpService PortalService

<MudDataGrid Items="accesses">
    <Columns>
        <PropertyColumn Property="x => x.CustomerName" />
        <ActionColumn>
            <MudButton OnClick="() => OpenDialog(context)">Edit</MudButton>
        </ActionColumn>
    </Columns>
</MudDataGrid>

@code {
    private List<CustomerPortalAccess> accesses = new();
    
    protected override async Task OnInitializedAsync() => await LoadData();
    
    private async Task LoadData()
    {
        var analytics = await PortalService.GetAnalyticsAsync(null, 
            DateTime.Now.AddDays(-30), DateTime.Now);
        // Parse and populate accesses
    }
}
```

---

## ✅ Definition of Done (Per Task)

### Task 1 ✅
- [ ] Service created with all 7 methods
- [ ] Registered in `Program.cs`
- [ ] Compiles without errors
- [ ] Pattern matches `ProductionPlanningHttpService`

### Task 2 ✅
- [ ] Main page loads and displays data
- [ ] Grant dialog validates all fields
- [ ] Details dialog shows read-only + action buttons
- [ ] Extend dialog calculates expiry correctly
- [ ] Revoke shows confirmation dialog

### Task 3 ✅
- [ ] Public dashboard accepts token in URL
- [ ] Shows only documents customer can access
- [ ] Download works end-to-end
- [ ] Expired token shows error message

### Task 4 ✅
- [ ] Analytics page loads metrics
- [ ] Date range filters work
- [ ] Charts display correctly
- [ ] Data updates on filter change

### Task 5 ✅
- [ ] Menu items added to nav
- [ ] Links route correctly
- [ ] Navigation items visible when feature enabled

### Task 6 ✅
- [ ] All 4 workflows tested end-to-end
- [ ] Error messages display correctly
- [ ] Mobile responsive layout works
- [ ] Keyboard navigation functional

---

## 🧪 Manual Test Scenarios

**Scenario 1: Grant Access**
```
1. Open /customer-portal-management
2. Click [Grant New Access]
3. Fill: project=Sample, email=test@example.com, name=Test Customer, docs=[DoP, Cert], days=30
4. Click [Grant Access]
✓ Verify: Record appears in grid with correct data
```

**Scenario 2: Customer Download**
```
1. Share portal link: {baseUrl}/customer-portal/{token}
2. Customer opens link
3. See customer name and expiry date
4. Click [Download] on document
✓ Verify: PDF downloads to browser
✓ Verify: Download count increments in analytics
```

**Scenario 3: Extend Access**
```
1. Open /customer-portal-management
2. Find access record, click [Details]
3. Click [Extend Access]
4. Set additional days = 15
5. Click [Extend]
✓ Verify: New expiry = old expiry + 15 days
✓ Verify: Record shows "Active" status
```

**Scenario 4: Revoke Access**
```
1. Open access record details
2. Click [Revoke Access]
3. Confirm in dialog
✓ Verify: Status changes to "Revoked"
✓ Verify: Customer cannot access on next load
```

---

## 📚 Reference Files in Codebase

| Need | File | Lines | Use For |
|------|------|-------|---------|
| HTTP Service Pattern | `ProductionPlanningHttpService.cs` | 358 | Copy structure & error handling |
| Dialog Pattern | `Manimp.Web/Components/Dialogs/*.razor` | 50-150 | MudBlazor dialog patterns |
| Page Pattern | `ProductionPlanning.razor` | 200+ | State mgmt, data grid, actions |
| Chart Pattern | `ProductionMonitoring.razor` | 150+ | MudBlazor chart integration |
| Navigation | `NavMenu.razor` | 100+ | Menu structure to copy |
| Service Interface | `ICustomerPortalService.cs` | 40 | Contract to implement in HTTP service |
| Database Model | `CustomerPortalAccess.cs` | 30 | Returned type from API |

---

## 🚀 Getting Started Checklist

- [ ] Read full `IMPLEMENTATION-PLAN.md` for detailed specs
- [ ] Review `ICustomerPortalService.cs` to understand contract
- [ ] Review `CustomerPortalController.cs` API endpoints
- [ ] Copy `ProductionPlanningHttpService.cs` as template
- [ ] Create `CustomerPortalHttpService.cs` (implement 7 methods)
- [ ] Test HTTP service by calling each method
- [ ] Build Razor components following plan

---

## ⚡ Fast-Track Tips

1. **Reduce scope if needed:** Analytics dashboard (Task 4) can move to Phase 2
2. **Reuse patterns:** Copy from ProductionPlanning for HTTP service and pages
3. **Placeholder content:** Start with hard-coded document list, add real data later
4. **Styling:** Use existing MudBlazor components (buttons, dialogs, cards, grids)
5. **Testing:** Focus on 4 main workflows (grant, download, extend, revoke)

---

## 📖 Documentation Links

- Full plan: `docs/customer-portal/IMPLEMENTATION-PLAN.md`
- Backend code: `Manimp.Services/Implementation/CustomerPortalService.cs`
- API: `Manimp.Api/Controllers/CustomerPortalController.cs`
- Models: `Manimp.Shared/Models/CustomerPortalAccess.cs`
- MudBlazor: `docs/mudblazor/` (component reference)
