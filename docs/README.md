# SteelAxis Documentation

**Last Updated:** October 27, 2025  
**Version:** 1.0.0  
**Project:** SteelAxis - Multi-tenant Steel Fabrication Management System

Welcome to the SteelAxis documentation! All documentation is organized by feature area for easy navigation.

**Note:** This documentation was originally from the Manimp repository and serves as a reference for implementing features in SteelAxis with .NET 8.0, Blazor Server, MudBlazor, and Microsoft Entra External ID.

---

## � Feature Documentation Template

**NEW:** For all new features, use the standardized documentation structure:
- See **[DOCUMENTATION-TEMPLATE.md](./DOCUMENTATION-TEMPLATE.md)** for complete templates
- Each feature requires a folder in `/docs/[feature-name]/` with:
  - `PLAN.md` (created before implementation)
  - `README.md` (what, why, how)
  - `API-SPEC.md` (API endpoints and contracts)
  - `DATABASE.md` (schema and migrations)
  - `UI.md` (UI components and pages)

See the `.github/copilot-instructions.md` for complete development workflow requirements.

---

## 📁 Documentation Structure

### 🔐 [Authentication](./authentication/)
**Status for SteelAxis:** ✅ Already implemented with Microsoft Entra External ID (CIAM)

Azure AD B2C / Entra External ID authentication and authorization implementation.

- **[azure-b2c-authentication.md](./authentication/azure-b2c-authentication.md)** - Complete B2C/CIAM setup reference (800+ lines)

**Key Features:**
- Customer Identity and Access Management (CIAM)
- FREE for first 50,000 monthly active users
- OpenID Connect integration with ASP.NET Core
- Custom claims transformation and role management

---

### � [Azure Infrastructure](./azure-infrastructure/)
**Status for SteelAxis:** ✅ Already configured (steelaxis-dev, steelaxis-dev-api)

Complete Azure cloud infrastructure setup and deployment guides.

- **[azure-infrastructure-setup.md](./azure-infrastructure/azure-infrastructure-setup.md)** - Complete Azure resource provisioning
- **[azure-dns-setup-guide.md](./azure-infrastructure/azure-dns-setup-guide.md)** - DNS configuration with visual diagrams
- **[azure-deployment.md](./azure-infrastructure/azure-deployment.md)** - Azure App Service deployment guide
- **[cicd-summary.md](./azure-infrastructure/cicd-summary.md)** - CI/CD pipeline configuration for GitHub Actions

---

### ⚙️ [EN 1090 Compliance](./en-1090-compliance/)
**Status for SteelAxis:** ⏸️ **Priority for implementation** - Core business logic for steel fabrication

European standard for fabrication of steel structures - quality management and traceability.

**⭐ Start Here:**
- **[EN-1090-COMPLETE-GUIDE.md](./en-1090-compliance/EN-1090-COMPLETE-GUIDE.md)** - Condensed implementation guide (800+ lines)
- **[README.md](./en-1090-compliance/README.md)** - Quick overview and document index

**Detailed Documentation:**
- **[en-1090-requirements.md](./en-1090-compliance/en-1090-requirements.md)** - Full standard requirements, database schemas, APIs
- **[en-1090-compliance.md](./en-1090-compliance/en-1090-compliance.md)** - Implementation roadmap
- **[en-1090-development.md](./en-1090-compliance/en-1090-development.md)** - Development guide
- **[en-1090-quick-reference.md](./en-1090-compliance/en-1090-quick-reference.md)** - Quick reference
- **[en-1090-ncr-management.md](./en-1090-compliance/en-1090-ncr-management.md)** - Non-Conformance Report system
- **[NCR-WORKFLOW-ENHANCEMENTS.md](./en-1090-compliance/NCR-WORKFLOW-ENHANCEMENTS.md)** - NCR workflow improvements
- **[en-1090-data-hashing-plan.md](./en-1090-compliance/en-1090-data-hashing-plan.md)** - Data integrity with SHA-256
- **[en-1090-immutability-summary.md](./en-1090-compliance/en-1090-immutability-summary.md)** - Immutable record system
- **[en-1090-supplementary-requirements.md](./en-1090-compliance/en-1090-supplementary-requirements.md)** - Additional requirements

