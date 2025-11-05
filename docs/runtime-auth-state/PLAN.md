# Runtime Authentication State Fix Plan

## Feature Description
Ensure MudBlazor components that require authorization context (e.g., `AuthorizeView`, `MudPopoverProvider`) operate without runtime exceptions by supplying the required cascading authentication state within the Blazor Server app shell.

## Business Value
- Restores functional UI so operators can access tenant management screens.
- Prevents production outages caused by missing authentication state configuration.
- Aligns with EN 1090 compliance requirements by guaranteeing secure, audited access flows.

## User Stories
- *As an authenticated tenant admin, I need the portal to render without runtime errors so I can manage users and invitations.*
- *As a developer, I need a documented pattern for providing authentication state in server-side Blazor to avoid regressions.*

## Acceptance Criteria
1. Blazor Server app starts and renders the home page without `InvalidOperationException` errors related to `Task<AuthenticationState>`.
2. `AuthorizeView` in `MainLayout` correctly shows the signed-in user information when authenticated and the sign-in button when anonymous.
3. MudBlazor providers (`MudPopoverProvider`, `MudDialogProvider`, etc.) initialize without missing service exceptions.
4. Automated build (`dotnet build`) passes for the entire solution.

## Technical Approach
- Wrap the routing component tree in `<CascadingAuthenticationState>` within `Components/App.razor`.
- Update `Components/Routes.razor` to use `<AuthorizeRouteView>` with a `Context` parameter to control unauthenticated redirects via the existing `RedirectToLogin` component.
- Retain MudBlazor providers in `MainLayout` to ensure the registered services are used.
- Verify `Program.cs` already calls `builder.Services.AddMudServices();` (no change expected).

## API Endpoints
No new API endpoints. Existing endpoints remain unchanged.

## Database Schema Changes
None.

## UI Components
- `Components/App.razor`
- `Components/Routes.razor`
- `Components/Layout/MainLayout.razor` (verification only)

## Dependencies & Risks
- Requires Microsoft Identity to be configured; absence of valid Azure AD B2C settings will still block sign-in but should no longer crash rendering.
- Must ensure documentation updates comply with SteelAxis standards.

## Implementation Steps
1. Update `App.razor` to wrap `<Routes />` inside `<CascadingAuthenticationState>`.
2. Replace the basic router in `Routes.razor` with an `AuthorizeRouteView` pattern that redirects unauthenticated users.
3. Run `dotnet build` to validate.
4. Restart local API and Web projects to confirm runtime behavior.
5. Produce documentation updates (README, API-SPEC, DATABASE, IMPLEMENTATION) summarizing the change.
