# Azure DNS Configuration - Visual Guide

## 🌐 DNS Architecture

```
┌─────────────────────────────────────────────────────────────┐
│              Azure DNS + Multi-Domain Setup                  │
└─────────────────────────────────────────────────────────────┘

Domain Registrar (GoDaddy/Namecheap)
         │
         │ (Update Name Servers)
         ↓
┌─────────────────────────────────┐
│     Azure DNS Zone              │
│     Zone: steelaxis.com            │
├─────────────────────────────────┤
│ Name Servers:                   │
│  ns1-01.azure-dns.com          │
│  ns2-01.azure-dns.net          │
│  ns3-01.azure-dns.org          │
│  ns4-01.azure-dns.info         │
└─────────────────────────────────┘
         │
         │ (DNS Records)
         ↓
┌─────────────────────────────────────────────────────────────┐
│                    DNS Record Sets                           │
├──────────────┬──────────────┬───────────────────────────────┤
│ Type         │ Name         │ Value                         │
├──────────────┼──────────────┼───────────────────────────────┤
│ A            │ *            │ 20.94.123.45 (App IP)         │
│ CNAME        │ *.files      │ app-steelaxis-prod.azure...net   │
│ CNAME        │ *.docs       │ app-steelaxis-prod.azure...net   │
│ TXT          │ asuid        │ <verification-code>           │
└──────────────┴──────────────┴───────────────────────────────┘
         │
         │ (Resolves to)
         ↓
┌─────────────────────────────────────────────────────────────┐
│              Azure App Service (P1v3)                        │
│              app-steelaxis-prod.azurewebsites.net              │
├─────────────────────────────────────────────────────────────┤
│ Custom Domains:                                             │
│  ✅ *.steelaxis.com                                            │
│  ✅ *.files.steelaxis.com                                      │
│  ✅ *.docs.steelaxis.com                                       │
├─────────────────────────────────────────────────────────────┤
│ SSL Certificates:                                           │
│  ✅ Managed Certificate (*.steelaxis.com)                      │
│  ✅ Managed Certificate (*.files.steelaxis.com)                │
│  ✅ Managed Certificate (*.docs.steelaxis.com)                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 📋 Step-by-Step DNS Setup

### Step 1: Create Azure DNS Zone (5 minutes)

```bash
# Via Azure CLI
az network dns zone create \
  --resource-group rg-steelaxis-prod \
  --name steelaxis.com

# Get name servers
az network dns zone show \
  --resource-group rg-steelaxis-prod \
  --name steelaxis.com \
  --query nameServers
```

**Via Azure Portal:**
1. Search "DNS zones" in Azure Portal
2. Click "+ Create"
3. Resource Group: `rg-steelaxis-prod`
4. Name: `steelaxis.com`
5. Click "Review + Create"
6. Copy the 4 name servers shown

---

### Step 2: Update Domain Registrar (10 minutes + 24hr propagation)

#### GoDaddy
```
1. Login to GoDaddy
2. My Products → Domains → steelaxis.com → Manage
3. Scroll to "Nameservers" → Click "Change"
4. Select "I'll use my own nameservers"
5. Add 4 Azure DNS name servers:
   - ns1-01.azure-dns.com
   - ns2-01.azure-dns.net
   - ns3-01.azure-dns.org
   - ns4-01.azure-dns.info
6. Save → Confirmation email sent
```

#### Namecheap
```
1. Login to Namecheap
2. Domain List → steelaxis.com → Manage
3. Domain tab → Nameservers dropdown
4. Select "Custom DNS"
5. Add Azure DNS name servers
6. ✓ Save
```

#### Cloudflare (if currently using)
```
1. Remove domain from Cloudflare
2. Update registrar to Azure DNS
3. OR: Keep Cloudflare as CDN, point to Azure App Service
```

---

### Step 3: Add DNS Records (15 minutes)

#### Get App Service IP

```bash
# Get outbound IPs
az webapp show \
  --resource-group rg-steelaxis-prod \
  --name app-steelaxis-prod \
  --query outboundIpAddresses \
  --output tsv

# Example output:
# 20.94.123.45,20.94.123.46,20.94.123.47,20.94.123.48
```

#### Add Wildcard Records

```bash
# Main app (A record)
az network dns record-set a add-record \
  --resource-group rg-steelaxis-prod \
  --zone-name steelaxis.com \
  --record-set-name "*" \
  --ipv4-address 20.94.123.45 \
  --ttl 3600

# File portal (CNAME)
az network dns record-set cname set-record \
  --resource-group rg-steelaxis-prod \
  --zone-name steelaxis.com \
  --record-set-name "*.files" \
  --cname app-steelaxis-prod.azurewebsites.net \
  --ttl 3600

# Client portal (CNAME)
az network dns record-set cname set-record \
  --resource-group rg-steelaxis-prod \
  --zone-name steelaxis.com \
  --record-set-name "*.docs" \
  --cname app-steelaxis-prod.azurewebsites.net \
  --ttl 3600

