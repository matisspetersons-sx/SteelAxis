# Authentication & Tenant Management - Implementation Plan

**Feature:** Multi-Tenant Authentication with Admin Registration, User Invitations, Roles, Subscription Tiers & Feature Flags  
**Created:** October 27, 2025  
**Status:** 📋 Planning Phase  
**Priority:** Critical - Foundation for entire SteelAxis platform

---

## 📋 Feature Description

Implement complete multi-tenant authentication and user management system for SteelAxis using Microsoft Entra External ID (CIAM). This includes:

### Core Authentication
- **Landing Page** - Public home page for anonymous users
- **Admin Registration** - New tenant creation with first admin user
- **Login Page** - Existing user authentication with tenant context
- **Logout Functionality** - Secure sign-out flow

### Tenant & User Management
- **Tenant Registration** - Company/organization account creation
- **User Invitation System** - Admins invite users with specific roles
- **Role-Based Access Control** - Admin, Upper Management, Lower Management, Supervisors, Shopfloor Workers, Office Workers
- **User Management UI** - View, invite, edit, deactivate users

### Subscription & Features
- **Subscription Tiers** - Starter, Professional, Enterprise, Custom
- **Feature Flags** - Control access to features per tenant
- **Payment Integration** - Stripe placeholder for future billing
- **Trial Management** - Demo tenant with full feature access

---

## 🎯 Business Value

- **User Onboarding:** Enable new customers to create accounts
- **Security:** Secure authentication using Microsoft Entra External ID
- **User Experience:** Clear, intuitive authentication flow
- **Multi-tenancy Foundation:** User authentication is prerequisite for tenant isolation

---

## 👤 User Stories

### Story 1: New User Registration
**As a** new customer  
**I want to** create an account with my email  
**So that** I can access the SteelAxis platform

**Acceptance Criteria:**
- Registration page accessible from landing page
- Email validation before submission
- Password requirements clearly displayed
- Microsoft Entra External ID handles account creation
- Email verification sent automatically
- Redirect to login page after successful registration
- Clear error messages for invalid inputs

### Story 2: Existing User Login
**As a** registered user  
**I want to** log in with my credentials  
**So that** I can access my tenant's data

**Acceptance Criteria:**
- Login page accessible from landing page and nav menu
- Email and password input fields
- "Forgot password" link (handled by Entra External ID)
- Redirect to home page after successful login
- Display error message for invalid credentials
- Session persists across page navigation

### Story 3: Landing Page Experience
**As a** visitor  
**I want to** see an overview of SteelAxis features  
**So that** I can decide to sign up or log in

**Acceptance Criteria:**
- Professional welcome message
- Key features highlighted
- Clear CTA buttons for Register and Login
- Different content for anonymous vs authenticated users
- Responsive design with MudBlazor components

### Story 4: User Logout
**As an** authenticated user  
**I want to** log out securely  
**So that** my session ends and data is protected

**Acceptance Criteria:**
- Logout option in navigation menu
- Microsoft Entra External ID sign-out flow
- Session cleared completely
- Redirect to landing page after logout
- No cached authenticated data

---

## 🏗️ Technical Approach

### Architecture

```
User Browser
    ↓
Landing Page (Index.razor)
    ↓ (Anonymous)
    ├─→ Register.razor → Microsoft Entra External ID
    │       ↓
    │   Email Verification
    │       ↓
    └─→ Login.razor → Microsoft Entra External ID
            ↓
        Authenticated State
            ↓
        Home/Dashboard
            ↓
        Logout → Landing Page
```

### Authentication Flow (Microsoft Entra External ID)

1. **Registration:**
   - User clicks "Register" → Redirects to Entra External ID signup page
   - User enters email, creates password
   - Entra sends verification email
   - User verifies email → Account activated
   - Redirect back to SteelAxis login page

2. **Login:**
   - User clicks "Login" → Redirects to Entra External ID login page
   - User enters credentials
   - Entra validates credentials
   - On success: OpenID Connect token returned
   - SteelAxis receives user claims (email, name, user ID)
   - Redirect to authenticated home page

3. **Logout:**
   - User clicks "Logout"
   - Clear local session
   - Redirect to Entra External ID logout endpoint
   - Entra clears session
   - Redirect back to SteelAxis landing page

