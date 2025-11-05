# GitHub Copilot UI Instructions for SteelAxis

**Project:** SteelAxis - Multi-tenant Steel Fabrication Management System  
**UI Framework:** MudBlazor 8.12.0 with Blazor Server (.NET 8.0)  
**Last Updated:** October 26, 2025

---

## 🎨 UI Architecture Overview

### Technology Stack
- **Framework:** Blazor Server with Interactive Server render mode
- **UI Library:** MudBlazor 8.12.0 (Material Design components)
- **State Management:** Scoped services with dependency injection
- **Authentication:** Microsoft Entra External ID (OIDC)
- **Real-time Updates:** SignalR (built into Blazor Server)

### UI Principles
1. **Material Design** - Follow Material Design guidelines via MudBlazor
2. **Responsive First** - Mobile-friendly layouts using MudBlazor's grid system
3. **Accessibility** - WCAG 2.1 Level AA compliance
4. **Performance** - Minimize re-renders, use virtualization for large lists
5. **Consistency** - Reusable components and shared patterns
6. **Multi-tenancy Awareness** - Always display tenant context in UI

---

## � Development Workflow Requirements

### Planning & Approval Process
**CRITICAL:** Before implementing any UI component, page, or feature:

1. **Create a Detailed Plan**
   - Define component structure and hierarchy
   - List all MudBlazor components to be used
   - Identify state management approach
   - Specify API endpoints or services needed
   - Outline responsive design breakpoints
   - Present the plan to the user clearly

2. **Get User Confirmation**
   - Wait for explicit user approval before coding
   - Do NOT create components without confirmation
   - If user requests design changes, revise and confirm again

3. **Implement the Approved Plan**
   - Build components following MudBlazor patterns
   - Test responsiveness and accessibility
   - Report completion and any issues

### Documentation Requirements
**CRITICAL:** After completing any UI work:

1. **Create/Update Feature Documentation Folder**
   - Every feature MUST have a dedicated folder in `/docs/[feature-name]/`
   - Add **UI.md** to the feature folder:
     ```
     docs/
     └── [feature-name]/
         ├── README.md           # Main feature documentation
         ├── PLAN.md             # Implementation plan
         ├── UI.md               # UI-specific documentation (add this)
         └── ...
     ```
   - **UI.md** should include:
     - Page routes and navigation
     - Component hierarchy and structure
     - MudBlazor components used
     - State management approach
     - Responsive design breakpoints
     - Accessibility considerations
     - Screenshots or wireframes
     - User interaction flows

2. **Update Project README Files**
   - Document new pages/components in project README
   - Link to feature documentation folder
   - Note any new dependencies or MudBlazor components used
   - Document navigation changes
   - Update user guides if applicable

3. **Commit Documentation**
   - Commit UI documentation updates separately
   - Use clear commit messages: "docs: add UI documentation for [feature-name]"
   - Ensure documentation reflects current UI state

---

## 🎨 UI Unification & Reusable Components

### Design System Principles
**CRITICAL:** All UI components MUST follow these standardization rules:

1. **Consistent Component Structure** - Same layout patterns across all features
2. **Reusable Dialogs** - Standardized dialog components with configurable content
3. **Unified Forms** - Same form layout, validation, and error handling
4. **Consistent Tables** - Standardized MudDataGrid configuration
5. **Shared Button Patterns** - Same button placement, colors, icons
6. **Consistent Feedback** - Standardized Snackbar messages and loading states

### Standard Component Library

Create reusable components in `SteelAxis.Web/Components/Shared/`:

**1. StandardDialog.razor** - Base dialog for all CRUD operations
```razor
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <EditForm Model="@Model" OnValidSubmit="@HandleSubmit">
            <DataAnnotationsValidator />
            <MudGrid>
                @ChildContent
            </MudGrid>
        </EditForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="@Cancel" Disabled="@_saving">@CancelText</MudButton>
        <MudButton Color="@SubmitColor" 
                   Variant="Variant.Filled" 
                   OnClick="@HandleSubmit" 
                   Disabled="@_saving"
                   StartIcon="@SubmitIcon">
            @(_saving ? "Saving..." : SubmitText)
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public object Model { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback<object> OnSubmit { get; set; }
    [Parameter] public string SubmitText { get; set; } = "Save";
    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public Color SubmitColor { get; set; } = Color.Primary;
    [Parameter] public string SubmitIcon { get; set; } = Icons.Material.Filled.Save;
    
    private bool _saving = false;
    
    private async Task HandleSubmit()
    {
        _saving = true;
        StateHasChanged();
        
        try
        {
            await OnSubmit.InvokeAsync(Model);
            MudDialog.Close(DialogResult.Ok(Model));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _saving = false;
            StateHasChanged();
        }
    }
    
    private void Cancel() => MudDialog.Cancel();
}
```

