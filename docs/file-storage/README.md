# File Storage Documentation

Multi-domain file storage architecture with Azure Blob Storage, role-based sharing, and external client access.

---

## 📚 Documents

### ⭐ [file-storage-multi-domain-architecture.md](./file-storage-multi-domain-architecture.md)
**Complete implementation guide** (3000+ lines) - THE definitive reference

**Contents:**
- Three-domain architecture design
- Database schema (5 new tables)
- Service implementations (AzureBlobStorageService, FileAccessControlService)
- Multi-domain tenant resolver
- Admin panel UI (file sharing settings)
- Internal file browser (MudBlazor)
- External client portal
- Role-based access control
- Time-limited share links
- Project phase-based visibility
- 6-week implementation plan

---

### 📖 [file-storage-multi-domain-quick-ref.md](./file-storage-multi-domain-quick-ref.md)
**Quick reference and command cheat sheet** (400 lines)

**Contents:**
- Architecture summary
- Key commands (migrations, Azure CLI)
- Service methods reference
- Common operations
- Troubleshooting quick fixes

---

### 📋 [file-storage-implementation-summary.md](./file-storage-implementation-summary.md)
**Executive summary** (300 lines)

**Contents:**
- High-level overview
- Key features list
- Cost breakdown
- Implementation phases
- Decision rationale

---

### 📝 [file-storage-subdomain-plan.md](./file-storage-subdomain-plan.md)
**Original single-domain plan** (500 lines)

**Contents:**
- Initial subdomain approach
- Single file portal design
- Evolution to multi-domain

---

### 🔍 [file-storage-comparison.md](./file-storage-comparison.md)
**Visual comparisons** (300 lines)

**Contents:**
- WebDAV vs Subdomain Portal
- Single domain vs Multi-domain
- Database vs Blob Storage
- Cost comparisons

---

### 📚 [file-storage-quick-ref.md](./file-storage-quick-ref.md)
**Legacy quick reference**

**Contents:**
- Early implementation notes
- Preserved for historical reference

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                  Three-Domain Architecture                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  🏢 Internal Employees                                      │
│  ├─ {tenant}.manimp.com          → Main Application        │
│  └─ {tenant}.files.manimp.com    → File Portal             │
│                                                              │
│  👤 External Clients                                        │
│  └─ {tenant}.docs.manimp.com     → Client Document Portal  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                    Azure Blob Storage                        │
├─────────────────────────────────────────────────────────────┤
│  Container per tenant: tenant-{guid}/                       │
│  ├─ projects/{project-id}/                                  │
│  ├─ documents/{doc-type}/                                   │
│  ├─ drawings/{drawing-id}/                                  │
│  └─ shared/{share-token}/                                   │
│                                                              │
│  Lifecycle Policies:                                        │
│  ├─ Hot tier: Recent files (30 days)                       │
│  ├─ Cool tier: Older files (90 days)                       │
│  └─ Archive tier: Old files (1 year+)                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Key Features

### Multi-Domain Access
✅ **Internal File Portal** (`*.files.manimp.com`)
- Full file browser with folder navigation
- Upload/download/share/delete operations
- MudBlazor UI with drag-and-drop
- Role-based permissions (Admin/Manager/User)

✅ **External Client Portal** (`*.docs.manimp.com`)
- Project-based file filtering
- Phase-based visibility (automatically release docs when phase completes)
- Optional authentication (configurable per tenant)
- Anonymous access with share links
- Read-only access (no upload/delete)

### Role-Based Sharing
```
Admin:
├─ Upload: ✅
├─ Download: ✅
├─ Share: ✅ Unlimited time or custom days
├─ Delete: ✅
└─ Manage external users: ✅

Manager:
├─ Upload: ✅
├─ Download: ✅
├─ Share: ✅ Max 90 days
├─ Delete: ❌ (own files only)
└─ View external users: ✅

User:
├─ Upload: ✅
├─ Download: ✅
├─ Share: ✅ Max 30 days
├─ Delete: ❌
└─ View external users: ❌
```

### Time-Limited Shares
✅ Unlimited (Admin only)  
✅ Custom days (7, 14, 30, 60, 90)  
✅ Automatic expiration  
✅ Revocable share links  
✅ Access logging

### Project Phase Visibility
```
Project: Bridge Construction
├─ Phase 1: Design        [Complete] ✅ → Docs visible to client
├─ Phase 2: Fabrication   [Complete] ✅ → Docs visible to client
├─ Phase 3: Delivery      [Active]   ⏳ → Docs hidden until complete
└─ Phase 4: Installation  [Pending]  ❌ → Docs hidden
```

