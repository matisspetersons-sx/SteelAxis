# MudTable Component Reference

## Overview
MudTable is a data display component for showing tabular data with sorting, filtering, pagination, and selection capabilities.

## Basic Usage

```razor
<MudTable Items="@people" Hover="true" SortLabel="Sort By">
    <HeaderContent>
        <MudTh><MudTableSortLabel SortBy="new Func<Person, object>(x => x.Name)">Name</MudTableSortLabel></MudTh>
        <MudTh><MudTableSortLabel SortBy="new Func<Person, object>(x => x.Age)">Age</MudTableSortLabel></MudTh>
        <MudTh>Email</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="Age">@context.Age</MudTd>
        <MudTd DataLabel="Email">@context.Email</MudTd>
    </RowTemplate>
</MudTable>

@code {
    private List<Person> people = new();
}
```

## Common Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | `null` | Data source for the table |
| `ServerData` | `Func<TableState, Task<TableData<T>>>` | `null` | Server-side data loading |
| `Dense` | `bool` | `false` | Compact row height |
| `Hover` | `bool` | `false` | Hover effect on rows |
| `Striped` | `bool` | `false` | Alternating row colors |
| `Bordered` | `bool` | `false` | Show borders |
| `Square` | `bool` | `false` | Square corners |
| `Outlined` | `bool` | `false` | Outlined style |
| `Elevation` | `int` | `1` | Shadow elevation |
| `FixedHeader` | `bool` | `false` | Fixed header on scroll |
| `FixedFooter` | `bool` | `false` | Fixed footer on scroll |
| `Height` | `string` | `null` | Table height |
| `SortLabel` | `string` | `"Sort"` | Sort button aria-label |
| `AllowUnsorted` | `bool` | `true` | Allow unsorted state |
| `MultiSelection` | `bool` | `false` | Enable row selection |
| `SelectOnRowClick` | `bool` | `true` | Select row on click |
| `SelectedItems` | `HashSet<T>` | `null` | Selected items |
| `RowClick` | `EventCallback<TableRowClickEventArgs<T>>` | - | Row click handler |
| `RowClassFunc` | `Func<T, int, string>` | `null` | Custom row CSS classes |
| `RowStyleFunc` | `Func<T, int, string>` | `null` | Custom row styles |
| `Loading` | `bool` | `false` | Show loading state |
| `LoadingProgressColor` | `Color` | `Color.Info` | Loading indicator color |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

## Examples

### Basic Table with Sorting
```razor
<MudTable Items="@products" 
          Hover="true" 
          Dense="true"
          SortLabel="Sort By">
    <HeaderContent>
        <MudTh>
            <MudTableSortLabel SortBy="new Func<Product, object>(x => x.Id)">
                ID
            </MudTableSortLabel>
        </MudTh>
        <MudTh>
            <MudTableSortLabel SortBy="new Func<Product, object>(x => x.Name)">
                Name
            </MudTableSortLabel>
        </MudTh>
        <MudTh>
            <MudTableSortLabel SortBy="new Func<Product, object>(x => x.Price)">
                Price
            </MudTableSortLabel>
        </MudTh>
        <MudTh>Category</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="ID">@context.Id</MudTd>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="Price">@context.Price.ToString("C")</MudTd>
        <MudTd DataLabel="Category">@context.Category</MudTd>
    </RowTemplate>
</MudTable>
```

### Table with Selection
```razor
<MudTable @ref="mudTable" 
          T="Employee"
          Items="@employees" 
          MultiSelection="true" 
          @bind-SelectedItems="selectedEmployees"
          Hover="true">
    <HeaderContent>
        <MudTh>
            <MudCheckBox @bind-Checked="@selectAll" 
                        Indeterminate="@(selectedEmployees.Any() && selectedEmployees.Count != employees.Count)"
                        T="bool" 
                        Color="Color.Primary"
                        TriState="true"
                        CheckedChanged="@SelectAllItems" />
        </MudTh>
        <MudTh>Name</MudTh>
        <MudTh>Department</MudTh>
        <MudTh>Salary</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>
            <MudCheckBox @bind-Checked="@selectedEmployees.Contains(context)" 
                        T="bool" 
                        Color="Color.Primary" />
        </MudTd>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="Department">@context.Department</MudTd>
        <MudTd DataLabel="Salary">@context.Salary.ToString("C")</MudTd>
    </RowTemplate>
</MudTable>

<MudText>Selected: @selectedEmployees.Count items</MudText>

@code {
    private MudTable<Employee> mudTable;
    private HashSet<Employee> selectedEmployees = new();
    private bool selectAll = false;
    
    private void SelectAllItems(bool value)
    {
        selectAll = value;
        if (value)
        {
            selectedEmployees = employees.ToHashSet();
        }
        else
        {
            selectedEmployees.Clear();
        }
        mudTable.ReloadServerData();
    }
}
```

