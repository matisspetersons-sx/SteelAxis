# MudIcon Component Reference

## Overview
MudIcon displays Material Design icons with customizable size, color, and styling options.

## Basic Usage

```razor
<MudIcon Icon="@Icons.Material.Filled.Home" />
<MudIcon Icon="@Icons.Material.Filled.Settings" Color="Color.Primary" />
<MudIcon Icon="@Icons.Material.Filled.Favorite" Size="Size.Large" Color="Color.Error" />
```

## Common Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Icon` | `string` | `""` | The icon to display |
| `Title` | `string` | `""` | Icon title/tooltip |
| `Size` | `Size` | `Size.Medium` | Icon size: Small, Medium, Large |
| `Color` | `Color` | `Color.Inherit` | Icon color |
| `ViewBox` | `string` | `"0 0 24 24"` | SVG viewBox attribute |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

## Icon Categories

### Material Design Filled Icons
```razor
<!-- Common filled icons -->
<MudIcon Icon="@Icons.Material.Filled.Home" />
<MudIcon Icon="@Icons.Material.Filled.Settings" />
<MudIcon Icon="@Icons.Material.Filled.Person" />
<MudIcon Icon="@Icons.Material.Filled.Email" />
<MudIcon Icon="@Icons.Material.Filled.Phone" />
<MudIcon Icon="@Icons.Material.Filled.Search" />
<MudIcon Icon="@Icons.Material.Filled.Add" />
<MudIcon Icon="@Icons.Material.Filled.Edit" />
<MudIcon Icon="@Icons.Material.Filled.Delete" />
<MudIcon Icon="@Icons.Material.Filled.Save" />
```

### Material Design Outlined Icons
```razor
<!-- Common outlined icons -->
<MudIcon Icon="@Icons.Material.Outlined.Home" />
<MudIcon Icon="@Icons.Material.Outlined.Settings" />
<MudIcon Icon="@Icons.Material.Outlined.Person" />
<MudIcon Icon="@Icons.Material.Outlined.Email" />
<MudIcon Icon="@Icons.Material.Outlined.Phone" />
<MudIcon Icon="@Icons.Material.Outlined.Search" />
<MudIcon Icon="@Icons.Material.Outlined.Add" />
<MudIcon Icon="@Icons.Material.Outlined.Edit" />
<MudIcon Icon="@Icons.Material.Outlined.Delete" />
<MudIcon Icon="@Icons.Material.Outlined.Save" />
```

### Material Design Two Tone Icons
```razor
<!-- Common two tone icons -->
<MudIcon Icon="@Icons.Material.TwoTone.Home" />
<MudIcon Icon="@Icons.Material.TwoTone.Settings" />
<MudIcon Icon="@Icons.Material.TwoTone.Person" />
<MudIcon Icon="@Icons.Material.TwoTone.Email" />
<MudIcon Icon="@Icons.Material.TwoTone.Phone" />
```

### Material Design Sharp Icons
```razor
<!-- Common sharp icons -->
<MudIcon Icon="@Icons.Material.Sharp.Home" />
<MudIcon Icon="@Icons.Material.Sharp.Settings" />
<MudIcon Icon="@Icons.Material.Sharp.Person" />
```

### Material Design Round Icons
```razor
<!-- Common round icons -->
<MudIcon Icon="@Icons.Material.Rounded.Home" />
<MudIcon Icon="@Icons.Material.Rounded.Settings" />
<MudIcon Icon="@Icons.Material.Rounded.Person" />
```

## Examples

### Icon Sizes
```razor
<MudStack Row AlignItems="AlignItems.Center" Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Star" Size="Size.Small" />
    <MudText>Small</MudText>
</MudStack>

<MudStack Row AlignItems="AlignItems.Center" Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Star" Size="Size.Medium" />
    <MudText>Medium (Default)</MudText>
</MudStack>

<MudStack Row AlignItems="AlignItems.Center" Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Star" Size="Size.Large" />
    <MudText>Large</MudText>
</MudStack>
```

### Icon Colors
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Default" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Primary" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Secondary" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Success" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Error" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Warning" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Info" />
    <MudIcon Icon="@Icons.Material.Filled.Circle" Color="Color.Dark" />
