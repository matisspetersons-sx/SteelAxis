# MudNavMenu Component Reference

## Overview
MudNavMenu provides navigation menu functionality with collapsible groups, icons, and nested navigation items.

## Basic Usage

```razor
<MudNavMenu>
    <MudNavLink Href="/" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Home">
        Home
    </MudNavLink>
    <MudNavLink Href="/about" Icon="@Icons.Material.Filled.Info">
        About
    </MudNavLink>
    <MudNavLink Href="/contact" Icon="@Icons.Material.Filled.ContactMail">
        Contact
    </MudNavLink>
</MudNavMenu>
```

## Common Parameters

### MudNavMenu Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Color` | `Color` | `Color.Default` | Menu text color |
| `Bordered` | `bool` | `false` | Show borders around menu |
| `Dense` | `bool` | `false` | Compact menu appearance |
| `Rounded` | `bool` | `false` | Rounded menu corners |
| `Margin` | `Margin` | `Margin.None` | Margin around menu |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

### MudNavLink Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Href` | `string` | `""` | Navigation URL |
| `Icon` | `string` | `""` | Icon to display |
| `IconColor` | `Color` | `Color.Inherit` | Icon color |
| `Match` | `NavLinkMatch` | `Prefix` | URL matching behavior |
| `Target` | `string` | `""` | Link target |
| `ForceLoad` | `bool` | `false` | Force page reload |
| `Disabled` | `bool` | `false` | Disable the link |
| `Class` | `string` | `""` | Additional CSS classes |
| `ActiveClass` | `string` | `"mud-nav-link-active"` | CSS class when active |

### MudNavGroup Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string` | `""` | Group title |
| `Icon` | `string` | `""` | Group icon |
| `IconColor` | `Color` | `Color.Inherit` | Icon color |
| `Expanded` | `bool` | `false` | Initially expanded |
| `Disabled` | `bool` | `false` | Disable the group |
| `HideExpandIcon` | `bool` | `false` | Hide expand/collapse icon |
| `MaxHeight` | `int?` | `null` | Maximum height when expanded |
| `ExpandIcon` | `string` | `Icons.Material.Filled.ExpandLess` | Expand icon |
| `CollapseIcon` | `string` | `Icons.Material.Filled.ExpandMore` | Collapse icon |

## Examples

### Simple Navigation Menu
```razor
<MudNavMenu>
    <MudNavLink Href="/" Match="NavLinkMatch.All">
        <MudIcon Icon="@Icons.Material.Filled.Home" Style="margin-right: 8px;" />
        Home
    </MudNavLink>
    <MudNavLink Href="/products">
        <MudIcon Icon="@Icons.Material.Filled.Inventory" Style="margin-right: 8px;" />
        Products
    </MudNavLink>
    <MudNavLink Href="/orders">
        <MudIcon Icon="@Icons.Material.Filled.ShoppingCart" Style="margin-right: 8px;" />
        Orders
    </MudNavLink>
    <MudNavLink Href="/customers">
        <MudIcon Icon="@Icons.Material.Filled.People" Style="margin-right: 8px;" />
        Customers
    </MudNavLink>
</MudNavMenu>
```

### Navigation with Groups
```razor
<MudNavMenu>
    <MudNavLink Href="/" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Dashboard">
        Dashboard
    </MudNavLink>
    
    <MudNavGroup Title="Products" Icon="@Icons.Material.Filled.Inventory" Expanded="true">
        <MudNavLink Href="/products" Icon="@Icons.Material.Filled.List">
            All Products
        </MudNavLink>
        <MudNavLink Href="/products/categories" Icon="@Icons.Material.Filled.Category">
            Categories
        </MudNavLink>
        <MudNavLink Href="/products/add" Icon="@Icons.Material.Filled.Add">
            Add Product
        </MudNavLink>
    </MudNavGroup>
    
    <MudNavGroup Title="Orders" Icon="@Icons.Material.Filled.ShoppingCart">
        <MudNavLink Href="/orders" Icon="@Icons.Material.Filled.List">
            All Orders
        </MudNavLink>
        <MudNavLink Href="/orders/pending" Icon="@Icons.Material.Filled.Schedule">
            Pending Orders
        </MudNavLink>
        <MudNavLink Href="/orders/completed" Icon="@Icons.Material.Filled.CheckCircle">
            Completed Orders
        </MudNavLink>
    </MudNavGroup>
    
    <MudNavGroup Title="Settings" Icon="@Icons.Material.Filled.Settings">
        <MudNavLink Href="/settings/profile" Icon="@Icons.Material.Filled.Person">
            Profile
        </MudNavLink>
        <MudNavLink Href="/settings/security" Icon="@Icons.Material.Filled.Security">
            Security
        </MudNavLink>
        <MudNavLink Href="/settings/preferences" Icon="@Icons.Material.Filled.Tune">
            Preferences
        </MudNavLink>
    </MudNavGroup>
</MudNavMenu>
```

