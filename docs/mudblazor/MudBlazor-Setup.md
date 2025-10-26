# MudBlazor Setup and Installation Guide

## Overview
Complete setup and installation guide for MudBlazor v8 in different types of Blazor applications.

## Prerequisites
- .NET 8 or .NET 9
- Visual Studio 2022 or VS Code
- Basic knowledge of Blazor

## Installation

### 1. Install NuGet Package
```bash
dotnet add package MudBlazor
```

Or via Package Manager Console:
```powershell
Install-Package MudBlazor
```

### 2. Add Using Statement
Add to `_Imports.razor`:
```razor
@using MudBlazor
```

### 3. Add Required Services
Add to `Program.cs`:
```csharp
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MudBlazor services
builder.Services.AddMudServices();

var app = builder.Build();
```

### 4. Add CSS and JavaScript References

#### For Blazor Server (`_Host.cshtml` or `_Layout.cshtml`)
```html
<head>
    <!-- MudBlazor CSS -->
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
</head>

<body>
    <!-- Your content -->
    
    <!-- MudBlazor JS -->
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
```

#### For Blazor WebAssembly (`index.html`)
```html
<head>
    <!-- MudBlazor CSS -->
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
</head>

<body>
    <!-- Your content -->
    
    <!-- MudBlazor JS -->
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
```

#### For .NET 8 Blazor Web App (`App.razor`)
```html
<head>
    <!-- MudBlazor CSS -->
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
</head>

<body>
    <!-- Your content -->
    
    <!-- MudBlazor JS -->
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
```

### 5. Add Providers to Layout
Add to `MainLayout.razor` or `App.razor`:
```razor
<MudThemeProvider />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

@* Your layout content *@
```

## Project-Specific Setup

### Blazor Server
Complete `Program.cs`:
```csharp
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

### Blazor WebAssembly
Client `Program.cs`:
```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

await builder.Build().RunAsync();
```

### .NET 8 Blazor Web App
`Program.cs`:
```csharp
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(YourClient._Imports).Assembly);

app.Run();
```

## Common Parameters and Global Configuration

### MudGlobal Configuration
You can set global defaults for MudBlazor components:
```csharp
// In Program.cs or wherever you configure services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
```

### Theme Configuration
Create a custom theme:
```razor
@code {
    MudTheme MyCustomTheme = new MudTheme()
    {
        Palette = new PaletteLight()
        {
            Primary = Colors.Blue.Default,
            Secondary = Colors.Green.Accent4,
            AppbarBackground = Colors.Red.Default,
        },
        PaletteDark = new PaletteDark()
        {
            Primary = Colors.Blue.Lighten1
        },
        LayoutProperties = new LayoutProperties()
        {
            DrawerWidthLeft = "260px",
            DrawerWidthRight = "300px"
        }
    };
}

<MudThemeProvider Theme="MyCustomTheme" />
```

## Testing Setup

### Unit Testing Setup
For unit testing with bUnit:
```csharp
[Test]
public void MyComponentTest()
{
    using var ctx = new TestContext();
    ctx.Services.AddMudServices();
    
    var component = ctx.RenderComponent<MyComponent>();
    
    // Your test logic
}
```

## Common Issues and Solutions

### 1. Icons Not Showing
Ensure the Google Fonts link is added:
```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
```

### 2. JavaScript Errors
Make sure MudBlazor.min.js is loaded:
```html
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

### 3. Dialogs/Snackbars Not Working
Ensure providers are added to your layout:
```razor
<MudDialogProvider />
<MudSnackbarProvider />
```

### 4. Static SSR Issues
MudBlazor requires interactive rendering. For .NET 8, use:
```razor
@rendermode InteractiveServer
```
or
```razor
@rendermode InteractiveWebAssembly
```

### 5. Styling Issues
Make sure CSS is loaded and there are no conflicts with other CSS frameworks like Bootstrap.

## Development Tools

### MudBlazor Templates
Install MudBlazor project templates:
```bash
dotnet new install MudBlazor.Templates
```

Create new project:
```bash
dotnet new mudblazor --host server --name MyMudApp
dotnet new mudblazor --host wasm --name MyMudApp
```

### Theme Manager (Optional)
For design-time theme editing:
```bash
dotnet add package MudBlazor.ThemeManager
```

Then add to your layout:
```razor
<MudThemeManagerButton />
```

## Best Practices

1. **Remove Bootstrap**: Remove Bootstrap CSS to avoid conflicts
2. **Use providers**: Always include required providers in your layout
3. **Global configuration**: Configure global settings in Program.cs
4. **Custom themes**: Create custom themes for brand consistency
5. **Testing**: Include MudServices in your test setup
6. **Performance**: Use static assets from CDN in production

## Next Steps
After installation, you can:
1. Start using MudBlazor components in your pages
2. Customize the theme to match your brand
3. Explore the extensive component library
4. Set up global configurations for consistent behavior

## Resources
- [Official Documentation](https://mudblazor.com/)
- [GitHub Repository](https://github.com/MudBlazor/MudBlazor)
- [Try MudBlazor Online](https://try.mudblazor.com/)
- [Discord Community](https://discord.gg/mudblazor)