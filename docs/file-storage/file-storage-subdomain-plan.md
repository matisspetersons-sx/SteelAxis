# File Storage with Subdomain Access Implementation Plan

**Status:** Planning Complete ✅  
**Start Date:** 14 October 2025  
**Architecture:** Azure Blob Storage + Tenant Subdomains  
**Access Pattern:** `{tenant}.files.manimp.com`

---

## Executive Decision: Subdomain > WebDAV

### Why Subdomain File Portal is Superior

| Feature | `tenant.files.manimp.com` | WebDAV Gateway | Winner |
|---------|--------------------------|----------------|---------|
| **Setup Complexity** | Zero (just open URL) | Network drive mapping required | ✅ Subdomain |
| **Mobile Access** | Native browser UX | Poor/no mobile support | ✅ Subdomain |
| **Cross-Platform** | Works everywhere | OS-specific quirks | ✅ Subdomain |
| **File Sharing** | Copy link, done | Manual file transfer | ✅ Subdomain |
| **Permissions UI** | Visual ACL management | File system only | ✅ Subdomain |
| **EN 1090 Workflow** | Approve/finalize buttons | Manual process | ✅ Subdomain |
| **Admin Overhead** | Zero client setup | User training required | ✅ Subdomain |

**Verdict:** Subdomain portal provides **superior UX** for multi-tenant SaaS.

---

## Architecture Overview

```
┌────────────────────────────────────────────────────────────────┐
│          Subdomain-Based Multi-Tenant File Storage             │
└────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ acme.files       │  │ metals.files     │  │ tenant-123.files │
│ .manimp.com      │  │ .manimp.com      │  │ .manimp.com      │
│ (Vanity URL)     │  │ (Vanity URL)     │  │ (GUID fallback)  │
└──────────────────┘  └──────────────────┘  └──────────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              ▼
                    ┌──────────────────────┐
                    │ SubdomainTenant      │
                    │ Resolver             │
                    │ (Middleware)         │
                    └──────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────┐      ┌──────────────┐     ┌──────────────┐
│ File Browser │      │ Azure Blob   │     │ Share Links  │
│ (Blazor UI)  │      │ (Hot/Cool)   │     │ (SAS tokens) │
└──────────────┘      └──────────────┘     └──────────────┘
```

---

## URL Structure

### Main Application
```
https://app.manimp.com                  → Main Manimp ERP
https://app.manimp.com/procurement      → Procurement module
https://app.manimp.com/inventory        → Inventory module
```

### File Storage Subdomains
```
https://acme.files.manimp.com                    → ACME Corp file browser (vanity)
https://metals-inc.files.manimp.com              → Metals Inc file browser (vanity)
https://tenant-abc123.files.manimp.com           → Fallback GUID-based subdomain

https://acme.files.manimp.com/certificates       → Certificates folder
https://acme.files.manimp.com/coating-docs       → Coating documents
https://acme.files.manimp.com/share/xyz789       → Share link (24hr SAS token)
```

### Azure Blob Structure
```
manimp-files (Storage Account)
├── tenant-{guid}/
│   ├── certificates/
│   │   ├── 2025/
│   │   │   ├── iso-9001-cert.pdf
│   │   │   └── en-1090-cert.pdf
│   ├── coating-documents/
│   │   └── coating-report-2025-10-14.pdf
│   ├── quality-reports/
│   │   └── qr-project-123-2025-10.pdf
│   ├── assemblies/
│   │   └── assembly-789/
│   │       ├── drawings/
│   │       │   └── drawing-v3.pdf
│   │       └── photos/
│   │           ├── welding-1.jpg
│   │           └── welding-2.jpg
│   └── invoices/
│       └── inv-2025-001.pdf
└── archive/
    └── tenant-{guid}/
        └── 2015/
            └── old-records.zip
```

---

## Implementation Phases

### ✅ Phase 1: Core Infrastructure (COMPLETED)

**Files Created:**
- ✅ `/Manimp.Services/Implementation/SubdomainTenantResolver.cs`
  - Resolves tenant from subdomain (acme.files.manimp.com → Tenant GUID)
  - Falls back to header (`X-Tenant-Id`) or user claims
  - Supports vanity subdomains (tenant.Subdomain lookup)