---

## 🚀 Quick Start

### 1. Run Database Migrations
```bash
# Add Subdomain field to Tenants
cd Manimp.Directory
dotnet ef migrations add AddSubdomainToTenant --context DirectoryDbContext
dotnet ef database update

# Add file storage tables
cd ../Manimp.Data
dotnet ef migrations add AddFileSharingSettings --context AppDbContext
dotnet ef migrations add AddExternalUsers --context AppDbContext
dotnet ef migrations add AddProjectFileVisibility --context AppDbContext
dotnet ef database update
```

### 2. Configure Azure Blob Storage
```bash
# Create storage account
az storage account create \
  --name stmanimp \
  --resource-group rg-manimp-prod \
  --location westeurope \
  --sku Standard_LRS \
  --kind StorageV2

# Get connection string
az storage account show-connection-string \
  --name stmanimp \
  --resource-group rg-manimp-prod
```

### 3. Update appsettings.json
```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "ContainerName": "files",
    "EnableCdn": false
  }
}
```

### 4. Implement Services
```bash
# Copy implementations from:
# file-storage-multi-domain-architecture.md

# Create:
# - Manimp.Services/Implementation/AzureBlobStorageService.cs
# - Manimp.Services/Implementation/FileAccessControlService.cs
# - Manimp.Services/Implementation/MultiDomainTenantResolver.cs
```

### 5. Register Services
```csharp
// Manimp.Web/Program.cs
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IFileAccessControlService, FileAccessControlService>();
builder.Services.AddScoped<ITenantResolver, MultiDomainTenantResolver>();
```

### 6. Add UI Components
```bash
# Copy Blazor components from architecture doc:
# - Manimp.Web/Components/Pages/FileBrowser.razor
# - Manimp.Web/Components/Pages/ClientPortal.razor
# - Manimp.Web/Components/Pages/Admin/FileSharingSettings.razor
```

---

## 💰 Cost Breakdown

```
Azure Blob Storage (Hot Tier):
├─ Storage (1TB):           $18.00/month
├─ Transactions (10M):      $0.50/month
├─ Data transfer (100GB):   $8.50/month
└─ Total:                   $27.00/month

With Lifecycle Policies:
├─ Hot (100GB, 30 days):    $1.80/month
├─ Cool (400GB, 90 days):   $4.00/month
├─ Archive (500GB, 1yr+):   $1.00/month
└─ Total:                   $6.80/month ✅ 75% savings

Azure DNS (for subdomains):
├─ Zone hosting:            $0.50/month
├─ Queries (10M):           $4.00/month
└─ Total:                   $4.50/month

GRAND TOTAL:                $11.30/month
```

---

## 🔧 Common Operations

### Upload File
```csharp
var fileStream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
var result = await fileStorageService.UploadFileAsync(
    fileStream,
    $"tenant-{tenantId}/projects/{projectId}",
    file.Name,
    file.ContentType
);
```

### Generate Share Link
```csharp
var sasToken = await fileStorageService.GetSasTokenAsync(
    blobUri,
    validityInDays: 30
);
var shareLink = $"{blobUri}?{sasToken}";
```

### Check Access Permission
```csharp
var canAccess = await fileAccessControlService.CanAccessFileAsync(
    userId,
    tenantId,
    fileId,
    FileAccessLevel.Read
);
```

### List Files with Phase Filter
```csharp
var files = await fileStorageService.ListFilesAsync(
    $"tenant-{tenantId}/projects/{projectId}"
);

// Filter by completed phases only (for external users)
var visibleFiles = files.Where(f => 
    f.ProjectPhase?.IsCompleted == true
);
```

---

## 🧪 Testing

### Test File Upload
```bash
# Start app
cd Manimp.Web
dotnet run --urls https://localhost:5001

# Visit internal file portal
https://localhost:5001/files

# Upload test file
# Should see in Azure Portal → Storage Account → Containers
```

### Test External Access
```bash
# Update /etc/hosts
127.0.0.1 acme.docs.localhost

# Visit
https://acme.docs.localhost:5001

# Should see only files from completed project phases
```

### Test Share Link
```bash
# Create share in internal portal
# Copy share link
# Open in incognito window
# Should download file without authentication
```

---

## 🐛 Troubleshooting

### "Storage account not found"
```bash
# Check connection string
az storage account show-connection-string \
  --name stmanimp \
  --resource-group rg-manimp-prod

# Verify in appsettings.json
```

### "Blob not found" when downloading
```bash
# Check blob path format
# Should be: tenant-{guid}/projects/{project-id}/filename.pdf

# List blobs in container
az storage blob list \
  --account-name stmanimp \
  --container-name files
```

