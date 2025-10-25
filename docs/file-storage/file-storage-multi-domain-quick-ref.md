# Multi-Domain File Storage - Quick Reference

## 🌐 Domain Structure

```
┌─────────────────────────────────────────────────────────────┐
│               Three-Domain Architecture                      │
└─────────────────────────────────────────────────────────────┘

1. acme.manimp.com
   ├── Purpose: Main ERP application
   ├── Users: Company employees only
   ├── Features: Full system access
   └── Files: Upload, manage, all projects

2. acme.files.manimp.com
   ├── Purpose: Internal file portal
   ├── Users: Company employees only
   ├── Features: Modern file browser
   └── Files: Browse, upload, share (internal)

3. acme.docs.manimp.com
   ├── Purpose: External client portal
   ├── Users: Customers/partners
   ├── Features: View-only document access
   └── Files: Download project docs (phase-gated)
```

## 🔐 Access Control Matrix

| Feature | Main App | File Portal | Client Portal |
|---------|----------|-------------|---------------|
| **Auth** | Required | Required | Configurable |
| **User Type** | Internal | Internal | External |
| **Upload** | ✅ Yes | ✅ Yes | ❌ No |
| **Download** | ✅ Yes | ✅ Yes | ✅ Yes |
| **All Files** | ✅ Yes | ✅ Yes | ❌ Project-only |
| **Share Links** | ✅ Yes (role-based) | ✅ Yes (role-based) | ❌ No |

## 📋 Admin Panel Settings

### File Sharing Configuration

```
Admin → File Settings

1. External Access
   [ ] Enable client portal (acme.docs.manimp.com)
   [ ] Require email registration
   
2. Default Share Expiry
   ( ) Unlimited
   (•) Limited: [30] days
   
3. Role Permissions
   ┌─────────┬──────────┬──────────┬──────────┐
   │ Role    │ Unlimited│ External │ Max Days │
   ├─────────┼──────────┼──────────┼──────────┤
   │ Admin   │    ✓     │    ✓     │    —     │
   │ Manager │    ✗     │    ✓     │    90    │
   │ User    │    ✗     │    ✗     │    30    │
   └─────────┴──────────┴──────────┴──────────┘
```

## 🎯 Use Cases

### Use Case 1: Share Internal Files

```
Manager at acme.files.manimp.com:
1. Right-click file → "Get Share Link"
2. Set expiry: 30 days (max for Manager role)
3. Copy link: acme.files.manimp.com/share/xyz123
4. Send to colleague via email
5. Colleague opens link → Downloads file
```

### Use Case 2: Grant Client Access

```
Admin at acme.manimp.com:
1. Go to Clients → Add External User
2. Enter:
   - Email: john@buildco.com
   - Name: John Doe
   - Projects: [Bridge Project #789]
   - Expiry: 1 year
3. Save → Email sent to john@buildco.com
4. John logs into acme.docs.manimp.com
5. Sees only Project #789 files
```

### Use Case 3: Phase-Based File Release

```
Project Manager at acme.manimp.com:
1. Go to Project #789 → Files
2. Right-click "Phase 2 Fabrication" folder
3. Select "Share with Clients"
4. Set: Required Phase = "Phase 2"
5. Phase 2 status: In Progress (files hidden)
6. Mark Phase 2 as "Completed"
7. Files automatically visible on acme.docs.manimp.com
```

## 🗄️ Database Schema

### New Tables

```sql
-- Tenant settings (JSON column)
ALTER TABLE Tenants 
ADD FileSharingSettings NVARCHAR(MAX);

-- External users (clients/partners)
CREATE TABLE ExternalUsers (
  ExternalUserId UNIQUEIDENTIFIER PRIMARY KEY,
  TenantId UNIQUEIDENTIFIER NOT NULL,
  Email NVARCHAR(255) NOT NULL,
  Name NVARCHAR(200) NOT NULL,
  Company NVARCHAR(200),
  PasswordHash NVARCHAR(MAX),
  AllowedProjectIds NVARCHAR(MAX), -- JSON array
  IsActive BIT DEFAULT 1,
  CreatedUtc DATETIME2 DEFAULT GETUTCDATE(),
  ExpiresUtc DATETIME2
);

-- Project file visibility
CREATE TABLE ProjectFileVisibilities (
  ProjectFileVisibilityId UNIQUEIDENTIFIER PRIMARY KEY,
  ProjectId UNIQUEIDENTIFIER NOT NULL,
  ResourcePath NVARCHAR(500) NOT NULL,
  IsVisibleToClients BIT DEFAULT 0,
  RequiredPhaseId UNIQUEIDENTIFIER,
  MarkedByUserId NVARCHAR(450),
  MarkedUtc DATETIME2
);
```

