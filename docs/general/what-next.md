# Manimp - What's Next

**Last Updated:** October 11, 2025  
**Planning Period:** Q4 2025 - Q4 2026  
**Document Version:** 1.0

This document outlines the next priorities, tasks, and roadmap for the Manimp project. It serves as the actionable task list for upcoming development work.

---

## 📊 Current State Summary

As of October 11, 2025:
- **Implementation Progress**: ~90% of core features complete
- **Production Status**: Ready for deployment with comprehensive EN 1090 compliance and full procurement
- **Active Users**: Pre-launch (awaiting first production deployment)
- **Technical Debt**: 9 build warnings to address (down from 51)
- **Test Coverage**: 0% (no tests implemented yet)
- **Recent Completion**: Multi-line item implementation for procurement workflows (RFQ and PO with unlimited line items), QuestPDF rollout for compliant PDF generation

---

## 🎯 Immediate Priorities (Q4 2025 - Q1 2026)

### 1. Complete UI for Existing Backend Services

#### Customer Portal UI (2-3 weeks)
**Status**: Backend complete, UI needed  
**Priority**: HIGH  
**Assignee**: TBD

**Tasks**:
- [ ] Design customer-facing dashboard layout
- [ ] Implement project status visualization component
- [ ] Create document download interface
- [ ] Add communication/messaging features
- [ ] Implement customer authentication flow
- [ ] Add mobile-responsive design
- [ ] Test with sample customer accounts

**Acceptance Criteria**:
- Customers can view their project status
- Customers can download project documents
- Customers can communicate with internal team
- All features work on mobile devices

---

#### Compliance Analytics Dashboard (2-3 weeks)
**Status**: Backend complete, UI needed  
**Priority**: HIGH  
**Assignee**: TBD

**Tasks**:
- [ ] Design executive compliance dashboard layout
- [ ] Implement trend visualization charts (Chart.js or similar)
- [ ] Create compliance metrics overview cards
- [ ] Add date range filters and drill-down capabilities
- [ ] Implement export functionality (PDF, Excel)
- [ ] Add custom report builder interface
- [ ] Create automated report scheduling

**Acceptance Criteria**:
- Executive dashboard shows key compliance metrics
- Charts display trends over time
- Users can export reports in multiple formats
- Custom reports can be created and scheduled

---

#### ✅ Procurement Management UI (COMPLETED October 2025)
**Status**: ✅ COMPLETE - Backend + Full UI with Multi-Line Support  
**Priority**: ~~MEDIUM~~ COMPLETED  
**Completed By**: AI Agent (October 5-6, 2025)

**Completed Tasks**:
- ✅ Created ProcurementService (721 lines) with full CRUD operations
- ✅ Built ProcurementController (13 REST API endpoints)
- ✅ Implemented ProcurementHttpService (368 lines)
- ✅ Designed price request management UI (`/procurement/price-requests`)
- ✅ Created PO receiving interface (`/procurement/po-receiving`)
- ✅ Implemented multi-line PriceRequestDialog (127 lines) with PriceRequestLineDialog (279 lines)
- ✅ Implemented multi-line PurchaseOrderDialog (269 lines) with PurchaseOrderLineDialog (279 lines)
- ✅ Built POReceivingDialog with EN 1090 certificate tracking
- ✅ Added automatic lot number generation (A1→AA999)
- ✅ Integrated with ProfileInventory for material traceability
- ✅ Implemented PO status workflow (Pending → InTransit → Received)
- ✅ Added feature gating (ProcurementManagement for Professional+)
- ✅ Removed remnant dependency from price requests (standalone workflow)

**Achievement Highlights**:
- Complete price request workflow (Draft → Sent → Quoted → Completed)
- **Multi-line RFQs with unlimited line items** (add/edit/delete before submission)
- **Multi-line Purchase Orders** with professional line item management
- Purchase order receiving with optional inventory creation
- EN 10204 certificate type tracking
- Heat/batch number traceability
- Auto-complete PO when all lines fully received
- Material specifications with profile types, steel grades, dimensions
- Consistent two-tier dialog pattern (main dialog + line editor) for RFQ and PO

---

#### ✅ Sourcing/RFQ Management UI (COMPLETED October 2025)
**Status**: ✅ COMPLETE - Backend + Full UI with Multi-Line Support  
**Priority**: ~~MEDIUM~~ COMPLETED  
**Completed By**: AI Agent (October 5-6, 2025)

