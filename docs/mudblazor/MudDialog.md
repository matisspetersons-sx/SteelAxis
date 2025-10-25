# MudDialog Component Reference

## Overview
MudDialog provides modal dialog functionality with customizable content, actions, and animations based on Material Design principles.

## Basic Usage

```razor
<!-- Dialog Component -->
<MudDialog>
    <DialogContent>
        <MudText>This is a simple dialog</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" OnClick="Submit">Ok</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    
    void Submit() => MudDialog.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog.Cancel();
}
```

## Dialog Service Usage

```razor
@inject IDialogService DialogService

<MudButton @onclick="OpenDialog" Variant="Variant.Filled" Color="Color.Primary">
    Open Dialog
</MudButton>

@code {
    private async Task OpenDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = DialogService.Show<MyDialog>("My Dialog Title", options);
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            // Handle dialog result
        }
    }
}
```

## Dialog Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `TitleContent` | `RenderFragment` | `null` | Custom title content |
| `DialogContent` | `RenderFragment` | `null` | Main dialog content |
| `DialogActions` | `RenderFragment` | `null` | Action buttons area |
| `Options` | `DialogOptions` | `null` | Dialog configuration options |
| `DefaultFocus` | `DefaultFocus` | `Element` | Default focus behavior |
| `OnBackdropClick` | `EventCallback` | - | Backdrop click handler |
| `Class` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |

## DialogOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MaxWidth` | `MaxWidth?` | `Small` | Maximum dialog width |
| `FullWidth` | `bool?` | `false` | Full width dialog |
| `FullHeight` | `bool?` | `false` | Full height dialog |
| `FullScreen` | `bool?` | `false` | Full screen dialog |
| `Position` | `DialogPosition?` | `Center` | Dialog position |
| `NoHeader` | `bool?` | `false` | Hide dialog header |
| `BackgroundClass` | `string` | `null` | Background CSS class |
| `CloseButton` | `bool?` | `false` | Show close button |
| `CloseOnEscapeKey` | `bool?` | `true` | Close on Escape key |
| `DisableBackdropClick` | `bool?` | `false` | Disable backdrop click to close |

## Examples

### Simple Confirmation Dialog
```razor
<!-- ConfirmDialog.razor -->
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">
            <MudIcon Icon="Icons.Material.Filled.Warning" Class="mr-3"/> 
            Confirm Action
        </MudText>
    </TitleContent>
    <DialogContent>
        <MudText>@ContentText</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Error" Variant="Variant.Filled" OnClick="Submit">Delete</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public string ContentText { get; set; }
    
    void Submit() => MudDialog.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog.Cancel();
}
```

### Form Dialog
```razor
<!-- EditUserDialog.razor -->
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Edit User</MudText>
    </TitleContent>
    <DialogContent>
        <MudContainer Style="max-height: 300px; overflow-y: scroll">
            <MudTextField @bind-Value="user.Name" 
                          Label="Name" 
                          Required="true" />
            <MudTextField @bind-Value="user.Email" 
                          Label="Email" 
                          InputType="InputType.Email"
                          Required="true" />
            <MudSelect @bind-Value="user.Role" 
                       Label="Role" 
                       Required="true">
                <MudSelectItem Value="Admin">Admin</MudSelectItem>
                <MudSelectItem Value="User">User</MudSelectItem>
                <MudSelectItem Value="Guest">Guest</MudSelectItem>
            </MudSelect>
        </MudContainer>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                   Variant="Variant.Filled" 
                   OnClick="Submit">Save</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public User User { get; set; }
    
    private User user = new();
    
    protected override void OnInitialized()
    {
        user = User?.Clone() ?? new User();
    }
    
    void Submit() => MudDialog.Close(DialogResult.Ok(user));
    void Cancel() => MudDialog.Cancel();
}
```

### Opening Dialogs with Parameters
```razor
@inject IDialogService DialogService

<MudButton OnClick="OpenConfirmDialog" Color="Color.Error">Delete Item</MudButton>
<MudButton OnClick="OpenEditDialog" Color="Color.Primary">Edit User</MudButton>

@code {
    private async Task OpenConfirmDialog()
    {
        var parameters = new DialogParameters();
        parameters.Add("ContentText", "Are you sure you want to delete this item? This action cannot be undone.");
        
        var options = new DialogOptions() 
        { 
            CloseButton = true, 
            MaxWidth = MaxWidth.Small 
        };
        
        var dialog = DialogService.Show<ConfirmDialog>("Delete Item", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            // Perform delete operation
        }
    }
    
    private async Task OpenEditDialog()
    {
        var parameters = new DialogParameters();
        parameters.Add("User", currentUser);
        
        var options = new DialogOptions() 
        { 
            MaxWidth = MaxWidth.Medium,
            FullWidth = true 
        };
        
        var dialog = DialogService.Show<EditUserDialog>("Edit User", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled && result.Data is User updatedUser)
        {
            // Update user
            currentUser = updatedUser;
        }
    }
    
    private User currentUser = new() { Name = "John Doe", Email = "john@example.com", Role = "User" };
}
```

### Full Screen Dialog
```razor
private async Task OpenFullScreenDialog()
{
    var options = new DialogOptions()
    {
        FullScreen = true,
        CloseButton = true
    };
    
    var dialog = DialogService.Show<DetailDialog>("Item Details", options);
    await dialog.Result;
}
```

### Custom Positioned Dialog
```razor
private async Task OpenCustomDialog()
{
    var options = new DialogOptions()
    {
        Position = DialogPosition.TopCenter,
        MaxWidth = MaxWidth.Medium,
        NoHeader = true
    };
    
    var dialog = DialogService.Show<NotificationDialog>("", options);
    await dialog.Result;
}
```

### Dialog with Custom Actions
```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Save Changes</MudText>
    </TitleContent>
    <DialogContent>
        <MudText>You have unsaved changes. What would you like to do?</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="DiscardChanges" Color="Color.Default">Discard</MudButton>
        <MudButton OnClick="SaveAndContinue" Color="Color.Primary">Save & Continue</MudButton>
        <MudButton OnClick="SaveAndClose" 
                   Color="Color.Primary" 
                   Variant="Variant.Filled">Save & Close</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    
    void DiscardChanges() => MudDialog.Close(DialogResult.Ok("discard"));
    void SaveAndContinue() => MudDialog.Close(DialogResult.Ok("save_continue"));
    void SaveAndClose() => MudDialog.Close(DialogResult.Ok("save_close"));
}
```

### Scrollable Content Dialog
```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Terms of Service</MudText>
    </TitleContent>
    <DialogContent>
        <div style="max-height: 400px; overflow-y: auto;">
            <MudText>
                <!-- Long content here -->
            </MudText>
        </div>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Decline">Decline</MudButton>
        <MudButton Color="Color.Primary" 
                   Variant="Variant.Filled" 
                   OnClick="Accept">Accept</MudButton>
    </DialogActions>
</MudDialog>
```

## Setup Requirements

### Program.cs
```csharp
builder.Services.AddMudServices();
```

### Layout Component
```razor
<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

## Best Practices

1. **Keep dialogs focused**: Use dialogs for specific tasks, not complex workflows
2. **Provide clear actions**: Always include Cancel and a primary action
3. **Handle results**: Always check dialog results before taking action
4. **Use appropriate sizes**: Choose MaxWidth based on content needs
5. **Consider mobile**: Test dialog behavior on smaller screens
6. **Escape handling**: Allow users to close dialogs with Escape key
7. **Loading states**: Show loading indicators for async operations in dialogs

## Related Components
- MudPopover
- MudMenu
- MudSnackbar