---

## 📡 API Endpoints

### 1. User Profile API
**Purpose:** Get current user information

```csharp
GET /api/auth/profile
Authorization: Bearer {token}

Response 200 OK:
{
  "userId": "guid",
  "email": "user@example.com",
  "displayName": "John Doe",
  "tenantId": "guid",
  "roles": ["User"]
}

Response 401 Unauthorized (if not authenticated)
```

### 2. Registration Status Check
**Purpose:** Check if user email already exists

```csharp
GET /api/auth/check-email?email={email}

Response 200 OK:
{
  "exists": true/false,
  "message": "Email available" or "Email already registered"
}
```

### 3. Logout Endpoint
**Purpose:** Server-side session cleanup

```csharp
POST /api/auth/logout
Authorization: Bearer {token}

Response 200 OK:
{
  "message": "Logged out successfully"
}
```

**Note:** Primary authentication is handled by Microsoft Entra External ID. These endpoints supplement the OIDC flow.

---

## 🗄️ Database Schema Changes

**No database changes required.** User authentication is handled entirely by Microsoft Entra External ID. User data is stored in:
- Entra External ID tenant (external)
- Local session storage (temporary, client-side)

Future enhancement: Store additional user profile data in `ApplicationUser` table (already exists in SteelAxis.Auth project).

---

## 🎨 UI Components & Pages

### 1. Landing Page (Index.razor)
**Location:** `SteelAxis.Web/Components/Pages/Index.razor`

**Layout:**
```
┌─────────────────────────────────────────┐
│  SteelAxis - Steel Fabrication Platform │
│                                          │
│  [Hero Image/Illustration]               │
│                                          │
│  Manage your steel fabrication projects │
│  with EN 1090 compliance built-in       │
│                                          │
│  ┌──────────┐  ┌──────────┐            │
│  │ Register │  │  Login   │            │
│  └──────────┘  └──────────┘            │
│                                          │
│  Key Features:                           │
│  ✓ Material Traceability                │
│  ✓ Quality Control (NCR/CAR)            │
│  ✓ EN 1090 Compliance                   │
│  ✓ Multi-tenant Architecture            │
└─────────────────────────────────────────┘
```

**Components:**
- `MudContainer` with `MaxWidth.Large`
- `MudCard` for hero section
- `MudButton` for Register and Login CTAs
- `MudGrid` for feature list
- Conditional rendering: Show different content if `@context.User.Identity.IsAuthenticated`

### 2. Registration Page (Register.razor)
**Location:** `SteelAxis.Web/Components/Pages/Register.razor`

**Layout:**
```
┌─────────────────────────────────────────┐
│          Create Your Account             │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │ Email Address *                    │ │
│  │ [________________________]         │ │
│  └────────────────────────────────────┘ │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │ Password *                         │ │
│  │ [________________________]         │ │
│  └────────────────────────────────────┘ │
│                                          │
│  Password Requirements:                  │
│  • At least 8 characters                 │
│  • One uppercase letter                  │
│  • One number                            │
│  • One special character                 │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │        Register Account            │ │
│  └────────────────────────────────────┘ │
│                                          │
│  Already have an account? [Login]        │
└─────────────────────────────────────────┘
```

**Components:**
- `MudCard` wrapper
- `MudTextField` for email (with validation)
- `MudTextField` for password (InputType.Password, with validation)
- `MudButton` for submit
- `MudAlert` for error messages
- `MudText` for password requirements

**Validation:**
- Email format validation
- Password complexity requirements
- Required field validation
- Client-side validation before redirect to Entra

### 3. Login Page (Login.razor)
**Location:** `SteelAxis.Web/Components/Pages/Login.razor`

**Layout:**
```
┌─────────────────────────────────────────┐
│          Sign In to SteelAxis            │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │ Email Address                      │ │
│  │ [________________________]         │ │
│  └────────────────────────────────────┘ │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │ Password                           │ │
│  │ [________________________]         │ │
│  └────────────────────────────────────┘ │
│                                          │
│  [Forgot Password?]                      │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │          Sign In                   │ │
│  └────────────────────────────────────┘ │
│                                          │
│  Don't have an account? [Register]       │
└─────────────────────────────────────────┘
```