# Verification TXT record
az network dns record-set txt add-record \
  --resource-group rg-steelaxis-prod \
  --zone-name steelaxis.com \
  --record-set-name "asuid" \
  --value "YOUR_VERIFICATION_CODE"
```

**Via Azure Portal:**
1. Azure DNS Zone → Record sets
2. Click "+ Record set"
3. Configure each record:
   - **Main App:**
     - Name: `*`
     - Type: `A`
     - IP: `20.94.123.45`
   - **File Portal:**
     - Name: `*.files`
     - Type: `CNAME`
     - Alias: `app-steelaxis-prod.azurewebsites.net`
   - **Client Portal:**
     - Name: `*.docs`
     - Type: `CNAME`
     - Alias: `app-steelaxis-prod.azurewebsites.net`

---

### Step 4: Verify DNS Propagation (30 min - 48 hours)

#### Check DNS Resolution

```bash
# Test with Azure DNS directly
nslookup acme.steelaxis.com ns1-01.azure-dns.com
nslookup acme.files.steelaxis.com ns1-01.azure-dns.com
nslookup acme.docs.steelaxis.com ns1-01.azure-dns.com

# Test with Google DNS
nslookup acme.steelaxis.com 8.8.8.8
dig @8.8.8.8 acme.files.steelaxis.com
dig @8.8.8.8 acme.docs.steelaxis.com

# Check propagation globally
# Visit: https://www.whatsmydns.net
# Enter: acme.steelaxis.com
```

#### Monitor Propagation Status

```bash
# Install dig (if not available)
brew install bind

# Check TTL countdown
dig +trace acme.steelaxis.com

# Check all DNS record types
dig acme.steelaxis.com ANY
```

---

## 🔐 SSL Certificate Setup

### Option 1: Azure Managed Certificates (FREE)

**Requirements:**
- ✅ App Service Plan: Standard (S1) or higher
- ✅ Custom domain verified and mapped
- ✅ DNS propagated (can take 24-48 hours)

**Steps (Azure Portal):**

```
1. App Service → Certificates
2. Click "Add Certificate"
3. Source: "Create App Service Managed Certificate"
4. Custom Domain: Select "*.steelaxis.com"
5. Validation Method: HTTP (automatic)
6. Click "Validate"
7. Click "Add"
8. Repeat for "*.files.steelaxis.com" and "*.docs.steelaxis.com"

Wait 10-15 minutes for certificate issuance
```

**Bind Certificate:**

```
1. App Service → Custom domains
2. Select "acme.steelaxis.com"
3. TLS/SSL binding → Add binding
4. Certificate: Select managed certificate
5. TLS/SSL Type: SNI SSL
6. Click "Add Binding"
7. Repeat for other domains
```

---

### Option 2: Let's Encrypt (FREE, Manual)

```bash
# Install certbot
brew install certbot

# Request wildcard certificate
sudo certbot certonly \
  --manual \
  --preferred-challenges dns \
  -d "*.steelaxis.com" \
  -d "*.files.steelaxis.com" \
  -d "*.docs.steelaxis.com" \
  --agree-tos \
  --email admin@steelaxis.com

# Follow instructions to add TXT records
# Example:
# _acme-challenge.steelaxis.com → "abc123xyz456..."

# Add TXT record to Azure DNS
az network dns record-set txt add-record \
  --resource-group rg-steelaxis-prod \
  --zone-name steelaxis.com \
  --record-set-name "_acme-challenge" \
  --value "abc123xyz456..."

# Wait for DNS propagation (5 minutes)
# Press Enter in certbot to continue

# Certificate saved to:
# /etc/letsencrypt/live/steelaxis.com/fullchain.pem
# /etc/letsencrypt/live/steelaxis.com/privkey.pem

# Convert to PFX for Azure
sudo openssl pkcs12 -export \
  -out wildcard-steelaxis.pfx \
  -inkey /etc/letsencrypt/live/steelaxis.com/privkey.pem \
  -in /etc/letsencrypt/live/steelaxis.com/fullchain.pem \
  -password pass:YourPassword123

# Upload to App Service
az webapp config ssl upload \
  --resource-group rg-steelaxis-prod \
  --name app-steelaxis-prod \
  --certificate-file wildcard-steelaxis.pfx \
  --certificate-password YourPassword123
```

---

## ✅ Verification Checklist

### DNS Checks

- [ ] Azure DNS zone created
- [ ] Domain registrar name servers updated
- [ ] DNS propagation complete (24-48 hours)
- [ ] A record for `*.steelaxis.com` resolves
- [ ] CNAME for `*.files.steelaxis.com` resolves
- [ ] CNAME for `*.docs.steelaxis.com` resolves

**Test Commands:**
```bash
nslookup acme.steelaxis.com
nslookup acme.files.steelaxis.com
nslookup acme.docs.steelaxis.com
```

### SSL Checks

- [ ] Custom domains added to App Service
- [ ] SSL certificates issued (managed or manual)
- [ ] HTTPS binding configured
- [ ] HTTP to HTTPS redirect enabled
- [ ] All domains accessible via HTTPS

**Test Commands:**
```bash
curl -I https://acme.steelaxis.com
curl -I https://acme.files.steelaxis.com
curl -I https://acme.docs.steelaxis.com

