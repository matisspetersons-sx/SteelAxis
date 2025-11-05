# Runtime Authentication State Fix – Implementation Details

## Summary
- Added explicit authentication-state cascading so MudBlazor providers and `AuthorizeView` instances no longer throw at runtime.
- Updated the router shell to redirect anonymous users through the existing `RedirectToLogin` component.
- Verified the solution builds without errors after the changes.

## Files Updated
- `SteelAxis.Web/Components/App.razor`
- `SteelAxis.Web/Components/Routes.razor`

## Key Decisions
- Wrapped the routing tree instead of each page to provide a single source of authentication state.
- Used `AuthorizeRouteView` with a context parameter to distinguish between unauthenticated and unauthorized outcomes.
- Left MudBlazor provider ordering unchanged in `MainLayout.razor`, as it already satisfied component requirements once the cascade exists.

## Testing
- `dotnet build`
