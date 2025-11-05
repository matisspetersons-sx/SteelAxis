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

## 🗄️ Database Schema (Directory Database)

### Tenant Model
**Table:** `Tenants`  
**Purpose:** Store tenant/organization information

```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyRegistrationNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    
    // Subscription
    public Guid SubscriptionTierId { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; } = null!;
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string SubscriptionStatus { get; set; } = "Active"; // Active, Suspended, Cancelled, Trial
    
    // Feature Flags (JSON column)
    public string FeaturesJson { get; set; } = "{}"; // {"EN1090_Compliance": "Enabled", "AdvancedReporting": "Beta", ...}
    
    // Database Connection
    public string DatabaseServer { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string? DatabaseConnectionString { get; set; } // Encrypted
    
    // Payment
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string PaymentStatus { get; set; } = "None"; // None, Valid, Overdue, Demo
    public DateTime? LastPaymentDate { get; set; }
    
    // Settings
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string DefaultLanguage { get; set; } = "en-US"; // For localization
    public string TimeZone { get; set; } = "UTC";
    
    // Audit
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Navigation
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<UserInvitation> Invitations { get; set; } = new List<UserInvitation>();
}
```

### UserTenant Model
**Table:** `UserTenants`  
**Purpose:** Map users to tenants with roles (many-to-many with role)

```csharp
public class UserTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string EntraUserId { get; set; } = string.Empty; // Object ID from Entra External ID
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    
    // Role: Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker
    public string Role { get; set; } = "OfficeWorker";
    
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
```

### UserInvitation Model
**Table:** `UserInvitations`  
**Purpose:** Track pending user invitations

```csharp
public class UserInvitation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string Email { get; set; } = string.Empty;
    // Role: Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker
    public string Role { get; set; } = "OfficeWorker";
    public string? CustomMessage { get; set; }
    
    public string InvitationToken { get; set; } = string.Empty; // Unique token for acceptance link
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Expired, Cancelled
    
    public DateTime InvitedAt { get; set; }
    public string InvitedBy { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } // 7 days from InvitedAt
    public DateTime? AcceptedAt { get; set; }
    
    public string? EntraUserId { get; set; } // Set when accepted
}
```

### SubscriptionTier Model
**Table:** `SubscriptionTiers`  
**Purpose:** Define available subscription plans

```csharp
public class SubscriptionTier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Starter, Professional, Enterprise, Custom
    public string NameKey { get; set; } = string.Empty; // "subscription.tier.starter" for localization
    public string? Description { get; set; }
    public string? DescriptionKey { get; set; } // For localization
    
    // Limits
    public int MaxProjects { get; set; } = 0; // 0 = unlimited
    public int MaxUsers { get; set; } = 0; // 0 = unlimited
    public long MaxStorageGB { get; set; } = 0; // 0 = unlimited
    public int MaxMaterialCertificates { get; set; } = 0;
    
    // Pricing
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    
    // Features (Default features for this tier - JSON)
    public string DefaultFeaturesJson { get; set; } = "{}"; 
    // Example: {"EN1090_Compliance": "Enabled", "AdvancedReporting": "Enabled", "API_Access": "Disabled"}
    
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
```

### Feature Model
**Table:** `Features`  
**Purpose:** Define all system features for feature flagging

```csharp
public class Feature
{
    public Guid Id { get; set; }
    public string FeatureKey { get; set; } = string.Empty; // "EN1090_Compliance", "AdvancedReporting"
    public string Name { get; set; } = string.Empty;
    public string NameKey { get; set; } = string.Empty; // "feature.en1090.name" for localization
    public string? Description { get; set; }
    public string? DescriptionKey { get; set; } // For localization
    
    public string Category { get; set; } = "General"; // Compliance, Reporting, Integration, Advanced
    public string DevelopmentStatus { get; set; } = "Stable"; // Alpha, Beta, Stable, Deprecated
    
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
```

### PaymentMethod Model (Optional - for future)
**Table:** `PaymentMethods`  
**Purpose:** Store payment method details (Stripe placeholder)

```csharp
public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    public string? StripePaymentMethodId { get; set; }
    public string CardBrand { get; set; } = string.Empty; // Visa, Mastercard, Amex
    public string CardLast4 { get; set; } = string.Empty;
    public int CardExpMonth { get; set; }
    public int CardExpYear { get; set; }
    
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
```

### Demo Tenant Seed Data

```csharp
// Demo Tenant
var demoTenant = new Tenant
{
    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), // Fixed ID for demo
    CompanyName = "SteelAxis",
    CompanyRegistrationNumber = "DEMO-2025",
    Address = "Demo Street 1",
    City = "Riga",
    PostalCode = "LV-1000",
    Country = "Latvia",
    ContactEmail = "demo@steelaxis.eu",
    ContactPhone = "+371 20000000",
    
    SubscriptionTierId = enterpriseTierId, // Enterprise tier
    SubscriptionStartDate = DateTime.UtcNow,
    SubscriptionStatus = "Demo", // Special demo status
    
    FeaturesJson = JsonSerializer.Serialize(new Dictionary<string, string>
    {
        { "EN1090_Compliance", "Enabled" },
        { "AdvancedReporting", "Enabled" },
        { "API_Access", "Enabled" },
        { "MaterialTraceability", "Enabled" },
        { "ProjectManagement", "Enabled" },
        { "QualityControl", "Enabled" },
        { "CustomerPortal", "Beta" },
        { "MobileApp", "Alpha" }
    }),
    
    DatabaseServer = "steelaxis-dev-sql.database.windows.net",
    DatabaseName = "SteelAxis_Demo",
    
    PaymentStatus = "Demo", // Mark as demo - completely paid, unlimited time
    
    DefaultLanguage = "en-US", // Can easily switch to other languages
    TimeZone = "Europe/Riga",
    
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    CreatedBy = "System"
};

// Demo Admin User
var demoAdmin = new UserTenant
{
    Id = Guid.NewGuid(),
    TenantId = demoTenant.Id,
    EntraUserId = "{SET_AFTER_ENTRA_REGISTRATION}", // Will be set when admin registers
    Email = "demo@steelaxis.eu",
    DisplayName = "Demo Administrator",
    Role = "Admin",
    IsActive = true,
    JoinedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    CreatedBy = "System"
};
```

---

## 📡 Complete API Specification

### 1. Tenants Controller
**Base Route:** `/api/tenants`

#### POST /api/tenants
**Purpose:** Register new tenant with admin user  
**Authorization:** Anonymous (public registration)