**Key Concepts:**
- Material traceability (EN 10204 certificates)
- Document control and version management
- Non-conformance tracking (NCR/CAR)
- Welding procedure specifications (WPS/WPQR)
- Immutable audit trails with SHA-256 hashing
- CE marking and Declaration of Performance (DoP)

---

### � [Customer Portal](./customer-portal/)
**Status for SteelAxis:** ⏸️ To be implemented

Customer-facing portal for project visibility and document sharing.

**⭐ Start Here:**
- **[START-HERE.md](./customer-portal/START-HERE.md)** - Quick overview
- **[README.md](./customer-portal/README.md)** - Complete feature guide

**Implementation Docs:**
- **[IMPLEMENTATION-PLAN.md](./customer-portal/IMPLEMENTATION-PLAN.md)** - Detailed implementation plan
- **[IMPLEMENTATION-COMPLETE.md](./customer-portal/IMPLEMENTATION-COMPLETE.md)** - Implementation summary
- **[QUICK-REFERENCE.md](./customer-portal/QUICK-REFERENCE.md)** - Quick reference guide
- **[MANAGEMENT-SUMMARY.md](./customer-portal/MANAGEMENT-SUMMARY.md)** - Management overview
- **[00-DELIVERY-SUMMARY.md](./customer-portal/00-DELIVERY-SUMMARY.md)** - Delivery summary

**Key Features:**
- Secure customer access to project information
- Document sharing and download
- Progress tracking and updates
- Communication portal

---

### 📂 [File Storage](./file-storage/)
**Status for SteelAxis:** ⏸️ To be implemented

Multi-domain file storage architecture with Azure Blob Storage.

**⭐ Main Reference:**
- **[file-storage-multi-domain-architecture.md](./file-storage/file-storage-multi-domain-architecture.md)** - Complete implementation (3000+ lines)

**Quick References:**
- **[file-storage-multi-domain-quick-ref.md](./file-storage/file-storage-multi-domain-quick-ref.md)** - Quick reference and commands (400 lines)
- **[file-storage-quick-ref.md](./file-storage/file-storage-quick-ref.md)** - Legacy quick reference

**Planning & Summaries:**
- **[file-storage-implementation-summary.md](./file-storage/file-storage-implementation-summary.md)** - Executive summary
- **[file-storage-subdomain-plan.md](./file-storage/file-storage-subdomain-plan.md)** - Original subdomain plan
- **[file-storage-comparison.md](./file-storage/file-storage-comparison.md)** - Architecture comparisons

**Proposed Architecture:**
- `{tenant}.steelaxis.com` - Main application
- `{tenant}.files.steelaxis.com` - Internal file portal
- `{tenant}.docs.steelaxis.com` - External client portal

**Features:**
- Role-based file sharing
- Time-limited share links
- External user access
- Project phase-based file visibility
- Azure Blob Storage integration

---

### 📦 [Inventory](./inventory/)
**Status for SteelAxis:** ⏸️ To be implemented

Material tracking, stock management, and remnants optimization.

- **[inventory-ui-implementation-summary.md](./inventory/inventory-ui-implementation-summary.md)** - UI implementation overview
- **[inventory-lot-number-improvements.md](./inventory/inventory-lot-number-improvements.md)** - Lot number traceability
- **[remnants-page-improvements.md](./inventory/remnants-page-improvements.md)** - Remnants management

**Features:**
- Sheet inventory tracking
- Structural profile inventory
- Lot number traceability (EN 1090 compliance)
- Material consumption tracking
- Remnants management and optimization
- Stock level alerts
- Integration with MRP

---

