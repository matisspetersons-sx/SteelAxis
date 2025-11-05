# Runtime Authentication State Fix

## What
Applies the required Blazor authentication cascade so authorization-aware components and MudBlazor services render without runtime exceptions.

## Why
- Prevents the `Task<AuthenticationState>` runtime failure that blocked every page load.
- Keeps MudBlazor providers functional for menus, dialogs, and popovers.
- Ensures authenticated tenant admins can sign in and reach compliance-critical workflows.

## How
- `Components/App.razor` now wraps the routing tree in `<CascadingAuthenticationState>`.
- `Components/Routes.razor` uses `AuthorizeRouteView` with redirect handling for anonymous users.
- MudBlazor services remain registered in `Program.cs`, and the layout continues to host the required providers.

## Usage
1. Start the API: `dotnet run --project SteelAxis.Api/SteelAxis.Api.csproj --urls http://localhost:5100`
2. Start the web app: `ApiBaseUrl=http://localhost:5100 ASPNETCORE_ENVIRONMENT=Development dotnet run --project SteelAxis.Web/SteelAxis.Web.csproj --urls http://localhost:5200`
3. Browse to `http://localhost:5200`; the portal should render and prompt for sign-in when unauthenticated.

## References
- `SteelAxis.Web/Components/App.razor`
- `SteelAxis.Web/Components/Routes.razor`
- `SteelAxis.Web/Components/Layout/MainLayout.razor`