**Request:**
```json
{
  "companyName": "Acme Steel Works",
  "companyRegistrationNumber": "LV40003123456",
  "vatNumber": "LV40003123456",
  "address": "Industrial Street 15",
  "city": "Riga",
  "postalCode": "LV-1050",
  "country": "Latvia",
  "contactEmail": "info@acmesteel.com",
  "contactPhone": "+371 67123456",
  
  "adminUser": {
    "email": "admin@acmesteel.com",
    "displayName": "John Smith",
    "password": "SecurePass123!" // Will be sent to Entra External ID
  },
  
  "subscriptionTierId": "guid",
  
  "paymentMethod": { // Optional - null for trial
    "stripeToken": "tok_visa", // Stripe token from Stripe.js
    "saveCard": true
  },
  
  "acceptedTerms": true,
  "defaultLanguage": "en-US"
}
```

**Response 201 Created:**
```json
{
  "tenantId": "guid",
  "companyName": "Acme Steel Works",
  "adminEmail": "admin@acmesteel.com",
  "subscriptionTier": "Professional",
  "subscriptionStatus": "Active",
  "databaseName": "SteelAxis_Tenant_abc123",
  "nextSteps": {
    "verifyEmail": true,
    "completeEntraRegistration": "https://entra-redirect-url",
    "setupPayment": false
  }
}
```

**Response 400 Bad Request:**
```json
{
  "errors": {
    "companyName": ["Company name is required"],
    "adminUser.email": ["Email already registered"]
  }
}
```

#### GET /api/tenants/current
**Purpose:** Get current user's tenant information  
**Authorization:** Authenticated user

**Response 200 OK:**
```json
{
  "tenantId": "guid",
  "companyName": "Acme Steel Works",
  "subscriptionTier": "Professional",
  "subscriptionStatus": "Active",
  "featuresEnabled": {
    "EN1090_Compliance": "Enabled",
    "AdvancedReporting": "Enabled",
    "API_Access": "Disabled"
  },
  "limits": {
    "maxProjects": 50,
    "maxUsers": 100,
    "currentProjects": 12,
    "currentUsers": 8
  },
  "userRole": "Admin",
  "logoUrl": "https://...",
  "primaryColor": "#1976D2",
  "defaultLanguage": "en-US"
}
```

#### PUT /api/tenants/{id}
**Purpose:** Update tenant settings  
**Authorization:** Tenant Admin only

**Request:**
```json
{
  "companyName": "Acme Steel Works Ltd",
  "address": "New Industrial Street 20",
  "city": "Riga",
  "postalCode": "LV-1055",
  "contactEmail": "info@acmesteel.com",
  "contactPhone": "+371 67123456",
  "logoUrl": "https://...",
  "primaryColor": "#1976D2",
  "defaultLanguage": "lv-LV"
}
```

**Response 200 OK:** (Updated tenant object)

#### PUT /api/tenants/{id}/subscription
**Purpose:** Change subscription tier (for demo purposes - easy switching)  
**Authorization:** Tenant Admin only

**Request:**
```json
{
  "subscriptionTierId": "guid",
  "reason": "Upgrading to Enterprise"
}
```

**Response 200 OK:**
```json
{
  "previousTier": "Professional",
  "newTier": "Enterprise",
  "effectiveDate": "2025-10-27T00:00:00Z",
  "newFeatures": ["API_Access", "CustomReports"],
  "newLimits": {
    "maxProjects": 0,
    "maxUsers": 0
  }
}
```

#### PUT /api/tenants/{id}/features
**Purpose:** Update feature flags for tenant  
**Authorization:** System Admin only (or Tenant Admin for beta features)

**Request:**
```json
{
  "features": {
    "EN1090_Compliance": "Enabled",
    "AdvancedReporting": "Enabled",
    "CustomerPortal": "Beta", // Enable beta feature for testing
    "NewFeatureX": "Alpha" // Special feature not in standard tiers
  }
}
```

**Response 200 OK:** (Updated features object)

---

### 2. Invitations Controller
**Base Route:** `/api/invitations`

#### POST /api/invitations
**Purpose:** Invite user to tenant  
**Authorization:** Tenant Admin only

**Request:**
```json
{
  "email": "user@example.com",
  "role": "OfficeWorker", // Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker
  "customMessage": "Welcome to our team! Looking forward to working with you."
}
```

**Response 201 Created:**
```json
{
  "invitationId": "guid",
  "email": "user@example.com",
  "role": "OfficeWorker",
  "invitationUrl": "https://app.steelaxis.eu/accept-invitation?token=abc123xyz",
  "expiresAt": "2025-11-03T12:00:00Z",
  "status": "Pending"
}
```

#### GET /api/invitations
**Purpose:** List all invitations for tenant  
**Authorization:** Tenant Admin only

**Query Parameters:**
- `status` (optional): Pending, Accepted, Expired, Cancelled
- `pageNumber` (optional): Default 1
- `pageSize` (optional): Default 20

**Response 200 OK:**
```json
{
  "items": [
    {
      "invitationId": "guid",
      "email": "user@example.com",
      "role": "User",
      "status": "Pending",
      "invitedAt": "2025-10-27T12:00:00Z",
      "invitedBy": "admin@acmesteel.com",
      "expiresAt": "2025-11-03T12:00:00Z"
    }
  ],
  "totalCount": 15,
  "pageNumber": 1,
  "pageSize": 20
}
```

#### POST /api/invitations/{token}/accept
**Purpose:** Accept invitation and create user account  
**Authorization:** Anonymous (uses invitation token)

**Request:**
```json
{
  "token": "abc123xyz",
  "displayName": "Jane Doe",
  "password": "SecurePass123!", // For Entra External ID
  "acceptedTerms": true
}
```

**Response 200 OK:**
```json
{
  "tenantId": "guid",
  "companyName": "Acme Steel Works",
  "role": "User",
  "email": "user@example.com",
  "entraRegistrationUrl": "https://entra-redirect-url",
  "nextSteps": {
    "verifyEmail": true,
    "completeEntraRegistration": true
  }
}
```

**Response 404 Not Found:**
```json
{
  "error": "Invitation not found or expired"
}
```

#### POST /api/invitations/{id}/resend
**Purpose:** Resend invitation email  
**Authorization:** Tenant Admin only

**Response 200 OK:**
```json
{
  "invitationId": "guid",
  "email": "user@example.com",
  "newExpiresAt": "2025-11-10T12:00:00Z",
  "message": "Invitation resent successfully"
}
```

#### DELETE /api/invitations/{id}
**Purpose:** Cancel pending invitation  
**Authorization:** Tenant Admin only

**Response 204 No Content**

---

### 3. Users Controller
**Base Route:** `/api/users`

#### GET /api/users
**Purpose:** List all users in tenant  
**Authorization:** Authenticated user (own tenant)