### � [Procurement](./procurement/)
**Status for SteelAxis:** ⏸️ To be implemented

Purchase order and supplier management system.

- **[procurement-implementation-summary.md](./procurement/procurement-implementation-summary.md)** - Implementation overview
- **[multi-line-procurement-implementation.md](./procurement/multi-line-procurement-implementation.md)** - Multi-line PO functionality

**Features:**
- Request for Quotation (RFQ) management
- Purchase Order (PO) creation and tracking
- Supplier management
- Quote comparison
- Multi-line items per PO
- Receiving and invoicing workflows

---

### �📊 [Project Management](./project-management/)
**Status for SteelAxis:** ⏸️ To be implemented

Project planning, task dependencies, critical path analysis, and production monitoring.

- **[project-management-enhancement.md](./project-management/project-management-enhancement.md)** - Project management features
- **[project-management-quick-ref.md](./project-management/project-management-quick-ref.md)** - Quick reference
- **[real-time-production-monitoring.md](./project-management/real-time-production-monitoring.md)** - SignalR-based monitoring

**Features:**
- Project creation and phase management
- Task dependencies (FS, SS, FF, SF)
- Critical path method (CPM) analysis
- Resource allocation (welders, equipment)
- Real-time production monitoring
- Gantt charts and timelines
- Budget tracking

---

### 🔒 [Security](./security/)
Security implementation and testing documentation.

- **[security.md](./security/security.md)** - Security architecture and best practices
- **[security-demo.md](./security/security-demo.md)** - Security feature demonstrations
- **[security-test.md](./security/security-test.md)** - Security testing procedures

**Topics:**
- Multi-tenant data isolation
- Row-level security
- Feature gating
- Microsoft Entra External ID authentication
- HTTPS enforcement
- SQL injection prevention
- XSS protection

---

### 🎨 [MudBlazor](./mudblazor/)
MudBlazor component documentation, patterns, and best practices.

**Purpose:**
- Quick reference for MudBlazor 8.12.0 components
- Consistent UI patterns (StandardDialog, StandardDataGrid, StandardForm)
- Common issues and solutions
- Integration with Blazor Server
- Theme customization

**See also:** `.github/copilot-ui-instructions.md` for complete UI standards

---

### 🔧 [Fixes & Improvements](./fixes-and-improvements/)
Bug fixes and platform improvements.

- **[mudblazor-dialog-fix.md](./fixes-and-improvements/mudblazor-dialog-fix.md)** - MudBlazor dialog rendering fix
- **[net8-blazor-rendermode-fix.md](./fixes-and-improvements/net8-blazor-rendermode-fix.md)** - .NET 8 Blazor render mode fix

---

### 📋 [General](./general/)
Strategic planning, roadmaps, and general documentation.

- **[manimp-strategic-guide.md](./general/manimp-strategic-guide.md)** - Strategic product vision reference
- **[manimp-development-roadmap.md](./general/manimp-development-roadmap.md)** - Development roadmap reference
- **[implementation-status.md](./general/implementation-status.md)** - Implementation status tracking
- **[DEMO-MODE.md](./general/DEMO-MODE.md)** - Demo mode configuration
- **[what-next.md](./general/what-next.md)** - Future feature planning
- **[Full db.erd](./general/Full%20db.erd)** - Database entity-relationship diagram

---

## 🎯 Quick Start Guides

### For New Developers

1. **Understand the Project:**
   - Read the main project [README.md](../README.md)
   - Review Copilot instructions in `.github/copilot-instructions.md`
   - Check [EN-1090-COMPLETE-GUIDE.md](./en-1090-compliance/EN-1090-COMPLETE-GUIDE.md) for business domain

2. **Setup Development Environment:**
   - Follow setup instructions in main README
   - Review authentication setup: [azure-b2c-authentication.md](./authentication/azure-b2c-authentication.md)
   - Check Azure infrastructure: [azure-infrastructure-setup.md](./azure-infrastructure/azure-infrastructure-setup.md)

