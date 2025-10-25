# Manimp Documentation

**Last Updated:** October 14, 2025  
**Version:** 1.0.0

Welcome to the Manimp documentation hub! All documentation is organized by feature area for easy navigation.

---

## 📁 Documentation Structure

### 🌐 [Azure Infrastructure](./azure-infrastructure/)
Complete Azure cloud infrastructure setup and deployment guides.

- **[azure-infrastructure-setup.md](./azure-infrastructure/azure-infrastructure-setup.md)** - Complete Azure resource provisioning with CLI commands (600+ lines)
- **[azure-dns-setup-guide.md](./azure-infrastructure/azure-dns-setup-guide.md)** - DNS configuration with visual diagrams and troubleshooting (800+ lines)
- **[azure-deployment.md](./azure-infrastructure/azure-deployment.md)** - Azure App Service deployment guide
- **[cicd-summary.md](./azure-infrastructure/cicd-summary.md)** - CI/CD pipeline configuration for GitHub Actions

**Quick Start:** Begin with `azure-infrastructure-setup.md` for full infrastructure provisioning.

---

### 🔐 [Authentication](./authentication/)
Azure AD B2C authentication and authorization implementation.

- **[azure-b2c-authentication.md](./authentication/azure-b2c-authentication.md)** - Complete B2C setup with internal & external user flows (800+ lines)

**Key Features:**
- Dual authentication schemes (internal employees + external clients)
- FREE for first 50,000 monthly active users
- OpenID Connect integration with ASP.NET Core
- Custom claims transformation and role management

---

### 🎨 [MudBlazor Documentation](./mudblazor/)
MudBlazor component documentation, patterns, and best practices.

- **Upload MudBlazor documentation files here** for easy reference
- Component usage examples and patterns
- Dialog, form, and table implementations
- Troubleshooting guides
- Theme customization

**Purpose:**
- Quick reference for MudBlazor components
- Consistent UI patterns across Manimp
- Common issues and solutions
- Integration with Blazor Server

---

### 📂 [File Storage](./file-storage/)
Multi-domain file storage architecture with Azure Blob Storage.

- **[file-storage-multi-domain-architecture.md](./file-storage/file-storage-multi-domain-architecture.md)** - Complete implementation guide (3000+ lines) ⭐
- **[file-storage-multi-domain-quick-ref.md](./file-storage/file-storage-multi-domain-quick-ref.md)** - Quick reference and command cheat sheet (400 lines)
- **[file-storage-implementation-summary.md](./file-storage/file-storage-implementation-summary.md)** - Executive summary (300 lines)
- **[file-storage-subdomain-plan.md](./file-storage/file-storage-subdomain-plan.md)** - Original single-domain plan (500 lines)
- **[file-storage-comparison.md](./file-storage/file-storage-comparison.md)** - Visual comparisons of approaches (300 lines)
- **[file-storage-quick-ref.md](./file-storage/file-storage-quick-ref.md)** - Legacy quick reference

**Architecture:**
- `{tenant}.manimp.com` - Main application
- `{tenant}.files.manimp.com` - Internal file portal
- `{tenant}.docs.manimp.com` - External client portal

**Features:**
- Role-based file sharing (Admin/Manager/User)
- Time-limited share links (unlimited or N days)
- External user access with optional registration
- Project phase-based file visibility
- Azure Blob Storage with Hot/Cool/Archive tiers

---

### ⚙️ [EN 1090 Compliance](./en-1090-compliance/)
European standard for fabrication of steel structures - quality management and traceability.

- **[en-1090-requirements.md](./en-1090-compliance/en-1090-requirements.md)** - Full EN 1090 standard requirements
- **[en-1090-compliance.md](./en-1090-compliance/en-1090-compliance.md)** - Implementation roadmap
- **[en-1090-development.md](./en-1090-compliance/en-1090-development.md)** - Development guide
- **[en-1090-quick-reference.md](./en-1090-compliance/en-1090-quick-reference.md)** - Quick reference
- **[en-1090-ncr-management.md](./en-1090-compliance/en-1090-ncr-management.md)** - Non-Conformance Report management
- **[en-1090-data-hashing-plan.md](./en-1090-compliance/en-1090-data-hashing-plan.md)** - Data integrity and hashing
- **[en-1090-immutability-summary.md](./en-1090-compliance/en-1090-immutability-summary.md)** - Immutable record system
- **[en-1090-supplementary-requirements.md](./en-1090-compliance/en-1090-supplementary-requirements.md)** - Additional requirements

**Key Concepts:**
- Traceability of materials and processes
- Document control and version management
- Non-conformance tracking (NCR)
- Welding procedure specifications (WPS)
- Material certificates and test reports
- Immutable audit trails with SHA-256 hashing

---

### 🛒 [Procurement](./procurement/)
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

### 📦 [Inventory](./inventory/)
Sheet inventory and material management system.

- **[inventory-ui-implementation-summary.md](./inventory/inventory-ui-implementation-summary.md)** - UI implementation overview
- **[inventory-lot-number-improvements.md](./inventory/inventory-lot-number-improvements.md)** - Lot number tracking enhancements
- **[remnants-page-improvements.md](./inventory/remnants-page-improvements.md)** - Remnants management improvements

**Features:**
- Sheet inventory tracking
- Lot number traceability
- Material consumption tracking
- Remnants management
- Cut optimization
- Stock level alerts

---

### 📊 [Project Management](./project-management/)
Project planning, tracking, and production monitoring.

