# Azure Infrastructure Setup - Complete Guide

**Date:** 14 October 2025  
**Infrastructure:** 100% Azure (DNS, App Service, Blob Storage, Key Vault)  
**Estimated Setup Time:** 4-6 hours

---

## 🏗️ Azure Infrastructure Overview

```
┌─────────────────────────────────────────────────────────────┐
│                  Complete Azure Stack                        │
└─────────────────────────────────────────────────────────────┘

1. Azure DNS
   ├── Zone: manimp.com
   ├── Wildcard Records: *.manimp.com, *.files.manimp.com, *.docs.manimp.com
   └── Cost: $0.50/zone + $0.40/million queries

2. Azure App Service (P1v3)
   ├── Hosts: Blazor Server app
   ├── Custom Domains: Wildcard support
   ├── SSL: Managed Certificates (free)
   └── Cost: $214/month

3. Azure Blob Storage
   ├── Container: manimp-files
   ├── Tier: Hot (active files), Cool (archive)
   └── Cost: $18/month (1TB)

4. Azure Key Vault
   ├── Secrets: Connection strings
   ├── Certificates: SSL (optional)
   └── Cost: $0.03/10k operations

5. Azure Application Insights
   ├── Monitoring: Performance, errors
   ├── Alerts: Budget, downtime
   └── Cost: $2.30/GB ingested

Total Monthly Cost: ~$235 (for 1TB storage, moderate traffic)
```

---

## 📋 Step-by-Step Setup

### Step 1: Create Resource Group

```bash
# Create resource group in East US (or your preferred region)
az group create \
  --name rg-manimp-prod \
  --location eastus \
  --tags Environment=Production Project=Manimp
```

---

### Step 2: Azure DNS Setup

#### 2.1 Create DNS Zone

```bash
# Create DNS zone for your domain
az network dns zone create \
  --resource-group rg-manimp-prod \
  --name manimp.com

# Get name servers (you'll need these for your domain registrar)
az network dns zone show \
  --resource-group rg-manimp-prod \
  --name manimp.com \
  --query nameServers \
  --output table

# Output example:
# ns1-01.azure-dns.com.
# ns2-01.azure-dns.net.
# ns3-01.azure-dns.org.
# ns4-01.azure-dns.info.
```

#### 2.2 Add DNS Records

```bash
# Get App Service outbound IP addresses first
az webapp show \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --query outboundIpAddresses \
  --output tsv

# Add wildcard A record for main app (using first outbound IP)
az network dns record-set a add-record \
  --resource-group rg-manimp-prod \
  --zone-name manimp.com \
  --record-set-name "*" \
  --ipv4-address <first-outbound-ip>

# Add CNAME for file portal
az network dns record-set cname set-record \
  --resource-group rg-manimp-prod \
  --zone-name manimp.com \
  --record-set-name "*.files" \
  --cname app-manimp-prod.azurewebsites.net

# Add CNAME for client portal
az network dns record-set cname set-record \
  --resource-group rg-manimp-prod \
  --zone-name manimp.com \
  --record-set-name "*.docs" \
  --cname app-manimp-prod.azurewebsites.net

# Add root A record (optional, for apex domain)
az network dns record-set a add-record \
  --resource-group rg-manimp-prod \
  --zone-name manimp.com \
  --record-set-name "@" \
  --ipv4-address <first-outbound-ip>

# Add TXT record for domain verification
az network dns record-set txt add-record \
  --resource-group rg-manimp-prod \
  --zone-name manimp.com \
  --record-set-name "asuid" \
  --value "<your-app-service-verification-code>"
```

#### 2.3 Verify DNS Records

```bash
# List all records
az network dns record-set list \
  --resource-group rg-manimp-prod \
  --zone-name manimp.com \
  --output table

# Test DNS resolution (after propagation)
nslookup acme.manimp.com ns1-01.azure-dns.com
nslookup acme.files.manimp.com ns1-01.azure-dns.com
nslookup acme.docs.manimp.com ns1-01.azure-dns.com
```

#### 2.4 Update Domain Registrar