**2. StandardDataGrid.razor** - Reusable data grid with consistent configuration
```razor
<MudDataGrid T="TItem" 
             Items="@Items"
             ServerData="@(ServerData != null ? ServerData : null)"
             Filterable="true"
             FilterMode="DataGridFilterMode.ColumnFilterRow"
             SortMode="SortMode.Multiple"
             Groupable="@Groupable"
             Dense="true"
             Hover="true"
             Loading="@Loading"
             ReadOnly="@ReadOnly">
    
    <ToolBarContent>
        <MudText Typo="Typo.h6">@Title</MudText>
        <MudSpacer />
        
        @if (ShowSearch)
        {
            <MudTextField @bind-Value="@SearchString" 
                          Placeholder="Search" 
                          Adornment="Adornment.Start" 
                          AdornmentIcon="@Icons.Material.Filled.Search" 
                          IconSize="Size.Medium" 
                          Class="mt-0"
                          Immediate="true"
                          OnDebounceIntervalElapsed="@OnSearchChanged"
                          DebounceInterval="500" />
        }
        
        @if (ShowCreateButton)
        {
            <MudButton Variant="Variant.Filled" 
                       Color="Color.Primary" 
                       StartIcon="@Icons.Material.Filled.Add"
                       OnClick="@OnCreate">
                @CreateButtonText
            </MudButton>
        }
        
        @ToolBarContent
    </ToolBarContent>
    
    <Columns>
        @Columns
        
        @if (ShowActions)
        {
            <TemplateColumn Title="Actions" Sortable="false" Filterable="false">
                <CellTemplate>
                    @if (ShowViewButton)
                    {
                        <MudIconButton Icon="@Icons.Material.Filled.Visibility" 
                                       Size="Size.Small" 
                                       Color="Color.Primary"
                                       OnClick="@(() => OnView.InvokeAsync(context.Item))" />
                    }
                    @if (ShowEditButton)
                    {
                        <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                                       Size="Size.Small" 
                                       Color="Color.Default"
                                       OnClick="@(() => OnEdit.InvokeAsync(context.Item))" />
                    }
                    @if (ShowDeleteButton)
                    {
                        <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                                       Size="Size.Small" 
                                       Color="Color.Error"
                                       OnClick="@(() => OnDelete.InvokeAsync(context.Item))" />
                    }
                </CellTemplate>
            </TemplateColumn>
        }
    </Columns>
    
    <PagerContent>
        <MudDataGridPager T="TItem" />
    </PagerContent>
</MudDataGrid>

@code {
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public Func<GridState<TItem>, Task<GridData<TItem>>>? ServerData { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool Loading { get; set; }
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public bool ShowCreateButton { get; set; } = true;
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public bool ShowViewButton { get; set; } = true;
    [Parameter] public bool ShowEditButton { get; set; } = true;
    [Parameter] public bool ShowDeleteButton { get; set; } = true;
    [Parameter] public bool ReadOnly { get; set; } = false;
    [Parameter] public bool Groupable { get; set; } = false;
    [Parameter] public string CreateButtonText { get; set; } = "New";
    [Parameter] public RenderFragment? Columns { get; set; }
    [Parameter] public RenderFragment? ToolBarContent { get; set; }
    [Parameter] public EventCallback OnCreate { get; set; }
    [Parameter] public EventCallback<TItem> OnView { get; set; }
    [Parameter] public EventCallback<TItem> OnEdit { get; set; }
    [Parameter] public EventCallback<TItem> OnDelete { get; set; }
    [Parameter] public EventCallback<string> OnSearchChanged { get; set; }
    
    public string SearchString { get; set; } = string.Empty;
}

@typeparam TItem
```

**3. StandardForm.razor** - Reusable form layout
```razor
<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Title</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    
    <MudCardContent>
        <EditForm Model="@Model" OnValidSubmit="@OnValidSubmit">
            <DataAnnotationsValidator />
            
            <MudGrid>
                @ChildContent
            </MudGrid>
        </EditForm>
    </MudCardContent>
    
    <MudCardActions>
        <MudButton ButtonType="ButtonType.Submit"
                   Variant="Variant.Filled"
                   Color="Color.Primary"
                   Disabled="@Saving"
                   StartIcon="@Icons.Material.Filled.Save"
                   OnClick="@OnValidSubmit">
            @(Saving ? "Saving..." : SubmitText)
        </MudButton>
        <MudButton OnClick="@OnCancel"
                   Variant="Variant.Text"
                   Disabled="@Saving">
            Cancel
        </MudButton>
    </MudCardActions>
</MudCard>

@code {
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public object Model { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnValidSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public bool Saving { get; set; }
    [Parameter] public string SubmitText { get; set; } = "Save";
}
```

### Usage Pattern - Standard CRUD Implementation

**Every feature follows this exact pattern:**

**List Page (e.g., MaterialCertificates.razor)**
```razor
@page "/certificates"
@inject IMaterialCertificateHttpService HttpService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<StandardDataGrid TItem="MaterialCertificateDto"
                  Items="@_items"
                  Title="Material Certificates"
                  Loading="@_loading"
                  OnCreate="@OpenCreateDialog"
                  OnEdit="@OpenEditDialog"
                  OnDelete="@DeleteItem">
    <Columns>
        <PropertyColumn Property="x => x.LotNumber" Title="Lot Number" />
        <PropertyColumn Property="x => x.MaterialGrade" Title="Grade" />
        <PropertyColumn Property="x => x.Supplier" Title="Supplier" />
        <PropertyColumn Property="x => x.TestDate" Title="Test Date" Format="yyyy-MM-dd" />
    </Columns>
</StandardDataGrid>

@code {
    private List<MaterialCertificateDto> _items = new();
    private bool _loading = true;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        _loading = true;
        StateHasChanged();
        
        try
        {
            _items = await HttpService.GetAllAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
    
    private async Task OpenCreateDialog()
    {
        var parameters = new DialogParameters<CreateMaterialCertificateDialog>();
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CreateMaterialCertificateDialog>("New Certificate", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            await LoadDataAsync();
            Snackbar.Add("Certificate created successfully", Severity.Success);
        }
    }
    
    private async Task OpenEditDialog(MaterialCertificateDto item)
    {
        var parameters = new DialogParameters<EditMaterialCertificateDialog>
        {
            { x => x.CertificateId, item.Id }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<EditMaterialCertificateDialog>("Edit Certificate", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            await LoadDataAsync();
            Snackbar.Add("Certificate updated successfully", Severity.Success);
        }
    }
    
    private async Task DeleteItem(MaterialCertificateDto item)
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Confirm Delete",
            $"Are you sure you want to delete '{item.LotNumber}'? This action cannot be undone.",
            yesText: "Delete", cancelText: "Cancel");
        
        if (confirm == true)
        {
            try
            {
                await HttpService.DeleteAsync(item.Id);
                await LoadDataAsync();
                Snackbar.Add("Certificate deleted successfully", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error deleting certificate: {ex.Message}", Severity.Error);
            }
        }
    }
}
```

