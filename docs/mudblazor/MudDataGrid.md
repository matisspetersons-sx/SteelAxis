# MudDataGrid Component Reference

## Overview
MudDataGrid is a powerful data display component with features like sorting, filtering, grouping, editing, and pagination for displaying tabular data.

## Basic Usage

```razor
<MudDataGrid T="Person" Items="@people">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Full Name" />
        <PropertyColumn Property="x => x.Age" />
        <PropertyColumn Property="x => x.Email" />
    </Columns>
</MudDataGrid>

@code {
    private List<Person> people = new();
}
```

## Common Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | `null` | The data source for the grid |
| `ServerData` | `Func<GridState<T>, Task<GridData<T>>>` | `null` | Server-side data loading function |
| `Dense` | `bool` | `false` | Makes rows more compact |
| `Hover` | `bool` | `true` | Enables row hover effects |
| `Striped` | `bool` | `false` | Alternating row colors |
| `Bordered` | `bool` | `false` | Shows borders around cells |
| `ReadOnly` | `bool` | `false` | Disables editing functionality |
| `EditMode` | `DataGridEditMode` | `None` | Edit mode: None, Form, Cell |
| `EditTrigger` | `DataGridEditTrigger` | `DoubleClick` | What triggers edit mode |
| `CommitEditTooltip` | `string` | `"Commit Edit"` | Tooltip for commit button |
| `CancelEditTooltip` | `string` | `"Cancel Edit"` | Tooltip for cancel button |
| `Height` | `string` | `null` | Fixed height for the grid |
| `FixedHeader` | `bool` | `false` | Keep header visible when scrolling |
| `FixedFooter` | `bool` | `false` | Keep footer visible when scrolling |
| `ColumnResizeMode` | `ResizeMode` | `None` | Column resizing: None, Container, Column |
| `Filterable` | `bool` | `true` | Enable column filtering |
| `FilterMode` | `DataGridFilterMode` | `Simple` | Filter UI mode |
| `SortMode` | `SortMode` | `Multiple` | Sorting behavior |
| `Groupable` | `bool` | `false` | Enable column grouping |
| `GroupExpanded` | `bool` | `false` | Default group expansion state |
| `MultiSelection` | `bool` | `false` | Enable multi-row selection |
| `SelectOnRowClick` | `bool` | `true` | Select row on click |
| `SelectedItems` | `HashSet<T>` | `new()` | Currently selected items |
| `RowClick` | `EventCallback<DataGridRowClickEventArgs<T>>` | - | Row click handler |
| `SelectedItemChanged` | `EventCallback<T>` | - | Selection change handler |
| `Class` | `string` | `""` | Additional CSS classes |

## Column Types

### PropertyColumn
```razor
<PropertyColumn Property="x => x.Name" 
                Title="Full Name" 
                Sortable="true" 
                Filterable="true" />

<PropertyColumn Property="x => x.Age" 
                Title="Age"
                HeaderStyle="text-align: center"
                CellStyle="text-align: center" />
```

### TemplateColumn
```razor
<TemplateColumn Title="Actions" Sortable="false" Filterable="false">
    <CellTemplate>
        <MudButton Size="Size.Small" 
                   Variant="Variant.Filled" 
                   Color="Color.Primary"
                   OnClick="@(() => EditItem(context.Item))">
            Edit
        </MudButton>
        <MudButton Size="Size.Small" 
                   Variant="Variant.Filled" 
                   Color="Color.Error"
                   OnClick="@(() => DeleteItem(context.Item))">
            Delete
        </MudButton>
    </CellTemplate>
</TemplateColumn>
```

### HierarchyColumn
```razor
<HierarchyColumn T="Person" />
<PropertyColumn Property="x => x.Name" />

<ChildRowContent>
    <MudCard>
        <MudCardContent>
            <div>Additional details for @context.Name</div>
        </MudCardContent>
    </MudCard>
</ChildRowContent>
```

## Examples

### Basic Grid with Sorting and Filtering
```razor
<MudDataGrid T="Product" Items="@products" 
             Filterable="true" 
             SortMode="SortMode.Multiple"
             Dense="true"
             Hover="true">
    <Columns>
        <PropertyColumn Property="x => x.Id" Title="ID" />
        <PropertyColumn Property="x => x.Name" Title="Product Name" />
        <PropertyColumn Property="x => x.Price" Title="Price" Format="C2" />
        <PropertyColumn Property="x => x.Category" Title="Category" />
        <PropertyColumn Property="x => x.InStock" Title="In Stock">
            <CellTemplate>
                <MudCheckBox @bind-Checked="context.Item.InStock" ReadOnly="true" />
            </CellTemplate>
        </PropertyColumn>
    </Columns>
</MudDataGrid>
```