**For GoDaddy:**
1. Log into GoDaddy account
2. Go to "My Products" → Domain
3. Click "Manage DNS"
4. Scroll to "Nameservers" → Click "Change"
5. Select "Custom" → "Enter my own nameservers"
6. Add Azure DNS name servers:
   - `ns1-01.azure-dns.com`
   - `ns2-01.azure-dns.net`
   - `ns3-01.azure-dns.org`
   - `ns4-01.azure-dns.info`
7. Save → Propagation takes 24-48 hours

**For Namecheap:**
1. Log into Namecheap
2. Domain List → Manage
3. Domain tab → Nameservers dropdown
4. Select "Custom DNS"
5. Add Azure DNS name servers
6. Save

**For Other Registrars:**
- Look for "Name Servers," "DNS Management," or "Nameservers"
- Replace existing name servers with Azure DNS servers
- Save changes

---

### Step 3: Azure App Service Setup

#### 3.1 Create App Service Plan

```bash
# Create Premium v3 plan (required for custom domains)
az appservice plan create \
  --name plan-manimp-prod \
  --resource-group rg-manimp-prod \
  --location eastus \
  --sku P1v3 \
  --is-linux false

# Alternative: Standard tier (cheaper, still supports custom domains)
az appservice plan create \
  --name plan-manimp-prod \
  --resource-group rg-manimp-prod \
  --location eastus \
  --sku S1 \
  --is-linux false
```

#### 3.2 Create App Service

```bash
# Create web app
az webapp create \
  --name app-manimp-prod \
  --resource-group rg-manimp-prod \
  --plan plan-manimp-prod \
  --runtime "DOTNET|8.0"

# Configure app settings
az webapp config appsettings set \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    WEBSITE_RUN_FROM_PACKAGE=1
```

#### 3.3 Add Custom Domains

```bash
# Add wildcard custom domain for main app
az webapp config hostname add \
  --resource-group rg-manimp-prod \
  --webapp-name app-manimp-prod \
  --hostname "*.manimp.com"

# Add wildcard for file portal
az webapp config hostname add \
  --resource-group rg-manimp-prod \
  --webapp-name app-manimp-prod \
  --hostname "*.files.manimp.com"

# Add wildcard for client portal
az webapp config hostname add \
  --resource-group rg-manimp-prod \
  --webapp-name app-manimp-prod \
  --hostname "*.docs.manimp.com"
```

**Note:** Wildcard custom domains require Standard tier or higher.

#### 3.4 Configure SSL Certificates

**Option A: Azure Managed Certificates (Recommended - Free)**

```bash
# Create managed certificate (only via Portal currently)
# 1. Azure Portal → App Service → Certificates
# 2. Click "Add Certificate" → "Create App Service Managed Certificate"
# 3. Select custom domain: *.manimp.com
# 4. Click "Validate" → "Add"
# 5. Repeat for *.files.manimp.com and *.docs.manimp.com
# 6. Certificates auto-renew every 6 months
```

**Option B: Let's Encrypt (Free, Manual)**

```bash
# Install certbot
brew install certbot

# Get wildcard certificate
sudo certbot certonly \
  --manual \
  --preferred-challenges dns \
  -d "*.manimp.com" \
  -d "*.files.manimp.com" \
  -d "*.docs.manimp.com"

# Follow instructions to add TXT records to Azure DNS
# After validation, upload certificate to Azure

# Upload certificate to App Service
az webapp config ssl upload \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --certificate-file /etc/letsencrypt/live/manimp.com/fullchain.pem \
  --certificate-password ""

# Bind certificate to custom domain
az webapp config ssl bind \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI
```

**Option C: Azure Key Vault Certificate**

```bash
# Import certificate to Key Vault
az keyvault certificate import \
  --vault-name kv-manimp-prod \
  --name wildcard-manimp-com \
  --file certificate.pfx \
  --password "cert-password"

# Grant App Service access to Key Vault
az webapp identity assign \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod

# Get service principal ID
SP_ID=$(az webapp identity show \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --query principalId \
  --output tsv)

# Grant permissions
az keyvault set-policy \
  --name kv-manimp-prod \
  --object-id $SP_ID \
  --secret-permissions get \
  --certificate-permissions get

# Reference certificate in App Service
az webapp config ssl import \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --key-vault kv-manimp-prod \
  --key-vault-certificate-name wildcard-manimp-com
```