**Query Parameters:**
- `role` (optional): Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker
- `isActive` (optional): true, false
- `search` (optional): Search by email or name

**Response 200 OK:**
```json
{
  "items": [
    {
      "userId": "guid",
      "entraUserId": "guid",
      "email": "admin@acmesteel.com",
      "displayName": "John Smith",
      "role": "Admin",
      "isActive": true,
      "joinedAt": "2025-01-15T10:00:00Z",
      "lastLoginAt": "2025-10-27T09:30:00Z"
    }
  ],
  "totalCount": 8
}
```

#### GET /api/users/{id}
**Purpose:** Get specific user details  
**Authorization:** Authenticated user (own tenant)

**Response 200 OK:**
```json
{
  "userId": "guid",
  "entraUserId": "guid",
  "email": "admin@acmesteel.com",
  "displayName": "John Smith",
  "role": "Admin",
  "isActive": true,
  "joinedAt": "2025-01-15T10:00:00Z",
  "lastLoginAt": "2025-10-27T09:30:00Z",
  "permissions": [
    "tenant.manage",
    "users.invite",
    "users.manage",
    "projects.create",
    "projects.delete"
  ]
}
```

#### PUT /api/users/{id}/role
**Purpose:** Change user role  
**Authorization:** Tenant Admin only

**Request:**
```json
{
  "role": "UpperManagement", // Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker
  "reason": "Promoting to upper management for strategic planning"
}
```

**Response 200 OK:**
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "previousRole": "LowerManagement",
  "newRole": "UpperManagement",
  "updatedAt": "2025-10-27T12:00:00Z"
}
```

#### PUT /api/users/{id}/deactivate
**Purpose:** Deactivate user (soft delete)  
**Authorization:** Tenant Admin only

**Request:**
```json
{
  "reason": "User left company"
}
```

**Response 200 OK:**
```json
{
  "userId": "guid",
  "email": "user@example.com",
  "isActive": false,
  "deactivatedAt": "2025-10-27T12:00:00Z"
}
```

#### PUT /api/users/{id}/reactivate
**Purpose:** Reactivate deactivated user  
**Authorization:** Tenant Admin only

**Response 200 OK**

---

### 4. Subscriptions Controller
**Base Route:** `/api/subscriptions`

#### GET /api/subscriptions/tiers
**Purpose:** List all available subscription tiers  
**Authorization:** Anonymous or Authenticated

**Response 200 OK:**
```json
{
  "tiers": [
    {
      "tierId": "guid",
      "name": "Starter",
      "nameKey": "subscription.tier.starter",
      "description": "Perfect for small fabrication shops",
      "descriptionKey": "subscription.tier.starter.description",
      "monthlyPrice": 49.00,
      "annualPrice": 490.00,
      "currency": "EUR",
      "limits": {
        "maxProjects": 5,
        "maxUsers": 10,
        "maxStorageGB": 10
      },
      "features": {
        "MaterialTraceability": "Enabled",
        "BasicReporting": "Enabled",
        "EN1090_Compliance": "Disabled"
      },
      "isPopular": false
    },
    {
      "tierId": "guid",
      "name": "Professional",
      "nameKey": "subscription.tier.professional",
      "monthlyPrice": 149.00,
      "annualPrice": 1490.00,
      "limits": {
        "maxProjects": 50,
        "maxUsers": 100,
        "maxStorageGB": 100
      },
      "features": {
        "MaterialTraceability": "Enabled",
        "AdvancedReporting": "Enabled",
        "EN1090_Compliance": "Enabled",
        "QualityControl": "Enabled"
      },
      "isPopular": true
    },
    {
      "tierId": "guid",
      "name": "Enterprise",
      "nameKey": "subscription.tier.enterprise",
      "monthlyPrice": 499.00,
      "annualPrice": 4990.00,
      "limits": {
        "maxProjects": 0,
        "maxUsers": 0,
        "maxStorageGB": 0
      },
      "features": {
        "MaterialTraceability": "Enabled",
        "AdvancedReporting": "Enabled",
        "EN1090_Compliance": "Enabled",
        "QualityControl": "Enabled",
        "API_Access": "Enabled",
        "CustomerPortal": "Enabled",
        "CustomIntegrations": "Enabled"
      },
      "isPopular": false
    }
  ]
}
```

#### GET /api/subscriptions/current
**Purpose:** Get current tenant's subscription details  
**Authorization:** Authenticated user

**Response 200 OK:**
```json
{
  "tenantId": "guid",
  "tier": "Professional",
  "status": "Active",
  "startDate": "2025-01-15T00:00:00Z",
  "endDate": null,
  "paymentStatus": "Valid",
  "lastPaymentDate": "2025-10-01T00:00:00Z",
  "nextBillingDate": "2025-11-01T00:00:00Z",
  "monthlyPrice": 149.00,
  "currency": "EUR",
  "usage": {
    "projects": 12,
    "maxProjects": 50,
    "users": 8,
    "maxUsers": 100,
    "storageGB": 23.5,
    "maxStorageGB": 100
  }
}
```

---

### 5. Features Controller
**Base Route:** `/api/features`

#### GET /api/features
**Purpose:** List all available features  
**Authorization:** Authenticated user

**Response 200 OK:**
```json
{
  "features": [
    {
      "featureKey": "EN1090_Compliance",
      "name": "EN 1090 Compliance Management",
      "nameKey": "feature.en1090.name",
      "description": "Complete EN 1090 compliance tracking, NCR management, and documentation",
      "descriptionKey": "feature.en1090.description",
      "category": "Compliance",
      "developmentStatus": "Stable",
      "requiredTier": "Professional",
      "isEnabledForCurrentTenant": true
    },
    {
      "featureKey": "CustomerPortal",
      "name": "Customer Portal",
      "nameKey": "feature.customerportal.name",
      "category": "Integration",
      "developmentStatus": "Beta",
      "requiredTier": "Enterprise",
      "isEnabledForCurrentTenant": false
    }
  ]
}
```

#### GET /api/features/tenant
**Purpose:** Get features enabled for current tenant  
**Authorization:** Authenticated user

**Response 200 OK:**
```json
{
  "tenantId": "guid",
  "subscriptionTier": "Enterprise",
  "enabledFeatures": {
    "EN1090_Compliance": "Enabled",
    "AdvancedReporting": "Enabled",
    "API_Access": "Enabled",
    "CustomerPortal": "Beta", // Special beta access
    "NewFeatureX": "Alpha" // Special feature for testing
  },
  "availableFeatures": ["MobileApp", "CustomIntegrations"]
}
```

---

## 🎨 Updated UI Components & Pages

### 1. Landing Page (Index.razor)
**Location:** `SteelAxis.Web/Components/Pages/Index.razor`

**Localization Support:**
```razor
@inject IStringLocalizer<Index> Localizer