**Create/Edit Dialog (e.g., CreateMaterialCertificateDialog.razor)**
```razor
@inject IMaterialCertificateHttpService HttpService

<StandardDialog Model="@_model" 
                OnSubmit="@Submit"
                SubmitText="Create">
    <MudItem xs="12" sm="6">
        <MudTextField @bind-Value="_model.LotNumber"
                      Label="Lot Number"
                      Variant="Variant.Outlined"
                      Required="true"
                      For="@(() => _model.LotNumber)" />
    </MudItem>
    
    <MudItem xs="12" sm="6">
        <MudSelect @bind-Value="_model.MaterialGrade"
                   Label="Material Grade"
                   Variant="Variant.Outlined"
                   Required="true"
                   For="@(() => _model.MaterialGrade)">
            <MudSelectItem Value="@("S355")">S355</MudSelectItem>
            <MudSelectItem Value="@("S275")">S275</MudSelectItem>
            <MudSelectItem Value="@("S235")">S235</MudSelectItem>
        </MudSelect>
    </MudItem>
    
    <MudItem xs="12" sm="6">
        <MudSelect @bind-Value="_model.CertificateType"
                   Label="Certificate Type"
                   Variant="Variant.Outlined"
                   Required="true"
                   For="@(() => _model.CertificateType)">
            <MudSelectItem Value="@("3.1")">3.1 - Mill Certificate</MudSelectItem>
            <MudSelectItem Value="@("3.2")">3.2 - Inspection Certificate</MudSelectItem>
        </MudSelect>
    </MudItem>
    
    <MudItem xs="12" sm="6">
        <MudTextField @bind-Value="_model.Supplier"
                      Label="Supplier"
                      Variant="Variant.Outlined"
                      Required="true"
                      For="@(() => _model.Supplier)" />
    </MudItem>
    
    <!-- More form fields... -->
</StandardDialog>

@code {
    private CreateMaterialCertificateRequest _model = new();
    
    private async Task Submit(object model)
    {
        var request = (CreateMaterialCertificateRequest)model;
        await HttpService.CreateAsync(request);
    }
}
```

### Standardization Rules

**✅ DO:**
- Use `StandardDataGrid` for all list views
- Use `StandardDialog` for all create/edit forms
- Use `StandardForm` for all page-level forms
- Use consistent button colors: Primary for submit, Error for delete, Default for edit
- Use consistent icons: `Save`, `Add`, `Edit`, `Delete`, `Visibility`
- Use consistent Snackbar messages: "[Entity] created/updated/deleted successfully"
- Use consistent loading states with `_loading` boolean
- Always show "Saving..." text during submission
- Always call API via HTTP service (never database directly)
- Always validate with DataAnnotationsValidator
- Always use MudGrid for form layouts
- Always use Variant.Outlined for form inputs

**❌ DON'T:**
- Create custom dialog layouts for each feature
- Use different button placements or styles
- Use different form layouts
- Create different loading indicators
- Use different error handling patterns
- Access database directly from UI components
- Skip validation
- Use different spacing or alignment

### Component Checklist

Before creating ANY new feature UI:

1. ✅ Does it use `StandardDataGrid` for lists?
2. ✅ Does it use `StandardDialog` for create/edit?
3. ✅ Does it use `StandardForm` for page-level forms?
4. ✅ Does it call API via HTTP service (not database directly)?
5. ✅ Does it follow the same button placement?
6. ✅ Does it use the same icons?
7. ✅ Does it show consistent loading states?
8. ✅ Does it use consistent Snackbar messages?
9. ✅ Does it handle errors the same way?
10. ✅ Is the code structure identical to other features?

**If you answered NO to any question, refactor to use standard components.**

---

## 📦 MudBlazor Setup & Configuration

---

## �📦 MudBlazor Setup & Configuration

### Installation & Setup

**Package Installation:**
```xml
<PackageReference Include="MudBlazor" Version="8.12.0" />
```

**Program.cs Configuration:**
```csharp
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});
```

**_Imports.razor:**
```razor
@using MudBlazor
```

**App.razor or MainLayout.razor:**
```razor
<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />
<MudPopoverProvider />
```

### Custom Theme Configuration

```csharp
// In Program.cs or separate ThemeService
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});

// Custom theme (optional)
public class SteelAxisTheme
{
    public static MudTheme Theme = new()
    {
        Palette = new PaletteLight
        {
            Primary = "#1976D2",      // Steel Blue
            Secondary = "#424242",     // Dark Grey
            AppbarBackground = "#1976D2",
            Success = "#4CAF50",
            Error = "#F44336",
            Warning = "#FF9800",
            Info = "#2196F3"
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" }
            }
        }
    };
}
```

---

## 🧩 Component Lifecycle & State Management

### Component Lifecycle Methods

**Standard Lifecycle Pattern:**
```razor
@inject IDbContextFactory<TenantDbContext> DbFactory
@inject ITenantService TenantService
@inject ISnackbar Snackbar
@implements IAsyncDisposable

@code {
    private TenantDbContext _context = default!;
    private List<MaterialCertificate> _certificates = new();
    private bool _loading = true;
    
    protected override async Task OnInitializedAsync()
    {
        // Initialize DbContext
        _context = await DbFactory.CreateDbContextAsync();
        
        // Load data
        await LoadDataAsync();
    }
    
    protected override void OnParametersSet()
    {
        // Called when parameters change
        // Use for parameter validation
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // JS interop calls go here
            // Operations that require the DOM to be ready
        }
    }
    
    private async Task LoadDataAsync()
    {
        _loading = true;
        StateHasChanged(); // Force UI update
        
        try
        {
            var tenantId = TenantService.GetCurrentTenantId();
            _certificates = await _context.MaterialCertificates
                .Where(c => c.TenantId == tenantId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }
}
```