- **[project-management-enhancement.md](./project-management/project-management-enhancement.md)** - Project management features
- **[project-management-quick-ref.md](./project-management/project-management-quick-ref.md)** - Quick reference
- **[real-time-production-monitoring.md](./project-management/real-time-production-monitoring.md)** - Live production dashboards with SignalR

**Features:**
- Project creation and phase management
- Task assignment and tracking
- Real-time production monitoring (SignalR)
- Gantt charts and timelines
- Resource allocation
- Budget tracking
- Client portal access

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
- Azure AD B2C authentication
- HTTPS enforcement
- SQL injection prevention
- XSS protection

---

### 🔧 [Fixes & Improvements](./fixes-and-improvements/)
Bug fixes and platform improvements.

- **[mudblazor-dialog-fix.md](./fixes-and-improvements/mudblazor-dialog-fix.md)** - MudBlazor dialog rendering fix
- **[net8-blazor-rendermode-fix.md](./fixes-and-improvements/net8-blazor-rendermode-fix.md)** - .NET 8 Blazor render mode fix

---

### 📋 [General](./general/)
Strategic planning, roadmaps, and database documentation.

- **[manimp-strategic-guide.md](./general/manimp-strategic-guide.md)** - Strategic product vision and architecture
- **[manimp-development-roadmap.md](./general/manimp-development-roadmap.md)** - Development roadmap and milestones
- **[implementation-status.md](./general/implementation-status.md)** - Current implementation status
- **[documentation-update-summary.md](./general/documentation-update-summary.md)** - Documentation change log
- **[what-next.md](./general/what-next.md)** - Future feature planning
- **[Full db.erd](./general/Full%20db.erd)** - Complete database entity-relationship diagram

---

## 🎯 Quick Navigation by Task

### Getting Started
1. Read [manimp-strategic-guide.md](./general/manimp-strategic-guide.md) for product overview
2. Check [implementation-status.md](./general/implementation-status.md) for current state
3. Review [manimp-development-roadmap.md](./general/manimp-development-roadmap.md) for planned features

### Setting Up Azure Infrastructure
1. Start with [azure-infrastructure-setup.md](./azure-infrastructure/azure-infrastructure-setup.md)
2. Configure DNS: [azure-dns-setup-guide.md](./azure-infrastructure/azure-dns-setup-guide.md)
3. Set up authentication: [azure-b2c-authentication.md](./authentication/azure-b2c-authentication.md)
4. Deploy application: [azure-deployment.md](./azure-infrastructure/azure-deployment.md)

### Implementing File Storage
1. Review architecture: [file-storage-multi-domain-architecture.md](./file-storage/file-storage-multi-domain-architecture.md)
2. Quick commands: [file-storage-multi-domain-quick-ref.md](./file-storage/file-storage-multi-domain-quick-ref.md)
3. Executive summary: [file-storage-implementation-summary.md](./file-storage/file-storage-implementation-summary.md)

### EN 1090 Compliance
1. Understand requirements: [en-1090-requirements.md](./en-1090-compliance/en-1090-requirements.md)
2. Implementation guide: [en-1090-development.md](./en-1090-compliance/en-1090-development.md)
3. Quick reference: [en-1090-quick-reference.md](./en-1090-compliance/en-1090-quick-reference.md)

### Feature Implementation
- **Procurement:** [procurement-implementation-summary.md](./procurement/procurement-implementation-summary.md)
- **Inventory:** [inventory-ui-implementation-summary.md](./inventory/inventory-ui-implementation-summary.md)
- **Project Management:** [project-management-enhancement.md](./project-management/project-management-enhancement.md)

---

## 📊 Documentation Statistics

```
Total Documents: 35+
Total Lines: 15,000+
Feature Areas: 8

By Category:
├─ File Storage:        5 documents (5,000+ lines)
├─ EN 1090 Compliance:  8 documents (3,000+ lines)
├─ Azure Infrastructure: 4 documents (2,500+ lines)
├─ Authentication:      1 document  (800 lines)
├─ Project Management:  3 documents (1,000+ lines)
├─ Procurement:         2 documents (800 lines)
├─ Inventory:           3 documents (600 lines)
├─ Security:            3 documents (500 lines)
├─ Fixes:               2 documents (300 lines)
└─ General:             6 documents (1,500+ lines)
```

---

## 🔍 Search Tips

**By Feature Area:**
```bash
# Find all file storage docs
ls -la docs/file-storage/

# Find all compliance docs
ls -la docs/en-1090-compliance/

# Find all infrastructure docs
ls -la docs/azure-infrastructure/
```

**By Content:**
```bash
# Search for specific terms
grep -r "Azure Blob" docs/
grep -r "SignalR" docs/
grep -r "B2C" docs/

# Search within specific feature
grep -r "SAS token" docs/file-storage/
grep -r "NCR" docs/en-1090-compliance/
```

---

## 🤝 Contributing to Documentation

When adding new documentation:

1. **Choose the correct folder** based on feature area
2. **Use consistent naming:**
   - `feature-name-overview.md` - High-level overview
   - `feature-name-implementation.md` - Detailed implementation guide
   - `feature-name-quick-ref.md` - Quick reference/cheat sheet
3. **Include metadata at the top:**
   ```markdown
   # Document Title
   **Last Updated:** YYYY-MM-DD
   **Status:** Draft | In Progress | Complete
   **Related:** [Links to related docs]
   ```
4. **Update this README** to include your new document

---

## 📞 Support

For questions or clarifications:
- Check the relevant feature folder first
- Review quick reference documents for common commands
- Consult implementation status for feature completion

---

**Happy coding!** 🚀