</MudStack>
```

### Action Icons
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Add" Color="Color.Success" Title="Add" />
    <MudIcon Icon="@Icons.Material.Filled.Edit" Color="Color.Primary" Title="Edit" />
    <MudIcon Icon="@Icons.Material.Filled.Delete" Color="Color.Error" Title="Delete" />
    <MudIcon Icon="@Icons.Material.Filled.Save" Color="Color.Success" Title="Save" />
    <MudIcon Icon="@Icons.Material.Filled.Cancel" Color="Color.Default" Title="Cancel" />
    <MudIcon Icon="@Icons.Material.Filled.Refresh" Color="Color.Info" Title="Refresh" />
</MudStack>
```

### Navigation Icons
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Home" Title="Home" />
    <MudIcon Icon="@Icons.Material.Filled.Dashboard" Title="Dashboard" />
    <MudIcon Icon="@Icons.Material.Filled.Person" Title="Profile" />
    <MudIcon Icon="@Icons.Material.Filled.Settings" Title="Settings" />
    <MudIcon Icon="@Icons.Material.Filled.Help" Title="Help" />
    <MudIcon Icon="@Icons.Material.Filled.Logout" Title="Logout" />
</MudStack>
```

### Communication Icons
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Email" Color="Color.Primary" Title="Email" />
    <MudIcon Icon="@Icons.Material.Filled.Phone" Color="Color.Success" Title="Phone" />
    <MudIcon Icon="@Icons.Material.Filled.Message" Color="Color.Info" Title="Message" />
    <MudIcon Icon="@Icons.Material.Filled.Chat" Color="Color.Secondary" Title="Chat" />
    <MudIcon Icon="@Icons.Material.Filled.VideoCall" Color="Color.Primary" Title="Video Call" />
</MudStack>
```

### Status Icons
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" Title="Success" />
    <MudIcon Icon="@Icons.Material.Filled.Error" Color="Color.Error" Title="Error" />
    <MudIcon Icon="@Icons.Material.Filled.Warning" Color="Color.Warning" Title="Warning" />
    <MudIcon Icon="@Icons.Material.Filled.Info" Color="Color.Info" Title="Information" />
    <MudIcon Icon="@Icons.Material.Filled.Schedule" Color="Color.Default" Title="Pending" />
</MudStack>
```

### File and Document Icons
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Description" Title="Document" />
    <MudIcon Icon="@Icons.Material.Filled.PictureAsPdf" Color="Color.Error" Title="PDF" />
    <MudIcon Icon="@Icons.Material.Filled.Image" Color="Color.Success" Title="Image" />
    <MudIcon Icon="@Icons.Material.Filled.VideoFile" Color="Color.Primary" Title="Video" />
    <MudIcon Icon="@Icons.Material.Filled.AudioFile" Color="Color.Warning" Title="Audio" />
    <MudIcon Icon="@Icons.Material.Filled.Folder" Color="Color.Info" Title="Folder" />
</MudStack>
```

### E-commerce Icons
```razor
<MudStack Row Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.ShoppingCart" Color="Color.Primary" Title="Shopping Cart" />
    <MudIcon Icon="@Icons.Material.Filled.Store" Color="Color.Success" Title="Store" />
    <MudIcon Icon="@Icons.Material.Filled.Payment" Color="Color.Warning" Title="Payment" />
    <MudIcon Icon="@Icons.Material.Filled.LocalShipping" Color="Color.Info" Title="Shipping" />
    <MudIcon Icon="@Icons.Material.Filled.Inventory" Color="Color.Secondary" Title="Inventory" />
</MudStack>
```

### Custom Styled Icons
```razor
<MudIcon Icon="@Icons.Material.Filled.Star" 
         Style="color: gold; font-size: 2rem;" 
         Title="Golden Star" />

<MudIcon Icon="@Icons.Material.Filled.Favorite" 
         Class="pulsing-heart" 
         Color="Color.Error" />

<style>
    .pulsing-heart {
        animation: pulse 1s infinite;
    }
    
    @keyframes pulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.1); }
        100% { transform: scale(1); }
    }
</style>
```