### State Management Patterns

**1. Component State (Local):**
```razor
@code {
    // Local state - specific to this component instance
    private bool _isEditing = false;
    private string _searchTerm = string.Empty;
    private DateTime? _selectedDate = null;
}
```

**2. Cascading Parameters (Parent → Child):**
```razor
<!-- Parent Component -->
<CascadingValue Value="@currentTenant">
    <ChildComponent />
</CascadingValue>

@code {
    private Tenant currentTenant = new();
}

<!-- Child Component -->
@code {
    [CascadingParameter]
    public Tenant CurrentTenant { get; set; } = default!;
}
```

**3. State Container Service (Global/Shared):**
```csharp
// AppState.cs
public class AppState
{
    public event Action? OnChange;
    
    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set
        {
            if (_userName != value)
            {
                _userName = value;
                NotifyStateChanged();
            }
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Program.cs
builder.Services.AddScoped<AppState>();

// Component usage
@inject AppState AppState
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
```

**4. StateHasChanged() Usage:**
```csharp
// Call StateHasChanged() when:
// 1. Async operations complete mid-method
private async Task LongRunningOperation()
{
    _status = "Starting...";
    StateHasChanged(); // Update UI
    
    await Task.Delay(2000);
    
    _status = "Processing...";
    StateHasChanged(); // Update UI again
    
    await ProcessDataAsync();
    
    _status = "Complete!";
    // No need to call StateHasChanged() - Blazor does it automatically at method end
}

// 2. External events (timers, SignalR, etc.)
private void OnTimerElapsed()
{
    _ = InvokeAsync(() =>
    {
        _counter++;
        StateHasChanged();
    });
}
```

---

## 📊 MudBlazor Component Patterns

### 1. Data Display - MudDataGrid (Recommended)

**Complete MudDataGrid Example:**
```razor
<MudDataGrid T="MaterialCertificate" 
             Items="@_certificates"
             Filterable="true"
             FilterMode="DataGridFilterMode.ColumnFilterRow"
             SortMode="SortMode.Multiple"
             Groupable="true"
             Dense="true"
             Hover="true"
             Loading="@_loading"
             ReadOnly="false"
             EditMode="DataGridEditMode.Cell"
             EditTrigger="DataGridEditTrigger.OnClick">
    
    <ToolBarContent>
        <MudText Typo="Typo.h6">Material Certificates</MudText>
        <MudSpacer />
        <MudTextField @bind-Value="_searchString" 
                      Placeholder="Search" 
                      Adornment="Adornment.Start" 
                      AdornmentIcon="@Icons.Material.Filled.Search" 
                      IconSize="Size.Medium" 
                      Class="mt-0"
                      Immediate="true" />
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="@Icons.Material.Filled.Add"
                   OnClick="@(() => OpenCreateDialog())">
            New Certificate
        </MudButton>
    </ToolBarContent>
    
    <Columns>
        <PropertyColumn Property="x => x.LotNumber" Title="Lot Number" />
        <PropertyColumn Property="x => x.MaterialGrade" Title="Grade" />
        <PropertyColumn Property="x => x.CertificateType" Title="Type" />
        <PropertyColumn Property="x => x.Supplier" Title="Supplier" />
        <PropertyColumn Property="x => x.TestDate" Title="Test Date" Format="yyyy-MM-dd" />
        
        <TemplateColumn Title="Status" Sortable="false">
            <CellTemplate>
                @if (context.Item.IsImmutable)
                {
                    <MudChip Color="Color.Success" Size="Size.Small" Icon="@Icons.Material.Filled.Lock">
                        Verified
                    </MudChip>
                }
                else
                {
                    <MudChip Color="Color.Warning" Size="Size.Small">
                        Pending
                    </MudChip>
                }
            </CellTemplate>
        </TemplateColumn>
        
        <TemplateColumn Title="Actions" Sortable="false" Filterable="false">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Visibility" 
                               Size="Size.Small" 
                               Color="Color.Primary"
                               OnClick="@(() => ViewCertificate(context.Item))" />
                <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                               Size="Size.Small" 
                               Color="Color.Default"
                               Disabled="@context.Item.IsImmutable"
                               OnClick="@(() => EditCertificate(context.Item))" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                               Size="Size.Small" 
                               Color="Color.Error"
                               OnClick="@(() => DeleteCertificate(context.Item))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
    
    <PagerContent>
        <MudDataGridPager T="MaterialCertificate" />
    </PagerContent>
</MudDataGrid>

@code {
    private List<MaterialCertificate> _certificates = new();
    private string _searchString = string.Empty;
    private bool _loading = true;
}
```

**Server-Side Pagination (Large Datasets):**
```razor
<MudDataGrid T="MaterialCertificate"
             ServerData="@LoadServerData"
             Filterable="true"
             SortMode="SortMode.Multiple">
    <!-- Columns same as above -->
</MudDataGrid>

@code {
    private async Task<GridData<MaterialCertificate>> LoadServerData(GridState<MaterialCertificate> state)
    {
        var tenantId = TenantService.GetCurrentTenantId();
        
        IQueryable<MaterialCertificate> query = _context.MaterialCertificates
            .Where(c => c.TenantId == tenantId);
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(state.FilterDefinitions?.FirstOrDefault()?.Value?.ToString()))
        {
            var searchTerm = state.FilterDefinitions.First().Value.ToString()!;
            query = query.Where(c => c.LotNumber.Contains(searchTerm) || 
                                    c.Supplier.Contains(searchTerm));
        }
        
        // Apply sorting
        if (state.SortDefinitions.Any())
        {
            var sortDef = state.SortDefinitions.First();
            query = sortDef.Descending 
                ? query.OrderByDescending(c => EF.Property<object>(c, sortDef.SortBy))
                : query.OrderBy(c => EF.Property<object>(c, sortDef.SortBy));
        }
        
        var totalItems = await query.CountAsync();
        
        // Apply pagination
        var items = await query
            .Skip(state.Page * state.PageSize)
            .Take(state.PageSize)
            .ToListAsync();
        
        return new GridData<MaterialCertificate>
        {
            Items = items,
            TotalItems = totalItems
        };
    }
}
```