### Editable Grid
```razor
<MudDataGrid @ref="dataGrid" 
             T="Product" 
             Items="@products"
             EditMode="DataGridEditMode.Cell"
             EditTrigger="DataGridEditTrigger.OnClick"
             ReadOnly="false"
             CommittedItemChanges="@OnCommittedItemChanges">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Product Name" />
        <PropertyColumn Property="x => x.Price" Title="Price" />
        <PropertyColumn Property="x => x.Category" Title="Category">
            <EditTemplate>
                <MudSelect @bind-Value="context.Item.Category" T="string">
                    <MudSelectItem Value="Electronics">Electronics</MudSelectItem>
                    <MudSelectItem Value="Clothing">Clothing</MudSelectItem>
                    <MudSelectItem Value="Books">Books</MudSelectItem>
                </MudSelect>
            </EditTemplate>
        </PropertyColumn>
    </Columns>
</MudDataGrid>

@code {
    private MudDataGrid<Product> dataGrid;
    
    private void OnCommittedItemChanges(Product item)
    {
        // Handle item changes
        Console.WriteLine($"Item {item.Name} was updated");
    }
}
```

### Server-Side Data Loading
```razor
<MudDataGrid T="User" 
             ServerData="@(new Func<GridState<User>, Task<GridData<User>>>(LoadServerData))"
             Filterable="true" 
             SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.Id" Title="ID" />
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <PropertyColumn Property="x => x.Email" Title="Email" />
    </Columns>
    <PagerContent>
        <MudDataGridPager T="User" />
    </PagerContent>
</MudDataGrid>

@code {
    private async Task<GridData<User>> LoadServerData(GridState<User> state)
    {
        // Implement server-side loading logic
        var data = await userService.GetUsersAsync(
            state.Page, 
            state.PageSize, 
            state.SortDefinitions, 
            state.FilterDefinitions);
            
        return new GridData<User>
        {
            Items = data.Items,
            TotalItems = data.TotalCount
        };
    }
}
```

### Grid with Selection
```razor
<MudDataGrid T="Customer" 
             Items="@customers"
             MultiSelection="true"
             @bind-SelectedItems="selectedCustomers"
             SelectOnRowClick="true">
    <Columns>
        <SelectColumn T="Customer" />
        <PropertyColumn Property="x => x.Id" Title="ID" />
        <PropertyColumn Property="x => x.Name" Title="Customer Name" />
        <PropertyColumn Property="x => x.Email" Title="Email" />
    </Columns>
</MudDataGrid>

<MudText>Selected: @selectedCustomers.Count customers</MudText>

@code {
    private HashSet<Customer> selectedCustomers = new();
}
```

### Grouped Grid
```razor
<MudDataGrid T="Employee" 
             Items="@employees"
             Groupable="true"
             GroupExpanded="true">
    <Columns>
        <PropertyColumn Property="x => x.Department" Title="Department" Grouping />
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <PropertyColumn Property="x => x.Position" Title="Position" />
        <PropertyColumn Property="x => x.Salary" Title="Salary" Format="C0" />
    </Columns>
</MudDataGrid>
```

### Custom Footer
```razor
<MudDataGrid T="Sale" Items="@sales">
    <Columns>
        <PropertyColumn Property="x => x.Date" Title="Date" />
        <PropertyColumn Property="x => x.Product" Title="Product" />
        <PropertyColumn Property="x => x.Amount" Title="Amount" Format="C2" />
    </Columns>
    <FooterTemplate>
        <MudTh>Total</MudTh>
        <MudTh></MudTh>
        <MudTh Style="text-align:right">
            @sales.Sum(x => x.Amount).ToString("C2")
        </MudTh>
    </FooterTemplate>
</MudDataGrid>
```

## Advanced Features

### Custom Filtering
```razor
<PropertyColumn Property="x => x.Price" Title="Price">
    <FilterTemplate>
        <MudStack Row>
            <MudNumericField @bind-Value="priceFrom" 
                           Label="From" 
                           HideSpinButtons="true" />
            <MudNumericField @bind-Value="priceTo" 
                           Label="To" 
                           HideSpinButtons="true" />
        </MudStack>
    </FilterTemplate>
</PropertyColumn>

@code {
    private decimal? priceFrom;
    private decimal? priceTo;
}
```

### Loading State
```razor
<MudDataGrid T="User" 
             ServerData="LoadServerData"
             Loading="@loading">
    <LoadingContent>
        <MudText>Loading users...</MudText>
    </LoadingContent>
    <Columns>
        <PropertyColumn Property="x => x.Name" />
        <PropertyColumn Property="x => x.Email" />
    </Columns>
</MudDataGrid>

@code {
    private bool loading = false;
}
```

## Best Practices

1. **Performance**: Use server-side data loading for large datasets
2. **Columns**: Keep column count reasonable for mobile responsiveness
3. **Loading**: Show loading states for better user experience
4. **Selection**: Provide clear feedback for selected items
5. **Editing**: Use appropriate edit triggers and validation
6. **Accessibility**: Ensure proper keyboard navigation and screen reader support

## Related Components
- MudTable
- MudSimpleTable
- MudPagination