### Icon Buttons
```razor
<MudStack Row Spacing="2">
    <MudIconButton Icon="@Icons.Material.Filled.ThumbUp" 
                   Color="Color.Primary"
                   Title="Like" />
    <MudIconButton Icon="@Icons.Material.Filled.Share" 
                   Color="Color.Secondary"
                   Title="Share" />
    <MudIconButton Icon="@Icons.Material.Filled.Bookmark" 
                   Color="Color.Warning"
                   Title="Bookmark" />
    <MudIconButton Icon="@Icons.Material.Filled.Download" 
                   Color="Color.Success"
                   Title="Download" />
</MudStack>
```

### Icons in Lists
```razor
<MudList>
    <MudListItem Icon="@Icons.Material.Filled.Inbox" Text="Inbox" />
    <MudListItem Icon="@Icons.Material.Filled.Send" Text="Sent" />
    <MudListItem Icon="@Icons.Material.Filled.Drafts" Text="Drafts" />
    <MudListItem Icon="@Icons.Material.Filled.Delete" Text="Trash" />
</MudList>
```

### Icons in Cards
```razor
<MudCard Style="max-width: 300px;">
    <MudCardHeader>
        <CardHeaderAvatar>
            <MudAvatar Color="Color.Primary">
                <MudIcon Icon="@Icons.Material.Filled.Person" />
            </MudAvatar>
        </CardHeaderAvatar>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">User Profile</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudIconButton Icon="@Icons.Material.Filled.MoreVert" />
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudText>User information and details...</MudText>
    </MudCardContent>
    <MudCardActions>
        <MudButton StartIcon="@Icons.Material.Filled.Edit" Color="Color.Primary">
            Edit
        </MudButton>
        <MudButton StartIcon="@Icons.Material.Filled.Share" Color="Color.Secondary">
            Share
        </MudButton>
    </MudCardActions>
</MudCard>
```

### Icon with Dynamic State
```razor
<MudIconButton Icon="@GetFavoriteIcon()" 
               Color="@GetFavoriteColor()" 
               OnClick="ToggleFavorite"
               Title="@(isFavorite ? "Remove from favorites" : "Add to favorites")" />

@code {
    private bool isFavorite = false;
    
    private string GetFavoriteIcon() => 
        isFavorite ? Icons.Material.Filled.Favorite : Icons.Material.Outlined.FavoriteBorder;
    
    private Color GetFavoriteColor() => 
        isFavorite ? Color.Error : Color.Default;
    
    private void ToggleFavorite()
    {
        isFavorite = !isFavorite;
    }
}
```

### Custom SVG Icons
```razor
<!-- Using custom SVG path -->
<MudIcon ViewBox="0 0 100 100" Style="width: 24px; height: 24px;">
    <path d="M50,10 L90,90 L10,90 Z" fill="currentColor"/>
</MudIcon>

<!-- Using external SVG -->
<MudIcon Icon="@customSvgPath" Color="Color.Primary" />

@code {
    private string customSvgPath = "<path d='M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z'/>";
}
```

## Common Icon Categories

### Navigation
- Home, Dashboard, Menu, ArrowBack, ArrowForward, Close, ExpandMore, ExpandLess

### Actions
- Add, Edit, Delete, Save, Cancel, Refresh, Search, Filter, Sort, Download, Upload

### Communication  
- Email, Phone, Message, Chat, VideoCall, Notifications, ContactMail

### Media
- Play, Pause, Stop, VolumeUp, VolumeOff, Image, VideoFile, AudioFile

### Status
- CheckCircle, Error, Warning, Info, Schedule, Done, Block

### Files
- Description, Folder, FolderOpen, AttachFile, PictureAsPdf, Image

## Best Practices

1. **Consistent sizing**: Use consistent icon sizes throughout your application
2. **Meaningful colors**: Use colors that convey meaning (red for delete, green for success)
3. **Accessibility**: Always provide meaningful titles for screen readers
4. **Performance**: Consider icon loading performance for large lists
5. **Context**: Choose icons that clearly represent their function
6. **Brand consistency**: Stick to one icon style (filled, outlined, etc.) per app
7. **Spacing**: Provide adequate spacing around icons in buttons and lists

## Related Components
- MudIconButton
- MudButton (with StartIcon/EndIcon)
- MudAvatar
- MudListItem