**Components:**
- `MudCard` wrapper
- `MudTextField` for email
- `MudTextField` for password (InputType.Password)
- `MudButton` for submit
- `MudLink` for "Forgot Password" (redirects to Entra)
- `MudLink` for "Register"
- `MudAlert` for error messages

### 4. Logout Page (Logout.razor)
**Location:** `SteelAxis.Web/Components/Pages/Logout.razor`

**Simple page with:**
- Loading spinner during logout
- Automatic redirect to Entra logout
- Then redirect to landing page
- No user interaction required

### 5. Navigation Menu Updates (NavMenu.razor)
**Location:** `SteelAxis.Web/Components/Layout/NavMenu.razor`

**Conditional Links:**

**Anonymous User:**
- Home
- Login
- Register

**Authenticated User:**
- Home
- Projects (placeholder)
- Materials (placeholder)
- Profile
- Logout

---

## 🔧 Implementation Steps

### Phase 1: API Layer (2 hours)
1. Create `AuthController.cs` in `SteelAxis.Api/Controllers/`
2. Implement GET `/api/auth/profile` endpoint
3. Implement GET `/api/auth/check-email` endpoint
4. Implement POST `/api/auth/logout` endpoint
5. Add `[Authorize]` attributes where needed
6. Test endpoints with authenticated user token

### Phase 2: HTTP Service Layer (1 hour)
1. Create `IAuthHttpService.cs` interface in `SteelAxis.Web/Services/`
2. Create `AuthHttpService.cs` implementation
3. Implement methods for all API endpoints
4. Register service in `Program.cs` DI container
5. Add error handling and logging

### Phase 3: Landing Page (2 hours)
1. Create `Index.razor` in `SteelAxis.Web/Components/Pages/`
2. Design hero section with MudBlazor components
3. Add feature list with icons
4. Implement conditional rendering for anonymous vs authenticated users
5. Add Register and Login CTAs
6. Style with consistent theme

### Phase 4: Registration Page (2 hours)
1. Create `Register.razor` in `SteelAxis.Web/Components/Pages/`
2. Add email and password input fields with validation
3. Display password requirements
4. Implement client-side validation
5. Add redirect to Microsoft Entra External ID signup
6. Handle registration success/error states
7. Add navigation to Login page

### Phase 5: Login Page (2 hours)
1. Create `Login.razor` in `SteelAxis.Web/Components/Pages/`
2. Add email and password fields
3. Implement Microsoft Identity Web integration
4. Add "Forgot Password" link (Entra flow)
5. Handle login success → redirect to home
6. Handle login errors with clear messages
7. Add navigation to Register page

### Phase 6: Logout Functionality (1 hour)
1. Create `Logout.razor` in `SteelAxis.Web/Components/Pages/`
2. Implement Microsoft Identity sign-out
3. Call logout API endpoint
4. Clear local session/storage
5. Redirect to landing page
6. Add loading state during logout

### Phase 7: Navigation Updates (1 hour)
1. Update `NavMenu.razor` with conditional links
2. Show Login/Register for anonymous users
3. Show Profile/Logout for authenticated users
4. Add `@context.User.Identity.IsAuthenticated` checks
5. Update `MainLayout.razor` if needed
6. Style active/selected menu items

### Phase 8: Testing & Refinement (2 hours)
1. Test complete user journey:
   - Landing → Register → Email verification → Login → Home
   - Landing → Login → Home → Logout → Landing
2. Test validation errors
3. Test authentication state persistence
4. Test navigation updates
5. Test mobile responsiveness
6. Fix any bugs or UI issues

### Phase 9: Documentation (1 hour)
1. Create `README.md` in `docs/authentication-pages/`
2. Create `API-SPEC.md` with endpoint documentation
3. Create `UI.md` with component specifications
4. Create `IMPLEMENTATION.md` with technical details
5. Update root `README.md` if needed
6. Add screenshots to documentation

**Total Estimated Time:** 14 hours

---

## 🧪 Testing Requirements

### Unit Tests
- `AuthController` endpoint tests
- `AuthHttpService` method tests
- Input validation tests