### 2. Forms & Validation

**Complete Form Example with MudBlazor:**
```razor
<EditForm Model="@_model" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />
    
    <MudCard>
        <MudCardHeader>
            <CardHeaderContent>
                <MudText Typo="Typo.h6">
                    @(_isEdit ? "Edit Certificate" : "New Certificate")
                </MudText>
            </CardHeaderContent>
        </MudCardHeader>
        
        <MudCardContent>
            <MudGrid>
                <MudItem xs="12" sm="6">
                    <MudTextField @bind-Value="_model.LotNumber"
                                  Label="Lot Number"
                                  Variant="Variant.Outlined"
                                  Required="true"
                                  For="@(() => _model.LotNumber)"
                                  HelperText="Unique lot identifier" />
                </MudItem>
                
                <MudItem xs="12" sm="6">
                    <MudSelect @bind-Value="_model.MaterialGrade"
                               Label="Material Grade"
                               Variant="Variant.Outlined"
                               Required="true"
                               For="@(() => _model.MaterialGrade)">
                        <MudSelectItem Value="@("S235")">S235</MudSelectItem>
                        <MudSelectItem Value="@("S275")">S275</MudSelectItem>
                        <MudSelectItem Value="@("S355")">S355</MudSelectItem>
                        <MudSelectItem Value="@("S420")">S420</MudSelectItem>
                        <MudSelectItem Value="@("S460")">S460</MudSelectItem>
                    </MudSelect>
                </MudItem>
                
                <MudItem xs="12" sm="6">
                    <MudSelect @bind-Value="_model.CertificateType"
                               Label="Certificate Type"
                               Variant="Variant.Outlined"
                               Required="true"
                               For="@(() => _model.CertificateType)">
                        <MudSelectItem Value="@("3.1")">3.1 - Mill Certificate</MudSelectItem>
                        <MudSelectItem Value="@("3.2")">3.2 - Inspection Certificate</MudSelectItem>
                    </MudSelect>
                </MudItem>
                
                <MudItem xs="12" sm="6">
                    <MudTextField @bind-Value="_model.Supplier"
                                  Label="Supplier"
                                  Variant="Variant.Outlined"
                                  Required="true"
                                  For="@(() => _model.Supplier)" />
                </MudItem>
                
                <MudItem xs="12" sm="6">
                    <MudTextField @bind-Value="_model.CertificateNumber"
                                  Label="Certificate Number"
                                  Variant="Variant.Outlined"
                                  Required="true"
                                  For="@(() => _model.CertificateNumber)" />
                </MudItem>
                
                <MudItem xs="12" sm="6">
                    <MudDatePicker @bind-Date="_model.TestDate"
                                   Label="Test Date"
                                   Variant="Variant.Outlined"
                                   Required="true"
                                   For="@(() => _model.TestDate)"
                                   MaxDate="DateTime.Today" />
                </MudItem>
                
                <MudItem xs="12">
                    <MudTextField @bind-Value="_model.Notes"
                                  Label="Notes"
                                  Variant="Variant.Outlined"
                                  Lines="3"
                                  For="@(() => _model.Notes)" />
                </MudItem>
                
                <MudItem xs="12">
                    <MudFileUpload T="IBrowserFile" 
                                   Accept=".pdf,.jpg,.png"
                                   OnFilesChanged="@OnFileSelected"
                                   MaximumFileCount="1">
                        <ButtonTemplate>
                            <MudButton HtmlTag="label"
                                       Variant="Variant.Filled"
                                       Color="Color.Primary"
                                       StartIcon="@Icons.Material.Filled.CloudUpload"
                                       for="@context">
                                Upload Certificate PDF
                            </MudButton>
                        </ButtonTemplate>
                    </MudFileUpload>
                    @if (_selectedFile != null)
                    {
                        <MudChip Color="Color.Success" OnClose="@(() => _selectedFile = null)">
                            @_selectedFile.Name
                        </MudChip>
                    }
                </MudItem>
            </MudGrid>
        </MudCardContent>
        
        <MudCardActions>
            <MudButton ButtonType="ButtonType.Submit"
                       Variant="Variant.Filled"
                       Color="Color.Primary"
                       Disabled="@_saving"
                       StartIcon="@Icons.Material.Filled.Save">
                @(_saving ? "Saving..." : "Save")
            </MudButton>
            <MudButton OnClick="@Cancel"
                       Variant="Variant.Text"
                       Disabled="@_saving">
                Cancel
            </MudButton>
        </MudCardActions>
    </MudCard>
</EditForm>

@code {
    private CertificateModel _model = new();
    private IBrowserFile? _selectedFile;
    private bool _saving = false;
    private bool _isEdit = false;
    
    private async Task HandleValidSubmit()
    {
        _saving = true;
        StateHasChanged();
        
        try
        {
            // Save logic here
            Snackbar.Add("Certificate saved successfully", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _saving = false;
            StateHasChanged();
        }
    }
    
    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        _selectedFile = e.File;
    }
    
    private void Cancel()
    {
        // Navigation logic
    }
    
    public class CertificateModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string LotNumber { get; set; } = string.Empty;
        
        [Required]
        public string MaterialGrade { get; set; } = string.Empty;
        
        [Required]
        public string CertificateType { get; set; } = string.Empty;
        
        [Required]
        public string Supplier { get; set; } = string.Empty;
        
        [Required]
        public string CertificateNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime? TestDate { get; set; }
        
        public string? Notes { get; set; }
    }
}
```

### 3. Dialogs