# Check certificate details
openssl s_client -connect acme.steelaxis.com:443 -servername acme.steelaxis.com
```

### App Service Checks

- [ ] App Service running
- [ ] Custom domains bound
- [ ] SSL certificates valid
- [ ] Application deployed
- [ ] Health check endpoint responding
- [ ] Azure AD B2C redirect URIs updated with wildcard domains

**Test Commands:**
```bash
curl https://acme.steelaxis.com/health
curl https://acme.files.steelaxis.com/health
curl https://acme.docs.steelaxis.com/health
```

---

## 🐛 Troubleshooting

### DNS Not Resolving

**Problem:** `nslookup acme.steelaxis.com` returns NXDOMAIN

**Solutions:**
```bash
# Check name servers updated at registrar
dig steelaxis.com NS

# Expected output should show Azure DNS:
# ns1-01.azure-dns.com
# ns2-01.azure-dns.net
# ns3-01.azure-dns.org
# ns4-01.azure-dns.info

# If not, update at registrar and wait 24-48 hours
```

### SSL Certificate Validation Failing

**Problem:** "Domain verification failed"

**Solutions:**
```bash
# 1. Ensure DNS propagated (wait 24-48 hours after registrar update)

# 2. Check TXT record for verification
az network dns record-set txt show \
  --resource-group rg-steelaxis-prod \
  --zone-name steelaxis.com \
  --name asuid

# 3. Verify custom domain in App Service
az webapp config hostname list \
  --resource-group rg-steelaxis-prod \
  --webapp-name app-steelaxis-prod

# 4. Retry certificate creation
```

### Wildcard Domain Not Working

**Problem:** `acme.steelaxis.com` doesn't resolve

**Solutions:**
```bash
# 1. Check App Service Plan tier (needs Standard or higher)
az appservice plan show \
  --resource-group rg-steelaxis-prod \
  --name plan-steelaxis-prod \
  --query sku.tier

# 2. If Basic tier, upgrade to Standard
az appservice plan update \
  --resource-group rg-steelaxis-prod \
  --name plan-steelaxis-prod \
  --sku S1
```

---

## 💰 Azure DNS Pricing

```
DNS Zone Hosting:              $0.50/zone/month
DNS Queries (first 1B):        $0.40/million queries

Example Costs (100k users, 10M queries/month):
├─ Zone hosting:               $0.50
├─ Queries (10M):              $4.00
└─ Total:                      $4.50/month

Compared to:
├─ Cloudflare:                 Free (with limitations)
├─ Route 53 (AWS):             $6.00/month (similar load)
└─ Azure DNS:                  $4.50/month ✅ Best value
```

---

## 📊 DNS Record Summary

### Final DNS Configuration

| Record Type | Name | Value | TTL | Purpose |
|-------------|------|-------|-----|---------|
| **A** | `*` | `20.94.123.45` | 3600 | Main app wildcard |
| **CNAME** | `*.files` | `app-steelaxis-prod.azurewebsites.net` | 3600 | File portal wildcard |
| **CNAME** | `*.docs` | `app-steelaxis-prod.azurewebsites.net` | 3600 | Client portal wildcard |
| **TXT** | `asuid` | `<verification-code>` | 3600 | Domain verification |
| **TXT** | `_acme-challenge` | `<lets-encrypt-token>` | 300 | SSL validation (if using Let's Encrypt) |

### Example Resolutions

```
acme.steelaxis.com
  ↓ Matches: *.steelaxis.com (A record)
  ↓ Returns: 20.94.123.45
  ↓ Connects to: App Service

metals-inc.files.steelaxis.com
  ↓ Matches: *.files.steelaxis.com (CNAME)
  ↓ Returns: app-steelaxis-prod.azurewebsites.net
  ↓ Connects to: App Service

buildco.docs.steelaxis.com
  ↓ Matches: *.docs.steelaxis.com (CNAME)
  ↓ Returns: app-steelaxis-prod.azurewebsites.net
  ↓ Connects to: App Service
```

---

## 🎯 Quick Setup Summary

**Total Time:** ~2-3 hours (excluding DNS propagation)

**Steps:**
1. ✅ Create Azure DNS zone (5 min)
2. ✅ Update domain registrar (10 min)
3. ⏱️ Wait for DNS propagation (24-48 hours)
4. ✅ Add DNS records (15 min)
5. ✅ Add custom domains to App Service (10 min)
6. ✅ Configure SSL certificates (30 min)
7. ✅ Verify everything works (15 min)

**Cost:** $4.50/month (DNS) + $214/month (App Service) = **$218.50/month**

**Next:** Deploy your application and test multi-domain routing! 🚀

---

**See also:**
- `docs/azure-infrastructure-setup.md` - Complete Azure setup guide
- `docs/file-storage-multi-domain-architecture.md` - Application architecture