### Integration Tests
1. **Registration Flow:**
   - Valid email registration → Success
   - Duplicate email → Error
   - Invalid email format → Validation error
   - Weak password → Validation error

2. **Login Flow:**
   - Valid credentials → Success, redirect to home
   - Invalid credentials → Error message
   - Account not verified → Error message
   - Forgot password link → Redirect to Entra

3. **Logout Flow:**
   - Authenticated user → Logout → Session cleared
   - Anonymous access to protected pages → Redirect to login

4. **Navigation:**
   - Anonymous user sees Login/Register
   - Authenticated user sees Profile/Logout
   - Menu items navigate correctly

### Manual Testing Checklist
- [ ] Landing page displays correctly for anonymous users
- [ ] Landing page displays correctly for authenticated users
- [ ] Register page validation works
- [ ] Register redirects to Entra External ID
- [ ] Email verification works (Entra sends email)
- [ ] Login page accepts credentials
- [ ] Login redirects to Entra External ID
- [ ] Successful login redirects to home
- [ ] Failed login shows error message
- [ ] Forgot password redirects to Entra
- [ ] Logout clears session
- [ ] Logout redirects to landing page
- [ ] Navigation menu updates based on auth state
- [ ] All pages are mobile responsive
- [ ] All pages follow MudBlazor theme

---

## 🔐 Security Considerations

1. **No Password Storage:** Passwords handled entirely by Microsoft Entra External ID
2. **HTTPS Only:** All authentication flows require HTTPS
3. **CSRF Protection:** Built into ASP.NET Core and Microsoft Identity Web
4. **Session Management:** Handled by OpenID Connect
5. **Token Validation:** JWT Bearer tokens validated on every API request
6. **Secure Redirects:** Only allow redirects to whitelisted URLs
7. **Input Validation:** Validate all user inputs client-side AND server-side

---

## 📦 Dependencies

### NuGet Packages (Already Installed)
- `Microsoft.Identity.Web` v3.6.0
- `Microsoft.Identity.Web.UI` v3.3.0
- `MudBlazor` v8.12.0

### Configuration Required
- Microsoft Entra External ID tenant configured ✅
- App registrations created ✅
- Redirect URIs configured ✅
- Azure Key Vault secrets configured ✅

**No additional dependencies needed.**

---

## 🎯 Success Criteria

### Definition of Done
- [ ] All API endpoints implemented and tested
- [ ] All UI pages created with MudBlazor components
- [ ] Complete user journey functional (Register → Login → Logout)
- [ ] Microsoft Entra External ID integration working
- [ ] Email verification flow working
- [ ] Navigation menu updates based on auth state
- [ ] All validation rules implemented
- [ ] Error handling and user feedback working
- [ ] Mobile responsive design verified
- [ ] All tests passing (unit and integration)
- [ ] Complete documentation created in `docs/authentication-pages/`
- [ ] Code reviewed and approved
- [ ] Committed to repository with clear commit messages

### User Acceptance
- User can register a new account
- User receives verification email
- User can log in with verified account
- User can access authenticated pages
- User can log out successfully
- Anonymous users see appropriate landing page
- Authenticated users see personalized content

---

## 🚀 Deployment Notes

1. Ensure Azure Key Vault secrets are configured:
   - `AzureAdB2C--Authority`
   - `AzureAdB2C--ClientId`
   - `AzureAdB2C--ApiClientId`
   - `AzureAdB2C--DefaultScopes`

2. Verify Entra External ID redirect URIs:
   - Development: `https://localhost:7071/signin-oidc`
   - Production: `https://steelaxis-dev.azurewebsites.net/signin-oidc`

3. Test complete flow in Dev environment before production deployment

---

## 📝 Notes

- This feature is the foundation for all other SteelAxis features
- No tenant assignment yet - that will come in Phase 2 (Tenant Management)
- User registration creates account in Entra External ID only
- Future enhancement: Create user profile record in SteelAxis database
- Future enhancement: Multi-factor authentication (MFA)

---

**Plan Status:** ✅ Ready for approval  
**Estimated Effort:** 14 hours  
**Priority:** High (blocking all other features)