**Completed Tasks**:
- ✅ Created unified sourcing/procurement service layer
- ✅ Implemented RFQ creation and distribution workflow with unlimited line items
- ✅ Built supplier quote management system
- ✅ Created quote comparison interface (in PriceRequests.razor)
- ✅ Implemented quote-to-PO conversion (ConvertToPurchaseOrderAsync)
- ✅ Added sourcing workflow automation
- ✅ Integrated with feature gating (SourcingManagement for Enterprise only)

**Achievement Highlights**:
- Multi-supplier RFQ distribution
- **Multi-line RFQs** - add unlimited materials to single request
- Quote comparison with pricing visibility
- One-click quote-to-PO conversion
- Supplier selection workflow
- Price request status tracking
- Professional two-tier dialog UX (main + line editor)

---

### 2. Code Quality Improvements

#### Eliminate Build Warnings (1 week)
**Status**: 9 warnings remaining (down from 51)  
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Fix remaining MudBlazor attribute warnings if any
- [ ] Fix async method warnings (CS1998)
  - [ ] ProductionPlanningController
- [ ] Verify all nullability warnings resolved
- [ ] Run code analysis and address any new warnings

**Acceptance Criteria**:
- Build produces 0 warnings
- No functionality is broken
- Code follows C# nullable reference types best practices

---

### 3. Demo Mode → Production Cutover Planning (1-2 weeks)
**Status**: Demo mode active, production toggle pending  
**Priority**: HIGH  
**Assignee**: TBD

**Tasks**:
- [ ] Document the service registration switch in `Program.cs` for demo vs production
- [ ] Configure user secrets for `ConnectionStrings:Directory`, `ConnectionStrings:SqlServerAdmin`, and `ConnectionStrings:TenantTemplate`
- [ ] Apply outstanding EF Core migrations for `DirectoryDbContext` and `AppDbContext`
- [ ] Seed mandatory baseline data through migrations or startup seeding
- [ ] Validate QuestPDF workflows (e.g., `PriceRequestDocumentGenerator`) against live tenant data in staging
- [ ] Update CI/CD variables and deployment docs for Azure resources

**Acceptance Criteria**:
- Toggling `DemoMode` cleanly switches between mock services and production services
- Procurement RFQ → PO → PDF flow succeeds against SQL-backed data
- Deployment checklist covers secrets, migrations, and validation steps

---

### 4. Test Infrastructure Setup

#### Unit Testing Framework (2 weeks)
**Status**: Not started  
**Priority**: HIGH  
**Assignee**: TBD

**Tasks**:
- [ ] Add xUnit test project to solution
- [ ] Configure test project structure
- [ ] Add Moq for mocking dependencies
- [ ] Create base test classes and helpers
- [ ] Write tests for core services:
  - [ ] InventoryService (target: 80% coverage)
  - [ ] QualityControlService (target: 80% coverage)
  - [ ] WeldingManagementService (target: 80% coverage)
  - [ ] NDTService (target: 80% coverage)
  - [ ] DocumentGenerationService (target: 80% coverage)
- [ ] Set up code coverage reporting
- [ ] Configure CI to run tests automatically

**Acceptance Criteria**:
- Unit test project builds successfully
- At least 50% code coverage for services
- Tests run in CI pipeline
- Coverage report available in CI

---

#### Integration Testing (2 weeks)
**Status**: Not started  
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Set up WebApplicationFactory for API testing
- [ ] Create test database configuration
- [ ] Write integration tests for API controllers:
  - [ ] InventoryController
  - [ ] QualityControlController
  - [ ] WeldingManagementController
  - [ ] NDTController
  - [ ] AssemblyProgressController
- [ ] Test authentication and authorization
- [ ] Test feature gating middleware
- [ ] Test database transactions and rollback

**Acceptance Criteria**:
- Integration tests cover major API endpoints
- Tests use isolated test database
- Authentication tests verify security
- Feature gating tests verify tier restrictions

---

## 🚀 Short-Term Goals (Q1 2026)

### 4. Advanced Analytics Features

#### Real-Time Production Monitoring (4-5 weeks)
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Design real-time dashboard architecture
- [ ] Implement SignalR for live updates
- [ ] Create production metrics tracking
- [ ] Add equipment utilization monitoring
- [ ] Implement alert system for anomalies
- [ ] Create production efficiency KPIs
- [ ] Add historical data visualization
- [ ] Implement role-based dashboard views

**Technology Stack**:
- SignalR for real-time communication
- Chart.js or D3.js for visualizations
- Background jobs for metric calculations

---

