# MudCard Component Reference

## Overview
MudCard is a flexible container component that displays content in a Material Design card layout with optional header, content, media, and action areas.

## Basic Usage

```razor
<MudCard>
    <MudCardContent>
        <MudText Typo="Typo.h5" GutterBottom="true">Card Title</MudText>
        <MudText Typo="Typo.body2">This is the card content area.</MudText>
    </MudCardContent>
    <MudCardActions>
        <MudButton Size="Size.Small">Learn More</MudButton>
        <MudButton Size="Size.Small" Color="Color.Primary">Action</MudButton>
    </MudCardActions>
</MudCard>
```

## Card Components

### MudCard Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Elevation` | `int` | `1` | Shadow depth (0-25) |
| `Square` | `bool` | `false` | Remove border radius |
| `Outlined` | `bool` | `false` | Outlined variant instead of elevated |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

### MudCardHeader Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Avatar` | `RenderFragment` | `null` | Avatar content (typically MudAvatar) |
| `CardHeaderContent` | `RenderFragment` | `null` | Main header content |
| `CardHeaderActions` | `RenderFragment` | `null` | Action buttons in header |

### MudCardMedia Parameters  
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Image` | `string` | `""` | Image URL |
| `Title` | `string` | `""` | Image alt text |
| `Height` | `int` | `300` | Media height in pixels |

### MudCardContent Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment` | `null` | Card content |
| `Class` | `string` | `""` | Additional CSS classes |

### MudCardActions Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment` | `null` | Action buttons |
| `Class` | `string` | `""` | Additional CSS classes |

## Examples

### Simple Card
```razor
<MudCard Elevation="3">
    <MudCardContent>
        <MudText Typo="Typo.h5">Simple Card</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">
            This is a simple card with just content.
        </MudText>
    </MudCardContent>
</MudCard>
```

### Card with Header and Avatar
```razor
<MudCard>
    <MudCardHeader>
        <Avatar>
            <MudAvatar Color="Color.Primary">JD</MudAvatar>
        </Avatar>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">John Doe</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">
                @DateTime.Now.ToString("MMMM dd, yyyy")
            </MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudIconButton Icon="@Icons.Material.Filled.MoreVert" 
                          Color="Color.Default" />
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudText Typo="Typo.body2">
            This card has a header with avatar and actions.
        </MudText>
    </MudCardContent>
</MudCard>
```

### Card with Media
```razor
<MudCard Style="max-width: 400px;">
    <MudCardMedia Image="https://via.placeholder.com/400x200" 
                  Height="200" 
                  Title="Sample Image" />
    <MudCardContent>
        <MudText Typo="Typo.h5" GutterBottom="true">
            Beautiful Landscape
        </MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">
            This is a beautiful landscape photo that showcases nature's beauty.
        </MudText>
    </MudCardContent>
    <MudCardActions>
        <MudButton Size="Size.Small" Color="Color.Primary">Share</MudButton>
        <MudButton Size="Size.Small">Learn More</MudButton>
    </MudCardActions>
</MudCard>
```

### Product Card
```razor
<MudCard Style="max-width: 350px;">
    <MudCardMedia Image="@product.ImageUrl" Height="200" />
    <MudCardContent>
        <MudText Typo="Typo.h6" GutterBottom="true">@product.Name</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">
            @product.Description
        </MudText>
        <MudText Typo="Typo.h5" Color="Color.Primary" Style="margin-top: 16px;">
            @product.Price.ToString("C")
        </MudText>
    </MudCardContent>
    <MudCardActions>
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   FullWidth="true"
                   StartIcon="@Icons.Material.Filled.ShoppingCart">
            Add to Cart
        </MudButton>
    </MudCardActions>
</MudCard>
```

### User Profile Card
```razor
<MudCard Style="max-width: 400px;">
    <MudCardHeader>
        <Avatar>
            <MudAvatar Size="Size.Large" 
                      Image="@user.AvatarUrl" 
                      Alt="@user.Name" />
        </Avatar>
        <CardHeaderContent>
            <MudText Typo="Typo.h5">@user.Name</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">
                @user.Title
            </MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudText Typo="Typo.body2">@user.Bio</MudText>
        <MudDivider Style="margin: 16px 0;" />
        <MudStack Row Spacing="4">
            <MudStack AlignItems="AlignItems.Center">
                <MudText Typo="Typo.h6">@user.PostsCount</MudText>
                <MudText Typo="Typo.caption" Color="Color.Secondary">Posts</MudText>
            </MudStack>
            <MudStack AlignItems="AlignItems.Center">
                <MudText Typo="Typo.h6">@user.FollowersCount</MudText>
                <MudText Typo="Typo.caption" Color="Color.Secondary">Followers</MudText>
            </MudStack>
            <MudStack AlignItems="AlignItems.Center">
                <MudText Typo="Typo.h6">@user.FollowingCount</MudText>
                <MudText Typo="Typo.caption" Color="Color.Secondary">Following</MudText>
            </MudStack>
        </MudStack>
    </MudCardContent>
    <MudCardActions>
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   FullWidth="true">
            Follow
        </MudButton>
    </MudCardActions>
</MudCard>
```