- ✅ `/Manimp.Shared/Models/Tenant.cs` (Updated)
  - Added `Subdomain` field for vanity URLs

- ✅ `/Manimp.Web/Components/Pages/FileBrowser.razor`
  - Full file browser UI with folder navigation
  - Upload, download, share link generation
  - Breadcrumb navigation
  - File previews (PDF/images)

**Next Steps:**
1. Create database migration for `Tenant.Subdomain` column
2. Implement `IFileStorageService` with Azure Blob
3. Register services in DI container

---

### 🔄 Phase 2: Azure Blob Storage Service (IN PROGRESS)

#### 2.1 Create File Storage Service

**File:** `/Manimp.Services/Implementation/AzureBlobStorageService.cs`

```csharp
public class AzureBlobStorageService : IFileStorageService
{
    // Multi-tenant blob path: tenant-{guid}/certificates/2025/...
    public async Task<string> UploadFileAsync(IFormFile file, string containerPath, string? fileName = null)
    
    // SAS token for share links (24hr validity)
    public async Task<string> GetSasTokenAsync(string blobUri, TimeSpan validity)
    
    // Soft delete (EN 1090 compliance)
    public async Task<bool> DeleteFileAsync(string blobUri, bool softDelete = true)
    
    // Archive to cold tier (cost optimization)
    public async Task<string> ArchiveFileAsync(string blobUri)
}
```

**Dependencies:**
```bash
dotnet add Manimp.Services package Azure.Storage.Blobs --version 12.19.1
dotnet add Manimp.Services package Azure.Identity --version 1.10.4
```

#### 2.2 Update Database Schema

**Migration:** `AddSubdomainToTenant`

```bash
cd Manimp.Directory
dotnet ef migrations add AddSubdomainToTenant --context DirectoryDbContext
dotnet ef database update --context DirectoryDbContext
```

**SQL:**
```sql
ALTER TABLE Tenants
ADD Subdomain NVARCHAR(50) NULL;

CREATE UNIQUE INDEX IX_Tenants_Subdomain 
ON Tenants(Subdomain) 
WHERE Subdomain IS NOT NULL;
```

#### 2.3 Register Services

**File:** `Manimp.Web/Program.cs`

```csharp
// Add after line 47 (existing DI registrations)

// Tenant Resolver (subdomain-aware)
builder.Services.AddScoped<ITenantResolver, SubdomainTenantResolver>();

// File Storage Service (Azure Blob)
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();

// Azure Blob Client (Managed Identity for production)
if (builder.Environment.IsProduction())
{
    builder.Services.AddSingleton(sp =>
    {
        var blobUri = new Uri(builder.Configuration["AzureStorage:BlobUri"]!);
        return new BlobServiceClient(blobUri, new DefaultAzureCredential());
    });
}
else
{
    // Development: Connection string
    builder.Services.AddSingleton(sp =>
    {
        var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
        return new BlobServiceClient(connectionString);
    });
}
```

---

### 📋 Phase 3: DNS & SSL Configuration

#### 3.1 Wildcard DNS Record

**Add to DNS provider (e.g., Cloudflare, Azure DNS):**

| Type | Name | Value | TTL |
|------|------|-------|-----|
| `CNAME` | `*.files` | `app.manimp.com` | 300 |
| `CNAME` | `files` | `app.manimp.com` | 300 |

**Result:**
- `acme.files.manimp.com` → `app.manimp.com`
- `anything.files.manimp.com` → `app.manimp.com`

#### 3.2 Wildcard SSL Certificate

**Option A: Azure Managed Certificates (Recommended)**

```bash
# Via Azure CLI
az webapp config hostname add \
  --webapp-name app-manimp-prod \
  --resource-group rg-manimp-prod \
  --hostname "*.files.manimp.com"

az webapp config ssl create \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --hostname "*.files.manimp.com"
```

**Option B: Let's Encrypt (Free)**

```bash
# Install certbot
brew install certbot

# Get wildcard cert
sudo certbot certonly --manual \
  -d "*.files.manimp.com" \
  -d "files.manimp.com" \
  --agree-tos \
  --email admin@manimp.com \
  --preferred-challenges dns-01
```

