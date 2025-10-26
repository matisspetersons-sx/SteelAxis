# Sheet Inventory UI Implementation Plan

**Date**: October 6, 2025  
**Status**: Backend Complete ✅ | UI Dialogs In Progress ⏳  
**Reference**: MPM PriceRequestDialog.razor pattern

## Overview

This document provides the complete implementation plan for adding UI dialogs to support sheet inventory management in Manimp. The backend (models, DTOs, migration) is complete. This plan focuses on creating and updating Blazor dialog components following the MPM dialog pattern.

## Backend Status ✅

- **Models**: SheetInventory, SheetUsageLog, SheetRemnantInventory (complete)
- **DTOs**: 7 DTOs in SheetInventoryDTOs.cs (complete)
- **Enum**: MaterialInventoryType (Profile=1, Sheet=2)
- **Database**: Migration `AddSheetInventoryAndMaterialType` created
- **Procurement**: PurchaseOrderLine updated with MaterialInventoryType, nullable ProfileTypeId
- **Sourcing**: PriceRequestLine updated with MaterialInventoryType, nullable ProfileTypeId
- **Build Status**: 0 errors, 0 warnings ✅

## UI Components Needed

### 1. New Dialog Components (Create from scratch)

#### A. ProfileInventoryDialog.razor ⏳ CREATED (has import errors)
**Location**: `Manimp.Web/Components/Dialogs/ProfileInventoryDialog.razor`  
**Purpose**: Add/Edit profile inventory items  
**Status**: Created (430+ lines) but missing MudBlazor `@using` directive  

**Fixes Needed**:
```razor
@page "/dialogs/profile-inventory"
@using MudBlazor
@using Manimp.Shared.Models
@using Manimp.Shared.Constants
@inject ISnackbar Snackbar
```

**Service Injections Needed** (in @code section):
```csharp
@inject MaterialTypeHttpService MaterialTypeService
@inject ProfileTypeHttpService ProfileTypeService
@inject SteelGradeHttpService SteelGradeService
@inject SupplierHttpService SupplierService
@inject ProjectHttpService ProjectService
```

**LoadData() Implementation**:
```csharp
private async Task LoadData()
{
    materialTypes = await MaterialTypeService.GetAllAsync();
    profileTypes = await ProfileTypeService.GetAllAsync();
    steelGrades = await SteelGradeService.GetAllAsync();
    suppliers = await SupplierService.GetAllAsync();
    projects = await ProjectService.GetAllAsync();
}
```

#### B. SheetInventoryDialog.razor ⏳ CREATED (has import errors)
**Location**: `Manimp.Web/Components/Dialogs/SheetInventoryDialog.razor`  
**Purpose**: Add/Edit sheet inventory items  
**Status**: Created (similar to ProfileInventoryDialog) - needs same fixes

**Fixes Needed**: Same as ProfileInventoryDialog (add @using directives, inject services)

**Key Differences from Profile Dialog**:
- No ProfileType selector
- Fields: Thickness, Width, Length (instead of Size/Dimensions)
- Label adjustments: "Sheets On Hand" instead of "Pieces"
- Weight calculation: per sheet instead of per meter

#### C. ProfileUsageDialog.razor ❌ TODO
**Location**: `Manimp.Web/Components/Dialogs/ProfileUsageDialog.razor`  
**Purpose**: Record usage/consumption of profile inventory  

**Key Sections**:
```razor
<!-- Profile Selection -->
<MudAutocomplete T="ProfileInventory"
                 @bind-Value="SelectedProfile"
                 Label="Select Profile *"
                 SearchFunc="SearchProfiles"
                 ToStringFunc="@(p => $"{p.LotNumber} - {p.ProfileType.Name} {p.Size}")" />

<!-- Usage Details -->
<MudNumericField @bind-Value="PiecesUsed"
                 Label="Pieces Used *"
                 Min="1"
                 Max="@SelectedProfile?.PiecesOnHand" />

<MudNumericField @bind-Value="LengthUsed"
                 Label="Length Used (m) *"
                 Min="0.1m" />

<!-- Project Association -->
<MudSelect @bind-Value="ProjectId"
           Label="Project *"
           Required="true">
    @foreach (var project in projects)
    {
        <MudSelectItem Value="@project.ProjectId">@project.Name</MudSelectItem>
    }
</MudSelect>

<!-- Remnant Creation Option -->
<MudSwitch @bind-Checked="CreateRemnant"
           Label="Create Remnant Inventory"
           Color="Color.Primary" />

@if (CreateRemnant)
{
    <MudNumericField @bind-Value="RemnantLength"
                     Label="Remnant Length (m)"
                     HelperText="Length of remaining material to track" />
}
```

