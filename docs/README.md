# SteelAxis Documentation

**Project:** SteelAxis - Multi-tenant Steel Fabrication Management System  
**Status:** 🚧 **INITIAL DEVELOPMENT PHASE**  
**Last Updated:** October 27, 2025

---

## 🎯 Project Overview

SteelAxis is a multi-tenant steel fabrication management system with focus on **EN 1090 compliance** for European steel construction standards.

### Technology Stack
- **.NET 8.0** - Latest LTS framework
- **Blazor Server** - Interactive web application
- **MudBlazor 8.12.0** - Material Design component library
- **ASP.NET Core Web API** - RESTful backend services
- **Microsoft Entra External ID** - Customer authentication (CIAM)
- **Azure Cloud** - App Services, SQL Database, Key Vault
- **Entity Framework Core 8.0** - Database ORM

### Architecture
- **Multi-tenancy:** Database per tenant pattern
- **Authentication:** Microsoft Entra External ID (CIAM)
- **Deployment:** Azure App Services with GitHub Actions CI/CD
- **API-First:** Every feature includes full API implementation

---

## 🚧 Current Status

**CLEAN SLATE - NO FEATURES IMPLEMENTED**

### ✅ Completed Setup
- Solution structure created (8 projects: Shared, Auth, Directory, Data, Services, API, Web, Tests)
- Authentication configured with Microsoft Entra External ID
- Azure infrastructure provisioned (steelaxis-dev, steelaxis-dev-api)
- CI/CD pipelines configured (GitHub Actions)
- Development guidelines established
- Documentation templates created

### ❌ Not Yet Implemented
- **No database models** (beyond base entities)
- **No business logic services**
- **No API controllers** (beyond health check)
- **No UI components** (beyond authentication)
- **No EN 1090 features**
- **No material management**
- **No project tracking**
- **No quality control workflows**

**SteelAxis is ready for feature development to begin.**

---

## 📚 Available Documentation

### 🔐 Authentication
**Status:** ✅ Configured and ready

Microsoft Entra External ID (CIAM) authentication setup guides.

- **[authentication/README.md](./authentication/README.md)** - Authentication overview and configuration

**Key Points:**
- FREE for first 50,000 monthly active users
- OpenID Connect integration
- Multi-tenant user isolation
- Role-based access control ready

---

### ☁️ Azure Infrastructure
**Status:** ✅ Provisioned (Dev environment)

Azure cloud infrastructure setup and deployment guides.

- **[azure-infrastructure/README.md](./azure-infrastructure/README.md)** - Infrastructure documentation index
- **[azure-infrastructure/azure-infrastructure-setup.md](./azure-infrastructure/azure-infrastructure-setup.md)** - Complete Azure resource provisioning
- **[azure-infrastructure/azure-dns-setup-guide.md](./azure-infrastructure/azure-dns-setup-guide.md)** - DNS configuration guide
- **[azure-infrastructure/azure-deployment.md](./azure-infrastructure/azure-deployment.md)** - App Service deployment
- **[azure-infrastructure/cicd-summary.md](./azure-infrastructure/cicd-summary.md)** - GitHub Actions CI/CD

**Resources Provisioned:**
- App Service: `steelaxis-dev` (Blazor Web)
- App Service: `steelaxis-dev-api` (Web API)
- SQL Server: Configured with elastic pool
- Key Vault: `kv-Steelaxis-dev`
- Azure DNS: Ready for custom domain

---

### ⚙️ EN 1090 Compliance
**Status:** 📋 **SPECIFICATION READY - AWAITING IMPLEMENTATION**

European standard (EN 1090) requirements and implementation specifications for steel fabrication quality management.

- **[en-1090-compliance/README.md](./en-1090-compliance/README.md)** - Documentation index
- **[en-1090-compliance/EN-1090-COMPLETE-GUIDE.md](./en-1090-compliance/EN-1090-COMPLETE-GUIDE.md)** - ⭐ **START HERE** - Complete implementation guide
- **[en-1090-compliance/en-1090-requirements.md](./en-1090-compliance/en-1090-requirements.md)** - Database schemas and API specifications
- **[en-1090-compliance/en-1090-development.md](./en-1090-compliance/en-1090-development.md)** - Development guide with code examples
- **[en-1090-compliance/en-1090-quick-reference.md](./en-1090-compliance/en-1090-quick-reference.md)** - Quick reference
- **[en-1090-compliance/en-1090-ncr-management.md](./en-1090-compliance/en-1090-ncr-management.md)** - Non-Conformance Report system
- **[en-1090-compliance/en-1090-data-hashing-plan.md](./en-1090-compliance/en-1090-data-hashing-plan.md)** - Data integrity strategy

