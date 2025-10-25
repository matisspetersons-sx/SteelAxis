# Customer Portal UI Implementation Plan

## 📋 Overview

The Customer Portal feature enables secure sharing of compliance documents (DoP, Material Certificates, Quality Reports) with external customers through time-limited, token-based access. Backend service and API controller are 95% complete; this plan outlines UI implementation.

### Current Status
- ✅ **Backend Service:** `ICustomerPortalService` fully implemented (600+ lines)
- ✅ **API Controller:** `CustomerPortalController` with 7 endpoints fully implemented
- ✅ **Database Model:** `CustomerPortalAccess` entity configured with access tracking
- ❌ **HTTP Service:** `CustomerPortalHttpService` not yet created
- ❌ **UI Components:** No Razor pages or dialogs created
- ❌ **Navigation:** Portal not integrated into site navigation

---

## 🎯 Implementation Tasks

### Task 1: Create HTTP Service Layer
**Priority:** P0 (Blocker for UI)  
**Estimated Time:** 1-2 hours  
**Dependencies:** None

#### Subtasks

**1.1 Create `CustomerPortalHttpService.cs`**
- File: `/Manimp.Web/Services/CustomerPortalHttpService.cs`
- Pattern: Mirror `ProductionPlanningHttpService.cs` structure
- Methods:
  ```csharp
  Task<CustomerPortalAccess> GrantCustomerAccessAsync(
    int projectId, string email, string name, string[] docTypes, int days, string userId)
  Task<CustomerPortalAccess?> GetAccessAsync(string token)
  Task<List<object>> GetDocumentsAsync(string token)
  Task<byte[]> DownloadDocumentAsync(string token, string type, Guid id)
  Task<CustomerPortalAccess> ExtendAccessAsync(Guid accessId, int days, string userId)
  Task RevokeAccessAsync(Guid accessId, string userId)
  Task<object> GetAnalyticsAsync(int? projectId, DateTime start, DateTime end)
  ```
- Error Handling: Wrap calls in try-catch, return null on 404, throw on 5xx
- Logging: Log method entry/exit for troubleshooting

**1.2 Register Service in DI**
- File: `/Manimp.Web/Program.cs`
- Add: `builder.Services.AddHttpClient<CustomerPortalHttpService>();`
- Follows same pattern as `ProductionPlanningHttpService`

---

### Task 2: Create Admin Access Management Interface
**Priority:** P1  
**Estimated Time:** 4-6 hours  
**Dependencies:** Task 1

#### Subtasks

**2.1 Create `CustomerPortalManagement.razor` (Main Page)**
- File: `/Manimp.Web/Components/Pages/CustomerPortalManagement.razor`
- Purpose: Admin dashboard for managing customer portal access
- Layout:
  - Header: "Customer Portal Access Management"
  - Toolbar: [Grant New Access] button, date range filter, search box
  - Main Grid: MudBlazor DataGrid showing:
    - Customer Name
    - Customer Email
    - Project Name
    - Document Types (comma-separated or badges)
    - Access Status (Active/Expired/Revoked)
    - Token (masked/copy button)
    - Granted Date
    - Expiry Date
    - Actions (View, Extend, Revoke)
  - Pagination: 10-25 rows per page
- Features:
  - Load data on component init via `GetAnalyticsAsync()`
  - Filter by status (Active/Expired/Revoked)
  - Search by customer name or email
  - Sort by columns
  - Refresh button to reload data
  - Empty state message when no records

**2.2 Create `GrantAccessDialog.razor`**
- File: `/Manimp.Web/Components/Dialogs/GrantAccessDialog.razor`
- Purpose: Form to grant new customer access
- Dialog Content:
  - **Project Selector:** MudBlazor AutoComplete dropdown
    - Loads all projects on init
    - Required field
  - **Customer Name:** Text input (required)
  - **Customer Email:** Text input with email validation (required)
  - **Document Type Selector:** MudBlazor Checkbox list
    - Options: "Declaration of Performance", "Material Certificate", "Quality Report"
    - At least one must be selected
  - **Access Duration:** Numeric input + unit selector
    - Input: 1-365 days
    - Default: 30 days
    - Units: Days (hard-coded for now)
  - **Action Buttons:**
    - [Grant Access] - validates all fields, calls service
    - [Cancel] - closes dialog without action
