# MudBlazor Documentation

Official MudBlazor component documentation, patterns, and best practices for Manimp.

---

## 📚 Purpose

This directory contains MudBlazor-specific documentation to help developers:
- Find component usage examples quickly
- Follow consistent UI patterns
- Understand MudBlazor configuration in Manimp
- Troubleshoot common MudBlazor issues

---

## 📖 How to Use

### For Developers
1. **Building UI components?** Check here first for MudBlazor patterns
2. **Dialog not working?** See troubleshooting guides
3. **Form validation?** Find MudForm examples
4. **Data tables?** MudDataGrid patterns and examples

### For AI Agents
When implementing UI features:
- Reference documentation in this directory
- Follow established MudBlazor patterns
- Add new patterns as you discover them
- Keep examples practical and tested

---

## 🎯 What Should Go Here

### ✅ Include:
- **Component Examples**: Working code snippets for common components
- **Patterns**: Reusable UI patterns (dialogs, forms, tables, etc.)
- **Configuration**: MudBlazor setup and configuration docs
- **Troubleshooting**: Common issues and solutions
- **Best Practices**: Dos and don'ts for MudBlazor in Manimp
- **Integration Notes**: How MudBlazor integrates with Blazor Server

### ❌ Don't Include:
- Feature-specific business logic (goes in feature docs)
- Database schemas (goes in general docs)
- API documentation (goes in feature docs)
- General .NET documentation (external references only)

---

## 📁 Suggested Structure

```
docs/mudblazor/
├── README.md (this file)
├── MudBlazor-Setup.md
├── MudButton.md
├── MudCard.md
├── MudDataGrid.md
├── MudDialog.md
├── MudIcon.md
├── MudNavMenu.md
├── MudSelect.md
├── MudTable.md
└── MudTextField.md
```

---

## 📚 Component Documentation

### [MudBlazor-Setup.md](./MudBlazor-Setup.md)
MudBlazor installation and configuration guide

**Contents:**
- Installation steps
- Service registration
- Theme configuration
- Initial setup

---

### [MudButton.md](./MudButton.md)
Button component documentation

**Contents:**
- Button variants and styles
- Icons and loading states
- Event handling
- Accessibility

---

### [MudCard.md](./MudCard.md)
Card component for content containers

**Contents:**
- Card structure and layout
- Header, content, and actions
- Media support
- Styling options

---

### [MudDataGrid.md](./MudDataGrid.md)
Advanced data grid component

**Contents:**
- Grid configuration
- Columns and templates
- Filtering and sorting
- Pagination
- Server-side data

---

### [MudDialog.md](./MudDialog.md)
Dialog/modal component

**Contents:**
- Dialog setup and configuration
- Parameters and options
- Return values
- Custom dialogs
- Troubleshooting

---

### [MudIcon.md](./MudIcon.md)
Icon component and icon usage

**Contents:**
- Material icons
- Custom icons
- Icon sizing and colors
- Common icon patterns

---

### [MudNavMenu.md](./MudNavMenu.md)
Navigation menu component

**Contents:**
- Menu structure
- Navigation items
- Nested menus
- Active state handling

---

### [MudSelect.md](./MudSelect.md)
Select/dropdown component

**Contents:**
- Single and multi-select
- Data binding
- Custom item templates
- Validation

---

### [MudTable.md](./MudTable.md)
Table component for data display

**Contents:**
- Table structure
- Columns and rows
- Sorting and filtering
- Custom cell templates
- Pagination

---

### [MudTextField.md](./MudTextField.md)
Text input field component

**Contents:**
- Text field variants
- Validation
- Input masking
- Adornments and icons
- Error handling

---

## 🚀 Quick Start

### Common MudBlazor Components in Manimp

#### Dialogs
```razor
@inject IDialogService DialogService

// Open dialog
var parameters = new DialogParameters { ["Item"] = item };
var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
var dialog = await DialogService.ShowAsync<MyDialog>("Title", parameters, options);
var result = await dialog.Result;
```

#### Forms
```razor
<MudForm @ref="form" @bind-IsValid="@isValid">
    <MudTextField @bind-Value="model.Name" 
                  Label="Name" 
                  Required="true" 
                  RequiredError="Name is required" />
    <MudButton Disabled="@(!isValid)" OnClick="Submit">Save</MudButton>
</MudForm>
```

#### Data Tables
```razor
<MudDataGrid Items="@items" Filterable="true" SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <PropertyColumn Property="x => x.Status" Title="Status" />
        <TemplateColumn Title="Actions">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                              OnClick="@(() => Edit(context.Item))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>
```

---

## 🔗 Official Resources

- **MudBlazor Website**: https://mudblazor.com
- **Component Gallery**: https://mudblazor.com/components
- **GitHub**: https://github.com/MudBlazor/MudBlazor
- **Documentation**: https://mudblazor.com/docs/overview

---

## 📝 Contributing Documentation

When adding MudBlazor documentation:

1. **Test First**: Ensure examples work in Manimp context
2. **Be Specific**: Show actual working code, not pseudo-code
3. **Context Matters**: Include render mode annotations (`@rendermode InteractiveServer`)
4. **Cross-Reference**: Link to official docs for detailed API info
5. **Real Examples**: Use examples from actual Manimp components when possible

---

## 🎨 Manimp MudBlazor Configuration

### Current Setup
- **Version**: Check `Manimp.Web/Manimp.Web.csproj` for MudBlazor version
- **Theme**: Custom theme in `Manimp.Web/Themes/`
- **Render Mode**: `InteractiveServer` (required for all components)
- **Configuration**: `Manimp.Web/Program.cs` → `builder.Services.AddMudServices()`

### Key Settings
```csharp
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});
```

---

## 🐛 Common Issues

### Issue: Dialogs Not Rendering
**Solution**: Ensure `@rendermode InteractiveServer` is on dialog component
- See: `docs/fixes-and-improvements/mudblazor-dialog-fix.md`

### Issue: Forms Not Validating
**Solution**: Use `<MudForm @ref="form" @bind-IsValid="@isValid">` pattern
- Ensure all fields have validation attributes

### Issue: Tables Not Updating
**Solution**: Call `StateHasChanged()` after data changes
- Use `@bind:after` for automatic updates

---

## 📞 Need Help?

1. Check this directory first
2. Search official MudBlazor docs
3. Check `docs/fixes-and-improvements/` for known issues
4. Review existing Manimp components for patterns
5. Ask in team chat or create documentation request

---

## 🎯 TODO

Upload MudBlazor documentation files here:
- [x] MudBlazor setup and configuration
- [x] MudButton component
- [x] MudCard component
- [x] MudDataGrid component
- [x] MudDialog component
- [x] MudIcon component
- [x] MudNavMenu component
- [x] MudSelect component
- [x] MudTable component
- [x] MudTextField component
- [ ] MudForm component and validation patterns
- [ ] MudSnackbar component
- [ ] MudDrawer component
- [ ] MudAppBar component
- [ ] Performance optimization tips
- [ ] Mobile responsiveness patterns
- [ ] Accessibility guidelines
- [ ] Theme customization examples

---

**Remember**: Keep documentation practical, tested, and Manimp-specific! 🚀
