# File Storage Subdomain Architecture - Quick Reference

## URL Structure

```
Main App:    https://app.manimp.com
File Portal: https://acme.files.manimp.com    (vanity subdomain)
             https://tenant-123.files.manimp.com (GUID fallback)
Share Link:  https://acme.files.manimp.com/share/abc123
```

## How It Works

```
User enters: acme.files.manimp.com
     ↓
SubdomainTenantResolver extracts "acme"
     ↓
Lookup in DirectoryDb: Tenants.Subdomain = "acme"
     ↓
Resolve to: TenantId = "abc-123-def"
     ↓
Azure Blob path: tenant-abc-123-def/certificates/...
     ↓
Render FileBrowser.razor with tenant's files
```

## File Access Patterns

### 1. Browse Files
```
URL: acme.files.manimp.com/certificates
→ Lists files in: tenant-{guid}/certificates/
```

### 2. Upload File
```
User drags file → FileBrowser.razor
→ Calls FileStorage.UploadFileAsync()
→ Saved to: tenant-{guid}/certificates/iso-9001.pdf
→ Returns blob URI
```

### 3. Share Link
```
User clicks "Share" on file
→ Generates SAS token (24hr expiry)
→ Returns: acme.files.manimp.com/share/xyz789
→ Anyone with link can download (no login)
```

### 4. From Main App
```
User views Procurement Order
→ Clicks attached invoice PDF
→ Opens: acme.files.manimp.com/invoices/inv-2025-001.pdf
→ Preview in browser or download
```

## DNS Setup

```
Type: CNAME
Name: *.files
Value: app.manimp.com
TTL: 300

Result:
- acme.files.manimp.com → app.manimp.com
- tenant-123.files.manimp.com → app.manimp.com
- anything.files.manimp.com → app.manimp.com
```

## Benefits vs WebDAV

| Feature | Subdomain Portal | WebDAV |
|---------|------------------|--------|
| Client Setup | None | Network drive mapping |
| Mobile | Native browser | Poor/none |
| Share Links | ✅ Copy link | ❌ Not possible |
| File Preview | ✅ PDF/images | ❌ Must download |
| Cross-Platform | ✅ Universal | ⚠️ OS-specific |

## Cost Comparison

```
Subdomain + Azure Blob (1TB):
  Storage: $18/month
  Operations: $5/month
  Bandwidth: $50/month
  ─────────────────────
  Total: $73/month

SQL Server (1TB):
  Storage: $150/month
  I/O: $50/month
  Backups: $30/month
  ─────────────────────
  Total: $230/month

SAVINGS: 68% ($157/month)
```

## Implementation Status

✅ **Completed:**
- SubdomainTenantResolver (tenant from subdomain)
- Tenant.Subdomain field added
- FileBrowser.razor UI created

🔄 **Next Steps:**
1. Database migration (add Subdomain column)
2. Implement AzureBlobStorageService
3. DNS wildcard setup: `*.files.manimp.com`
4. SSL certificate (Cloudflare recommended)
5. Deploy and test

## Quick Start Commands

```bash
# 1. Add Subdomain column
cd Manimp.Directory
dotnet ef migrations add AddSubdomainToTenant
dotnet ef database update

# 2. Install Azure Blob SDK
cd ../Manimp.Services
dotnet add package Azure.Storage.Blobs
dotnet add package Azure.Identity

# 3. Build and test
cd ..
dotnet build
dotnet run --project Manimp.Web

# 4. Access file portal
# Visit: https://localhost:5000/files
```

## Configuration Checklist

- [ ] Add Azure Storage connection string to Key Vault
- [ ] Update appsettings.json with Blob settings
- [ ] Configure wildcard DNS: `*.files.manimp.com`
- [ ] Obtain SSL cert (or use Cloudflare)
- [ ] Register services in Program.cs
- [ ] Seed vanity subdomains for tenants
- [ ] Test file upload/download
- [ ] Enable soft delete (30 days)
- [ ] Set up budget alerts ($100/month)

## Questions to Confirm

1. ✅ Subdomain approach approved?
2. ⏳ Azure Storage Account created?
3. ⏳ DNS access to add wildcard record?
4. ⏳ SSL strategy (Cloudflare vs Let's Encrypt)?
5. ⏳ Budget approval ($100/month estimated)?

---

**See full plan:** `docs/file-storage-subdomain-plan.md`