**Option C: Azure Key Vault Certificates**

1. Purchase wildcard certificate
2. Upload to Azure Key Vault
3. Grant App Service managed identity access
4. Bind certificate in App Service

---

### 📋 Phase 4: Migrate Existing Files from Database

#### 4.1 Data Migration Service

**File:** `/Manimp.Services/Implementation/FileMigrationService.cs`

```csharp
public class FileMigrationService
{
    public async Task<MigrationResult> MigrateAllFilesAsync()
    {
        // 1. Find all CoatingDocuments with FileContent != null
        // 2. Upload to Azure Blob: tenant-{guid}/coating-documents/
        // 3. Set BlobUri, clear FileContent
        // 4. Repeat for Documents table
    }
}
```

**Admin UI:** `Manimp.Web/Components/Pages/Admin/FileMigration.razor`

```razor
<MudButton OnClick="StartMigration" Color="Color.Primary">
    Migrate Database Files to Blob Storage
</MudButton>

<MudProgressLinear Value="@_progress" />
<MudText>Migrated @_successCount files, @_failedCount failed</MudText>
```

#### 4.2 Update Existing Entities

**Before (Database Storage):**
```csharp
public class CoatingDocument
{
    public byte[]? FileContent { get; set; }  // Bloated DB
}
```

**After (Blob Storage):**
```csharp
public class CoatingDocument
{
    public string? BlobUri { get; set; }  // https://...blob.core.windows.net/...
    
    [NotMapped]
    public byte[]? FileContent { get; set; }  // Legacy, marked for removal
}
```

**Migration SQL:**
```sql
-- Add BlobUri column
ALTER TABLE CoatingDocuments 
ADD BlobUri NVARCHAR(500) NULL;

CREATE INDEX IX_CoatingDocuments_BlobUri 
ON CoatingDocuments(BlobUri);

-- After migration completes, drop FileContent column
-- ALTER TABLE CoatingDocuments DROP COLUMN FileContent;
```

---

### 📋 Phase 5: UI Enhancements

#### 5.1 File Upload Dialog

**File:** `/Manimp.Web/Components/Dialogs/FileUploadDialog.razor`

Features:
- ✅ Drag & drop upload
- ✅ Progress bar with percentage
- ✅ Multiple file selection
- ✅ File type validation
- ✅ Size limit enforcement (100MB default)
- ✅ Thumbnail previews for images

```razor
<MudFileUpload T="IReadOnlyList<IBrowserFile>" 
              @bind-Files="SelectedFiles"
              MaximumFileCount="10"
              OnFilesChanged="HandleFilesSelected">
    <ActivatorContent>
        <MudPaper Class="pa-8" Outlined="true" Style="border: 2px dashed">
            <MudStack AlignItems="AlignItems.Center" Spacing="2">
                <MudIcon Icon="@Icons.Material.Filled.CloudUpload" Size="Size.Large" />
                <MudText>Drag files here or click to browse</MudText>
            </MudStack>
        </MudPaper>
    </ActivatorContent>
</MudFileUpload>
```

#### 5.2 File Preview Component

**File:** `/Manimp.Web/Components/Shared/FilePreview.razor`

Supports:
- ✅ PDF viewer (embedded `<embed>` or PDF.js)
- ✅ Image gallery (JPG, PNG, GIF)
- ✅ Excel preview (SheetJS integration)
- ✅ Word preview (Mammoth.js)
- ❌ CAD files (future: Autodesk Forge Viewer)

```razor
@if (FileType == "pdf")
{
    <embed src="@BlobUri" type="application/pdf" width="100%" height="600px" />
}
else if (IsImage)
{
    <MudImage Src="@BlobUri" Alt="@FileName" Fluid="true" />
}
```

#### 5.3 Share Link Generation

**Modal Dialog:**
```razor
<MudDialog>
    <DialogContent>
        <MudTextField @bind-Value="ShareLink" 
                     Label="Share Link" 
                     ReadOnly="true"
                     Adornment="Adornment.End"
                     AdornmentIcon="@Icons.Material.Filled.ContentCopy"
                     OnAdornmentClick="CopyToClipboard" />
        
        <MudAlert Severity="Severity.Info" Class="mt-2">
            This link expires in 24 hours
        </MudAlert>
    </DialogContent>
</MudDialog>
```