### Server-Side Table
```razor
<MudTable ServerData="@(new Func<TableState, Task<TableData<User>>>(ServerReload))"
          Dense="true" 
          Hover="true" 
          Loading="@loading"
          @ref="table">
    <ToolBarContent>
        <MudText Typo="Typo.h6">Users</MudText>
        <MudSpacer />
        <MudTextField T="string" 
                      ValueChanged="@(s => OnSearch(s))" 
                      Placeholder="Search" 
                      Adornment="Adornment.Start"
                      AdornmentIcon="Icons.Material.Filled.Search" 
                      IconSize="Size.Medium" 
                      Class="mt-0" />
    </ToolBarContent>
    <HeaderContent>
        <MudTh><MudTableSortLabel T="User" SortLabel="name">Name</MudTableSortLabel></MudTh>
        <MudTh><MudTableSortLabel T="User" SortLabel="email">Email</MudTableSortLabel></MudTh>
        <MudTh><MudTableSortLabel T="User" SortLabel="created">Created</MudTableSortLabel></MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="Email">@context.Email</MudTd>
        <MudTd DataLabel="Created">@context.Created.ToString("dd/MM/yyyy")</MudTd>
    </RowTemplate>
    <PagerContent>
        <MudTablePager />
    </PagerContent>
</MudTable>

@code {
    private MudTable<User> table;
    private bool loading = false;
    private string searchString = "";
    
    private async Task<TableData<User>> ServerReload(TableState state)
    {
        loading = true;
        
        var data = await userService.GetUsersAsync(
            page: state.Page,
            pageSize: state.PageSize,
            sortBy: state.SortLabel,
            sortDirection: state.SortDirection,
            search: searchString);
        
        loading = false;
        
        return new TableData<User>
        {
            TotalItems = data.TotalCount,
            Items = data.Items
        };
    }
    
    private void OnSearch(string text)
    {
        searchString = text;
        table.ReloadServerData();
    }
}
```

### Table with Custom Row Actions
```razor
<MudTable Items="@orders" Hover="true">
    <HeaderContent>
        <MudTh>Order ID</MudTh>
        <MudTh>Customer</MudTh>
        <MudTh>Total</MudTh>
        <MudTh>Status</MudTh>
        <MudTh>Actions</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Order ID">@context.Id</MudTd>
        <MudTd DataLabel="Customer">@context.CustomerName</MudTd>
        <MudTd DataLabel="Total">@context.Total.ToString("C")</MudTd>
        <MudTd DataLabel="Status">
            <MudChip Color="GetStatusColor(context.Status)" Size="Size.Small">
                @context.Status
            </MudChip>
        </MudTd>
        <MudTd DataLabel="Actions">
            <MudIconButton Size="Size.Small" 
                          Icon="Icons.Material.Outlined.Edit" 
                          Color="Color.Primary"
                          OnClick="@(() => EditOrder(context))" />
            <MudIconButton Size="Size.Small" 
                          Icon="Icons.Material.Outlined.Delete" 
                          Color="Color.Error"
                          OnClick="@(() => DeleteOrder(context))" />
        </MudTd>
    </RowTemplate>
</MudTable>

@code {
    private Color GetStatusColor(string status) => status switch
    {
        "Completed" => Color.Success,
        "Pending" => Color.Warning,
        "Cancelled" => Color.Error,
        _ => Color.Default
    };
    
    private void EditOrder(Order order)
    {
        // Handle edit
    }
    
    private void DeleteOrder(Order order)
    {
        // Handle delete
    }
}
```

### Table with Custom Footer
```razor
<MudTable Items="@salesData" Hover="true">
    <HeaderContent>
        <MudTh>Product</MudTh>
        <MudTh>Quantity</MudTh>
        <MudTh>Price</MudTh>
        <MudTh>Total</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Product">@context.Product</MudTd>
        <MudTd DataLabel="Quantity">@context.Quantity</MudTd>
        <MudTd DataLabel="Price">@context.Price.ToString("C")</MudTd>
        <MudTd DataLabel="Total">@((context.Quantity * context.Price).ToString("C"))</MudTd>
    </RowTemplate>
    <FooterContent>
        <MudTd colspan="3"><strong>Total</strong></MudTd>
        <MudTd><strong>@salesData.Sum(x => x.Quantity * x.Price).ToString("C")</strong></MudTd>
    </FooterContent>
</MudTable>
```

### Grouped Table
```razor
<MudTable Items="@employees.OrderBy(x => x.Department)" 
          GroupBy="@groupDefinition"
          Hover="true">
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>Position</MudTh>
        <MudTh>Salary</MudTh>
    </HeaderContent>
    <GroupHeaderTemplate>
        <MudTh Class="mud-table-cell-custom-group" colspan="3">
            @($"{context.GroupName}: {context.Key} ({context.Items.Count()} employees)")
        </MudTh>
    </GroupHeaderTemplate>
    <RowTemplate>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="Position">@context.Position</MudTd>
        <MudTd DataLabel="Salary">@context.Salary.ToString("C")</MudTd>
    </RowTemplate>
</MudTable>

@code {
    private TableGroupDefinition<Employee> groupDefinition = new()
    {
        GroupName = "Department",
        Indentation = false,
        Expandable = true,
        Selector = (e) => e.Department
    };
}
```