## 🚀 Implementation Steps

### Week 1: Database
```bash
cd Manimp.Data
dotnet ef migrations add AddMultiDomainFileStorage
dotnet ef database update
```

### Week 2: Services
- Implement `MultiDomainTenantResolver`
- Implement `FileAccessControlService`
- Add role-based permission checks

### Week 3: Admin UI
- Create file settings page
- Create external user management
- Create project file marking UI

### Week 4: Client Portal
- Create `acme.docs.manimp.com` UI
- Implement optional login
- Add phase filtering

### Week 5: DNS & SSL
```bash
# Create Azure DNS Zone
az network dns zone create -g rg-manimp-prod -n manimp.com

# Add wildcard records
az network dns record-set cname set-record \
  -g rg-manimp-prod -z manimp.com \
  -n "*.files" -c app-manimp-prod.azurewebsites.net

az network dns record-set cname set-record \
  -g rg-manimp-prod -z manimp.com \
  -n "*.docs" -c app-manimp-prod.azurewebsites.net

# SSL: Azure Managed Certificates (free with App Service)
# Portal → App Service → Certificates → Add Managed Certificate
```

### Week 6: Testing
- Test all three domains
- Test role-based sharing
- Test external user access
- Test phase-based visibility

## 💰 Cost Estimate

```
Azure Blob Storage (1TB):     $18/month
Azure App Service (P1v2):    $146/month
DNS (Cloudflare):             Free
SSL Certificates:             Free
──────────────────────────────────────
Total:                       $164/month
```

## 🎓 Key Concepts

### 1. Domain-Based Routing
```
Host: acme.files.manimp.com
  ↓ Extract: "acme" (tenant)
  ↓ Extract: "files" (domain type)
  ↓ Resolve: TenantId = "abc-123-def"
  ↓ Route: File Portal UI
```

### 2. Role-Based Sharing
```
User Role: Manager
  ↓ Check tenant settings
  ↓ Manager.CanShareUnlimited = false
  ↓ Manager.MaxShareDays = 90
  ↓ Request: 180 days
  ↓ Result: ❌ Denied (exceeds limit)
```

### 3. Phase-Based Visibility
```
File: "Welding Photos" (Phase 2)
  ↓ Check visibility: IsVisibleToClients = true
  ↓ Check phase: RequiredPhaseId = Phase 2
  ↓ Check status: Phase 2 = InProgress
  ↓ Result: ❌ Hidden from clients
  
  ✅ Mark Phase 2 as Completed
  ↓ Result: ✅ Now visible to clients
```

## 📊 Security Model

```
┌─────────────────────────────────────────────────────────┐
│                  Access Control Layers                   │
└─────────────────────────────────────────────────────────┘

Layer 1: Domain Detection
  ├── Main App: Requires employee login
  ├── File Portal: Requires employee login
  └── Client Portal: Optional login (configurable)

Layer 2: Tenant Isolation
  ├── Extract tenant from subdomain
  ├── All queries scoped to TenantId
  └── No cross-tenant access

Layer 3: Role-Based Permissions
  ├── Admin: Full access
  ├── Manager: Limited sharing
  └── User: Restricted sharing

Layer 4: Project-Based Access
  ├── Internal users: All projects
  └── External users: Assigned projects only

Layer 5: Phase-Based Gating
  ├── Completed phases: Visible
  └── In-progress phases: Hidden
```

## 🎯 Decision Summary

**Original Request:**
> "Can I use subdomains for easy usability for tenant?"

**Enhanced Solution:**
✅ **Yes, with three-domain architecture:**
1. `{tenant}.manimp.com` - Main app (internal)
2. `{tenant}.files.manimp.com` - File portal (internal)
3. `{tenant}.docs.manimp.com` - Client portal (external)

**Key Features:**
✅ Role-based sharing (Admin, Manager, User)  
✅ Unlimited or time-limited shares  
✅ External user registration (optional)  
✅ Project-based access control  
✅ Phase-based file visibility  
✅ Automatic phase completion gating  

**Total Effort:** 9 weeks (220 hours)  
**Monthly Cost:** ~$164 (Azure + storage)

---

**Next:** Read `docs/file-storage-multi-domain-architecture.md` for complete implementation guide (3000+ lines).