### Nested Navigation Groups
```razor
<MudNavMenu>
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">Home</MudNavLink>
    
    <MudNavGroup Title="Administration" Icon="@Icons.Material.Filled.AdminPanelSettings">
        <MudNavGroup Title="User Management" Icon="@Icons.Material.Filled.People">
            <MudNavLink Href="/admin/users" Icon="@Icons.Material.Filled.PersonAdd">
                Users
            </MudNavLink>
            <MudNavLink Href="/admin/roles" Icon="@Icons.Material.Filled.Security">
                Roles
            </MudNavLink>
            <MudNavLink Href="/admin/permissions" Icon="@Icons.Material.Filled.VpnKey">
                Permissions
            </MudNavLink>
        </MudNavGroup>
        
        <MudNavGroup Title="System" Icon="@Icons.Material.Filled.Settings">
            <MudNavLink Href="/admin/system/logs" Icon="@Icons.Material.Filled.Description">
                Logs
            </MudNavLink>
            <MudNavLink Href="/admin/system/backup" Icon="@Icons.Material.Filled.Backup">
                Backup
            </MudNavLink>
            <MudNavLink Href="/admin/system/config" Icon="@Icons.Material.Filled.Tune">
                Configuration
            </MudNavLink>
        </MudNavGroup>
    </MudNavGroup>
</MudNavMenu>
```

### Navigation with Badge Indicators
```razor
<MudNavMenu>
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Dashboard">
        Dashboard
    </MudNavLink>
    
    <MudNavLink Href="/messages" Icon="@Icons.Material.Filled.Mail">
        <div style="display: flex; justify-content: space-between; align-items: center; width: 100%;">
            <span>Messages</span>
            @if (unreadCount > 0)
            {
                <MudBadge Content="@unreadCount" Color="Color.Error" Overlap="true">
                    <div style="width: 20px;"></div>
                </MudBadge>
            }
        </div>
    </MudNavLink>
    
    <MudNavLink Href="/notifications" Icon="@Icons.Material.Filled.Notifications">
        <div style="display: flex; justify-content: space-between; align-items: center; width: 100%;">
            <span>Notifications</span>
            @if (notificationCount > 0)
            {
                <MudChip Size="Size.Small" Color="Color.Primary">@notificationCount</MudChip>
            }
        </div>
    </MudNavLink>
</MudNavMenu>

@code {
    private int unreadCount = 5;
    private int notificationCount = 12;
}
```

### Conditional Navigation Items
```razor
<MudNavMenu>
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">Home</MudNavLink>
    
    @if (userIsAuthenticated)
    {
        <MudNavLink Href="/profile" Icon="@Icons.Material.Filled.Person">
            Profile
        </MudNavLink>
        
        @if (userIsAdmin)
        {
            <MudNavGroup Title="Admin" Icon="@Icons.Material.Filled.AdminPanelSettings">
                <MudNavLink Href="/admin/users" Icon="@Icons.Material.Filled.People">
                    Users
                </MudNavLink>
                <MudNavLink Href="/admin/settings" Icon="@Icons.Material.Filled.Settings">
                    Settings
                </MudNavLink>
            </MudNavGroup>
        }
        
        <MudNavLink Href="/logout" Icon="@Icons.Material.Filled.Logout">
            Logout
        </MudNavLink>
    }
    else
    {
        <MudNavLink Href="/login" Icon="@Icons.Material.Filled.Login">
            Login
        </MudNavLink>
        <MudNavLink Href="/register" Icon="@Icons.Material.Filled.PersonAdd">
            Register
        </MudNavLink>
    }
</MudNavMenu>

@code {
    private bool userIsAuthenticated = true;
    private bool userIsAdmin = false;
}
```

### Custom Styled Navigation
```razor
<MudNavMenu Color="Color.Primary" Class="custom-nav-menu">
    <MudNavLink Href="/" 
                Icon="@Icons.Material.Filled.Home" 
                Class="custom-nav-link">
        Home
    </MudNavLink>
    <MudNavLink Href="/about" 
                Icon="@Icons.Material.Filled.Info"
                IconColor="Color.Secondary">
        About
    </MudNavLink>
</MudNavMenu>

<style>
    .custom-nav-menu {
        background-color: #f5f5f5;
        border-radius: 8px;
        padding: 8px;
    }
    
    .custom-nav-link {
        margin-bottom: 4px;
        border-radius: 4px;
    }
    
    .custom-nav-link:hover {
        background-color: rgba(25, 118, 210, 0.08);
    }
</style>
```