- Behavior:
  - Show loading state during API call
  - Display success toast: "Access granted to {email}"
  - Display error toast on failure with error details
  - Auto-close on success (2 second delay)
  - Return data to parent to refresh grid

**2.3 Create `AccessDetailsDialog.razor`**
- File: `/Manimp.Web/Components/Dialogs/AccessDetailsDialog.razor`
- Purpose: View and manage individual access record
- Dialog Content:
  - Read-only fields:
    - Customer Name
    - Customer Email
    - Project Name
    - Document Types (as badges)
    - Granted Date & Time
    - Granted By (user email)
  - Editable/Action fields:
    - Current Expiry Date (read-only, highlighted if expired)
    - Access Status (badge: green/yellow/red)
    - Download Token (masked, copy-to-clipboard button)
  - Action Buttons (conditional on status):
    - [Extend Access] - opens extend dialog or inline form
    - [Revoke Access] - shows confirmation, then revokes
    - [Close] - closes dialog
- Behavior:
  - Populate from passed `CustomerPortalAccess` object
  - Disable extend/revoke if already revoked
  - Disable extend if 7 days or less to expiry (optional UX rule)
  - Show confirmation dialog before revoke: "Are you sure? Customer will lose access."

**2.4 Create `ExtendAccessDialog.razor`**
- File: `/Manimp.Web/Components/Dialogs/ExtendAccessDialog.razor`
- Purpose: Extend expiry date for active access
- Dialog Content:
  - Display: Current expiry date, days remaining
  - Input: Additional days to extend (1-365)
  - Default: 30 days
  - Calculation display: "New expiry will be: {calculated date}"
  - Action Buttons:
    - [Extend] - calls service with additional days
    - [Cancel] - closes without action
- Behavior:
  - Validate input (1-365 range)
  - Call `ExtendAccessAsync(accessId, additionalDays)`
  - Show success toast
  - Auto-close on success
  - Return updated access record to parent

---

### Task 3: Create Customer-Facing Portal Pages
**Priority:** P2  
**Estimated Time:** 3-4 hours  
**Dependencies:** Task 1

#### Subtasks

**3.1 Create `CustomerPortalDashboard.razor` (Public Page)**
- File: `/Manimp.Web/Components/Pages/CustomerPortalDashboard.razor`
- Route: `@page "/customer-portal/{accessToken}"`
- Purpose: Customer's view of accessible documents
- Layout:
  - Header: Company logo + "Document Portal"
  - Authentication Display:
    - "Access granted to: {customerName}" (from ValidateAccessAsync)
    - "Valid until: {expiryDate}" (with warning if < 7 days)
  - Document List:
    - Section 1: Declaration of Performance (if included)
      - Card with title, project, date, [Download] button
      - Icon indicator for document type
    - Section 2: Material Certificates (if included)
      - Grid of certificate cards with material info
      - [Download] button per certificate
    - Section 3: Quality Reports (if included)
      - Similar card layout
  - Footer:
    - "This access expires on {date}"
    - Contact info: "Questions? Contact {admin_email}"
- Features:
  - Auto-validate token on page load
  - Display error message if token invalid/expired
  - Disable download buttons if access expired
  - Loading spinner while fetching documents
  - Responsive design (mobile-friendly)
- Behavior:
  - Call `GetAccessAsync(token)` to validate
  - Call `GetDocumentsAsync(token)` to list available docs
  - On download click, call `DownloadDocumentAsync(token, type, id)`
  - Browser handles PDF download
  - Track access in analytics (implicit via GetDocumentsAsync tracking)

**3.2 Create `CustomerPortalDocumentViewer.razor` (Reusable Component)**
- File: `/Manimp.Web/Components/CustomerPortalDocumentViewer.razor`
- Purpose: Reusable component to display/download documents
- Parameters:
  ```csharp
  [Parameter] public string DocumentType { get; set; } // "DoP", "Certificate", "QualityReport"
  [Parameter] public object DocumentData { get; set; } // Document metadata
  [Parameter] public string AccessToken { get; set; } // For download
  [Parameter] public EventCallback OnDownload { get; set; } // Track download
  ```
- Layout:
  - Document Icon (based on type)
  - Title and metadata
  - [Preview] button (if applicable)
  - [Download] button
  - "Downloaded {count} times" text