**Model**:
```csharp
public class ProfileUsageModel
{
    public int ProfileInventoryId { get; set; }
    public int PiecesUsed { get; set; }
    public decimal LengthUsed { get; set; }
    public int ProjectId { get; set; }
    public bool CreateRemnant { get; set; }
    public decimal? RemnantLength { get; set; }
    public string? Notes { get; set; }
}
```

#### D. SheetUsageDialog.razor ❌ TODO
**Location**: `Manimp.Web/Components/Dialogs/SheetUsageDialog.razor`  
**Purpose**: Record usage/consumption of sheet inventory  

**Key Differences from Profile Usage**:
- Select sheet by LotNumber + Thickness/Width/Length
- Fields: SheetsUsed (int), AreaUsed (decimal m²)
- Remnant: Width/Length of remaining sheet
- No "pieces" concept (sheets are tracked by count + area)

**Model**:
```csharp
public class SheetUsageModel
{
    public int SheetInventoryId { get; set; }
    public int SheetsUsed { get; set; }
    public decimal AreaUsed { get; set; } // m²
    public int ProjectId { get; set; }
    public bool CreateRemnant { get; set; }
    public decimal? RemnantWidth { get; set; }
    public decimal? RemnantLength { get; set; }
    public string? Notes { get; set; }
}
```

### 2. Update Existing Dialog Components

#### E. PurchaseOrderLineDialog.razor ⚠️ NEEDS UPDATE
**Location**: `Manimp.Web/Components/Dialogs/PurchaseOrderLineDialog.razor`  
**Current**: 279 lines, profile-only  
**Needed Changes**:

**1. Add MaterialInventoryType selector at top**:
```razor
<MudItem xs="12" md="6">
    <MudSelect T="MaterialInventoryType"
               @bind-Value="SelectedMaterialType"
               Label="Material Type *"
               Required="true"
               Variant="Variant.Outlined"
               HelperText="Steel Profiles or Steel Sheets">
        <MudSelectItem Value="MaterialInventoryType.Profile">Steel Profiles</MudSelectItem>
        <MudSelectItem Value="MaterialInventoryType.Sheet">Steel Sheets</MudSelectItem>
    </MudSelect>
</MudItem>
```

**2. Conditional Profile Type (only show if MaterialInventoryType == Profile)**:
```razor
@if (SelectedMaterialType == MaterialInventoryType.Profile)
{
    <MudItem xs="12" md="6">
        <MudAutocomplete T="string"
                         @bind-Value="SelectedProfileType"
                         Label="Profile Type *"
                         SearchFunc="SearchProfileTypes"
                         ... />
    </MudItem>
    
    <MudItem xs="12" md="6">
        <MudAutocomplete T="string"
                         @bind-Value="SelectedDimension"
                         Label="Dimensions *"
                         ... />
    </MudItem>
}
```

**3. Conditional Sheet Fields (show if MaterialInventoryType == Sheet)**:
```razor
@if (SelectedMaterialType == MaterialInventoryType.Sheet)
{
    <MudItem xs="12" md="4">
        <MudNumericField @bind-Value="Thickness"
                         Label="Thickness (mm) *"
                         Required="true" />
    </MudItem>
    
    <MudItem xs="12" md="4">
        <MudNumericField @bind-Value="Width"
                         Label="Width (mm) *"
                         Required="true" />
    </MudItem>
    
    <MudItem xs="12" md="4">
        <MudNumericField @bind-Value="Length"
                         Label="Length (mm) *"
                         Required="true" />
    </MudItem>
    
    <MudItem xs="12" md="6">
        <MudNumericField @bind-Value="Sheets"
                         Label="Sheets *"
                         Required="true"
                         Min="1" />
    </MudItem>
}
```

**4. Update POLineItem class in PurchaseOrderDialog.razor**:
```csharp
public class POLineItem
{
    public int LineNumber { get; set; }
    public MaterialInventoryType MaterialInventoryType { get; set; } = MaterialInventoryType.Profile;
    
    // Profile fields (nullable when MaterialInventoryType == Sheet)
    public string? ProfileType { get; set; }
    public string? Dimension { get; set; }
    public decimal? UnitLength { get; set; }
    public int? Pieces { get; set; }
    
    // Sheet fields (nullable when MaterialInventoryType == Profile)
    public decimal? Thickness { get; set; }
    public decimal? Width { get; set; }
    public decimal? Length { get; set; }
    public int? Sheets { get; set; }
    
    // Common fields
    public string SteelGrade { get; set; } = string.Empty;
}
```

