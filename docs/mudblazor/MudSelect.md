# MudSelect Component Reference

## Overview
MudSelect provides a dropdown selection component with single or multiple selection capabilities, search functionality, and custom item templates.

## Basic Usage

```razor
<MudSelect T="string" @bind-Value="selectedValue" Label="Choose Option">
    <MudSelectItem Value="Option1">Option 1</MudSelectItem>
    <MudSelectItem Value="Option2">Option 2</MudSelectItem>
    <MudSelectItem Value="Option3">Option 3</MudSelectItem>
</MudSelect>

@code {
    private string selectedValue;
}
```

## Common Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `T` | `default(T)` | The selected value |
| `MultiSelection` | `bool` | `false` | Enable multiple selection |
| `SelectedValues` | `IEnumerable<T>` | `null` | Selected values for multi-selection |
| `Label` | `string` | `""` | Label text |
| `Placeholder` | `string` | `""` | Placeholder text |
| `HelperText` | `string` | `""` | Helper text below the field |
| `Required` | `bool` | `false` | Whether selection is required |
| `RequiredError` | `string` | `"Required"` | Error message for required validation |
| `Disabled` | `bool` | `false` | Whether the select is disabled |
| `ReadOnly` | `bool` | `false` | Whether the select is read-only |
| `Variant` | `Variant` | `Variant.Text` | Field style: Text, Filled, Outlined |
| `Margin` | `Margin` | `Margin.None` | Margin around the field |
| `Dense` | `bool` | `false` | More compact appearance |
| `OpenIcon` | `string` | `Icons.Material.Filled.ArrowDropDown` | Icon when closed |
| `CloseIcon` | `string` | `Icons.Material.Filled.ArrowDropUp` | Icon when open |
| `Adornment` | `Adornment` | `Adornment.None` | Icon/text adornment position |
| `AdornmentIcon` | `string` | `""` | Adornment icon |
| `AdornmentColor` | `Color` | `Color.Default` | Adornment color |
| `MaxHeight` | `int` | `300` | Maximum dropdown height |
| `AnchorOrigin` | `Origin` | `Origin.TopCenter` | Dropdown anchor position |
| `TransformOrigin` | `Origin` | `Origin.TopCenter` | Dropdown transform origin |
| `Strict` | `bool` | `true` | Whether to enforce selection from available options |
| `SearchBox` | `bool` | `false` | Show search box in dropdown |
| `SearchFunc` | `Func<T, string, bool>` | `null` | Custom search function |
| `ToStringFunc` | `Func<T, string>` | `null` | Function to convert items to string |
| `OnOpen` | `EventCallback` | - | Dropdown open event |
| `OnClose` | `EventCallback` | - | Dropdown close event |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

## Examples

### Basic Single Selection
```razor
<MudSelect T="string" 
           @bind-Value="selectedCategory" 
           Label="Category" 
           Variant="Variant.Outlined">
    <MudSelectItem Value="Electronics">Electronics</MudSelectItem>
    <MudSelectItem Value="Clothing">Clothing</MudSelectItem>
    <MudSelectItem Value="Books">Books</MudSelectItem>
    <MudSelectItem Value="Sports">Sports</MudSelectItem>
</MudSelect>

@code {
    private string selectedCategory;
}
```

### Multiple Selection
```razor
<MudSelect T="string" 
           @bind-SelectedValues="selectedTags" 
           MultiSelection="true"
           Label="Tags" 
           Variant="Variant.Filled">
    <MudSelectItem Value="Important">Important</MudSelectItem>
    <MudSelectItem Value="Urgent">Urgent</MudSelectItem>
    <MudSelectItem Value="Work">Work</MudSelectItem>
    <MudSelectItem Value="Personal">Personal</MudSelectItem>
</MudSelect>

<MudText>Selected: @string.Join(", ", selectedTags)</MudText>

@code {
    private IEnumerable<string> selectedTags = new HashSet<string>();
}
```

### Select with Custom Objects
```razor
<MudSelect T="User" 
           @bind-Value="selectedUser" 
           Label="Select User"
           ToStringFunc="u => u?.Name">
    @foreach (var user in users)
    {
        <MudSelectItem Value="user">@user.Name (@user.Email)</MudSelectItem>
    }
</MudSelect>

@code {
    private User selectedUser;
    private List<User> users = new()
    {
        new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
        new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" },
        new User { Id = 3, Name = "Bob Johnson", Email = "bob@example.com" }
    };
}
```

### Select with Search
```razor
<MudSelect T="Country" 
           @bind-Value="selectedCountry" 
           Label="Country"
           SearchBox="true"
           SearchFunc="SearchCountries"
           ToStringFunc="c => c?.Name">
    @foreach (var country in countries)
    {
        <MudSelectItem Value="country">
            <div style="display: flex; align-items: center;">
                <MudIcon Icon="@country.FlagIcon" Style="margin-right: 8px;" />
                @country.Name
            </div>
        </MudSelectItem>
    }
</MudSelect>

@code {
    private Country selectedCountry;
    private List<Country> countries = new();
    
    private bool SearchCountries(Country country, string searchString)
    {
        return string.IsNullOrWhiteSpace(searchString) || 
               country.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}
```