---

### Step 4: Azure Blob Storage Setup

#### 4.1 Create Storage Account

```bash
# Create storage account
az storage account create \
  --name manimpblob \
  --resource-group rg-manimp-prod \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2 \
  --access-tier Hot \
  --https-only true \
  --min-tls-version TLS1_2

# Enable blob versioning
az storage account blob-service-properties update \
  --account-name manimpblob \
  --resource-group rg-manimp-prod \
  --enable-versioning true

# Enable soft delete (30 days retention)
az storage account blob-service-properties update \
  --account-name manimpblob \
  --resource-group rg-manimp-prod \
  --enable-delete-retention true \
  --delete-retention-days 30
```

#### 4.2 Create Blob Container

```bash
# Get storage account key
STORAGE_KEY=$(az storage account keys list \
  --resource-group rg-manimp-prod \
  --account-name manimpblob \
  --query "[0].value" \
  --output tsv)

# Create container for files
az storage container create \
  --name manimp-files \
  --account-name manimpblob \
  --account-key $STORAGE_KEY \
  --public-access off

# Create archive container
az storage container create \
  --name manimp-archive \
  --account-name manimpblob \
  --account-key $STORAGE_KEY \
  --public-access off
```

#### 4.3 Configure CORS (for browser uploads)

```bash
az storage cors add \
  --services b \
  --methods GET POST PUT DELETE HEAD \
  --origins "https://*.manimp.com" "https://*.files.manimp.com" \
  --allowed-headers "*" \
  --exposed-headers "*" \
  --max-age 3600 \
  --account-name manimpblob \
  --account-key $STORAGE_KEY
```

#### 4.4 Configure Lifecycle Policy

Create `lifecycle-policy.json`:

```json
{
  "rules": [
    {
      "enabled": true,
      "name": "MoveToCool",
      "type": "Lifecycle",
      "definition": {
        "actions": {
          "baseBlob": {
            "tierToCool": {
              "daysAfterModificationGreaterThan": 730
            }
          }
        },
        "filters": {
          "blobTypes": ["blockBlob"],
          "prefixMatch": ["tenant-"]
        }
      }
    },
    {
      "enabled": true,
      "name": "MoveArchiveToArchiveTier",
      "type": "Lifecycle",
      "definition": {
        "actions": {
          "baseBlob": {
            "tierToArchive": {
              "daysAfterModificationGreaterThan": 3650
            }
          }
        },
        "filters": {
          "blobTypes": ["blockBlob"],
          "prefixMatch": ["archive/"]
        }
      }
    }
  ]
}
```

Apply policy:

```bash
az storage account management-policy create \
  --account-name manimpblob \
  --resource-group rg-manimp-prod \
  --policy @lifecycle-policy.json
```

---

### Step 5: Azure Key Vault Setup

#### 5.1 Create Key Vault

```bash
# Create Key Vault
az keyvault create \
  --name kv-manimp-prod \
  --resource-group rg-manimp-prod \
  --location eastus \
  --enable-soft-delete true \
  --retention-days 90

# Grant App Service access
az keyvault set-policy \
  --name kv-manimp-prod \
  --object-id $SP_ID \
  --secret-permissions get list
```

#### 5.2 Store Secrets

```bash
# Storage connection string
az keyvault secret set \
  --vault-name kv-manimp-prod \
  --name BlobStorageConnectionString \
  --value "DefaultEndpointsProtocol=https;AccountName=manimpblob;AccountKey=$STORAGE_KEY;EndpointSuffix=core.windows.net"

# Directory database connection string
az keyvault secret set \
  --vault-name kv-manimp-prod \
  --name DirectoryDbConnectionString \
  --value "Server=tcp:manimp-sql.database.windows.net,1433;Database=ManimpDirectory;User ID=manimp-admin;Password=<your-password>;Encrypt=True;"

# Add more secrets as needed
```

