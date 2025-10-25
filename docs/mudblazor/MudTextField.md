# MudTextField Component Reference

## Overview
MudTextField is a versatile input component for text entry with built-in validation, labeling, and helper text support.

## Basic Usage

```razor
<MudTextField @bind-Value="textValue" Label="Enter text" />
<MudTextField @bind-Value="textValue" Label="Required Field" Required="true" />
<MudTextField @bind-Value="textValue" Label="With Helper Text" HelperText="This is helper text" />
```

## Common Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `string` | `""` | The current value of the text field |
| `Label` | `string` | `""` | The label text displayed above/in the field |
| `Placeholder` | `string` | `""` | Placeholder text when field is empty |
| `HelperText` | `string` | `""` | Helper text displayed below the field |
| `Required` | `bool` | `false` | Whether the field is required |
| `RequiredError` | `string` | `"Required"` | Error message for required validation |
| `Disabled` | `bool` | `false` | Whether the field is disabled |
| `ReadOnly` | `bool` | `false` | Whether the field is read-only |
| `Variant` | `Variant` | `Variant.Text` | Field style: Text, Filled, Outlined |
| `Margin` | `Margin` | `Margin.None` | Margin around the field |
| `Dense` | `bool` | `false` | Makes the field more compact |
| `Lines` | `int` | `1` | Number of lines (1 for single, >1 for multiline) |
| `MaxLines` | `int?` | `null` | Maximum number of lines for multiline |
| `AutoGrow` | `bool` | `false` | Auto-grow height for multiline |
| `Counter` | `int?` | `null` | Character counter limit |
| `MaxLength` | `int` | `524288` | Maximum character length |
| `Immediate` | `bool` | `false` | Update value immediately on input |
| `DebounceInterval` | `double` | `300` | Debounce interval in milliseconds |
| `OnBlur` | `EventCallback<FocusEventArgs>` | - | Blur event handler |
| `OnKeyDown` | `EventCallback<KeyboardEventArgs>` | - | Key down event handler |
| `Adornment` | `Adornment` | `Adornment.None` | Icon/text adornment position |
| `AdornmentIcon` | `string` | `""` | Icon for adornment |
| `AdornmentText` | `string` | `""` | Text for adornment |
| `AdornmentColor` | `Color` | `Color.Default` | Color of adornment |
| `IconSize` | `Size` | `Size.Medium` | Size of adornment icon |
| `OnAdornmentClick` | `EventCallback<MouseEventArgs>` | - | Adornment click handler |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

## Examples

### Basic Text Field
```razor
@code {
    private string textValue = "";
}

<MudTextField @bind-Value="textValue" 
              Label="First Name" 
              Variant="Variant.Outlined" />
```

### Required Field with Validation
```razor
<MudTextField @bind-Value="email" 
              Label="Email" 
              Required="true"
              RequiredError="Email is required!"
              Validation="@(new EmailAddressAttribute())" />

@code {
    private string email = "";
}
```

### Multiline Text Field
```razor
<MudTextField @bind-Value="description" 
              Label="Description" 
              Lines="5"
              MaxLines="10"
              AutoGrow="true"
              Variant="Variant.Outlined" />

@code {
    private string description = "";
}
```

### Text Field with Counter
```razor
<MudTextField @bind-Value="message" 
              Label="Message" 
              Counter="100"
              MaxLength="100"
              HelperText="Maximum 100 characters" />

@code {
    private string message = "";
}
```

### Text Field with Icon Adornment
```razor
<MudTextField @bind-Value="password" 
              Label="Password" 
              InputType="InputType.Password"
              Adornment="Adornment.End"
              AdornmentIcon="@Icons.Material.Filled.Visibility"
              OnAdornmentClick="TogglePasswordVisibility" />

@code {
    private string password = "";
    private bool showPassword = false;
    
    private void TogglePasswordVisibility()
    {
        showPassword = !showPassword;
    }
}
```

### Text Field with Text Adornment
```razor
<MudTextField @bind-Value="amount" 
              Label="Amount" 
              Adornment="Adornment.Start"
              AdornmentText="$"
              AdornmentColor="Color.Secondary" />

@code {
    private string amount = "";
}
```

### Dense and Immediate Update
```razor
<MudTextField @bind-Value="searchTerm" 
              Label="Search" 
              Dense="true"
              Immediate="true"
              Variant="Variant.Filled"
              OnKeyDown="HandleKeyDown" />

@code {
    private string searchTerm = "";
    
    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            // Handle search
        }
    }
}
```

### Disabled and ReadOnly States
```razor
<MudTextField Value="Disabled field" 
              Label="Disabled" 
              Disabled="true" />

<MudTextField Value="Read-only field" 
              Label="Read Only" 
              ReadOnly="true" />
```

### With Helper Text and Error State
```razor
<MudTextField @bind-Value="username" 
              Label="Username" 
              HelperText="Username must be at least 3 characters"
              Error="@hasError"
              ErrorText="Username is too short" />

@code {
    private string username = "";
    private bool hasError => username.Length > 0 && username.Length < 3;
}
```

## Input Types
```razor
<!-- Email -->
<MudTextField @bind-Value="email" 
              Label="Email" 
              InputType="InputType.Email" />

<!-- Password -->
<MudTextField @bind-Value="password" 
              Label="Password" 
              InputType="InputType.Password" />

<!-- Number -->
<MudTextField @bind-Value="phone" 
              Label="Phone" 
              InputType="InputType.Telephone" />

<!-- URL -->
<MudTextField @bind-Value="website" 
              Label="Website" 
              InputType="InputType.Url" />
```

## Form Integration
```razor
<MudForm>
    <MudTextField @bind-Value="firstName" 
                  Label="First Name" 
                  Required="true" />
    
    <MudTextField @bind-Value="lastName" 
                  Label="Last Name" 
                  Required="true" />
    
    <MudTextField @bind-Value="email" 
                  Label="Email" 
                  InputType="InputType.Email"
                  Required="true" />
</MudForm>
```

## Best Practices

1. **Always use labels**: Provide clear, descriptive labels for accessibility
2. **Helper text**: Use helper text to guide users on input format or requirements
3. **Validation**: Implement proper validation with meaningful error messages
4. **Immediate vs Debounced**: Use `Immediate="true"` for search, default debouncing for forms
5. **Required fields**: Mark required fields clearly with `Required="true"`
6. **Consistent variants**: Use consistent field variants throughout your application

## Related Components
- MudNumericField
- MudSelect
- MudAutocomplete
- MudForm