**Dialog Component Pattern:**
```razor
<!-- EditCertificateDialog.razor -->
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <EditForm Model="@_model" OnValidSubmit="@Submit">
            <DataAnnotationsValidator />
            
            <MudTextField @bind-Value="_model.LotNumber"
                          Label="Lot Number"
                          Variant="Variant.Outlined"
                          Required="true"
                          For="@(() => _model.LotNumber)" />
            
            <MudSelect @bind-Value="_model.MaterialGrade"
                       Label="Material Grade"
                       Variant="Variant.Outlined"
                       Required="true">
                <MudSelectItem Value="@("S355")">S355</MudSelectItem>
                <MudSelectItem Value="@("S275")">S275</MudSelectItem>
            </MudSelect>
            
            <!-- More fields -->
        </EditForm>
    </DialogContent>
    
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" OnClick="Submit">Save</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;
    
    [Parameter]
    public Guid CertificateId { get; set; }
    
    private CertificateModel _model = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Load data if editing
        if (CertificateId != Guid.Empty)
        {
            await LoadCertificateAsync();
        }
    }
    
    private async Task Submit()
    {
        try
        {
            // Save logic
            MudDialog.Close(DialogResult.Ok(_model));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
    
    private void Cancel() => MudDialog.Cancel();
}
```

**Opening a Dialog:**
```razor
@inject IDialogService DialogService

@code {
    private async Task OpenEditDialog(Guid certificateId)
    {
        var parameters = new DialogParameters<EditCertificateDialog>
        {
            { x => x.CertificateId, certificateId }
        };
        
        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            DisableBackdropClick = true
        };
        
        var dialog = await DialogService.ShowAsync<EditCertificateDialog>(
            "Edit Certificate", 
            parameters, 
            options);
            
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            await LoadDataAsync();
            Snackbar.Add("Certificate updated successfully", Severity.Success);
        }
    }
}
```

**Confirmation Dialog:**
```razor
private async Task<bool> ConfirmDelete(string itemName)
{
    var parameters = new DialogParameters
    {
        ["ContentText"] = $"Are you sure you want to delete '{itemName}'? This action cannot be undone.",
        ["ButtonText"] = "Delete",
        ["Color"] = Color.Error
    };
    
    var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };
    
    var dialog = await DialogService.ShowAsync<MudMessageBox>(
        "Confirm Delete", 
        parameters, 
        options);
        
    var result = await dialog.Result;
    return !result.Canceled;
}

private async Task DeleteCertificate(MaterialCertificate certificate)
{
    if (await ConfirmDelete(certificate.LotNumber))
    {
        try
        {
            // Delete logic
            Snackbar.Add("Certificate deleted", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

### 4. Loading States & Progress

**Loading Overlay:**
```razor
<MudOverlay Visible="@_loading" DarkBackground="true" Absolute="true">
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" Size="Size.Large" />
</MudOverlay>

<!-- Or use MudProgressLinear for top of page -->
@if (_loading)
{
    <MudProgressLinear Color="Color.Primary" Indeterminate="true" Class="mb-4" />
}
```

**Skeleton Loaders:**
```razor
@if (_loading)
{
    <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="60px" />
    <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="40px" Class="mt-2" />
    <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="40px" Class="mt-2" />
}
else
{
    <!-- Actual content -->
}
```

### 5. Notifications (Snackbar)

```razor
@inject ISnackbar Snackbar

@code {
    private void ShowSuccess(string message)
    {
        Snackbar.Add(message, Severity.Success);
    }
    
    private void ShowError(string message)
    {
        Snackbar.Add(message, Severity.Error);
    }
    
    private void ShowWarning(string message)
    {
        Snackbar.Add(message, Severity.Warning);
    }
    
    private void ShowInfo(string message)
    {
        Snackbar.Add(message, Severity.Info);
    }
    
    private void ShowCustom(string message)
    {
        Snackbar.Add(message, Severity.Normal, config =>
        {
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = 3000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.Action = "Undo";
            config.ActionColor = Color.Primary;
            config.Onclick = snackbar =>
            {
                // Handle action click
                return Task.CompletedTask;
            };
        });
    }
}
```

### 6. Navigation & Layout

**NavMenu Pattern:**
```razor
<!-- NavMenu.razor -->
<MudNavMenu>
    <MudNavLink Href="/" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Dashboard">
        Dashboard
    </MudNavLink>
    
    <MudNavGroup Title="EN 1090 Compliance" Icon="@Icons.Material.Filled.VerifiedUser" Expanded="true">
        <MudNavLink Href="/certificates" Icon="@Icons.Material.Filled.Certificate">
            Material Certificates
        </MudNavLink>
        <MudNavLink Href="/ncr" Icon="@Icons.Material.Filled.Warning">
            NCR Management
        </MudNavLink>
        <MudNavLink Href="/welding" Icon="@Icons.Material.Filled.Construction">
            Welding Procedures
        </MudNavLink>
    </MudNavGroup>
    
    <MudNavGroup Title="Projects" Icon="@Icons.Material.Filled.Business">
        <MudNavLink Href="/projects" Icon="@Icons.Material.Filled.List">
            All Projects
        </MudNavLink>
        <MudNavLink Href="/projects/create" Icon="@Icons.Material.Filled.Add">
            New Project
        </MudNavLink>
    </MudNavGroup>
    
    <MudNavLink Href="/inventory" Icon="@Icons.Material.Filled.Inventory">
        Inventory
    </MudNavLink>
    
    <MudNavLink Href="/customer-portal" Icon="@Icons.Material.Filled.Group">
        Customer Portal
    </MudNavLink>
