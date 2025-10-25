# Azure Infrastructure Documentation

Complete Azure cloud infrastructure setup, DNS configuration, and CI/CD deployment guides for Manimp.

---

## 📚 Documents

### 🏗️ [azure-infrastructure-setup.md](./azure-infrastructure-setup.md)
**Complete Azure resource provisioning guide** (600+ lines)

**Contents:**
- Azure components overview with cost breakdown
- Resource group creation
- App Service Plan (P1v3) setup
- SQL Server database configuration
- Blob Storage with lifecycle policies
- Azure DNS zone creation
- Key Vault for secrets management
- Azure AD B2C tenant setup
- Application Insights monitoring
- All-in-one deployment script

**Monthly Cost:** $398.03 (with B2C free tier)

---

### 🌐 [azure-dns-setup-guide.md](./azure-dns-setup-guide.md)
**DNS configuration with visual diagrams** (800+ lines)

**Contents:**
- DNS architecture diagrams
- Step-by-step Azure DNS zone setup
- Domain registrar name server update (GoDaddy, Namecheap)
- Wildcard DNS records configuration
- SSL certificate setup (Azure Managed + Let's Encrypt)
- DNS propagation verification
- Troubleshooting guide
- Complete verification checklist

**Wildcard Domains:**
- `*.manimp.com` → Main application
- `*.files.manimp.com` → File portal
- `*.docs.manimp.com` → Client portal

---

### 🚀 [azure-deployment.md](./azure-deployment.md)
**Azure App Service deployment guide**

**Contents:**
- Deployment via Azure CLI
- Deployment via GitHub Actions
- Environment configuration
- Health check endpoints
- Blue-green deployment strategies

---

### ⚙️ [cicd-summary.md](./cicd-summary.md)
**CI/CD pipeline configuration**

**Contents:**
- GitHub Actions workflows
- Automated testing
- Build and deployment automation
- Environment variables management
- Deployment rollback procedures

---

## 🎯 Quick Start

### 1. Provision Infrastructure
```bash
# Clone the all-in-one deployment script from:
# azure-infrastructure-setup.md (bottom of file)

chmod +x setup-azure-infrastructure.sh
./setup-azure-infrastructure.sh
```

### 2. Configure DNS
```bash
# Follow azure-dns-setup-guide.md:
# - Create Azure DNS zone
# - Update domain registrar name servers
# - Wait 24-48 hours for propagation
# - Add wildcard DNS records
```

### 3. Setup SSL Certificates
```bash
# Via Azure Portal (Managed Certificates - FREE):
# App Service → Certificates → Add Certificate
# Select custom domain → Validate → Add

# SSL automatically renews
```

### 4. Deploy Application
```bash
# Via GitHub Actions (see cicd-summary.md)
git push origin main

# Or manually:
dotnet publish -c Release
az webapp deployment source config-zip \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --src ./publish.zip
```

---

## 💰 Cost Summary

```
Component                   Monthly Cost
─────────────────────────────────────────
App Service Plan (P1v3)     $214.00
SQL Server (Standard S2)    $150.00
Blob Storage (1TB)          $18.00
Azure DNS (1 zone)          $0.50
DNS Queries (10M)           $4.00
Key Vault                   $0.03
Application Insights        $11.50
Azure AD B2C (<50k MAU)     $0.00 (FREE)
─────────────────────────────────────────
TOTAL                       $398.03/month
ANNUAL                      $4,776.36/year
```

**Cost Optimization:**
- Reserved instances: Save 30% on App Service
- Archive tier: Move old files to save 98% on storage
- B2C free tier: First 50,000 users free

---

## 🔧 Common Tasks

### Check Resource Status
```bash
# App Service
az webapp show \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --query state

# SQL Database
az sql db show \
  --resource-group rg-manimp-prod \
  --server sql-manimp-prod \
  --name sqldb-manimp-prod \
  --query status
```

### View Application Logs
```bash
# Stream logs
az webapp log tail \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod

# Download logs
az webapp log download \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod
```

### Scale Application
```bash
# Scale out (add instances)
az appservice plan update \
  --resource-group rg-manimp-prod \
  --name plan-manimp-prod \
  --number-of-workers 3

# Scale up (upgrade tier)
az appservice plan update \
  --resource-group rg-manimp-prod \
  --name plan-manimp-prod \
  --sku P2v3
```

---

## 🐛 Troubleshooting

### DNS Not Resolving
```bash
# Check name servers
dig manimp.com NS

# Check DNS propagation
nslookup acme.manimp.com
nslookup acme.files.manimp.com

# Force DNS cache clear (macOS)
sudo dscacheutil -flushcache
sudo killall -HUP mDNSResponder
```

### SSL Certificate Issues
```bash
# Check certificate status
az webapp config ssl list \
  --resource-group rg-manimp-prod

# Verify SSL binding
curl -I https://acme.manimp.com
openssl s_client -connect acme.manimp.com:443
```

### App Service Not Starting
```bash
# Check application logs
az webapp log tail --resource-group rg-manimp-prod --name app-manimp-prod

# Restart app service
az webapp restart --resource-group rg-manimp-prod --name app-manimp-prod

# Check health endpoint
curl https://app-manimp-prod.azurewebsites.net/health
```

---

## 📊 Monitoring & Alerts

### Application Insights Queries
```kusto
// Failed requests
requests
| where success == false
| summarize count() by resultCode, bin(timestamp, 1h)

// Slow requests
requests
| where duration > 5000
| project timestamp, name, url, duration
| order by duration desc

// Exceptions
exceptions
| summarize count() by type, outerMessage
| order by count_ desc
```

### Set Up Alerts
```bash
# High CPU alert
az monitor metrics alert create \
  --resource-group rg-manimp-prod \
  --name "High CPU Usage" \
  --scopes /subscriptions/.../app-manimp-prod \
  --condition "avg Percentage CPU > 80" \
  --description "Alert when CPU exceeds 80%"
```

---

## 🔗 Related Documentation

- **[../authentication/azure-b2c-authentication.md](../authentication/azure-b2c-authentication.md)** - Azure AD B2C setup
- **[../file-storage/file-storage-multi-domain-architecture.md](../file-storage/file-storage-multi-domain-architecture.md)** - File storage Azure Blob integration
- **[../general/manimp-strategic-guide.md](../general/manimp-strategic-guide.md)** - Overall architecture strategy

---

## 🎯 What's Next

### Immediate Tasks (This Week)
- [ ] Create Azure resource group: `rg-manimp-prod`
- [ ] Provision SQL Server and database
- [ ] Create Azure DNS zone for `manimp.com`
- [ ] Update domain registrar name servers
- [ ] Create Key Vault and store secrets

### Short-term (Next 2 Weeks)
- [ ] Wait for DNS propagation (24-48 hours)
- [ ] Configure wildcard DNS records
- [ ] Set up Azure App Service (P1v3)
- [ ] Configure Azure Managed Certificates (SSL)
- [ ] Deploy application to App Service

### Long-term (Next Month)
- [ ] Set up Application Insights monitoring
- [ ] Configure alerts for high CPU/memory
- [ ] Implement Azure Blob Storage lifecycle policies
- [ ] Set up CI/CD pipeline with GitHub Actions
- [ ] Enable auto-scaling rules
- [ ] Configure backup and disaster recovery

### Optimization
- [ ] Review cost optimization opportunities
- [ ] Consider reserved instances (30% savings)
- [ ] Implement CDN for static assets
- [ ] Set up Azure Front Door for DDoS protection

---

**Ready to deploy!** 🚀