### Editable Table
```razor
<MudTable @ref="mudTable" 
          T="Item" 
          Items="@items" 
          CanCancelEdit="@canCancelEdit"
          RowEditPreview="@BackupItem" 
          RowEditCancel="@ResetItemToOriginalValues"
          RowEditCommit="@ItemHasBeenCommitted"
          IsEditRowSwitchingBlocked="@blockSwitch"
          ApplyButtonPosition="@applyButtonPosition"
          EditButtonPosition="@editButtonPosition"
          EditTrigger="@editTrigger">
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>Calories</MudTh>
        <MudTh>Fat</MudTh>
        <MudTh>Actions</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Name">@context.Name</MudTd>
        <MudTd DataLabel="Calories">@context.Calories</MudTd>
        <MudTd DataLabel="Fat">@context.Fat</MudTd>
        <MudTd DataLabel="">
            <MudIconButton Size="@Size.Small" 
                          Icon="Icons.Material.Outlined.Edit" 
                          OnClick="@(() => mudTable.SetEditingItem(context))" />
        </MudTd>
    </RowTemplate>
    <RowEditingTemplate>
        <MudTd DataLabel="Name">
            <MudTextField @bind-Value="@context.Name" Required />
        </MudTd>
        <MudTd DataLabel="Calories">
            <MudNumericField @bind-Value="@context.Calories" Required Min="0" />
        </MudTd>
        <MudTd DataLabel="Fat">
            <MudNumericField @bind-Value="@context.Fat" Required Min="0" />
        </MudTd>
        <MudTd DataLabel="">
            <MudIconButton Size="@Size.Small" 
                          Icon="Icons.Material.Outlined.Check" 
                          OnClick="@(() => mudTable.SetEditingItem(null))" />
            <MudIconButton Size="@Size.Small" 
                          Icon="Icons.Material.Outlined.Cancel" 
                          OnClick="@(() => mudTable.CancelEditingItem())" />
        </MudTd>
    </RowEditingTemplate>
</MudTable>

@code {
    private MudTable<Item> mudTable;
    private Item itemBeforeEdit;
    private bool canCancelEdit = false;
    private bool blockSwitch = false;
    private TableApplyButtonPosition applyButtonPosition = TableApplyButtonPosition.End;
    private TableEditButtonPosition editButtonPosition = TableEditButtonPosition.End;
    private TableEditTrigger editTrigger = TableEditTrigger.RowClick;
    
    private void BackupItem(object item)
    {
        itemBeforeEdit = new()
        {
            Name = ((Item)item).Name,
            Calories = ((Item)item).Calories,
            Fat = ((Item)item).Fat
        };
    }
    
    private void ResetItemToOriginalValues(object item)
    {
        ((Item)item).Name = itemBeforeEdit.Name;
        ((Item)item).Calories = itemBeforeEdit.Calories;
        ((Item)item).Fat = itemBeforeEdit.Fat;
    }
    
    private void ItemHasBeenCommitted(object item)
    {
        // Save changes
    }
}
```

### Custom Row Styling
```razor
<MudTable Items="@accounts" 
          RowClassFunc="@SelectedRowClassFunc"
          Hover="true">
    <HeaderContent>
        <MudTh>Account</MudTh>
        <MudTh>Balance</MudTh>
        <MudTh>Status</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Account">@context.Name</MudTd>
        <MudTd DataLabel="Balance">@context.Balance.ToString("C")</MudTd>
        <MudTd DataLabel="Status">@context.Status</MudTd>
    </RowTemplate>
</MudTable>

@code {
    private string SelectedRowClassFunc(Account account, int rowNumber)
    {
        if (account.Balance < 0)
            return "negative-balance";
        if (account.Status == "Active")
            return "active-account";
        return "";
    }
}

<style>
    .negative-balance {
        background-color: #ffebee !important;
    }
    .active-account {
        background-color: #e8f5e8 !important;
    }
</style>
```

## Best Practices

1. **Performance**: Use server-side loading for large datasets
2. **Responsive**: Use DataLabel for mobile-friendly tables
3. **Selection**: Provide clear visual feedback for selected rows
4. **Loading**: Show loading states during data operations
5. **Sorting**: Make sortable columns obvious with proper labels
6. **Actions**: Keep row actions minimal and contextual
7. **Accessibility**: Ensure proper keyboard navigation and screen reader support

## Related Components
- MudDataGrid
- MudSimpleTable
- MudPagination