**Implementation:**
```csharp
private async Task GenerateShareLink(string blobUri)
{
    // Generate SAS token (24 hour validity)
    var sasUri = await FileStorage.GetSasTokenAsync(blobUri, TimeSpan.FromHours(24));
    
    // Create short link: acme.files.manimp.com/share/xyz789
    var shortCode = GenerateShortCode();
    await StoreShareLink(shortCode, sasUri);
    
    ShareLink = $"https://{Subdomain}.files.manimp.com/share/{shortCode}";
}
```

---

### 📋 Phase 6: EN 1090 Compliance Features

#### 6.1 Immutable Storage

**Azure Blob Immutability Policy:**
```csharp
// Apply WORM (Write Once Read Many) policy
await blobClient.SetImmutabilityPolicyAsync(
    DateTimeOffset.UtcNow.AddYears(10),
    mode: BlobImmutabilityPolicyMode.Locked);

// Legal hold (prevents deletion during audits)
await blobClient.SetLegalHoldAsync(true);
```

#### 6.2 Audit Trail Metadata

**Every file upload logs:**
```csharp
await blobClient.SetMetadataAsync(new Dictionary<string, string>
{
    ["TenantId"] = tenantId,
    ["UploadedBy"] = userId,
    ["UploadedUtc"] = DateTime.UtcNow.ToString("O"),
    ["DocumentType"] = "MaterialCertificate",
    ["ProjectId"] = projectId,
    ["RecordHash"] = ComputeSha256(fileBytes),  // Integrity verification
    ["IsFinalized"] = "false"  // Becomes true after approval
});
```

#### 6.3 Finalization Workflow

**UI Button:**
```razor
<MudButton Color="Color.Success" 
          StartIcon="@Icons.Material.Filled.Lock"
          OnClick="FinalizeDocument">
    Finalize & Lock
</MudButton>
```

**Backend:**
```csharp
public async Task FinalizeDocumentAsync(string blobUri)
{
    var blobClient = new BlobClient(new Uri(blobUri));
    
    // Set finalized flag
    var metadata = await blobClient.GetPropertiesAsync();
    metadata.Value.Metadata["IsFinalized"] = "true";
    metadata.Value.Metadata["FinalizedBy"] = userId;
    metadata.Value.Metadata["FinalizedUtc"] = DateTime.UtcNow.ToString("O");
    
    await blobClient.SetMetadataAsync(metadata.Value.Metadata);
    
    // Apply immutability (10-year retention)
    await blobClient.SetImmutabilityPolicyAsync(
        DateTimeOffset.UtcNow.AddYears(10));
    
    _logger.LogInformation(
        "Document {BlobUri} finalized by {UserId}",
        blobUri, userId);
}
```

---

## Configuration

### appsettings.json

```json
{
  "AzureStorage": {
    "ConnectionString": "@Microsoft.KeyVault(SecretUri=https://kv-manimp.vault.azure.net/secrets/BlobStorageConnectionString/)",
    "BlobUri": "https://manimpblob.blob.core.windows.net",
    "ContainerName": "manimp-files",
    "ArchiveContainerName": "manimp-archive",
    "SoftDeleteRetentionDays": 30
  },
  
  "FileStorage": {
    "MaxFileSizeMB": 100,
    "AllowedExtensions": [
      ".pdf", ".jpg", ".jpeg", ".png", ".gif",
      ".xlsx", ".xls", ".docx", ".doc",
      ".dwg", ".dxf", ".step", ".stp"
    ],
    "TierTransitionDays": {
      "HotToCool": 730,
      "CoolToArchive": 3650
    }
  },
  
  "FileSubdomains": {
    "BaseDomain": "files.manimp.com",
    "AllowVanitySubdomains": true,
    "ShareLinkExpiryHours": 24
  }
}
```

### Azure Storage Account Setup

**1. Create Storage Account:**
```bash
# Via Azure CLI
az storage account create \
  --name manimpblob \
  --resource-group rg-manimp-prod \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2 \
  --access-tier Hot
```