**Core Requirements:**
- Material traceability (EN 10204 Type 3.1/3.2 certificates)
- Non-Conformance Reports (NCR) and Corrective Actions (CAR)
- Welding Procedure Specifications (WPS/WPQR)
- Declaration of Performance (DoP) and CE marking
- Immutable audit trails with SHA-256 hashing
- Quality control documentation

**⚠️ NOTE:** These are specifications and requirements. **No EN 1090 features are implemented yet.**

---

### 🎨 MudBlazor Components
**Status:** 📚 Reference documentation

MudBlazor component usage guides and examples.

- **[mudblazor/README.md](./mudblazor/README.md)** - Component library index
- Component-specific guides: MudDataGrid, MudDialog, MudButton, MudTable, etc.

---

## 📝 Feature Documentation Template

**IMPORTANT:** All new features MUST follow the standardized documentation structure.

See **[DOCUMENTATION-TEMPLATE.md](./DOCUMENTATION-TEMPLATE.md)** for complete templates.

### Required Documentation Structure

Each feature requires a dedicated folder: `/docs/[feature-name]/`

**Required Files:**
1. **PLAN.md** - Created BEFORE implementation starts
   - Feature description and business value
   - User stories and acceptance criteria
   - Technical approach and architecture
   - API endpoints to be created
   - Database schema changes
   - UI components and pages
   - Implementation steps

2. **README.md** - Updated AFTER implementation completes
   - What: Feature description
   - Why: Business value
   - How: Usage instructions
   - Screenshots/diagrams
   - Configuration
   - Known limitations

3. **API-SPEC.md** - API endpoint documentation
4. **DATABASE.md** - Database schema and migrations
5. **UI.md** - UI components and page specifications
6. **IMPLEMENTATION.md** - Implementation details and decisions

### Workflow Requirements

See `.github/copilot-instructions.md` for complete development workflow:

1. **Plan First** - Create detailed plan, get user approval
2. **Implement** - Follow API-first approach (Models → Service → API → HTTP Service → UI)
3. **Document** - Update all documentation files
4. **Commit** - Commit documentation separately after feature completion

---

## 🚀 Getting Started

### For Developers

1. **Review Solution Structure**
   - See root `ReadMe.md` for project structure
   - Review `.github/copilot-instructions.md` for development standards

2. **Understand Multi-Tenancy**
   - Database per tenant pattern
   - Central directory service for tenant management
   - Tenant resolution from authenticated user claims

3. **Review EN 1090 Requirements**
   - Start with `EN-1090-COMPLETE-GUIDE.md`
   - Understand material traceability requirements
   - Review NCR workflow specifications

4. **Follow API-First Development**
   - Every feature requires API implementation
   - Use standardized component patterns
   - Document as you build

### For New Features

1. Read `DOCUMENTATION-TEMPLATE.md`
2. Create feature folder in `/docs/[feature-name]/`
3. Create `PLAN.md` with detailed implementation plan
4. Get plan approved before starting
5. Follow API-first development pattern
6. Update all documentation upon completion
7. Commit documentation separately

---

## 📖 External References

- **[Microsoft Entra External ID Documentation](https://learn.microsoft.com/en-us/entra/external-id/)**
- **[MudBlazor Documentation](https://mudblazor.com/)**
- **[EN 1090 Standard Overview](https://www.steelconstruction.info/EN_1090)**
- **[Azure App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)**

---

## 🎯 Next Steps

SteelAxis is ready for feature development. Recommended implementation order:

1. **Core Infrastructure**
   - Tenant management service
   - User management and roles
   - Base entity models

2. **EN 1090 Foundation**
   - Material certificate management
   - Material traceability system
   - Document management system

3. **Quality Management**
   - Non-Conformance Reports (NCR)
   - Corrective Action Requests (CAR)
   - Quality control workflows

4. **Project Management**
   - Project creation and tracking
   - Material assignment to projects
   - Progress tracking

5. **Customer Portal**
   - Secure document sharing
   - Project visibility for customers
   - Token-based access

---

**Remember:** SteelAxis is a clean slate. All features must be built from scratch following the established patterns and documentation standards.
