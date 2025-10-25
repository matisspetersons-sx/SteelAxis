# MudButton Component Reference

## Overview
MudButton is a Material Design button component that provides various styles, sizes, and behaviors for user interactions.

## Basic Usage

```razor
<MudButton>Default Button</MudButton>
<MudButton Variant="Variant.Filled" Color="Color.Primary">Filled Button</MudButton>
<MudButton Variant="Variant.Outlined" Color="Color.Secondary">Outlined Button</MudButton>
<MudButton Variant="Variant.Text" Color="Color.Success">Text Button</MudButton>
```

## Common Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Variant` | `Variant` | `Variant.Text` | Button style: Text, Filled, Outlined |
| `Color` | `Color` | `Color.Default` | Button color: Primary, Secondary, Success, Error, Warning, Info, Dark |
| `Size` | `Size` | `Size.Medium` | Button size: Small, Medium, Large |
| `Disabled` | `bool` | `false` | Whether the button is disabled |
| `DisableElevation` | `bool` | `false` | Disables button shadow/elevation |
| `FullWidth` | `bool` | `false` | Makes button take full container width |
| `StartIcon` | `string` | `null` | Icon displayed at the start of button |
| `EndIcon` | `string` | `null` | Icon displayed at the end of button |
| `IconColor` | `Color` | `Color.Inherit` | Color of the icon |
| `IconSize` | `Size` | `Size.Medium` | Size of the icon |
| `Href` | `string` | `null` | Makes button behave as link |
| `Target` | `string` | `null` | Link target (_blank, _self, etc.) |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | Click event handler |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

## Examples

### Button Variants
```razor
<MudButton Variant="Variant.Text" Color="Color.Primary">
    Text Button
</MudButton>

<MudButton Variant="Variant.Filled" Color="Color.Primary">
    Filled Button
</MudButton>

<MudButton Variant="Variant.Outlined" Color="Color.Primary">
    Outlined Button
</MudButton>
```

### Button with Icons
```razor
<MudButton StartIcon="@Icons.Material.Filled.Add" 
           Variant="Variant.Filled" 
           Color="Color.Primary">
    Add Item
</MudButton>

<MudButton EndIcon="@Icons.Material.Filled.Send" 
           Variant="Variant.Outlined" 
           Color="Color.Secondary">
    Send Message
</MudButton>
```

### Button Sizes
```razor
<MudButton Size="Size.Small" Variant="Variant.Filled">Small</MudButton>
<MudButton Size="Size.Medium" Variant="Variant.Filled">Medium</MudButton>
<MudButton Size="Size.Large" Variant="Variant.Filled">Large</MudButton>
```

### Disabled Button
```razor
<MudButton Disabled="true" Variant="Variant.Filled" Color="Color.Primary">
    Disabled Button
</MudButton>
```

### Full Width Button
```razor
<MudButton FullWidth="true" Variant="Variant.Filled" Color="Color.Primary">
    Full Width Button
</MudButton>
```

### Button as Link
```razor
<MudButton Href="https://example.com" 
           Target="_blank" 
           Variant="Variant.Outlined" 
           Color="Color.Info">
    External Link
</MudButton>
```

### Button with Click Handler
```razor
<MudButton OnClick="HandleClick" 
           Variant="Variant.Filled" 
           Color="Color.Success">
    Click Me
</MudButton>

@code {
    private void HandleClick()
    {
        // Handle button click
    }
}
```

## Styling
```razor
<MudButton Class="my-custom-button" 
           Style="border-radius: 20px;" 
           Variant="Variant.Filled" 
           Color="Color.Primary">
    Custom Styled Button
</MudButton>
```

## Best Practices

1. **Use appropriate variants**: 
   - `Filled` for primary actions
   - `Outlined` for secondary actions
   - `Text` for low-emphasis actions

2. **Consider accessibility**: Always provide meaningful text content

3. **Icon usage**: Use icons to enhance understanding, not replace text

4. **Loading states**: Disable buttons during async operations

5. **Consistent sizing**: Use consistent button sizes throughout your application

## Related Components
- MudIconButton
- MudButtonGroup
- MudFab
- MudToggleIconButton