### Outlined Card
```razor
<MudCard Outlined="true" Elevation="0">
    <MudCardContent>
        <MudText Typo="Typo.h6">Outlined Card</MudText>
        <MudText Typo="Typo.body2">
            This card uses outline style instead of elevation.
        </MudText>
    </MudCardContent>
</MudCard>
```

### Card with Custom Actions
```razor
<MudCard>
    <MudCardContent>
        <MudText Typo="Typo.h6">Article Title</MudText>
        <MudText Typo="Typo.body2" Style="margin-top: 8px;">
            This is a preview of the article content...
        </MudText>
    </MudCardContent>
    <MudCardActions Style="justify-content: space-between;">
        <div>
            <MudIconButton Icon="@Icons.Material.Filled.Favorite" 
                          Color="@(isLiked ? Color.Error : Color.Default)"
                          OnClick="ToggleLike" />
            <MudIconButton Icon="@Icons.Material.Filled.Share" 
                          Color="Color.Default" />
        </div>
        <MudButton Size="Size.Small" Color="Color.Primary">
            Read More
        </MudButton>
    </MudCardActions>
</MudCard>

@code {
    private bool isLiked = false;
    
    private void ToggleLike()
    {
        isLiked = !isLiked;
    }
}
```

### Clickable Card
```razor
<MudCard Style="cursor: pointer; transition: all 0.3s ease;"
         @onclick="HandleCardClick"
         Class="hover-card">
    <MudCardContent>
        <MudText Typo="Typo.h6">Clickable Card</MudText>
        <MudText Typo="Typo.body2">
            Click anywhere on this card to trigger an action.
        </MudText>
    </MudCardContent>
</MudCard>

<style>
    .hover-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 20px rgba(0,0,0,0.12);
    }
</style>

@code {
    private void HandleCardClick()
    {
        // Handle card click
    }
}
```

### Card Grid Layout
```razor
<MudGrid>
    @foreach (var item in items)
    {
        <MudItem xs="12" sm="6" md="4">
            <MudCard Style="height: 100%;">
                <MudCardMedia Image="@item.ImageUrl" Height="140" />
                <MudCardContent Style="flex-grow: 1;">
                    <MudText Typo="Typo.h6">@item.Title</MudText>
                    <MudText Typo="Typo.body2" Color="Color.Secondary">
                        @item.Description
                    </MudText>
                </MudCardContent>
                <MudCardActions>
                    <MudButton Size="Size.Small" Color="Color.Primary">
                        View Details
                    </MudButton>
                </MudCardActions>
            </MudCard>
        </MudItem>
    }
</MudGrid>
```

### Card with Loading State
```razor
<MudCard>
    @if (loading)
    {
        <MudCardContent>
            <MudSkeleton Height="40px" />
            <MudSkeleton Height="20px" Style="margin-top: 8px;" />
            <MudSkeleton Height="20px" Width="60%" Style="margin-top: 4px;" />
        </MudCardContent>
    }
    else
    {
        <MudCardContent>
            <MudText Typo="Typo.h6">@data.Title</MudText>
            <MudText Typo="Typo.body2">@data.Content</MudText>
        </MudCardContent>
    }
</MudCard>
```

## Styling Examples

### Custom Elevation
```razor
<MudCard Elevation="8">
    <MudCardContent>
        <MudText>High elevation card</MudText>
    </MudCardContent>
</MudCard>
```

### Square Card
```razor
<MudCard Square="true">
    <MudCardContent>
        <MudText>Square corners card</MudText>
    </MudCardContent>
</MudCard>
```

### Custom Styled Card
```razor
<MudCard Class="custom-card" Style="background: linear-gradient(45deg, #FE6B8B 30%, #FF8E53 90%);">
    <MudCardContent>
        <MudText Typo="Typo.h6" Style="color: white;">
            Custom Styled Card
        </MudText>
    </MudCardContent>
</MudCard>
```

## Best Practices

1. **Content hierarchy**: Use appropriate typography hierarchy in card content
2. **Action placement**: Place primary actions in CardActions, secondary in header
3. **Consistent elevation**: Use consistent elevation levels throughout your app
4. **Responsive design**: Use MudGrid for responsive card layouts
5. **Loading states**: Show skeleton loading while data is being fetched
6. **Accessibility**: Ensure clickable cards have proper keyboard support
7. **Performance**: Avoid complex content in large lists of cards

## Related Components
- MudPaper
- MudAvatar
- MudButton
- MudGrid