3. **Start Development:**
   - Create feature folder in `/docs/[feature-name]/`
   - Use [DOCUMENTATION-TEMPLATE.md](./DOCUMENTATION-TEMPLATE.md) for planning
   - Follow API-first and UI standardization patterns from Copilot instructions

### Implementing EN 1090 Compliance

1. Start: [EN-1090-COMPLETE-GUIDE.md](./en-1090-compliance/EN-1090-COMPLETE-GUIDE.md)
2. Database schemas: [en-1090-requirements.md](./en-1090-compliance/en-1090-requirements.md)
3. Development guide: [en-1090-development.md](./en-1090-compliance/en-1090-development.md)
4. NCR system: [en-1090-ncr-management.md](./en-1090-compliance/en-1090-ncr-management.md)

### Implementing File Storage

1. Architecture overview: [file-storage-multi-domain-architecture.md](./file-storage/file-storage-multi-domain-architecture.md)
2. Quick commands: [file-storage-multi-domain-quick-ref.md](./file-storage/file-storage-multi-domain-quick-ref.md)
3. Implementation summary: [file-storage-implementation-summary.md](./file-storage/file-storage-implementation-summary.md)

### Implementing Customer Portal

1. Overview: [START-HERE.md](./customer-portal/START-HERE.md)
2. Implementation plan: [IMPLEMENTATION-PLAN.md](./customer-portal/IMPLEMENTATION-PLAN.md)
3. Quick reference: [QUICK-REFERENCE.md](./customer-portal/QUICK-REFERENCE.md)

---

## 📊 Implementation Status for SteelAxis

| Feature Area | Status | Priority | Notes |
|-------------|--------|----------|-------|
| Authentication | ✅ Complete | - | Microsoft Entra External ID |
| Azure Infrastructure | ✅ Complete | - | App Services deployed |
| EN 1090 Compliance | ⏸️ Pending | **High** | Core business logic |
| Customer Portal | ⏸️ Pending | High | External user access |
| File Storage | ⏸️ Pending | Medium | Multi-domain architecture |
| Inventory | ⏸️ Pending | Medium | Material tracking |
| Procurement | ⏸️ Pending | Medium | RFQ/PO system |
| Project Management | ⏸️ Pending | Medium | Task dependencies & CPM |
| Real-time Monitoring | ⏸️ Pending | Low | SignalR dashboards |

---

## � Documentation Standards

### For All New Features:

1. **Create feature folder:** `/docs/[feature-name]/`
2. **Before implementation:**
   - Create `PLAN.md` using template
   - Get user approval
3. **After implementation:**
   - Complete `README.md` (what, why, how)
   - Document APIs in `API-SPEC.md`
   - Document schema in `DATABASE.md`
   - Document UI in `UI.md`
   - Update this main README

See [DOCUMENTATION-TEMPLATE.md](./DOCUMENTATION-TEMPLATE.md) for complete templates.

---

## 🔍 Search Tips

**By Feature Area:**
```bash
ls -la docs/en-1090-compliance/
ls -la docs/customer-portal/
ls -la docs/file-storage/
```

**By Content:**
```bash
grep -r "MaterialCertificate" docs/
grep -r "NCR" docs/en-1090-compliance/
grep -r "Azure Blob" docs/file-storage/
grep -r "SignalR" docs/project-management/
```

---

## 🤝 Contributing to Documentation

When adding new documentation:

1. **Choose the correct folder** based on feature area
2. **Use the DOCUMENTATION-TEMPLATE.md** for new features
3. **Follow naming conventions:**
   - `PLAN.md` - Implementation plan (required before coding)
   - `README.md` - Main feature documentation
   - `API-SPEC.md` - API endpoints and contracts
   - `DATABASE.md` - Database schema
   - `UI.md` - UI components
4. **Update this README** to include your new feature

---

**Happy coding!** 🚀