- Styling: MudBlazor Card component with shadow

---

### Task 4: Create Analytics Dashboard
**Priority:** P3  
**Estimated Time:** 2-3 hours  
**Dependencies:** Task 1

#### Subtasks

**4.1 Create `CustomerPortalAnalytics.razor` (Admin View)**
- File: `/Manimp.Web/Components/Pages/CustomerPortalAnalytics.razor`
- Route: `@page "/customer-portal/analytics"`
- Purpose: View portal usage metrics and trends
- Layout:
  - Date Range Picker: "From {date} to {date}" [Apply Filter]
  - KPI Cards (4 columns, responsive):
    - Total Access Grants: {count}
    - Active Access: {count}
    - Expired Access: {count}
    - Unique Customers: {count}
  - Metrics Section:
    - Chart 1: Access Trends (line chart, 30-day rolling)
    - Chart 2: Document Downloads by Type (bar chart)
    - Chart 3: Access Duration Distribution (histogram)
  - Details Table:
    - Recent Activity: Customer, Action (grant/download/extend), Date, Project
    - Sortable by date
    - Pagination 25 per page
- Features:
  - Use MudBlazor Chart components (already used in production planning)
  - Load data on init via `GetAnalyticsAsync()`
  - Date range defaults to last 30 days
  - Refresh on date range change
  - Export button (optional, Phase 2): Export to CSV
- Behavior:
  - Call `GetAnalyticsAsync(projectId: null, start, end)`
  - Parse returned metrics object and display
  - Handle empty data gracefully

---

### Task 5: Add Navigation & Route Integration
**Priority:** P1  
**Estimated Time:** 1 hour  
**Dependencies:** Tasks 2, 3

#### Subtasks

**5.1 Add Menu Items to Navigation**
- File: `/Manimp.Web/Components/Layout/NavMenu.razor`
- Add menu section: "Customer Portal"
  - Link: "Access Management" → `/customer-portal-management`
  - Link: "Analytics" → `/customer-portal/analytics`
- Placement: After "Procurement" or in appropriate section based on menu structure
- Feature Gate: (Optional) Wrap in `@if (HasFeature("CustomerPortal"))` if gated

**5.2 Document Public URL Pattern**
- Create documentation showing how to share portal links
- URL format: `{base_url}/customer-portal/{accessToken}`
- Document in README or help section

---

### Task 6: Integration Testing & Refinement
**Priority:** P1  
**Estimated Time:** 3-4 hours  
**Dependencies:** All Tasks 1-5

#### Subtasks

**6.1 End-to-End Workflow Testing**
- Test Scenario 1: Grant Access
  1. Admin opens CustomerPortalManagement.razor
  2. Clicks [Grant New Access]
  3. Fills GrantAccessDialog with test data
  4. Clicks [Grant Access]
  5. Verify: Access appears in grid with correct data
  6. Verify: Email sent to customer (if configured)
- Test Scenario 2: Customer Downloads Document
  1. Customer receives access link
  2. Navigates to CustomerPortalDashboard.razor with token
  3. See documents list
  4. Click [Download] on document
  5. Verify: PDF downloads successfully
  6. Verify: Access count incremented in analytics
- Test Scenario 3: Extend Access
  1. Admin opens access record via AccessDetailsDialog
  2. Clicks [Extend Access]
  3. Sets additional days (e.g., 15)
  4. Clicks [Extend]
  5. Verify: New expiry date calculated correctly
  6. Verify: Record shows updated expiry
- Test Scenario 4: Revoke Access
  1. Admin opens access record
  2. Clicks [Revoke Access]
  3. Confirms in dialog
  4. Verify: Status changes to "Revoked"
  5. Verify: Customer access denied on next page load

**6.2 Error Handling & Edge Cases**
- Test invalid token on portal page (should show error)
- Test expired token (should show "Access Expired")
- Test rapid API calls (throttle/debounce if needed)
- Test missing customer email validation
- Test duplicate access grants to same customer
- Test document list with no documents (empty state)

**6.3 UI/UX Polish**
- Verify responsive design on mobile/tablet
- Test accessibility (keyboard nav, ARIA labels, contrast)
- Add loading skeletons for data grids
- Add error snackbars with retry buttons
- Verify consistent styling with existing MudBlazor theme
- Test dialog animations/transitions