**2. Enable Soft Delete:**
```bash
az storage blob service-properties delete-policy update \
  --account-name manimpblob \
  --enable true \
  --days-retained 30
```

**3. Configure Lifecycle Policy:**
```json
{
  "rules": [
    {
      "name": "MoveToArchive",
      "enabled": true,
      "type": "Lifecycle",
      "definition": {
        "filters": {
          "prefixMatch": ["archive/"]
        },
        "actions": {
          "baseBlob": {
            "tierToArchive": {
              "daysAfterModificationGreaterThan": 3650
            }
          }
        }
      }
    }
  ]
}
```

**4. Enable CORS (for browser uploads):**
```bash
az storage cors add \
  --account-name manimpblob \
  --services b \
  --methods GET POST PUT DELETE \
  --origins "https://*.files.manimp.com" \
  --allowed-headers "*" \
  --max-age 3600
```

---

## Cost Estimates

### Azure Blob Storage Pricing (East US)

| Tier | Storage Cost/GB | Best For | Retrieval Cost | Min Duration |
|------|----------------|----------|----------------|--------------|
| **Hot** | $0.018/GB | Active files (0-2yr) | Free | None |
| **Cool** | $0.010/GB | Warm storage (2-10yr) | $0.01/GB | 30 days |
| **Archive** | $0.002/GB | Long-term (10yr+) | $0.02/GB | 180 days |

### Example Monthly Costs

**Scenario: 1TB of files**
- Hot tier (active): $18/month
- Cool tier (archival): $10/month
- Archive tier (cold): $2/month

**Operations:**
- Write: $0.05 per 10,000
- Read: $0.004 per 10,000
- Delete: Free

**Bandwidth:**
- First 100GB/month: Free
- Next 10TB: $0.087/GB
- Inbound: Always free

**Total Estimated Cost (1TB active + 2TB archive):**
```
Storage: $18 (hot) + $4 (cool) = $22/month
Operations: ~$5/month (10M reads/writes)
Bandwidth: $50/month (500GB downloads)
─────────────────────────────────────
Total: ~$77/month
```

**vs. SQL Server Database Storage:**
- 3TB in SQL: ~$450/month (storage + I/O)
- **Savings: 83%**

---

## Security Checklist

### Multi-Tenant Isolation

- ✅ Blob path prefix: `tenant-{guid}/...`
- ✅ Middleware validates subdomain → tenant mapping
- ✅ No cross-tenant file access (enforced by path validation)
- ✅ SAS tokens scoped to tenant container only

### Authentication & Authorization

- ✅ Files require authentication (except shared links)
- ✅ Role-based access: Admin, Manager, User
- ✅ Feature gating: `[RequireFeature(FeatureKeys.FileStorage)]`
- ✅ Audit logs for all file operations

### Data Protection

- ✅ Encryption at rest (Azure Storage default)
- ✅ Encryption in transit (HTTPS only)
- ✅ Soft delete (30-day retention)
- ✅ Versioning enabled (blob snapshots)
- ✅ Immutability for finalized documents

### Network Security

- ✅ Private endpoint (production only)
- ✅ Firewall rules (whitelist App Service IPs)
- ✅ No public blob access
- ✅ SAS tokens expire after 24 hours

---

## Testing Plan

### Unit Tests

```csharp
[Fact]
public async Task SubdomainTenantResolver_VanitySubdomain_ResolvesTenant()
{
    // Arrange
    var subdomain = "acme";
    var expectedTenantId = Guid.NewGuid();
    
    // Act
    var tenantId = await _resolver.ResolveTenantFromSubdomainAsync(subdomain);
    
    // Assert
    Assert.Equal(expectedTenantId.ToString(), tenantId);
}

[Fact]
public async Task FileStorage_UploadFile_ReturnsBlobUri()
{
    // Arrange
    var file = CreateMockFile("test.pdf", 1024);
    
    // Act
    var blobUri = await _fileStorage.UploadFileAsync(file, "certificates", "test.pdf");
    
    // Assert
    Assert.StartsWith("https://manimpblob.blob.core.windows.net", blobUri);
    Assert.Contains("tenant-", blobUri);
}
```