#### Business Intelligence Dashboard (3-4 weeks)
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Design executive BI dashboard
- [ ] Implement financial performance metrics
- [ ] Create operational efficiency analytics
- [ ] Add project profitability tracking
- [ ] Implement resource utilization reports
- [ ] Create customizable widget system
- [ ] Add data export capabilities
- [ ] Implement scheduled report generation

---

### 5. Mobile Application Development

#### .NET MAUI Mobile App (6-8 weeks)
**Priority**: HIGH  
**Assignee**: TBD

**Tasks**:
- [ ] Create .NET MAUI project structure
- [ ] Design mobile-first UI/UX
- [ ] Implement offline-first architecture
- [ ] Create shop floor worker interface:
  - [ ] Assembly status updates
  - [ ] Quality check recording
  - [ ] Photo capture for documentation
  - [ ] Barcode/QR code scanning
- [ ] Implement material scanning features
- [ ] Add certificate verification scanning
- [ ] Implement data synchronization
- [ ] Test on iOS and Android devices
- [ ] Deploy to app stores

**Key Features**:
- Offline capability for shop floor
- Barcode/QR code scanning
- Photo capture and upload
- Real-time sync when online

---

## 📱 Medium-Term Goals (Q2-Q3 2026)

### 6. AI-Powered Features

#### AI Quality Control System (6-8 weeks)
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Research ML.NET capabilities for quality detection
- [ ] Collect training data from existing quality records
- [ ] Design ML pipeline architecture
- [ ] Implement image processing for weld quality
- [ ] Train defect detection model
- [ ] Create automated quality scoring system
- [ ] Implement risk prediction for assemblies
- [ ] Add process optimization recommendations
- [ ] Test accuracy and tune model
- [ ] Deploy ML model to production

**Success Metrics**:
- >90% accuracy in defect detection
- <5 second response time
- Reduced quality inspection time by 25%

---

#### Predictive Maintenance (4-6 weeks)
**Priority**: LOW  
**Assignee**: TBD

**Tasks**:
- [ ] Design predictive maintenance data model
- [ ] Collect equipment usage and failure data
- [ ] Implement ML model for failure prediction
- [ ] Create maintenance scheduling system
- [ ] Add equipment health monitoring
- [ ] Implement automated alerts for maintenance needs
- [ ] Create maintenance cost tracking

---

### 7. Advanced Compliance Features

#### Sustainability & ESG Tracking (4-5 weeks)
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Design ESG data model
- [ ] Implement carbon footprint calculation
- [ ] Add energy consumption monitoring
- [ ] Create waste generation tracking
- [ ] Implement recycling opportunity identification
- [ ] Add ESG reporting dashboard
- [ ] Create sustainability metrics visualization
- [ ] Implement compliance report generation

---

#### Automated Compliance Reporting (3-4 weeks)
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Design automated report generation system
- [ ] Implement scheduled report creation
- [ ] Add email distribution for reports
- [ ] Create report templates for different standards
- [ ] Implement automated data collection
- [ ] Add exception handling and notifications
- [ ] Create report archive and retrieval

---

## 🏗️ Long-Term Goals (Q4 2026+)

### 8. Architecture Evolution

#### Microservices Migration (8-10 weeks)
**Priority**: LOW  
**Assignee**: TBD

**Phases**:
1. **Service Extraction** (3 weeks)
   - Extract Inventory service
   - Extract Authentication service
   - Extract Reporting service

2. **Container Orchestration** (3 weeks)
   - Set up Kubernetes cluster
   - Configure service mesh
   - Implement inter-service communication

3. **Migration & Testing** (4 weeks)
   - Gradual migration of features
   - Load testing
   - Performance optimization

---

#### Digital Twin Implementation (10-12 weeks)
**Priority**: LOW  
**Assignee**: TBD

**Tasks**:
- [ ] Research digital twin frameworks
- [ ] Design 3D visualization architecture
- [ ] Implement assembly 3D models
- [ ] Add process simulation capabilities
- [ ] Create predictive modeling
- [ ] Implement virtual quality testing
- [ ] Add real-time synchronization
- [ ] Create digital twin dashboard

---

### 9. Integration & Ecosystem

#### ERP Integration (4-6 weeks)
**Priority**: MEDIUM  
**Assignee**: TBD

**Tasks**:
- [ ] Design integration architecture
- [ ] Implement SAP connector
- [ ] Implement Oracle ERP connector
- [ ] Add data synchronization
- [ ] Create mapping configuration UI
- [ ] Implement error handling and retry logic
- [ ] Add integration monitoring dashboard