**5. Update validation logic**:
```csharp
private void Submit()
{
    if (!IsFormValid) return;

    if (SelectedMaterialType == MaterialInventoryType.Profile)
    {
        if (string.IsNullOrEmpty(SelectedProfileType) || 
            string.IsNullOrEmpty(SelectedDimension) ||
            UnitLength <= 0 || Pieces <= 0)
        {
            Snackbar.Add("Please fill all profile fields", Severity.Warning);
            return;
        }
    }
    else // MaterialInventoryType.Sheet
    {
        if (!Thickness.HasValue || !Width.HasValue || !Length.HasValue || !Sheets.HasValue)
        {
            Snackbar.Add("Please fill all sheet dimensions", Severity.Warning);
            return;
        }
    }

    var lineItem = new POLineItem
    {
        LineNumber = Line?.LineNumber ?? LineNumber,
        MaterialInventoryType = SelectedMaterialType,
        ProfileType = SelectedProfileType,
        Dimension = SelectedDimension,
        SteelGrade = SelectedSteelGrade!,
        UnitLength = UnitLength,
        Pieces = Pieces,
        Thickness = Thickness,
        Width = Width,
        Length = Length,
        Sheets = Sheets
    };

    MudDialog.Close(DialogResult.Ok(lineItem));
}
```

#### F. PriceRequestLineDialog.razor ⚠️ NEEDS UPDATE
**Location**: `Manimp.Web/Components/Dialogs/` (check if exists)  
**Needed Changes**: Exactly same pattern as PurchaseOrderLineDialog.razor  

If this dialog doesn't exist yet, it should follow the MPM PriceRequestDialog.razor pattern with:
- Dynamic line items (Add/Remove buttons)
- MaterialInventoryType selector
- Conditional Profile/Sheet fields
- Dimension suggestions from existing inventory
- Draft/Submit workflow

**MPM Pattern Reference** (from PriceRequestDialog.razor):
```razor
<!-- Line Items Section -->
<MudItem xs="12">
    <MudText Typo="Typo.h6">Line Items</MudText>
    @foreach (var line in Lines)
    {
        <MudCard Class="mb-3">
            <MudCardContent>
                <!-- Material Type Selector -->
                <MudSelect @bind-Value="line.MaterialInventoryType">
                    <MudSelectItem Value="MaterialInventoryType.Sheet">Steel Sheets</MudSelectItem>
                    <MudSelectItem Value="MaterialInventoryType.Profile">Steel Profiles</MudSelectItem>
                </MudSelect>
                
                <!-- Conditional Profile Type -->
                @if (line.MaterialInventoryType == MaterialInventoryType.Profile)
                {
                    <MudSelect @bind-Value="line.ProfileTypeId">
                        @foreach (var pt in profileTypes)
                        {
                            <MudSelectItem Value="@pt.ProfileTypeId">@pt.Name</MudSelectItem>
                        }
                    </MudSelect>
                }
            </MudCardContent>
            <MudCardActions>
                <MudButton OnClick="@(() => RemoveLine(line))">Remove</MudButton>
            </MudCardActions>
        </MudCard>
    }
    <MudButton OnClick="AddLine">Add Line</MudButton>
</MudItem>
```

### 3. Update Existing Pages

#### G. Inventory/Profiles.razor ⚠️ CHECK STATUS
**Location**: `Manimp.Web/Components/Pages/Inventory/Profiles.razor`  
**Needed Changes**:
- Update "Add Profile" button to use new ProfileInventoryDialog.razor
- Update edit actions to pass ProfileInventory to dialog
- Handle dialog result (DialogResult.Ok)

**Pattern**:
```csharp
private async Task OpenAddDialog()
{
    var parameters = new DialogParameters { };
    var dialog = await DialogService.ShowAsync<ProfileInventoryDialog>("Add Profile Inventory", parameters);
    var result = await dialog.Result;
    
    if (!result.Canceled && result.Data is ProfileInventory profile)
    {
        // Call API to create profile
        await ProfileInventoryService.CreateAsync(profile);
        await LoadProfiles(); // Refresh grid
        Snackbar.Add("Profile added successfully", Severity.Success);
    }
}

private async Task OpenEditDialog(ProfileInventory profile)
{
    var parameters = new DialogParameters 
    { 
        ["ProfileInventory"] = profile 
    };
    var dialog = await DialogService.ShowAsync<ProfileInventoryDialog>("Edit Profile Inventory", parameters);
    var result = await dialog.Result;
    
    if (!result.Canceled && result.Data is ProfileInventory updated)
    {
        await ProfileInventoryService.UpdateAsync(updated);
        await LoadProfiles();
        Snackbar.Add("Profile updated successfully", Severity.Success);
    }
}
```