### Share link expired
```csharp
// Regenerate SAS token
var newSasToken = await fileStorageService.GetSasTokenAsync(
    blobUri,
    validityInDays: 30
);

// Update FileShare record in database
fileShare.SasToken = newSasToken;
fileShare.ExpiresAt = DateTime.UtcNow.AddDays(30);
await context.SaveChangesAsync();
```

### External user can't see files
```csharp
// Check project phase completion
var phase = await context.ProjectPhases
    .FirstOrDefaultAsync(p => p.Id == file.ProjectPhaseId);

if (!phase.IsCompleted)
{
    // Phase not complete - file hidden
    // Mark phase as complete:
    phase.IsCompleted = true;
    phase.CompletedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
}
```

---

## 📊 Monitoring

### Storage Metrics
```bash
# Check storage usage
az storage account show-usage \
  --account-name stmanimp

# Blob transaction metrics
az monitor metrics list \
  --resource stmanimp \
  --metric Transactions \
  --aggregation Total
```

### File Access Logs
```csharp
// Log in FileAccessControlService
_logger.LogInformation(
    "File accessed: {FileId} by {UserId} ({AccessLevel})",
    fileId, userId, accessLevel);

// Query in Application Insights
traces
| where message contains "File accessed"
| summarize count() by tostring(customDimensions.FileId)
| order by count_ desc
```

---

## 🔗 Related Documentation

- **[../azure-infrastructure/azure-infrastructure-setup.md](../azure-infrastructure/azure-infrastructure-setup.md)** - Blob Storage setup
- **[../azure-infrastructure/azure-dns-setup-guide.md](../azure-infrastructure/azure-dns-setup-guide.md)** - Wildcard domain configuration
- **[../authentication/azure-b2c-authentication.md](../authentication/azure-b2c-authentication.md)** - Multi-domain authentication
- **[../project-management/project-management-enhancement.md](../project-management/project-management-enhancement.md)** - Project phase management

---

## 📅 Implementation Timeline

**Week 1:** Database migrations  
**Week 2:** Service implementations (Blob Storage + Access Control)  
**Week 3:** Admin panel (file sharing settings)  
**Week 4:** Client portal UI  
**Week 5:** Azure infrastructure  
**Week 6:** Testing + production deployment

---

**Total Implementation Time:** 6 weeks  
**Lines of Code:** ~2,500 lines  
**Complexity:** Medium-High

---

## 🎯 What's Next

### Week 1: Database Setup
- [ ] Create migration: AddSubdomainToTenant
- [ ] Create migration: AddFileSharingSettings
- [ ] Create migration: AddExternalUsers table
- [ ] Create migration: AddProjectFileVisibility table
- [ ] Create migration: AddFileShare table
- [ ] Run migrations and verify schema

### Week 2: Azure Blob Storage
- [ ] Create Azure Storage Account (stmanimp)
- [ ] Configure blob containers with access levels
- [ ] Implement AzureBlobStorageService
- [ ] Add lifecycle policies (Hot → Cool → Archive)
- [ ] Test file upload/download operations
- [ ] Implement SAS token generation

### Week 3: Services & Access Control
- [ ] Implement FileAccessControlService
- [ ] Create MultiDomainTenantResolver
- [ ] Add role-based permission checks
- [ ] Implement time-limited share links
- [ ] Test project phase-based filtering

### Week 4: Internal File Portal
- [ ] Create FileBrowser.razor component
- [ ] Add folder navigation and breadcrumbs
- [ ] Implement drag-and-drop upload
- [ ] Add file sharing dialog
- [ ] Create file preview functionality
- [ ] Test on {tenant}.files.manimp.com

### Week 5: External Client Portal
- [ ] Create ClientPortal.razor component
- [ ] Implement project-based file filtering
- [ ] Add phase completion checks
- [ ] Create anonymous access with share links
- [ ] Add optional authentication flow
- [ ] Test on {tenant}.docs.manimp.com

### Week 6: Admin & Testing
- [ ] Create FileSharingSettings admin page
- [ ] Add external user management UI
- [ ] Implement project file marking interface
- [ ] End-to-end testing all workflows
- [ ] Performance testing (large files)
- [ ] Security audit of access controls

### Future Enhancements
- [ ] Add file versioning system
- [ ] Implement file search/indexing
- [ ] Add thumbnail generation for images/PDFs
- [ ] Create file activity logs
- [ ] Implement bulk download (ZIP)
- [ ] Add file expiration policies
- [ ] Integrate with document management system

---

**Cloud-native file storage!** ☁️