</MudNavMenu>
```

**AppBar with User Menu:**
```razor
<MudAppBar Elevation="1">
    <MudIconButton Icon="@Icons.Material.Filled.Menu" 
                   Color="Color.Inherit" 
                   Edge="Edge.Start" 
                   OnClick="@ToggleDrawer" />
    
    <MudText Typo="Typo.h6" Class="ml-3">SteelAxis</MudText>
    
    <MudSpacer />
    
    <!-- Tenant Indicator -->
    <MudChip Color="Color.Primary" Variant="Variant.Outlined">
        @TenantService.GetTenantName()
    </MudChip>
    
    <!-- Notifications -->
    <MudIconButton Icon="@Icons.Material.Filled.Notifications" Color="Color.Inherit" />
    
    <!-- User Menu -->
    <MudMenu Icon="@Icons.Material.Filled.AccountCircle" Color="Color.Inherit">
        <MudText Typo="Typo.body2" Class="px-4 py-2">@userName</MudText>
        <MudDivider />
        <MudMenuItem Icon="@Icons.Material.Filled.Person">Profile</MudMenuItem>
        <MudMenuItem Icon="@Icons.Material.Filled.Settings">Settings</MudMenuItem>
        <MudDivider />
        <MudMenuItem Icon="@Icons.Material.Filled.Logout" OnClick="@Logout">
            Logout
        </MudMenuItem>
    </MudMenu>
</MudAppBar>
```

---

## 🎯 Common UI Patterns for SteelAxis

### 1. List → Detail → Edit Pattern

```razor
<!-- MaterialCertificatesList.razor -->
@page "/certificates"
@inject NavigationManager Navigation

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudDataGrid T="MaterialCertificate" Items="@_certificates" ...>
        <!-- Columns -->
        <TemplateColumn Title="Actions">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.ChevronRight"
                               OnClick="@(() => ViewDetails(context.Item.Id))" />
            </CellTemplate>
        </TemplateColumn>
    </MudDataGrid>
</MudContainer>

@code {
    private void ViewDetails(Guid id)
    {
        Navigation.NavigateTo($"/certificates/{id}");
    }
}

<!-- MaterialCertificateDetails.razor -->
@page "/certificates/{Id:guid}"
@inject NavigationManager Navigation

<MudContainer MaxWidth="MaxWidth.Large">
    <MudCard>
        <MudCardHeader>
            <CardHeaderActions>
                <MudIconButton Icon="@Icons.Material.Filled.Edit"
                               OnClick="@EditCertificate" />
            </CardHeaderActions>
        </MudCardHeader>
        <MudCardContent>
            <!-- Display details -->
        </MudCardContent>
    </MudCard>
</MudContainer>

@code {
    [Parameter]
    public Guid Id { get; set; }
    
    private void EditCertificate()
    {
        Navigation.NavigateTo($"/certificates/{Id}/edit");
    }
}
```

### 2. Dashboard with Cards & Stats

```razor
<MudGrid>
    <!-- KPI Cards -->
    <MudItem xs="12" sm="6" md="3">
        <MudCard Elevation="2">
            <MudCardContent>
                <div class="d-flex justify-space-between">
                    <div>
                        <MudText Typo="Typo.body2" Color="Color.Secondary">
                            Active Projects
                        </MudText>
                        <MudText Typo="Typo.h4">24</MudText>
                    </div>
                    <MudIcon Icon="@Icons.Material.Filled.Business" 
                             Color="Color.Primary" 
                             Size="Size.Large" />
                </div>
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <MudItem xs="12" sm="6" md="3">
        <MudCard Elevation="2">
            <MudCardContent>
                <div class="d-flex justify-space-between">
                    <div>
                        <MudText Typo="Typo.body2" Color="Color.Secondary">
                            Pending NCRs
                        </MudText>
                        <MudText Typo="Typo.h4" Color="Color.Error">7</MudText>
                    </div>
                    <MudIcon Icon="@Icons.Material.Filled.Warning" 
                             Color="Color.Error" 
                             Size="Size.Large" />
                </div>
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <!-- Charts -->
    <MudItem xs="12" md="8">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">Project Progress</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                <!-- Chart component here -->
            </MudCardContent>
        </MudCard>
    </MudItem>
    
    <!-- Recent Activity -->
    <MudItem xs="12" md="4">
        <MudCard>
            <MudCardHeader>
                <CardHeaderContent>
                    <MudText Typo="Typo.h6">Recent Activity</MudText>
                </CardHeaderContent>
            </MudCardHeader>
            <MudCardContent>
                <MudList Dense="true">
                    @foreach (var activity in _recentActivities)
                    {
                        <MudListItem Icon="@Icons.Material.Filled.Circle" IconColor="Color.Primary">
                            <MudText Typo="Typo.body2">@activity.Description</MudText>
                            <MudText Typo="Typo.caption" Color="Color.Secondary">
                                @activity.Timestamp.ToString("g")
                            </MudText>
                        </MudListItem>
                    }
                </MudList>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>
```

### 3. Filtered Search with Advanced Filters

```razor
<MudCard Class="mb-4">
    <MudCardContent>
        <MudGrid>
            <MudItem xs="12" sm="6" md="3">
                <MudTextField @bind-Value="_searchTerm"
                              Label="Search"
                              Variant="Variant.Outlined"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Filled.Search"
                              Immediate="true"
                              OnDebounceIntervalElapsed="@ApplyFilters"
                              DebounceInterval="500" />
            </MudItem>
            
            <MudItem xs="12" sm="6" md="3">
                <MudSelect @bind-Value="_selectedGrade"
                           Label="Material Grade"
                           Variant="Variant.Outlined"
                           Clearable="true"
                           OnClearButtonClick="@(() => { _selectedGrade = null; ApplyFilters(); })">
                    <MudSelectItem Value="@("S355")">S355</MudSelectItem>
                    <MudSelectItem Value="@("S275")">S275</MudSelectItem>
                </MudSelect>
            </MudItem>
            
            <MudItem xs="12" sm="6" md="3">
                <MudDateRangePicker @bind-DateRange="_dateRange"
                                    Label="Date Range"
                                    Variant="Variant.Outlined"
                                    OnDateRangeChanged="@ApplyFilters" />
            </MudItem>
            
            <MudItem xs="12" sm="6" md="3">
                <MudButton Variant="Variant.Outlined"
                           StartIcon="@Icons.Material.Filled.Clear"
                           OnClick="@ClearFilters"
                           FullWidth="true">
                    Clear Filters
                </MudButton>
            </MudItem>
        </MudGrid>
    </MudCardContent>