### Required Select with Validation
```razor
<MudForm>
    <MudSelect T="string" 
               @bind-Value="selectedPriority" 
               Label="Priority" 
               Required="true"
               RequiredError="Please select a priority"
               Variant="Variant.Outlined">
        <MudSelectItem Value="Low">
            <div style="display: flex; align-items: center;">
                <MudIcon Icon="@Icons.Material.Filled.LowPriority" Color="Color.Success" />
                <span style="margin-left: 8px;">Low</span>
            </div>
        </MudSelectItem>
        <MudSelectItem Value="Medium">
            <div style="display: flex; align-items: center;">
                <MudIcon Icon="@Icons.Material.Filled.PriorityHigh" Color="Color.Warning" />
                <span style="margin-left: 8px;">Medium</span>
            </div>
        </MudSelectItem>
        <MudSelectItem Value="High">
            <div style="display: flex; align-items: center;">
                <MudIcon Icon="@Icons.Material.Filled.Report" Color="Color.Error" />
                <span style="margin-left: 8px;">High</span>
            </div>
        </MudSelectItem>
    </MudSelect>
</MudForm>

@code {
    private string selectedPriority;
}
```

### Select with Custom Item Templates
```razor
<MudSelect T="Product" 
           @bind-Value="selectedProduct" 
           Label="Product"
           ToStringFunc="p => p?.Name">
    @foreach (var product in products)
    {
        <MudSelectItem Value="product">
            <div style="display: flex; justify-content: space-between; align-items: center; width: 100%;">
                <div>
                    <MudText Typo="Typo.body1">@product.Name</MudText>
                    <MudText Typo="Typo.caption" Color="Color.Secondary">@product.Category</MudText>
                </div>
                <MudChip Size="Size.Small" Color="Color.Primary">@product.Price.ToString("C")</MudChip>
            </div>
        </MudSelectItem>
    }
</MudSelect>

@code {
    private Product selectedProduct;
    private List<Product> products = new();
}
```

### Disabled and ReadOnly States
```razor
<MudSelect T="string" 
           Value="Selected Option" 
           Label="Disabled Select" 
           Disabled="true">
    <MudSelectItem Value="Option1">Option 1</MudSelectItem>
    <MudSelectItem Value="Option2">Option 2</MudSelectItem>
</MudSelect>

<MudSelect T="string" 
           Value="Selected Option" 
           Label="ReadOnly Select" 
           ReadOnly="true">
    <MudSelectItem Value="Option1">Option 1</MudSelectItem>
    <MudSelectItem Value="Option2">Option 2</MudSelectItem>
</MudSelect>
```

### Select with Adornment
```razor
<MudSelect T="string" 
           @bind-Value="selectedStatus" 
           Label="Status"
           Adornment="Adornment.Start"
           AdornmentIcon="@Icons.Material.Filled.Circle"
           AdornmentColor="GetStatusColor(selectedStatus)">
    <MudSelectItem Value="Active">Active</MudSelectItem>
    <MudSelectItem Value="Inactive">Inactive</MudSelectItem>
    <MudSelectItem Value="Pending">Pending</MudSelectItem>
</MudSelect>

@code {
    private string selectedStatus;
    
    private Color GetStatusColor(string status) => status switch
    {
        "Active" => Color.Success,
        "Inactive" => Color.Error,
        "Pending" => Color.Warning,
        _ => Color.Default
    };
}
```

### Programmatic Control
```razor
<MudSelect @ref="selectRef" 
           T="string" 
           @bind-Value="selectedValue" 
           Label="Controlled Select">
    <MudSelectItem Value="A">Option A</MudSelectItem>
    <MudSelectItem Value="B">Option B</MudSelectItem>
    <MudSelectItem Value="C">Option C</MudSelectItem>
</MudSelect>

<MudButtonGroup>
    <MudButton OnClick="OpenSelect">Open</MudButton>
    <MudButton OnClick="CloseSelect">Close</MudButton>
    <MudButton OnClick="ClearSelection">Clear</MudButton>
</MudButtonGroup>

@code {
    private MudSelect<string> selectRef;
    private string selectedValue;
    
    private async Task OpenSelect() => await selectRef.OpenMenuAsync();
    private async Task CloseSelect() => await selectRef.CloseMenuAsync();
    private void ClearSelection() => selectedValue = null;
}
```

## Best Practices

1. **Use ToStringFunc**: Always provide ToStringFunc for custom objects
2. **Search functionality**: Enable search for lists with many options
3. **Meaningful labels**: Provide clear, descriptive labels
4. **Validation**: Use Required and custom validation for important selections
5. **Custom templates**: Use item templates to provide rich selection experiences
6. **Performance**: Consider virtualization for very large lists
7. **Accessibility**: Ensure keyboard navigation works properly

## Related Components
- MudAutocomplete
- MudTextField
- MudCheckBox
- MudRadioGroup