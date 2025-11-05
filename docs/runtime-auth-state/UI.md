# Runtime Authentication State Fix – UI Notes

## Updated Components
- `SteelAxis.Web/Components/App.razor` now provides a cascading authentication state for all routed components.
- `SteelAxis.Web/Components/Routes.razor` uses `AuthorizeRouteView` to manage authenticated and unauthenticated states consistently within the existing `MainLayout` shell.

## UX Impact
- Anonymous users are redirected to the existing `RedirectToLogin` component instead of encountering an exception page.
- Authorized users retain access to the full MudBlazor-enhanced layout, including popovers, dialogs, and snackbars.

## Accessibility
- Unauthorized message uses `role="alert"` to ensure screen readers notify the user when access is denied.
