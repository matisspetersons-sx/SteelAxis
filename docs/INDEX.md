# SteelAxis Documentation Index

This directory contains comprehensive documentation copied from the Manimp repository to guide the development of SteelAxis with all planned functionality.

## 📚 Documentation Structure

### Authentication (`docs/authentication/`)
**Core CIAM/Authentication Implementation**
- `azure-b2c-authentication.md` - Complete Azure AD B2C setup guide (800+ lines)
  - Dual authentication schemes (internal + external users)
  - Multi-tenant domain detection
  - Claims transformation
  - Token management
- `README.md` - Authentication overview

**Status for SteelAxis:** ✅ Already implemented with Entra External ID

---

### Azure Infrastructure (`docs/azure-infrastructure/`)
**Cloud Deployment & CI/CD**
- `azure-infrastructure-setup.md` - Complete Azure resource setup
- `azure-deployment.md` - Deployment procedures
- `cicd-summary.md` - GitHub Actions workflows
- `azure-dns-setup-guide.md` - Custom domain configuration

**Status for SteelAxis:** ✅ Already configured (steelaxis-dev, steelaxis-dev-api)

---

### EN 1090 Compliance (`docs/en-1090-compliance/`)
**Steel Construction Standards Compliance System**

**Core Documentation:**
- `en-1090-compliance.md` (868 lines) - Complete standard overview
- `en-1090-requirements.md` (962 lines) - Database schemas, API specs, validation
- `README.md` - Quick reference guide

**Phase-Specific Guides:**
- `PHASE1-BASIC-COMPLIANCE.md` - Execution class determination, material certificates
- `PHASE2-WELDING-NDT.md` - Welding procedures, NDT requirements, quality control
- `PHASE3-DOCUMENTATION.md` - Declaration of Performance (DoP), CE marking, manufacturing dossiers

**Implementation Guides:**
- `IMPLEMENTATION-GUIDE.md` - Step-by-step development guide
- `TESTING-GUIDE.md` - QA procedures and test scenarios

**Status for SteelAxis:** ⏸️ **Priority for implementation** - Core business logic for steel fabrication

---

### File Storage (`docs/file-storage/`)
**Multi-Domain File Management Architecture**

- `file-storage-multi-domain-architecture.md` (3000+ lines) - Complete architecture
  - Three domains: `{tenant}.app.com`, `{tenant}.files.com`, `{tenant}.docs.com`
  - Azure Blob Storage integration
  - Role-based access control
  - Client portal file sharing
- `file-storage-subdomain-plan.md` - Subdomain routing implementation
- `file-storage-implementation-summary.md` - Implementation checklist

**Status for SteelAxis:** ⏸️ To be implemented

---

### Inventory Management (`docs/inventory/`)
**Material Tracking & Stock Management**

- `inventory-overview.md` - Inventory system architecture
- `inventory-compliance.md` - EN 1090 material traceability integration
- `profile-inventory.md` - Structural profiles tracking
- `sheet-inventory.md` - Sheet metal inventory
- `remnants-tracking.md` - Remnant management and optimization
- Database schemas for inventory tables
- Material receiving workflow
- Stock alerts and MRP integration

**Status for SteelAxis:** ⏸️ To be implemented

---

### Project Management (`docs/project-management/`)
**Task Dependencies & Critical Path Analysis**

**Phase 2 Implementation:**
- `README-PHASE2.md` - Resource allocation, task dependencies
- `task-dependencies.md` - Four dependency types (FS, SS, FF, SF)
- `critical-path-method.md` - CPM algorithm implementation
- `resource-planning.md` - Welder/equipment scheduling
- `gantt-chart.md` - Visual timeline implementation

**Core Features:**
- ProjectTask model with early/late dates, slack time
- TaskDependency with lag/lead time
- Critical path identification
- Schedule impact analysis
- Resource availability tracking

**Status for SteelAxis:** ⏸️ To be implemented

---

### Procurement (`docs/procurement/`)
**Purchase Orders & Supplier Management**

- `procurement-overview.md` - Procurement system architecture
- `purchase-orders.md` - PO creation and approval workflow
- `material-receiving.md` - Receipt process with EN 1090 data capture
- `supplier-management.md` - Supplier catalog and performance tracking
- `mrp-integration.md` - Material Requirements Planning
- Database schemas and API endpoints

**Status for SteelAxis:** ⏸️ To be implemented

---

### MudBlazor (`docs/mudblazor/`)
**UI Component Library & Theming**

- `mudblazor-setup.md` - MudBlazor installation and configuration
- `theme-customization.md` - Custom theme implementation
- `component-examples.md` - Common component patterns
- `form-validation.md` - Form handling with MudBlazor
- `data-tables.md` - Advanced table features
- `dialogs-and-navigation.md` - Modal dialogs and routing

**Status for SteelAxis:** ✅ MudBlazor already installed (v8.12.0)

---

### Customer Portal (`docs/customer-portal/`)
**External Client Access System**

- `customer-portal-overview.md` - Client portal architecture
- `external-user-management.md` - Client user provisioning
- `project-file-sharing.md` - Controlled file access by project phase
- `read-only-documents.md` - Document preview without download
- `branding-customization.md` - Per-tenant portal branding

**Status for SteelAxis:** ⏸️ To be implemented (requires file storage first)