---

## 📁 File Summary

### New Files to Create

| File | Type | LOC Est. | Purpose |
|------|------|---------|---------|
| `Manimp.Web/Services/CustomerPortalHttpService.cs` | Service | 150-200 | HTTP client wrapper for portal API |
| `Manimp.Web/Components/Pages/CustomerPortalManagement.razor` | Page | 120-150 | Admin dashboard for access management |
| `Manimp.Web/Components/Dialogs/GrantAccessDialog.razor` | Dialog | 80-100 | Form to grant new access |
| `Manimp.Web/Components/Dialogs/AccessDetailsDialog.razor` | Dialog | 100-120 | View/manage individual access |
| `Manimp.Web/Components/Dialogs/ExtendAccessDialog.razor` | Dialog | 60-80 | Extend access expiry form |
| `Manimp.Web/Components/Pages/CustomerPortalDashboard.razor` | Page | 150-180 | Customer's public portal view |
| `Manimp.Web/Components/CustomerPortalDocumentViewer.razor` | Component | 60-80 | Reusable document display component |
| `Manimp.Web/Components/Pages/CustomerPortalAnalytics.razor` | Page | 120-150 | Portal usage analytics dashboard |

### Modified Files

| File | Change | Impact |
|------|--------|--------|
| `Manimp.Web/Program.cs` | Add HttpClient DI | Register CustomerPortalHttpService |
| `Manimp.Web/Components/Layout/NavMenu.razor` | Add menu items | Navigation to admin pages |

**Total New Code:** ~850-1,040 lines  
**Total Effort:** ~15-20 hours across all tasks

---

## 🔄 Execution Sequence

### Phase 1: Foundation (4-6 hours)
1. ✅ Task 1: Create HTTP Service Layer
2. ✅ Register service in DI

### Phase 2: Admin Interface (4-6 hours)
3. ✅ Task 2.1: Main management page
4. ✅ Task 2.2: Grant access dialog
5. ✅ Task 2.3: Access details dialog
6. ✅ Task 2.4: Extend access dialog

### Phase 3: Customer Interface (3-4 hours)
7. ✅ Task 3.1: Customer portal dashboard
8. ✅ Task 3.2: Document viewer component

### Phase 4: Analytics & Navigation (3-4 hours)
9. ✅ Task 4.1: Analytics dashboard
10. ✅ Task 5: Navigation integration

### Phase 5: Testing & Refinement (3-4 hours)
11. ✅ Task 6: E2E testing, error handling, UI polish

---

## 🎓 Code Examples & Patterns

### HTTP Service Pattern (Task 1)
```csharp
public class CustomerPortalHttpService
{
    private readonly HttpClient _httpClient;

    public CustomerPortalHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomerPortalAccess?> GrantCustomerAccessAsync(
        int projectId, string email, string name, string[] docTypes, int days, string userId)
    {
        try
        {
            var request = new
            {
                projectId,
                customerEmail = email,
                customerName = name,
                documentTypes = docTypes,
                accessDurationDays = days,
                grantedByUserId = userId
            };
            var response = await _httpClient.PostAsJsonAsync("api/customportal/grant-access", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerPortalAccess>();
        }
        catch (Exception ex)
        {
            // Log error
            return null;
        }
    }
}
```

### Razor Component Pattern (Task 2.1)
```razor
@page "/customer-portal-management"
@inject CustomerPortalHttpService PortalService
@inject IDialogService DialogService
@implements IAsyncDisposable

<PageTitle>Customer Portal Access Management</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large">
    <MudStack>
        <MudText Typo="Typo.H4">Customer Portal Access Management</MudText>
        
        <MudStack Row Spacing="2">
            <MudButton Variant="Variant.Filled" Color="Color.Primary" 
                OnClick="OpenGrantDialog">Grant New Access</MudButton>
            <MudButton Variant="Variant.Outlined" OnClick="RefreshData">Refresh</MudButton>
        </MudStack>

        @if (loading)
        {
            <MudProgressCircular IsIndeterminate Color="Color.Primary" />
        }
        else if (accesses.Count == 0)
        {
            <MudAlert Severity="Severity.Info">No access records found.</MudAlert>
        }
        else
        {
            <MudDataGrid Items="accesses" Pagination="pagination">
                <!-- Columns here -->
            </MudDataGrid>
        }
    </MudStack>
</MudContainer>

@code {
    private List<CustomerPortalAccess> accesses = new();
    private bool loading = true;
    private MudPagination pagination = new() { Count = 1 };

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        loading = true;
        try
        {
            var analytics = await PortalService.GetAnalyticsAsync(null, 
                DateTime.Now.AddDays(-30), DateTime.Now);
            // Parse and populate accesses list
        }
        finally
        {
            loading = false;
        }
    }

    private async Task OpenGrantDialog()
    {
        var dialog = await DialogService.ShowAsync<GrantAccessDialog>("Grant Customer Access");
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await LoadData();
        }
    }
}
```