<MudText Typo="Typo.h2">@Localizer["landing.hero.title"]</MudText>
<MudText Typo="Typo.body1">@Localizer["landing.hero.subtitle"]</MudText>
```

**Layout:** (Same as before with localized strings)

---

### 2. Tenant Registration Page (RegisterTenant.razor)
**Location:** `SteelAxis.Web/Components/Pages/RegisterTenant.razor`

**Multi-step form:**

**Step 1: Company Information**
```
┌─────────────────────────────────────────────────┐
│   Create Your SteelAxis Account - Step 1 of 4   │
│                                                  │
│   Company Information                            │
│                                                  │
│   Company Name *                                 │
│   [_____________________________________]        │
│                                                  │
│   Registration Number                            │
│   [_____________________________________]        │
│                                                  │
│   VAT Number                                     │
│   [_____________________________________]        │
│                                                  │
│   Address *                                      │
│   [_____________________________________]        │
│                                                  │
│   City *              Postal Code *              │
│   [______________]    [______________]           │
│                                                  │
│   Country *                                      │
│   [▼ Select Country _______________]             │
│                                                  │
│   Contact Email *                                │
│   [_____________________________________]        │
│                                                  │
│   Contact Phone                                  │
│   [_____________________________________]        │
│                                                  │
│                     [Next →]                     │
└─────────────────────────────────────────────────┘
```

**Step 2: Admin User**
```
┌─────────────────────────────────────────────────┐
│   Create Your SteelAxis Account - Step 2 of 4   │
│                                                  │
│   Administrator Account                          │
│                                                  │
│   Email Address *                                │
│   [_____________________________________]        │
│                                                  │
│   Full Name *                                    │
│   [_____________________________________]        │
│                                                  │
│   Password *                                     │
│   [_____________________________________]        │
│                                                  │
│   Confirm Password *                             │
│   [_____________________________________]        │
│                                                  │
│   Password Requirements:                         │
│   ✓ At least 8 characters                       │
│   ✓ One uppercase letter                        │
│   ✓ One lowercase letter                        │
│   ✓ One number                                   │
│   ✓ One special character                       │
│                                                  │
│   Note: First user is automatically assigned     │
│   Admin role with full system access.            │
│                                                  │
│   [← Back]              [Next →]                 │
└─────────────────────────────────────────────────┘
```

**Step 3: Subscription Selection**
```
┌─────────────────────────────────────────────────┐
│   Create Your SteelAxis Account - Step 3 of 4   │
│                                                  │
│   Choose Your Plan                               │
│                                                  │
│ ┌───────┐  ┌──────────┐  ┌───────────┐         │
│ │Starter│  │Professional│ │ Enterprise│         │
│ │       │  │  (Popular) │  │           │         │
│ │€49/mo │  │  €149/mo  │  │  €499/mo  │         │
│ │       │  │           │  │           │         │
│ │5 proj │  │ 50 projects│ │ Unlimited │         │
│ │10 user│  │ 100 users  │ │ Unlimited │         │
│ │       │  │           │  │           │         │
│ │[Select│  │  [Select] │  │ [Select]  │         │
│ └───────┘  └──────────┘  └───────────┘         │
│                                                  │
│   Selected: Professional (€149/month)            │
│                                                  │
│   ☐ Annual billing (save 17% - €1,490/year)     │
│                                                  │
│   Preferred Language                             │
│   [▼ English (US) _______________]               │
│                                                  │
│   [← Back]              [Next →]                 │
└─────────────────────────────────────────────────┘
```

**Step 4: Payment & Confirmation**
```
┌─────────────────────────────────────────────────┐
│   Create Your SteelAxis Account - Step 4 of 4   │
│                                                  │
│   Payment Information                            │
│                                                  │
│   ┌─ Stripe Card Element ───────────────────┐   │
│   │ Card Number                              │   │
│   │ [________________________________]       │   │
│   │                                          │   │
│   │ Expiry          CVC                      │   │
│   │ [MM / YY]       [___]                    │   │
│   └──────────────────────────────────────────┘   │
│                                                  │
│   ☐ Save card for future payments                │
│                                                  │
│   Order Summary:                                 │
│   Professional Plan            €149.00/month     │
│   Setup Fee                    €0.00             │
│   ─────────────────────────────────────          │
│   Due Today                    €149.00           │
│                                                  │
│   ☐ I accept the Terms of Service and           │
│      Privacy Policy                              │
│                                                  │
│   [← Back]       [Complete Registration]         │
└─────────────────────────────────────────────────┘

NOTE: Demo tenant will show:
│   Payment Status: DEMO - Fully Paid             │
│   No payment required for demo account          │
```

**Components:**
- `MudStepper` for multi-step form
- `MudTextField` for all text inputs with validation
- `MudSelect` for country and language dropdowns
- `MudCard` components for subscription tier cards
- Stripe Elements integration (placeholder for demo)
- `MudCheckBox` for terms acceptance
- `MudButton` for navigation and submit
- Form validation with `DataAnnotationsValidator`

---

### 3. User Invitation Page (InviteUser.razor)
**Location:** `SteelAxis.Web/Components/Pages/Admin/InviteUser.razor`
**Authorization:** `@attribute [Authorize(Roles = "Admin")]`

**Layout:**
```
┌─────────────────────────────────────────────────┐
│   Invite Team Member                             │
│                                                  │
│   Email Address *                                │
│   [_____________________________________]        │
│                                                  │
│   Role *                                         │
│   [▼ OfficeWorker __________________]            │
│   ├─ Admin (Full system access)                  │
│   ├─ UpperManagement (Strategic oversight)       │
│   ├─ LowerManagement (Operational management)    │
│   ├─ Supervisor (Team supervision)               │
│   ├─ ShopFloorWorker (Production work)           │
│   └─ OfficeWorker (Administrative work)          │
│                                                  │
│   Personal Message (Optional)                    │
│   [_____________________________________]        │
│   [_____________________________________]        │
│   [_____________________________________]        │
│                                                  │
│   [Send Invitation]                              │
└─────────────────────────────────────────────────┘

│   Pending Invitations                            │
│                                                  │
│ ┌───────────────────────────────────────────┐   │
│ │Email           │Role          │Status│Actions││
│ ├───────────────────────────────────────────┤   │
│ │user@email.com  │OfficeWorker  │Pending│[↻][✕]│
│ │mgr@email.com   │LowerManagement│Accepted│     │
│ │super@email.com │Supervisor    │Expired│[↻]  │
│ └───────────────────────────────────────────┘   │
└─────────────────────────────────────────────────┘
```

**Components:**
- `MudCard` for invitation form
- `MudTextField` for email
- `MudSelect` for role with localized role names and descriptions
- `MudTextField` (multiline) for custom message
- `MudDataGrid` for invitation list with filtering
- `MudIconButton` for Resend and Cancel actions
- `MudChip` for status display (color-coded)

---

### 4. Accept Invitation Page (AcceptInvitation.razor)
**Location:** `SteelAxis.Web/Components/Pages/AcceptInvitation.razor`
**Authorization:** Anonymous (uses invitation token)

**Layout:**
```
┌─────────────────────────────────────────────────┐
│   You've Been Invited to Join SteelAxis         │
│                                                  │
│   Acme Steel Works has invited you to join      │
│   their team on SteelAxis.                      │
│                                                  │
│   Inviter: admin@acmesteel.com                   │
│   Role: Office Worker                            │
│                                                  │
│   Message from admin:                            │
│   "Welcome to our team! Looking forward to       │
│    working with you."                            │
│                                                  │
│   Create Your Account                            │
│                                                  │
│   Email Address (pre-filled)                     │
│   [user@example.com______________] (disabled)    │
│                                                  │
│   Full Name *                                    │
│   [_____________________________________]        │
│                                                  │
│   Password *                                     │
│   [_____________________________________]        │
│                                                  │
│   Confirm Password *                             │
│   [_____________________________________]        │
│                                                  │
│   ☐ I accept the Terms of Service               │
│                                                  │
│   [Accept Invitation & Create Account]           │
└─────────────────────────────────────────────────┘
```

**Components:**
- `MudCard` for invitation details
- `MudAlert` showing company and inviter info
- `MudTextField` (disabled) for pre-filled email
- `MudTextField` for name and password
- Password strength indicator
- `MudCheckBox` for terms
- Redirect to Entra External ID for account creation

---

### 5. User Management Page (ManageUsers.razor)
**Location:** `SteelAxis.Web/Components/Pages/Admin/ManageUsers.razor`
**Authorization:** `@attribute [Authorize(Roles = "Admin")]`

**Layout:**
```
┌─────────────────────────────────────────────────┐
│   Team Members                                   │
│                                                  │
│   [🔍 Search...______]  [⚙️ Role ▼] [+ Invite]  │
│                                                  │
│ ┌───────────────────────────────────────────┐   │
│ │Name       │Email        │Role │Last Login │   │
│ ├───────────────────────────────────────────┤   │
│ │John Smith │admin@...    │Admin│10/27 9:30│⚙│ │
│ │Jane Doe   │jane@...     │User │10/26 14:20│⚙│ │
│ │Bob View   │bob@...      │Viewer│10/25 8:00│⚙│ │
│ └───────────────────────────────────────────┘   │
│                                                  │
│   Total Users: 8 / 100                           │
└─────────────────────────────────────────────────┘
```

**User Actions Menu (⚙):**
- Change Role
- Deactivate User
- View Activity Log

---

## 🔐 Role-Based Authorization

### User Roles Hierarchy

```
Admin (Full System Access)
  ↓
UpperManagement (Strategic Oversight)
  ↓
LowerManagement (Operational Management)
  ↓
Supervisor (Team Supervision)
  ↓
ShopFloorWorker (Production Work)
  ↓
OfficeWorker (Administrative Work)
```

### Authorization Policies

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    // Admin-only features
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("Admin"));
    
    // Management level (Admin, Upper, Lower)
    options.AddPolicy("RequireManagement", policy => 
        policy.RequireRole("Admin", "UpperManagement", "LowerManagement"));
    
    // Supervisory level and above
    options.AddPolicy("RequireSupervisory", policy => 
        policy.RequireRole("Admin", "UpperManagement", "LowerManagement", "Supervisor"));
    
    // All except shop floor workers (office and above)
    options.AddPolicy("RequireOfficeAccess", policy => 
        policy.RequireRole("Admin", "UpperManagement", "LowerManagement", "Supervisor", "OfficeWorker"));
    
    // All authenticated users
    options.AddPolicy("RequireAuthenticated", policy => 
        policy.RequireAuthenticatedUser());
    
    // Feature-based policies
    options.AddPolicy("RequireEN1090", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("feature:EN1090_Compliance", "Enabled")));
});
```

### Role Permissions Matrix

| Feature | Admin | Upper Mgmt | Lower Mgmt | Supervisor | Shop Floor | Office |
|---------|-------|------------|------------|------------|------------|--------|
| **System Administration** |
| Tenant Settings | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Subscription Management | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Feature Flags | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **User Management** |
| Invite Users | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| View Users | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Change User Roles | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Deactivate Users | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Project Management** |
| View Projects | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Create Projects | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| Edit Projects | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Delete Projects | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Assign Resources | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| View Project Financials | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| **Materials & Inventory** |
| View Materials | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Add Materials | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Edit Materials | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Delete Materials | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| Approve Purchase Orders | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| Material Certificates | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Quality Control (EN 1090)** |
| View NCRs | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Create NCRs | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Assign NCR Actions | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Close NCRs | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| Approve Corrective Actions | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| View Quality Reports | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| **Production & Shop Floor** |
| View Work Orders | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Update Work Progress | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Record Time | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| View Production Schedule | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Modify Production Schedule | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Reports & Analytics** |
| View Standard Reports | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| View Financial Reports | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| Export Data | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Create Custom Reports | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Documents & Files** |
| View Documents | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Upload Documents | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Delete Documents | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Approve Documents | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |

### NavMenu Updates

```razor
@* SteelAxis.Web/Components/Layout/NavMenu.razor *@

<AuthorizeView>
    <Authorized>
        <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">
            @Localizer["nav.home"]
        </MudNavLink>
        
        @* All authenticated users *@
        <MudNavLink Href="/projects" Icon="@Icons.Material.Filled.Assignment">
            @Localizer["nav.projects"]
        </MudNavLink>
        
        @* All users can view materials *@
        <MudNavLink Href="/materials" Icon="@Icons.Material.Filled.Inventory">
            @Localizer["nav.materials"]
        </MudNavLink>
        
        @* Shop floor and supervisors see production *@
        <AuthorizeView Roles="Admin,UpperManagement,LowerManagement,Supervisor,ShopFloorWorker">
            <MudNavLink Href="/production" Icon="@Icons.Material.Filled.Precision Manufacturing">
                @Localizer["nav.production"]
            </MudNavLink>
        </AuthorizeView>
        
        @* Quality control for most roles *@
        <AuthorizeView Roles="Admin,UpperManagement,LowerManagement,Supervisor,ShopFloorWorker">
            <MudNavLink Href="/quality" Icon="@Icons.Material.Filled.VerifiedUser">
                @Localizer["nav.quality"]
            </MudNavLink>
        </AuthorizeView>
        
        @* Reports for office and above *@
        <AuthorizeView Roles="Admin,UpperManagement,LowerManagement,Supervisor,OfficeWorker">
            <MudNavLink Href="/reports" Icon="@Icons.Material.Filled.Assessment">
                @Localizer["nav.reports"]
            </MudNavLink>
        </AuthorizeView>
        
        @* Management features *@
        <AuthorizeView Roles="Admin,UpperManagement,LowerManagement">
            <MudNavGroup Title="@Localizer["nav.management"]" Icon="@Icons.Material.Filled.BusinessCenter">
                <MudNavLink Href="/management/users" Icon="@Icons.Material.Filled.People">
                    @Localizer["nav.management.users"]
                </MudNavLink>
                <MudNavLink Href="/management/resources" Icon="@Icons.Material.Filled.Engineering">
                    @Localizer["nav.management.resources"]
                </MudNavLink>
                <MudNavLink Href="/management/planning" Icon="@Icons.Material.Filled.CalendarMonth">
                    @Localizer["nav.management.planning"]
                </MudNavLink>
            </MudNavGroup>
        </AuthorizeView>
        
        @* Admin only *@
        <AuthorizeView Roles="Admin">
            <MudNavGroup Title="@Localizer["nav.admin"]" Icon="@Icons.Material.Filled.AdminPanelSettings">
                <MudNavLink Href="/admin/users" Icon="@Icons.Material.Filled.People">
                    @Localizer["nav.admin.users"]
                </MudNavLink>
                <MudNavLink Href="/admin/invitations" Icon="@Icons.Material.Filled.PersonAdd">
                    @Localizer["nav.admin.invitations"]
                </MudNavLink>
                <MudNavLink Href="/admin/tenant" Icon="@Icons.Material.Filled.Business">
                    @Localizer["nav.admin.tenant"]
                </MudNavLink>
                <MudNavLink Href="/admin/subscription" Icon="@Icons.Material.Filled.Payment">
                    @Localizer["nav.admin.subscription"]
                </MudNavLink>
                <MudNavLink Href="/admin/features" Icon="@Icons.Material.Filled.Flag">
                    @Localizer["nav.admin.features"]
                </MudNavLink>
            </MudNavGroup>
        </AuthorizeView>
        
        <MudDivider />
        
        <MudNavLink Href="/profile" Icon="@Icons.Material.Filled.Person">
            @Localizer["nav.profile"]
        </MudNavLink>
        <MudNavLink Href="/logout" Icon="@Icons.Material.Filled.Logout">
            @Localizer["nav.logout"]
        </MudNavLink>
    </Authorized>
    <NotAuthorized>
        <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">
            @Localizer["nav.home"]
        </MudNavLink>
        <MudNavLink Href="/login" Icon="@Icons.Material.Filled.Login">
            @Localizer["nav.login"]
        </MudNavLink>
        <MudNavLink Href="/register" Icon="@Icons.Material.Filled.PersonAdd">
            @Localizer["nav.register"]
        </MudNavLink>
    </NotAuthorized>
</AuthorizeView>
```

---

## 🌐 Localization Strategy

### Resource Files Structure

```
SteelAxis.Web/
└── Resources/
    ├── Components/
    │   └── Pages/
    │       ├── Index.en-US.resx
    │       ├── Index.lv-LV.resx
    │       ├── RegisterTenant.en-US.resx
    │       └── RegisterTenant.lv-LV.resx
    └── Shared/
        ├── Common.en-US.resx
        ├── Common.lv-LV.resx
        ├── Roles.en-US.resx
        └── Roles.lv-LV.resx
```

### Example Resource Entries

**Common.en-US.resx:**
```xml
<data name="button.save" xml:space="preserve">
  <value>Save</value>
</data>
<data name="button.cancel" xml:space="preserve">
  <value>Cancel</value>
</data>
<data name="button.next" xml:space="preserve">
  <value>Next</value>
</data>
<data name="button.back" xml:space="preserve">
  <value>Back</value>
</data>
```

**Common.lv-LV.resx:**
```xml
<data name="button.save" xml:space="preserve">
  <value>Saglabāt</value>
</data>
<data name="button.cancel" xml:space="preserve">
  <value>Atcelt</value>
</data>
<data name="button.next" xml:space="preserve">
  <value>Tālāk</value>
</data>
<data name="button.back" xml:space="preserve">
  <value>Atpakaļ</value>
</data>
```

**Roles.en-US.resx:**
```xml
<data name="role.admin" xml:space="preserve">
  <value>Administrator</value>
</data>
<data name="role.admin.description" xml:space="preserve">
  <value>Full system access and configuration</value>
</data>
<data name="role.uppermanagement" xml:space="preserve">
  <value>Upper Management</value>
</data>
<data name="role.uppermanagement.description" xml:space="preserve">
  <value>Strategic oversight and high-level decision making</value>
</data>
<data name="role.lowermanagement" xml:space="preserve">
  <value>Lower Management</value>
</data>
<data name="role.lowermanagement.description" xml:space="preserve">
  <value>Operational management and team coordination</value>
</data>
<data name="role.supervisor" xml:space="preserve">
  <value>Supervisor</value>
</data>
<data name="role.supervisor.description" xml:space="preserve">
  <value>Direct team supervision and task assignment</value>
</data>
<data name="role.shopfloorworker" xml:space="preserve">
  <value>Shop Floor Worker</value>
</data>
<data name="role.shopfloorworker.description" xml:space="preserve">
  <value>Production and fabrication work</value>
</data>
<data name="role.officeworker" xml:space="preserve">
  <value>Office Worker</value>
</data>
<data name="role.officeworker.description" xml:space="preserve">
  <value>Administrative and office-based tasks</value>
</data>
```

### Usage in Components

```razor
@inject IStringLocalizer<RegisterTenant> Localizer
@inject IStringLocalizer<SharedResources> SharedLocalizer

<MudTextField Label="@Localizer["companyName.label"]" 
              Placeholder="@Localizer["companyName.placeholder"]"
              HelperText="@Localizer["companyName.helper"]" />

<MudButton>@SharedLocalizer["button.save"]</MudButton>

<MudSelect Label="@Localizer["role.label"]">
    <MudSelectItem Value="Admin">
        @RoleLocalizer["role.admin"]
        <MudText Typo="Typo.caption">@RoleLocalizer["role.admin.description"]</MudText>
    </MudSelectItem>
    <MudSelectItem Value="UpperManagement">
        @RoleLocalizer["role.uppermanagement"]
        <MudText Typo="Typo.caption">@RoleLocalizer["role.uppermanagement.description"]</MudText>
    </MudSelectItem>
    <MudSelectItem Value="LowerManagement">
        @RoleLocalizer["role.lowermanagement"]
        <MudText Typo="Typo.caption">@RoleLocalizer["role.lowermanagement.description"]</MudText>
    </MudSelectItem>
    <MudSelectItem Value="Supervisor">
        @RoleLocalizer["role.supervisor"]
        <MudText Typo="Typo.caption">@RoleLocalizer["role.supervisor.description"]</MudText>
    </MudSelectItem>
    <MudSelectItem Value="ShopFloorWorker">
        @RoleLocalizer["role.shopfloorworker"]
        <MudText Typo="Typo.caption">@RoleLocalizer["role.shopfloorworker.description"]</MudText>
    </MudSelectItem>
    <MudSelectItem Value="OfficeWorker">
        @RoleLocalizer["role.officeworker"]
        <MudText Typo="Typo.caption">@RoleLocalizer["role.officeworker.description"]</MudText>
    </MudSelectItem>
</MudSelect>
```

### Language Switching

```csharp
// Program.cs
builder.Services.AddLocalization(options => 
    options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "lv-LV", "de-DE", "ru-RU" };
    options.SetDefaultCulture("en-US")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Language selector component
<MudSelect T="string" Value="@CurrentLanguage" ValueChanged="OnLanguageChanged">
    <MudSelectItem Value="en-US">🇬🇧 English</MudSelectItem>
    <MudSelectItem Value="lv-LV">🇱🇻 Latviešu</MudSelectItem>
    <MudSelectItem Value="de-DE">🇩🇪 Deutsch</MudSelectItem>
    <MudSelectItem Value="ru-RU">🇷🇺 Русский</MudSelectItem>
</MudSelect>
```

---

## 🔧 Entra External ID Configuration Requirements

### Custom User Attributes Configuration

1. **Navigate to Entra Admin Center**
   - Portal: https://entra.microsoft.com
   - Tenant: steelaxistenants.onmicrosoft.com

2. **Create Custom Attributes**
   - Go to: **External Identities** → **Custom user attributes**
   - Add attribute: `extension_tenantId` (String)
   - Add attribute: `extension_role` (String)

3. **Add to User Flows**
   - Go to: **External Identities** → **User flows**
   - Edit sign-up flow
   - Add custom attributes to collection during sign-up
   - Include in token claims

### Token Claims Configuration

**App Registration → Token Configuration:**

Add optional claims:
- `extension_tenantId` → Map to `tenantId`
- `extension_role` → Map to `role`
- `email`
- `name`
- `oid` (Object ID)

**Example token payload:**
```json
{
  "oid": "00000000-0000-0000-0000-000000000001",
  "email": "demo@steelaxis.eu",
  "name": "Demo Administrator",
  "tenantId": "00000000-0000-0000-0000-000000000001",
  "role": "Admin",
  "aud": "api://steelaxis-api",
  "iss": "https://steelaxistenants.b2clogin.com/..."
}
```

### Invitation Email Templates

**Navigate to:** **External Identities** → **Email templates**

**Template for User Invitation:**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
</head>
<body style="font-family: Arial, sans-serif;">
    <h2>You've been invited to join {CompanyName} on SteelAxis</h2>
    
    <p>Hello,</p>
    
    <p>{InviterName} ({InviterEmail}) has invited you to join their team on SteelAxis as a <strong>{Role}</strong>.</p>
    
    <p>{CustomMessage}</p>
    
    <p>Click the button below to accept the invitation and create your account:</p>
    
    <a href="{AcceptInvitationUrl}" style="display: inline-block; padding: 12px 24px; background-color: #1976D2; color: white; text-decoration: none; border-radius: 4px;">Accept Invitation</a>
    
    <p>Or copy and paste this link into your browser:<br>
    {AcceptInvitationUrl}</p>
    
    <p>This invitation will expire on {ExpirationDate}.</p>
    
    <p>If you didn't expect this invitation, you can safely ignore this email.</p>
    
    <hr>
    <p style="font-size: 12px; color: #666;">
        SteelAxis - Steel Fabrication Management Platform<br>
        © 2025 SteelAxis. All rights reserved.
    </p>
</body>
</html>
```

### User Flows Configuration

**Sign-up and sign-in flow:**
1. Enable email verification
2. Collect custom attributes during sign-up
3. Return tenantId and role in token claims
4. Redirect to app after sign-in: `https://app.steelaxis.eu/signin-oidc`

---

## 🧪 Implementation Phases

### Phase 1: Database Models & Migrations (4 hours)
1. Create all models in `SteelAxis.Shared/Models/Directory/`
2. Update `DirectoryDbContext` with DbSets
3. Create initial migration
4. Seed subscription tiers data
5. Seed demo tenant data
6. Test migration on local SQL Server

**Deliverables:**
- All entity models created
- DbContext configured
- Migration file generated
- Seed data script created

---

### Phase 2: Directory Service APIs (8 hours)

**2.1 Tenant Management (3 hours)**
- Create `ITenantService` and `TenantService`
- Implement tenant registration logic
- Create tenant database provisioning
- Implement `TenantsController`
- Test POST /api/tenants endpoint

**2.2 User Management (2 hours)**
- Create `IUserManagementService` and `UserManagementService`
- Implement user-tenant mapping
- Implement `UsersController`
- Test user endpoints

**2.3 Invitation System (3 hours)**
- Create `IInvitationService` and `InvitationService`
- Implement invitation token generation
- Create email templates
- Implement `InvitationsController`
- Test invitation flow

**Deliverables:**
- All service interfaces and implementations
- All API controllers with endpoints
- Unit tests for services
- Integration tests for APIs

---

### Phase 3: Tenant Registration UI (6 hours)

**3.1 Registration Form (4 hours)**
- Create `RegisterTenant.razor` with 4-step wizard
- Implement form validation
- Add Stripe Elements placeholder
- Add subscription tier selection
- Integrate with tenant registration API

**3.2 Success & Error Handling (2 hours)**
- Create success page with next steps
- Implement error handling and display
- Add loading states
- Test complete flow

**Deliverables:**
- Complete multi-step registration form
- Subscription tier selection UI
- Payment placeholder (Stripe)
- Error handling and validation

---

### Phase 4: User Invitation System (6 hours)

**4.1 Invitation UI (3 hours)**
- Create `InviteUser.razor` (admin only)
- Add email input and role selection
- Create invitation list with MudDataGrid
- Add resend and cancel actions

**4.2 Accept Invitation UI (3 hours)**
- Create `AcceptInvitation.razor`
- Display invitation details
- Create account form
- Integrate with Entra External ID
- Handle token validation

**Deliverables:**
- Complete invitation UI for admins
- Invitation acceptance flow
- Email integration
- Entra External ID integration

---

### Phase 5: Role-Based Authorization (4 hours)

**5.1 Authorization Policies (2 hours)**
- Define authorization policies in `Program.cs`
- Create custom authorization attributes
- Implement role-based access control
- Add claims transformation

**5.2 UI Authorization (2 hours)**
- Update `NavMenu.razor` with role-based visibility
- Add `[Authorize(Roles = "...")]` to pages
- Create access denied page
- Test all role combinations

**Deliverables:**
- Complete authorization policies
- Role-based navigation
- Protected pages and endpoints
- Access control testing

---

### Phase 6: Subscription & Feature Management (6 hours)

**6.1 Subscription Management (3 hours)**
- Create `SubscriptionsController`
- Implement tier management service
- Create subscription details UI
- Add tier switching for demo

**6.2 Feature Flags (3 hours)**
- Create `FeaturesController`
- Implement feature flag service
- Create feature management UI (admin)
- Add feature-based authorization

**Deliverables:**
- Subscription management APIs and UI
- Feature flag system
- Demo tier switching capability
- Feature-based access control

---

### Phase 7: Demo Tenant Seeding (2 hours)

**7.1 Seed Script (1 hour)**
- Create demo tenant seed data
- Seed subscription tiers
- Seed features
- Create admin user record (pending Entra)

**7.2 Demo Configuration (1 hour)**
- Configure demo payment status
- Enable all Enterprise features
- Set up tier switching
- Add demo data indicators

**Deliverables:**
- Complete demo tenant in database
- All features enabled
- Easy tier switching
- Demo indicators in UI

---

### Phase 8: Testing & Documentation (6 hours)

**8.1 End-to-End Testing (3 hours)**
- Test complete registration flow
- Test invitation flow
- Test all role permissions
- Test tier switching
- Test feature flags

**8.2 Documentation (3 hours)**
- Create README.md
- Create API-SPEC.md
- Create DATABASE.md
- Create UI.md
- Create IMPLEMENTATION.md
- Update ENTRA-CONFIG-GUIDE.md

**Deliverables:**
- All tests passing
- Complete documentation
- Known issues documented
- Deployment guide updated

---

## 📝 Notes & Considerations

### Critical Implementation Notes

1. **User Roles Structure**
   - **Admin:** Full system access, tenant settings, subscription management, feature flags
   - **UpperManagement:** Strategic oversight, financial reports, high-level decision making, user management
   - **LowerManagement:** Operational management, team coordination, resource planning, user invitations
   - **Supervisor:** Team supervision, task assignment, production oversight, quality control
   - **ShopFloorWorker:** Production work, work order updates, time recording, NCR reporting
   - **OfficeWorker:** Administrative tasks, document management, basic reporting, project support
   - **First registered user always gets Admin role**

2. **Demo Tenant Switching**
   - Demo tenant (`demo@steelaxis.eu`) starts on Enterprise tier
   - Admin can switch tiers via UI: Admin → Tenant Settings → Change Subscription Tier
   - Tier switching immediately updates feature access
   - Use for demonstrating different subscription levels to potential customers

3. **Payment Status**
   - Demo tenant marked as `PaymentStatus = "Demo"`
   - Display "DEMO - Fully Paid" in UI instead of payment info
   - No Stripe integration required for demo tenant
   - Production tenants will use Stripe for payment processing

4. **Localization**
   - All UI text must use `IStringLocalizer`
   - Resource files required for: en-US, lv-LV (minimum)
   - Database text fields (descriptions, names) should have corresponding `*Key` fields
   - Example: `Name = "Starter"`, `NameKey = "subscription.tier.starter"`
   - Role names and descriptions fully localizable
   - Tenant's `DefaultLanguage` field determines UI language on login

5. **Feature Flags JSON Structure**
   - Stored in `Tenant.FeaturesJson` as JSON string
   - Format: `{"FeatureKey": "Status", ...}`
   - Status values: "Enabled", "Disabled", "Beta", "Alpha"
   - Merged with subscription tier's default features
   - Tenant-specific overrides take precedence

6. **Entra External ID Integration**
   - Custom attributes must be configured in Entra before user registration
   - Token claims must include `tenantId` and `role`
   - User registration creates Entra account first, then maps to tenant
   - Invitation flow: Create invitation → Send email → User creates Entra account → Map to tenant

7. **Security**
   - Invitation tokens expire after 7 days
   - Tokens are single-use (status changes to "Accepted" or "Expired")
   - Role changes require admin authorization
   - Tenant switching not allowed (users belong to one tenant)
   - API endpoints validate tenant context from user claims
   - Role hierarchy enforced in authorization policies

### Role-Based Feature Access

**Admin:**
- Complete system control
- All features unlocked
- Can manage subscription, features, and all users

**UpperManagement:**
- Strategic features: financial reports, analytics, planning
- Can invite and manage users
- Cannot change subscription or feature flags

**LowerManagement:**
- Operational features: project management, resource allocation
- Can invite users
- Cannot access financials or strategic tools

**Supervisor:**
- Team oversight: task assignment, progress tracking
- Quality control and NCR management
- Cannot invite users or delete projects

**ShopFloorWorker:**
- Production-focused: work orders, time tracking, material usage
- Can create NCRs and update work progress
- No access to office features or reports

**OfficeWorker:**
- Administrative support: documents, basic reports, project viewing
- Can view but not modify production data
- No user management or strategic access

### Future Enhancements

- **Multi-factor Authentication (MFA)** - Require MFA for admin and management users
- **Audit Logging** - Track all user actions and role changes
- **Advanced Feature Flags** - Percentage rollouts, A/B testing
- **Usage Analytics** - Track feature usage per role and tenant
- **Tenant Branding** - Custom logos, colors, email templates
- **API Keys** - Allow tenants to generate API keys for integrations
- **Webhooks** - Notify external systems of events
- **Single Sign-On (SSO)** - Support SAML/OIDC for enterprise customers
- **Role Customization** - Allow tenants to create custom roles with specific permissions
- **Department/Team Structure** - Add organizational hierarchy within tenants

---

**Plan Status:** ✅ Ready for approval  
**Estimated Effort:** 42 hours (5-6 working days)  
**Priority:** Critical - Foundation for entire SteelAxis platform  
**Demo Tenant:** demo@steelaxis.eu (Enterprise tier, all features, easy tier switching)  
**User Roles:** 6 roles (Admin, UpperManagement, LowerManagement, Supervisor, ShopFloorWorker, OfficeWorker)