---

### Security (`docs/security/`)
**Security Best Practices & Testing**

- `security.md` - Security architecture overview
- `security-test.md` - Security testing procedures
- `security-demo.md` - Security feature demonstrations

**Status for SteelAxis:** ✅ Basic security configured (CIAM, HTTPS, Key Vault)

---

### General (`docs/general/`)
**Cross-Cutting Concerns & Development Guides**

- `manimp-development-roadmap.md` - Overall development strategy
- `manimp-strategic-guide.md` - Business strategy and feature prioritization
- `implementation-status.md` - Current implementation status
- `what-next.md` - Next steps for development
- `QUICK-INTEGRATION-GUIDE.md` - Quick start for new developers
- `DEMO-MODE.md` - Demo data and presentation mode

---

### Fixes & Improvements (`docs/fixes-and-improvements/`)
**Bug Fixes & Enhancement Tracking**

- Documentation of specific fixes and improvements
- Performance optimization guides
- Refactoring notes

---

## 🎯 Recommended Implementation Order for SteelAxis

### Phase 1: Foundation (Current)
1. ✅ Authentication (Entra External ID) - **COMPLETE**
2. ✅ Azure infrastructure (App Services, Key Vault) - **COMPLETE**
3. ✅ CI/CD pipeline (GitHub Actions) - **COMPLETE**
4. ✅ MudBlazor UI framework - **COMPLETE**

### Phase 2: Core Business Logic
5. **EN 1090 Compliance System** (`docs/en-1090-compliance/`)
   - Start with Phase 1: Basic compliance (execution classes, material certificates)
   - Implement Phase 2: Welding and NDT management
   - Complete Phase 3: DoP and CE marking documentation
   - **Priority:** HIGH - Core differentiator for steel fabrication industry

6. **Inventory Management** (`docs/inventory/`)
   - Profile and sheet inventory
   - Material traceability with EN 1090 integration
   - Remnant tracking
   - **Priority:** HIGH - Required for compliance system

### Phase 3: Operations
7. **Project Management** (`docs/project-management/`)
   - Task creation and dependencies
   - Critical path analysis
   - Resource scheduling
   - **Priority:** MEDIUM - Improves operational efficiency

8. **Procurement** (`docs/procurement/`)
   - Purchase order management
   - Supplier management
   - Material receiving with compliance data
   - **Priority:** MEDIUM - Supports inventory and compliance

### Phase 4: Advanced Features
9. **File Storage & Multi-Domain** (`docs/file-storage/`)
   - Three-domain architecture
   - Azure Blob Storage integration
   - Role-based file access
   - **Priority:** MEDIUM - Enables document management

10. **Customer Portal** (`docs/customer-portal/`)
    - External user management
    - Project file sharing by phase
    - Branded client experience
    - **Priority:** LOW - Nice to have, depends on file storage

## 📖 How to Use This Documentation

### For Developers
1. Read `docs/STRUCTURE.md` for overall architecture understanding
2. Follow implementation guides in each domain folder
3. Reference database schemas when creating Entity Framework models
4. Use API endpoint specifications when building controllers
5. Follow UI patterns documented in MudBlazor section

### For Project Planning
1. Review `docs/general/manimp-development-roadmap.md` for strategic overview
2. Use phase-specific guides for sprint planning
3. Reference `implementation-status.md` to understand feature maturity
4. Check `what-next.md` for recommended next steps

### For Quality Assurance
1. Use testing guides in each domain
2. Follow `docs/en-1090-compliance/TESTING-GUIDE.md` for compliance validation
3. Reference `docs/security/security-test.md` for security testing

## 🔗 Quick Links

**Essential Reading:**
- 📘 [EN 1090 Implementation Guide](docs/en-1090-compliance/IMPLEMENTATION-GUIDE.md)
- 📘 [Authentication Setup](docs/authentication/azure-b2c-authentication.md)
- 📘 [File Storage Architecture](docs/file-storage/file-storage-multi-domain-architecture.md)
- 📘 [Project Management Phase 2](docs/project-management/README-PHASE2.md)

**Database Schemas:**
- All domain folders contain database schema documentation
- Look for files ending in `-schema.md` or within README files

**API Specifications:**
- Each domain folder includes API endpoint documentation
- Look for files containing "API" or "endpoints"

## 💡 Tips for SteelAxis Development

1. **Start with EN 1090:** This is the core value proposition for steel fabricators
2. **Build incrementally:** Implement Phase 1, test thoroughly, then move to Phase 2
3. **Leverage existing patterns:** The Manimp documentation shows proven implementation patterns
4. **Database first:** Design your Entity Framework models based on the documented schemas
5. **Test compliance:** EN 1090 has strict requirements - test against the compliance guide
6. **Multi-tenancy:** All features must support tenant isolation (already configured in auth)

## 📞 Documentation Maintenance

- Documentation is copied from Manimp repository
- Update this index as features are implemented in SteelAxis
- Mark completed features with ✅
- Add SteelAxis-specific notes and deviations from Manimp patterns
- Keep implementation status current

---

**Last Updated:** October 26, 2025
**Source:** Manimp Repository (https://github.com/petersonmatiss/manimp)
**Status:** Documentation imported, ready for SteelAxis development