---

## 📖 Related Documentation

- **Backend Service:** See `Manimp.Services/Implementation/CustomerPortalService.cs`
- **API Endpoints:** See `Manimp.Api/Controllers/CustomerPortalController.cs`
- **Data Model:** See `Manimp.Shared/Models/CustomerPortalAccess.cs`
- **MudBlazor Patterns:** See `docs/mudblazor/` for component examples
- **Similar Feature (Reference):** `ProductionPlanning` feature for HTTP service pattern

---

## ⚠️ Implementation Notes

1. **Token Security:** Access tokens are generated server-side and stored in DB. Never transmit in logs.
2. **Document Download:** Backend returns byte[] from `DownloadDocumentAsync`. Browser handles streaming.
3. **Date/Time:** Use UTC throughout. Convert to local time in UI component if needed.
4. **Feature Gating:** If Customer Portal is gated, wrap admin pages in `[RequireFeature]` or UI checks.
5. **Email Integration:** Consider if email notifications needed when access granted (not in scope for this plan).
6. **Concurrency:** `CustomerPortalAccess` has `RowVersion` for optimistic locking; handle conflicts in UI.

---

## 🚀 Definition of Done

All tasks complete when:
- [ ] All 8 new files created with full functionality
- [ ] All HTTP service methods return correct types
- [ ] All CRUD operations (grant/extend/revoke/view) work end-to-end
- [ ] Admin can grant, view, extend, and revoke customer access
- [ ] Customers can view and download their documents via portal link
- [ ] Analytics dashboard displays accurate metrics
- [ ] All error scenarios handled with user-friendly messages
- [ ] UI is responsive and accessible (keyboard nav, ARIA labels)
- [ ] Navigation updated with portal links
- [ ] E2E workflow tests pass (6 scenarios in Task 6.1)
- [ ] Code reviewed and merged to main branch
- [ ] Documentation updated in `docs/customer-portal/README.md`

---

## 📋 Checklist

Use this to track progress:

```
Phase 1: Foundation
  - [ ] Create CustomerPortalHttpService.cs
  - [ ] Register in Program.cs

Phase 2: Admin Interface
  - [ ] Create CustomerPortalManagement.razor (main page)
  - [ ] Create GrantAccessDialog.razor
  - [ ] Create AccessDetailsDialog.razor
  - [ ] Create ExtendAccessDialog.razor

Phase 3: Customer Interface
  - [ ] Create CustomerPortalDashboard.razor
  - [ ] Create CustomerPortalDocumentViewer.razor

Phase 4: Analytics & Navigation
  - [ ] Create CustomerPortalAnalytics.razor
  - [ ] Update NavMenu.razor

Phase 5: Testing & Refinement
  - [ ] E2E workflow testing (all 4 scenarios)
  - [ ] Error handling & edge cases
  - [ ] UI/UX polish
  - [ ] Accessibility check
  - [ ] Performance review

Final
  - [ ] Code review
  - [ ] Merge to main
  - [ ] Update README.md
```

---

## 📞 Support & Questions

- **Service Contract:** `ICustomerPortalService` in `Manimp.Shared/Interfaces`
- **API Reference:** `CustomerPortalController` in `Manimp.Api/Controllers`
- **Database Model:** `CustomerPortalAccess` in `Manimp.Shared/Models`
- **HTTP Pattern:** Copy from `ProductionPlanningHttpService`
- **UI Pattern:** Follow existing MudBlazor dialogs in `Manimp.Web/Components/Dialogs`