#### 5.3 Reference Secrets in App Service

Update `appsettings.json`:

```json
{
  "AzureStorage": {
    "ConnectionString": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/BlobStorageConnectionString/)",
    "BlobUri": "https://manimpblob.blob.core.windows.net"
  },
  "ConnectionStrings": {
    "DirectoryDb": "@Microsoft.KeyVault(SecretUri=https://kv-manimp-prod.vault.azure.net/secrets/DirectoryDbConnectionString/)"
  }
}
```

---

### Step 6: Application Insights (Monitoring)

#### 6.1 Create Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app app-manimp-insights \
  --location eastus \
  --resource-group rg-manimp-prod \
  --application-type web

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app app-manimp-insights \
  --resource-group rg-manimp-prod \
  --query instrumentationKey \
  --output tsv)

# Configure App Service to use App Insights
az webapp config appsettings set \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSTRUMENTATION_KEY"
```

#### 6.2 Configure Alerts

```bash
# Alert for high error rate
az monitor metrics alert create \
  --name alert-high-error-rate \
  --resource-group rg-manimp-prod \
  --scopes $(az webapp show -g rg-manimp-prod -n app-manimp-prod --query id -o tsv) \
  --condition "avg Http5xx > 10" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action email admin@manimp.com

# Alert for high response time
az monitor metrics alert create \
  --name alert-slow-response \
  --resource-group rg-manimp-prod \
  --scopes $(az webapp show -g rg-manimp-prod -n app-manimp-prod --query id -o tsv) \
  --condition "avg ResponseTime > 3000" \
  --window-size 5m \
  --evaluation-frequency 1m
```

---

## 🔐 Security Configuration

### Enable Managed Identity

```bash
# Enable system-assigned managed identity
az webapp identity assign \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod

# Grant access to Blob Storage
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee $SP_ID \
  --scope $(az storage account show -n manimpblob -g rg-manimp-prod --query id -o tsv)
```

### Configure Network Security

```bash
# Restrict access to storage account (optional - production only)
az storage account update \
  --resource-group rg-manimp-prod \
  --name manimpblob \
  --default-action Deny

# Allow access from App Service
az storage account network-rule add \
  --resource-group rg-manimp-prod \
  --account-name manimpblob \
  --subnet $(az webapp vnet-integration list -g rg-manimp-prod -n app-manimp-prod --query [0].id -o tsv)
```

---

## 💰 Cost Optimization

### 1. Use Azure Reserved Instances

```bash
# Save 30-50% on App Service by purchasing 1-year reservation
# Azure Portal → Reservations → Add → App Service Plan
# Select: P1v3, 1 year, East US
# Savings: ~$75/month
```

### 2. Configure Auto-Scaling

```bash
# Scale based on CPU usage
az monitor autoscale create \
  --resource-group rg-manimp-prod \
  --resource $(az appservice plan show -g rg-manimp-prod -n plan-manimp-prod --query id -o tsv) \
  --min-count 1 \
  --max-count 3 \
  --count 1

az monitor autoscale rule create \
  --resource-group rg-manimp-prod \
  --autoscale-name autoscale-cpu \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1
```

### 3. Set Budget Alerts

```bash
# Create budget ($300/month threshold)
az consumption budget create \
  --budget-name budget-manimp-prod \
  --amount 300 \
  --time-grain Monthly \
  --time-period start=2025-10-01 \
  --resource-group rg-manimp-prod \
  --notifications \
    email=admin@manimp.com \
    thresholds=80,90,100
```

---

## ✅ Verification Checklist

### DNS Verification

```bash
# Test DNS resolution
dig @8.8.8.8 acme.manimp.com
dig @8.8.8.8 acme.files.manimp.com
dig @8.8.8.8 acme.docs.manimp.com

# Check SSL
curl -I https://acme.manimp.com
curl -I https://acme.files.manimp.com
curl -I https://acme.docs.manimp.com
```

### App Service Verification

```bash
# Check app service status
az webapp show \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod \
  --query state

# View logs
az webapp log tail \
  --resource-group rg-manimp-prod \
  --name app-manimp-prod