---

#### Supply Chain Integration (3-4 weeks)
**Priority**: LOW  
**Assignee**: TBD

**Tasks**:
- [ ] Design supply chain API
- [ ] Implement supplier portal integration
- [ ] Add logistics tracking integration
- [ ] Create automated ordering system
- [ ] Implement inventory level alerts
- [ ] Add supplier performance tracking

---

## 🔧 Technical Debt & Maintenance

### Ongoing Tasks

#### Documentation Maintenance
**Frequency**: Monthly  
**Assignee**: ALL

**Tasks**:
- [ ] Update README.md with new features
- [ ] Update API documentation
- [ ] Update database schema documentation
- [ ] Verify all dates are current (October 2025+)
- [ ] Update statistics (project count, file count, etc.)
- [ ] Update implementation status document
- [ ] Review and update roadmap

---

#### Security Updates
**Frequency**: Weekly  
**Assignee**: DevOps

**Tasks**:
- [ ] Monitor NuGet package vulnerabilities
- [ ] Update packages with security patches
- [ ] Run security scans (Trivy)
- [ ] Review and fix security alerts
- [ ] Update dependencies regularly

---

#### Performance Monitoring
**Frequency**: Bi-weekly  
**Assignee**: TBD

**Tasks**:
- [ ] Review Application Insights metrics
- [ ] Identify performance bottlenecks
- [ ] Optimize slow database queries
- [ ] Review and optimize API response times
- [ ] Monitor database size and growth
- [ ] Optimize frontend bundle size

---

## 📝 Documentation Requirements

For each feature/task completed, ensure:

### Required Documentation Updates
1. **README.md**
   - Add feature to "Available Now" section
   - Update statistics if applicable
   - Update current status

2. **Implementation Status Document**
   - Move feature from "Planned" to "Complete"
   - Update module breakdown
   - Update statistics

3. **API Documentation**
   - Document new endpoints
   - Update OpenAPI/Swagger specs
   - Add usage examples

4. **Database Documentation**
   - Document new tables/columns
   - Update schema diagrams
   - Document migrations

5. **User Documentation**
   - Create user guides for new features
   - Update quick reference guides
   - Add screenshots/videos

---

## 🎯 Success Metrics

### Q4 2025 Targets
- [ ] Customer Portal UI complete and deployed
- [ ] Compliance Analytics Dashboard complete
- [ ] 0 build warnings
- [ ] >50% unit test coverage for services
- [ ] Integration tests for major API controllers

### Q1 2026 Targets
- [ ] Procurement UI complete
- [ ] Sourcing/RFQ UI complete
- [ ] Mobile app MVP launched
- [ ] >70% unit test coverage
- [ ] Real-time production monitoring live

### Q2-Q3 2026 Targets
- [ ] AI quality control in production
- [ ] ESG tracking implemented
- [ ] >80% unit test coverage
- [ ] E2E tests for critical workflows

### Q4 2026 Targets
- [ ] Microservices architecture evaluation complete
- [ ] Digital twin POC completed
- [ ] ERP integration live with first customer
- [ ] Mobile app feature parity with web

---

## 🚦 Priority Matrix

### Must Have (Critical)
1. Customer Portal UI
2. Compliance Analytics Dashboard
3. Unit test infrastructure
4. Zero build warnings

### Should Have (Important)
1. Procurement UI
2. Sourcing/RFQ UI
3. Integration tests
4. Mobile app

### Nice to Have (Enhancement)
1. Real-time monitoring
2. AI quality control
3. ESG tracking
4. Advanced analytics

### Future Consideration (Long-term)
1. Microservices migration
2. Digital twin
3. Blockchain traceability
4. Predictive maintenance

---

## 📞 Contact & Resources

### Project Leads
- **Technical Lead**: TBD
- **Product Owner**: TBD
- **DevOps Lead**: TBD

### Resources
- **GitHub Repository**: https://github.com/petersonmatiss/manimp
- **Documentation**: `/docs` folder in repository
- **CI/CD**: GitHub Actions
- **Deployment**: Azure Container Apps (recommended)

---

## 📅 Review Schedule

This document should be reviewed and updated:
- **Weekly**: During sprint planning
- **Monthly**: Comprehensive review of priorities
- **Quarterly**: Major roadmap adjustments

**Next Scheduled Review**: November 2025

---

**Document Status**: ACTIVE  
**Maintained By**: Development Team  
**Last Updated**: October 2025