#### H. Create New Page: Inventory/Sheets.razor ❌ TODO
**Location**: `Manimp.Web/Components/Pages/Inventory/Sheets.razor`  
**Purpose**: Manage sheet inventory (CRUD operations)  

**Structure** (mirror Profiles.razor):
```razor
@page "/inventory/sheets"
@using Manimp.Shared.Models
@using Manimp.Web.Services
@inject SheetInventoryHttpService SheetInventoryService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<PageTitle>Sheet Inventory - Manimp</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Sheet Inventory</MudText>
    
    <MudPaper Class="pa-4 mb-4">
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="@Icons.Material.Filled.Add"
                   OnClick="OpenAddDialog">
            Add Sheet
        </MudButton>
    </MudPaper>
    
    <MudDataGrid T="SheetInventory"
                 Items="@sheets"
                 Filterable="true"
                 SortMode="SortMode.Multiple"
                 Pagination="true"
                 RowsPerPage="25">
        <Columns>
            <PropertyColumn Property="x => x.LotNumber" Title="Lot #" />
            <PropertyColumn Property="x => x.Thickness" Title="Thickness" Format="F2" />
            <PropertyColumn Property="x => x.Width" Title="Width" Format="F2" />
            <PropertyColumn Property="x => x.Length" Title="Length" Format="F2" />
            <PropertyColumn Property="x => x.SheetsOnHand" Title="Sheets" />
            <PropertyColumn Property="x => x.MaterialType.Name" Title="Material" />
            <PropertyColumn Property="x => x.SteelGrade.Name" Title="Grade" />
            <PropertyColumn Property="x => x.Location" Title="Location" />
            <TemplateColumn Title="Actions">
                <CellTemplate>
                    <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                                   OnClick="@(() => OpenEditDialog(context.Item))" />
                    <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                                   Color="Color.Error"
                                   OnClick="@(() => DeleteSheet(context.Item))" />
                </CellTemplate>
            </TemplateColumn>
        </Columns>
    </MudDataGrid>
</MudContainer>

@code {
    private List<SheetInventory> sheets = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadSheets();
    }
    
    private async Task LoadSheets()
    {
        // sheets = await SheetInventoryService.GetAllAsync();
    }
    
    private async Task OpenAddDialog()
    {
        // Implementation from pattern above
    }
    
    private async Task OpenEditDialog(SheetInventory sheet)
    {
        // Implementation from pattern above
    }
    
    private async Task DeleteSheet(SheetInventory sheet)
    {
        // Confirmation dialog + API call
    }
}
```

### 4. HTTP Services Needed ❌ TODO

#### SheetInventoryHttpService.cs
**Location**: `Manimp.Web/Services/SheetInventoryHttpService.cs`  

```csharp
using System.Net.Http.Json;
using Manimp.Shared.DTOs;
using Manimp.Shared.Models;

namespace Manimp.Web.Services;

public class SheetInventoryHttpService
{
    private readonly HttpClient _httpClient;
    
    public SheetInventoryHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<SheetInventory>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<SheetInventory>>("api/sheetinventory") 
            ?? new List<SheetInventory>();
    }
    
    public async Task<SheetInventory?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<SheetInventory>($"api/sheetinventory/{id}");
    }
    
    public async Task<SheetInventory> CreateAsync(CreateSheetInventoryDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sheetinventory", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SheetInventory>() 
            ?? throw new Exception("Failed to create sheet inventory");
    }
    
    public async Task<SheetInventory> UpdateAsync(int id, UpdateSheetInventoryDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/sheetinventory/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SheetInventory>() 
            ?? throw new Exception("Failed to update sheet inventory");
    }
    
    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/sheetinventory/{id}");
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<SheetUsageLog> RecordUsageAsync(RecordSheetUsageDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sheetinventory/usage", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SheetUsageLog>() 
            ?? throw new Exception("Failed to record usage");
    }
    
    public async Task<List<SheetRemnantInventory>> GetRemnantsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<SheetRemnantInventory>>("api/sheetinventory/remnants") 
            ?? new List<SheetRemnantInventory>();
    }
}
```