```

### Blob Storage Verification

```bash
# List containers
az storage container list \
  --account-name manimpblob \
  --account-key $STORAGE_KEY \
  --output table

# Test upload
echo "test" > test.txt
az storage blob upload \
  --account-name manimpblob \
  --account-key $STORAGE_KEY \
  --container-name manimp-files \
  --name test.txt \
  --file test.txt
```

---

## 📊 Cost Breakdown (Monthly)

```
Azure DNS Zone:                    $0.50/zone
  └─ Query charges:                $0.40/million (negligible)

App Service Plan (P1v3):          $214.00
  └─ Includes: 1 core, 3.5GB RAM, custom domains, SSL
  
Storage Account:
  ├─ Blob Storage (1TB Hot):       $18.00
  ├─ Operations (10M):              $5.00
  └─ Bandwidth (500GB egress):     $43.50

Key Vault:                          $0.03
  └─ Secret operations (10k):      $0.03

Application Insights:               $2.30
  └─ 1GB data ingestion:           $2.30

SQL Database (Basic tier):         $5.00
  └─ Directory database
  
─────────────────────────────────────────────
TOTAL MONTHLY:                    ~$288.36

Potential Savings:
  └─ Reserved Instance (1yr):     -$75/month
  └─ With RI:                     ~$213/month
```

---

## 🚀 Deployment Script (All-in-One)

Save as `setup-azure-infrastructure.sh`:

```bash
#!/bin/bash

# Configuration
RG="rg-manimp-prod"
LOCATION="eastus"
DOMAIN="manimp.com"
APP_NAME="app-manimp-prod"
STORAGE_NAME="manimpblob"
KV_NAME="kv-manimp-prod"

# Create resource group
az group create --name $RG --location $LOCATION

# Create DNS zone
az network dns zone create --resource-group $RG --name $DOMAIN

# Create App Service Plan
az appservice plan create \
  --name plan-manimp-prod \
  --resource-group $RG \
  --location $LOCATION \
  --sku P1v3

# Create App Service
az webapp create \
  --name $APP_NAME \
  --resource-group $RG \
  --plan plan-manimp-prod \
  --runtime "DOTNET|8.0"

# Create Storage Account
az storage account create \
  --name $STORAGE_NAME \
  --resource-group $RG \
  --location $LOCATION \
  --sku Standard_LRS \
  --kind StorageV2

# Create Key Vault
az keyvault create \
  --name $KV_NAME \
  --resource-group $RG \
  --location $LOCATION

# Create Application Insights
az monitor app-insights component create \
  --app app-manimp-insights \
  --location $LOCATION \
  --resource-group $RG \
  --application-type web

echo "✅ Azure infrastructure created successfully!"
echo "Next steps:"
echo "1. Update domain registrar name servers"
echo "2. Add custom domains to App Service"
echo "3. Configure SSL certificates"
echo "4. Deploy application"
```

Run with:

```bash
chmod +x setup-azure-infrastructure.sh
./setup-azure-infrastructure.sh
```

---

## 📚 Additional Resources

- [Azure DNS Documentation](https://docs.microsoft.com/azure/dns/)
- [App Service Custom Domains](https://docs.microsoft.com/azure/app-service/app-service-web-tutorial-custom-domain)
- [Blob Storage Best Practices](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction)
- [Key Vault Integration](https://docs.microsoft.com/azure/key-vault/general/tutorial-net-create-vault-azure-web-app)

---

## 🎯 Summary

**Your complete Azure infrastructure setup includes:**

✅ **Azure DNS** - Wildcard support for all three domains  
✅ **App Service** - Premium tier with custom domains & SSL  
✅ **Blob Storage** - Hot/Cool tiering with lifecycle policies  
✅ **Key Vault** - Secure secret management  
✅ **Application Insights** - Performance monitoring & alerts  
✅ **Managed Identity** - Secure service-to-service auth  

**Total Setup Time:** 4-6 hours  
**Monthly Cost:** ~$213 (with reserved instance)  
**Uptime SLA:** 99.95%

**Next Step:** Run the setup script and start Phase 1 database migrations! 🚀