### Navigation with Tooltips
```razor
<MudNavMenu Dense="true">
    <MudTooltip Text="Go to Dashboard">
        <MudNavLink Href="/" Icon="@Icons.Material.Filled.Dashboard">
            Dashboard
        </MudNavLink>
    </MudTooltip>
    
    <MudTooltip Text="View all products">
        <MudNavLink Href="/products" Icon="@Icons.Material.Filled.Inventory">
            Products
        </MudNavLink>
    </MudTooltip>
    
    <MudTooltip Text="Manage orders">
        <MudNavLink Href="/orders" Icon="@Icons.Material.Filled.ShoppingCart">
            Orders
        </MudNavLink>
    </MudTooltip>
</MudNavMenu>
```

### Programmatic Navigation Control
```razor
<MudNavMenu>
    <MudNavGroup @ref="adminGroup" 
                 Title="Administration" 
                 Icon="@Icons.Material.Filled.AdminPanelSettings"
                 Expanded="@adminExpanded">
        <MudNavLink Href="/admin/users" Icon="@Icons.Material.Filled.People">
            Users
        </MudNavLink>
        <MudNavLink Href="/admin/settings" Icon="@Icons.Material.Filled.Settings">
            Settings
        </MudNavLink>
    </MudNavGroup>
</MudNavMenu>

<MudButtonGroup>
    <MudButton OnClick="ExpandAdmin">Expand Admin</MudButton>
    <MudButton OnClick="CollapseAdmin">Collapse Admin</MudButton>
    <MudButton OnClick="ToggleAdmin">Toggle Admin</MudButton>
</MudButtonGroup>

@code {
    private MudNavGroup adminGroup;
    private bool adminExpanded = false;
    
    private void ExpandAdmin()
    {
        adminExpanded = true;
        adminGroup?.Expand();
    }
    
    private void CollapseAdmin()
    {
        adminExpanded = false;
        adminGroup?.Collapse();
    }
    
    private void ToggleAdmin()
    {
        adminExpanded = !adminExpanded;
        adminGroup?.Toggle();
    }
}
```

### Navigation with External Links
```razor
<MudNavMenu>
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">
        Home
    </MudNavLink>
    
    <MudNavLink Href="/products" Icon="@Icons.Material.Filled.Inventory">
        Products
    </MudNavLink>
    
    <MudDivider Style="margin: 8px 0;" />
    
    <MudNavLink Href="https://github.com/example" 
                Target="_blank" 
                Icon="@Icons.Custom.Brands.GitHub"
                ForceLoad="true">
        GitHub
    </MudNavLink>
    
    <MudNavLink Href="https://docs.example.com" 
                Target="_blank" 
                Icon="@Icons.Material.Filled.MenuBook">
        Documentation
    </MudNavLink>
</MudNavMenu>
```

## Layout Integration

### In MainLayout with Drawer
```razor
<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" 
                       Color="Color.Inherit" 
                       OnClick="@((e) => DrawerToggle())" />
        <MudSpacer />
        <MudIconButton Icon="@Icons.Material.Filled.AccountCircle" 
                       Color="Color.Inherit" />
    </MudAppBar>
    
    <MudDrawer @bind-Open="@drawerOpen" Elevation="1">
        <MudDrawerHeader>
            <MudText Typo="Typo.h6">My Application</MudText>
        </MudDrawerHeader>
        <MudNavMenu>
            <!-- Navigation items here -->
        </MudNavMenu>
    </MudDrawer>
    
    <MudMainContent>
        @Body
    </MudMainContent>
</MudLayout>

@code {
    private bool drawerOpen = true;
    
    private void DrawerToggle()
    {
        drawerOpen = !drawerOpen;
    }
}
```

## Best Practices

1. **Use icons consistently**: Provide icons for all navigation items for better UX
2. **Group related items**: Use MudNavGroup to organize related navigation items
3. **Indicate active state**: Use appropriate NavLinkMatch for current page highlighting
4. **Keep it simple**: Don't nest groups too deeply (max 2-3 levels)
5. **Responsive design**: Consider how navigation works on mobile devices
6. **Loading states**: Show loading indicators when navigation is in progress
7. **Accessibility**: Ensure keyboard navigation and screen reader support

## Related Components
- MudDrawer
- MudAppBar
- MudBreadcrumbs
- MudTabs