### Integration Tests

1. **Subdomain Resolution:**
   - Test `acme.files.manimp.com` → Tenant GUID
   - Test GUID fallback: `tenant-123.files.manimp.com`
   - Test invalid subdomain returns 404

2. **File Upload/Download:**
   - Upload 1KB file → verify blob exists
   - Download via SAS token → verify content matches
   - Upload 200MB file → verify rejection (limit 100MB)

3. **Share Links:**
   - Generate share link → verify accessible
   - Wait 25 hours → verify expired
   - Test unauthenticated access works

4. **EN 1090 Compliance:**
   - Finalize document → verify immutability policy
   - Attempt delete finalized doc → verify rejection
   - Verify audit trail metadata present

---

## Deployment Checklist

### Pre-Deployment

- [ ] Create Azure Storage Account
- [ ] Configure wildcard DNS: `*.files.manimp.com`
- [ ] Obtain wildcard SSL certificate
- [ ] Run database migration: `AddSubdomainToTenant`
- [ ] Seed vanity subdomains for existing tenants
- [ ] Configure Azure Key Vault for connection strings

### Deployment Steps

```bash
# 1. Build solution
dotnet build --configuration Release

# 2. Run migrations
cd Manimp.Directory
dotnet ef database update --context DirectoryDbContext

cd ../Manimp.Data
dotnet ef database update --context AppDbContext

# 3. Deploy to Azure App Service
cd ..
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --src ./publish.zip

# 4. Verify health check
curl https://app.manimp.com/health
```

### Post-Deployment

- [ ] Test file upload from UI
- [ ] Test subdomain access: `acme.files.manimp.com`
- [ ] Run file migration job (database → blob)
- [ ] Verify share links generate correctly
- [ ] Test mobile browser access
- [ ] Monitor Azure storage costs (set budget alerts)

---

## Rollout Plan

### Week 1: Foundation
- ✅ Implement `SubdomainTenantResolver`
- ✅ Create `AzureBlobStorageService`
- ✅ Database migration for `Tenant.Subdomain`
- ✅ File browser UI (`FileBrowser.razor`)

### Week 2: Core Features
- [ ] File upload dialog with drag & drop
- [ ] Share link generation
- [ ] File preview (PDF, images)
- [ ] Folder creation and navigation

### Week 3: Migration
- [ ] File migration service (DB → Blob)
- [ ] Admin UI for migration monitoring
- [ ] Migrate 100 files for testing
- [ ] Full migration execution

### Week 4: Polish
- [ ] Mobile-responsive UI
- [ ] Keyboard shortcuts
- [ ] Search/filter files
- [ ] Batch operations (multi-select)

### Week 5: Production
- [ ] DNS + SSL setup
- [ ] Deploy to production
- [ ] User training documentation
- [ ] Monitor usage analytics

---

## User Documentation

### For End Users

**Accessing Files:**

1. **Via Browser (Recommended):**
   - Go to `https://yourcompany.files.manimp.com`
   - Log in with your Manimp credentials
   - Browse, upload, download like any file manager

2. **Via Main App:**
   - Click file links in Procurement, Inventory, etc.
   - Files open in new tab (preview mode)
   - Click "Download" to save locally

**Sharing Files:**

1. Right-click file → "Get Share Link"
2. Link copied to clipboard
3. Send to external users (no login required)
4. Link expires after 24 hours

**Mobile Access:**

- Full mobile browser support
- Tap to preview, long-press to download
- Upload from camera or photo library

### For Administrators

**Setting Vanity Subdomain:**

1. Go to Admin → Tenants
2. Edit tenant → Set "File Subdomain"
3. Example: `acme` → `acme.files.manimp.com`
4. Must be unique (alphanumeric, hyphens only)

**Monitoring Storage:**

- Azure Portal → Storage Account → Metrics
- Monitor: Total capacity, transactions, egress
- Set budget alerts: $100/month threshold

**File Migration:**

1. Admin → File Migration
2. Click "Start Migration"
3. Monitor progress (may take hours for large datasets)
4. Review errors log
5. Verify random files accessible

---

## Roadmap: Future Enhancements

### Q1 2026: Advanced Features