**Register in Program.cs**:
```csharp
builder.Services.AddHttpClient<SheetInventoryHttpService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001");
});
```

### 5. Backend Services & Controllers ❌ TODO

#### SheetInventoryService.cs
**Location**: `Manimp.Services/Implementation/SheetInventoryService.cs`  
**Interface**: `Manimp.Shared/Interfaces/ISheetInventoryService.cs`  

Key methods:
- `CreateAsync(CreateSheetInventoryDto dto)`
- `UpdateAsync(int id, UpdateSheetInventoryDto dto)`
- `DeleteAsync(int id)`
- `RecordUsageAsync(RecordSheetUsageDto dto)` - with remnant auto-creation
- `GetRemnants Async()`
- `GetByIdAsync(int id)`
- `GetAllAsync()`
- `SearchAsync(SheetInventorySearchDto search)`

#### SheetInventoryController.cs
**Location**: `Manimp.Api/Controllers/SheetInventoryController.cs`  

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SheetInventoryController : ControllerBase
{
    private readonly ISheetInventoryService _sheetInventoryService;
    
    [HttpGet]
    public async Task<ActionResult<List<SheetInventory>>> GetAll()
    {
        return Ok(await _sheetInventoryService.GetAllAsync());
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<SheetInventory>> GetById(int id)
    {
        var sheet = await _sheetInventoryService.GetByIdAsync(id);
        if (sheet == null) return NotFound();
        return Ok(sheet);
    }
    
    [HttpPost]
    public async Task<ActionResult<SheetInventory>> Create(CreateSheetInventoryDto dto)
    {
        var sheet = await _sheetInventoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = sheet.SheetInventoryId }, sheet);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<SheetInventory>> Update(int id, UpdateSheetInventoryDto dto)
    {
        var sheet = await _sheetInventoryService.UpdateAsync(id, dto);
        return Ok(sheet);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sheetInventoryService.DeleteAsync(id);
        return NoContent();
    }
    
    [HttpPost("usage")]
    public async Task<ActionResult<SheetUsageLog>> RecordUsage(RecordSheetUsageDto dto)
    {
        var usageLog = await _sheetInventoryService.RecordUsageAsync(dto);
        return Ok(usageLog);
    }
    
    [HttpGet("remnants")]
    public async Task<ActionResult<List<SheetRemnantInventory>>> GetRemnants()
    {
        return Ok(await _sheetInventoryService.GetRemnantsAsync());
    }
}
```

### 6. Update Navigation Menu ❌ TODO

**Location**: `Manimp.Web/Components/Layout/NavMenu.razor`  

Add Sheets link under Inventory section:
```razor
<MudNavGroup Title="Inventory" Icon="@Icons.Material.Filled.Inventory" Expanded="@_inventoryExpanded">
    <MudNavLink Href="/inventory/profiles" Icon="@Icons.Material.Filled.ViewInAr">
        Profiles
    </MudNavLink>
    <MudNavLink Href="/inventory/sheets" Icon="@Icons.Material.Filled.RectangleOutlined">
        Sheets
    </MudNavLink>
    <MudNavLink Href="/inventory/remnants" Icon="@Icons.Material.Filled.Recycling">
        Remnants
    </MudNavLink>