</MudCard>

@code {
    private string _searchTerm = string.Empty;
    private string? _selectedGrade;
    private DateRange? _dateRange;
    
    private async Task ApplyFilters()
    {
        await LoadDataAsync();
    }
    
    private async Task ClearFilters()
    {
        _searchTerm = string.Empty;
        _selectedGrade = null;
        _dateRange = null;
        await LoadDataAsync();
    }
}
```

---

## ⚡ Performance Best Practices

### 1. Virtualization for Large Lists

```razor
<MudVirtualize Items="@_items" Context="item">
    <MudListItem>
        <MudText>@item.Name</MudText>
    </MudListItem>
</MudVirtualize>
```

### 2. Minimize StateHasChanged Calls

```csharp
// Bad - unnecessary StateHasChanged
private async Task LoadData()
{
    _items = await GetItemsAsync();
    StateHasChanged(); // Not needed - Blazor will re-render automatically
}

// Good - only when needed
private async Task UpdateStatus()
{
    await Task.Delay(1000);
    _status = "Processing...";
    StateHasChanged(); // Needed - mid-operation update
    
    await ProcessAsync();
    _status = "Complete"; // Blazor re-renders automatically at method end
}
```

### 3. Use @key for Dynamic Lists

```razor
@foreach (var item in _items)
{
    <CertificateCard @key="item.Id" Certificate="@item" />
}
```

### 4. Dispose Resources

```csharp
@implements IAsyncDisposable

@code {
    private TenantDbContext _context = default!;
    private CancellationTokenSource _cts = new();
    
    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }
}
```

---

## 🎨 Responsive Design

### Grid System

```razor
<MudGrid>
    <!-- Full width on mobile, half on tablet, third on desktop -->
    <MudItem xs="12" sm="6" md="4">
        <MudCard>Content</MudCard>
    </MudItem>
    
    <!-- Responsive visibility -->
    <MudItem xs="12" Class="d-none d-md-block">
        <MudText>Only visible on desktop</MudText>
    </MudItem>
</MudGrid>
```

### Breakpoint Service

```razor
@inject IBreakpointService BreakpointService

@code {
    private bool _isMobile;
    
    protected override async Task OnInitializedAsync()
    {
        var breakpoint = await BreakpointService.GetBreakpoint();
        _isMobile = breakpoint == Breakpoint.Xs || breakpoint == Breakpoint.Sm;
        
        // Subscribe to breakpoint changes
        BreakpointService.OnBreakpointChanged += OnBreakpointChanged;
    }
    
    private void OnBreakpointChanged(object? sender, Breakpoint breakpoint)
    {
        InvokeAsync(() =>
        {
            _isMobile = breakpoint == Breakpoint.Xs || breakpoint == Breakpoint.Sm;
            StateHasChanged();
        });
    }
}
```

---

## 🛡️ Security Considerations

### 1. Always Validate Tenant Context

```razor
@inject ITenantService TenantService

@code {
    protected override async Task OnInitializedAsync()
    {
        var tenantId = TenantService.GetCurrentTenantId();
        
        // Always filter by tenantId
        _items = await _context.Items
            .Where(i => i.TenantId == tenantId)
            .ToListAsync();
    }
}
```

### 2. Sanitize User Input

```razor
<MudTextField @bind-Value="_input"
              Label="User Input"
              Validation="@(new Func<string, string?>(ValidateInput))" />

@code {
    private string? ValidateInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Required";
            
        if (input.Length > 200)
            return "Max 200 characters";
            
        // Add more validation
        return null;
    }
}
```

### 3. Authorize Operations

```razor
@attribute [Authorize(Roles = "Admin,Manager")]

<!-- Or in code -->
@code {
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    
    private async Task<bool> CanEdit()
    {
        var authState = await AuthenticationStateTask;
        return authState.User.IsInRole("Admin") || 
               authState.User.IsInRole("Manager");
    }
}
```

---

## 📝 Accessibility (A11y) Best Practices

### 1. Semantic HTML & ARIA

```razor
<MudButton aria-label="Delete certificate" OnClick="@Delete">
    <MudIcon Icon="@Icons.Material.Filled.Delete" />
</MudButton>

<MudTextField Label="Lot Number" 
              aria-describedby="lot-number-help" />
<MudText id="lot-number-help" Typo="Typo.caption">
    Enter the unique lot identifier
</MudText>
```

### 2. Keyboard Navigation

```razor
<div tabindex="0" @onkeydown="@HandleKeyDown">
    <!-- Content -->
</div>

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == " ")
        {
            // Handle activation
        }
    }
}
```

---

## 🎯 Code Generation Checklist

When generating UI components, always include:

1. ✅ **Dependency injection** - Services, DbContext, Snackbar
2. ✅ **Loading states** - `_loading` boolean + `StateHasChanged()`
3. ✅ **Error handling** - Try-catch with Snackbar notifications
4. ✅ **Tenant filtering** - Filter all queries by `TenantId`
5. ✅ **Dispose pattern** - `IAsyncDisposable` for DbContext
6. ✅ **Validation** - DataAnnotations + MudBlazor `For` attribute
7. ✅ **Responsive design** - MudGrid with xs/sm/md breakpoints
8. ✅ **Accessibility** - ARIA labels, semantic HTML
9. ✅ **Consistent styling** - MudBlazor Variant.Outlined for forms
10. ✅ **User feedback** - Success/error Snackbar messages

---

**Remember:** This is a multi-tenant steel fabrication system. Always show tenant context in the UI and ensure data isolation in all components!