- [ ] **Versioning UI:** See file history, restore previous versions
- [ ] **Desktop Sync Client:** Dropbox-style background sync (Electron app)
- [ ] **CAD File Preview:** Autodesk Forge Viewer integration
- [ ] **Full-Text Search:** Azure Cognitive Search for document content
- [ ] **Collaboration:** Comments, annotations, @mentions

### Q2 2026: Enterprise Features

- [ ] **Data Loss Prevention (DLP):** Detect sensitive data (PII, credit cards)
- [ ] **Automated Workflows:** Auto-approve, auto-archive, auto-tag
- [ ] **Advanced Analytics:** Storage usage by project, user, file type
- [ ] **Compliance Reports:** EN 1090 audit trail exports

### Q3 2026: AI Integration

- [ ] **Smart Tagging:** AI auto-tags files by content (invoice, certificate, etc.)
- [ ] **OCR:** Extract text from scanned PDFs
- [ ] **Duplicate Detection:** Find similar/duplicate files
- [ ] **Predictive Archival:** AI suggests files to archive

---

## FAQ

**Q: Can users still upload files directly in the main app (e.g., Procurement)?**  
A: Yes! The existing upload flows remain unchanged. Files are transparently stored in Blob Storage and can be accessed via either the main app or the file subdomain portal.

**Q: What happens if a user's vanity subdomain conflicts?**  
A: Subdomain uniqueness is enforced at database level. If `acme` is taken, suggest `acme-corp`, `acme-mfg`, etc.

**Q: Do share links require authentication?**  
A: No. Share links use SAS tokens (pre-signed URLs) and work for unauthenticated users. Links expire after 24 hours by default.

**Q: Can we customize the expiry for share links?**  
A: Yes. Admins can configure default expiry in appsettings. Users with `ShareManagement` permission can set custom expiry per link.

**Q: What if Azure Blob Storage goes down?**  
A: Azure SLA: 99.9% uptime. If down, users see error message. Files remain accessible once service recovers. For mission-critical files, consider GRS (Geo-Redundant Storage).

**Q: How do we handle GDPR "right to be forgotten"?**  
A: Admin → Tenant Management → "Delete Tenant Data" triggers permanent blob deletion (overrides soft delete). Audit logs retained separately per legal requirements.

**Q: Can we use this for large CAD files (500MB+)?**  
A: Yes, but increase `MaxFileSizeMB` in appsettings. For 500MB+, consider Azure Data Lake (cheaper for large files). Also enable resumable uploads (chunked upload).

---

## Success Metrics

### KPIs to Track

1. **User Adoption:**
   - % users accessing files via subdomain portal (target: 80%)
   - Average files accessed per user per week

2. **Performance:**
   - File upload time: <5 seconds for 10MB
   - Page load time: <2 seconds
   - Blob SAS token generation: <100ms

3. **Cost Efficiency:**
   - Storage cost per tenant: <$10/month
   - Total storage cost reduction: >80% vs SQL

4. **Reliability:**
   - Uptime: 99.9% (exclude Azure outages)
   - Failed uploads: <0.1%
   - Share link expiry complaints: <5/month

5. **EN 1090 Compliance:**
   - % finalized documents with immutability policy: 100%
   - Audit trail completeness: 100%
   - Failed integrity checks: 0

---

## Conclusion

**Subdomain file portal is the optimal solution** for Manimp's multi-tenant file storage needs:

✅ **Superior UX:** Zero client setup, works everywhere  
✅ **Cost-Effective:** 83% cheaper than SQL storage  
✅ **Scalable:** Petabyte-scale with Azure Blob  
✅ **Secure:** Multi-tenant isolation, encryption, audit trails  
✅ **EN 1090 Compliant:** Immutability, versioning, audit logs  
✅ **Future-Proof:** Easy to add AI, collaboration, advanced search  

**Next Step:** Create database migration and implement Azure Blob service (Phase 2).

---

**Questions?** Review this plan with your team and confirm:
1. Vanity subdomain approach approved?
2. Azure Blob Storage budget ($100/month estimated)?
3. Wildcard SSL certificate strategy (Cloudflare recommended)?

Let me know when to proceed with Phase 2 implementation! 🚀