</MudNavGroup>
```

## Implementation Priority

### Phase 1: Fix Existing Dialogs ⚠️ URGENT
1. ✅ Fix ProfileInventoryDialog.razor - Add `@using MudBlazor` and service injections
2. ✅ Fix SheetInventoryDialog.razor - Add `@using MudBlazor` and service injections
3. ✅ Test both dialogs compile without errors

### Phase 2: Create Missing Dialogs
4. ❌ Create ProfileUsageDialog.razor
5. ❌ Create SheetUsageDialog.razor
6. ❌ Update PurchaseOrderLineDialog.razor with MaterialInventoryType
7. ❌ Update or create PriceRequestLineDialog.razor

### Phase 3: Backend Services
8. ❌ Create SheetInventoryService.cs (business logic)
9. ❌ Create SheetInventoryController.cs (API endpoints)
10. ❌ Create SheetInventoryHttpService.cs (Blazor HTTP client)
11. ❌ Register services in Program.cs (both API and Web)

### Phase 4: Pages & Navigation
12. ❌ Create Sheets.razor page (grid + CRUD)
13. ❌ Update Profiles.razor to use ProfileInventoryDialog
14. ❌ Update NavMenu.razor (add Sheets link)
15. ❌ Create or update UsageTracking.razor (profiles + sheets)

### Phase 5: Integration Testing
16. ❌ Apply migration: `dotnet ef database update`
17. ❌ Test full workflow: Add sheet → PO with sheets → Usage → Remnants
18. ❌ Test EN 1090 traceability fields
19. ❌ Test multi-tenant isolation
20. ❌ Update documentation

## Testing Checklist

### Dialog Tests
- [ ] ProfileInventoryDialog opens and closes
- [ ] SheetInventoryDialog opens and closes
- [ ] ProfileUsageDialog validates piece/length constraints
- [ ] SheetUsageDialog validates sheet/area constraints
- [ ] PO Line Dialog switches between Profile/Sheet correctly
- [ ] All dialogs submit valid data
- [ ] Edit mode populates existing data correctly

### API Tests
- [ ] Create sheet inventory (POST /api/sheetinventory)
- [ ] Get all sheets (GET /api/sheetinventory)
- [ ] Get sheet by ID (GET /api/sheetinventory/{id})
- [ ] Update sheet (PUT /api/sheetinventory/{id})
- [ ] Delete sheet (DELETE /api/sheetinventory/{id})
- [ ] Record usage (POST /api/sheetinventory/usage)
- [ ] Get remnants (GET /api/sheetinventory/remnants)

### Workflow Tests
- [ ] Add profile to inventory → view in Profiles.razor
- [ ] Add sheet to inventory → view in Sheets.razor
- [ ] Create PO with mixed profiles + sheets
- [ ] Receive PO → creates ProfileInventory + SheetInventory
- [ ] Record profile usage → creates remnant if requested
- [ ] Record sheet usage → creates remnant with dimensions
- [ ] View remnants page (profiles + sheets)
- [ ] Search/filter inventory by MaterialInventoryType

### EN 1090 Compliance Tests
- [ ] Sheet inventory requires MaterialBatch for EXC3/EXC4
- [ ] Mill Test Certificate validation
- [ ] Certificate Type (3.1 min for EXC3)
- [ ] Traceability across remnants (parent lineage)

## Known Issues & Notes

### Import Errors
- ProfileInventoryDialog.razor and SheetInventoryDialog.razor are missing `@using MudBlazor` directive
- Solution: Add directive at top of file (after @page, before other @using statements)

### Service Dependencies
- Dialogs need HttpService implementations for MaterialType, ProfileType, SteelGrade, Supplier, Project
- Some services may not exist yet - check `Manimp.Web/Services/` directory

### MPM Pattern vs Manimp Pattern
- MPM uses more sophisticated line items (Add/Remove within dialog)
- Current Manimp pattern: simpler per-line dialog (opened from parent page)
- Decision: Keep simpler pattern for now, can evolve to MPM pattern later

### PurchaseOrderLineDialog Update Complexity
- Current file is 279 lines profile-only
- Adding MaterialInventoryType will make it ~400 lines (profile + sheet fields)
- Consider splitting into separate dialogs if too complex: POProfileLineDialog.razor + POSheetLineDialog.razor

### Migration Application
- Migration `AddSheetInventoryAndMaterialType` must be applied before testing UI
- Command: `cd Manimp.Data && dotnet ef database update --context AppDbContext`
- Verify tables created: SheetInventories, SheetUsageLogs, SheetRemnantInventories

## Next Steps

1. **Immediate**: Fix import errors in ProfileInventoryDialog.razor and SheetInventoryDialog.razor
2. **Short-term**: Create ProfileUsageDialog.razor and SheetUsageDialog.razor
3. **Medium-term**: Implement backend services (SheetInventoryService, controller, HTTP service)
4. **Long-term**: Create Sheets.razor page, update navigation, test full workflow

## References

- **MPM Dialog Pattern**: https://github.com/petersonmatiss/MPM/blob/main/src/Mpm.Web/Components/Pages/PriceRequestDialog.razor
- **Existing Manimp Dialogs**: `Manimp.Web/Components/Dialogs/` (26 files)
- **Sheet Models**: `Manimp.Shared/Models/SheetInventory.cs`
- **DTOs**: `Manimp.Shared/DTOs/SheetInventoryDTOs.cs`
- **Backend Implementation Guide**: `SHEET-INVENTORY-IMPLEMENTATION.md`
- **Quick Reference**: `SHEET-INVENTORY-QUICK-REF.md`

---

**Document Status**: Living document - update as implementation progresses  
**Last Updated**: October 6, 2025  
**Next Review**: After Phase 1 completion
