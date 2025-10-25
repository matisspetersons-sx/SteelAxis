# Sheet Inventory - Quick Reference Guide

## Model Comparison: Profiles vs. Sheets

| Aspect | ProfileInventory | SheetInventory |
|--------|-----------------|----------------|
| **Primary Key** | ProfileInventoryId | SheetInventoryId |
| **Dimensions** | Size + Length | Thickness × Width × Length |
| **Quantity** | PiecesOnHand | SheetsOnHand |
| **Weight** | WeightPerPiece | WeightPerSheet |
| **Profile Type** | Required (FK) | N/A (no FK) |
| **Remnant** | Linear (length) | Area (width × length) |
| **Lot Format** | A1, B23, AA5 | S1, S23, SS5 |
| **Remnant Format** | A1-3.5m | S1-1200x600 |

## MaterialInventoryType Enum

```csharp
public enum MaterialInventoryType
{
    Profile = 1,  // Beams, channels, angles, tubes
    Sheet = 2     // Plates, sheets, flat stock
}
```

## Size Field Format Examples

### Profiles (MaterialInventoryType = 1)
- W-beams: "W12x26", "W24x76"
- Channels: "C8x11.5", "MC10x25"
- Angles: "L4x4x1/2", "L6x6x3/4"
- Tubes: "HSS4x4x1/4", "HSS6x4x3/8"

### Sheets (MaterialInventoryType = 2)
- Metric: "1200x2400x10mm", "1500x3000x12mm"
- Imperial: "4'x8'x0.5in", "5'x10'x0.25in"
- Mixed: "48x96x12mm", "1.2mx2.4mx10mm"

## Database Schema Quick Ref

### SheetInventories Table
```sql
SheetInventoryId INT PRIMARY KEY
LotNumber NVARCHAR(10) NOT NULL UNIQUE
Thickness DECIMAL(10,3) NOT NULL
Width DECIMAL(10,3) NOT NULL
Length DECIMAL(10,3) NOT NULL
SheetsOnHand INT NOT NULL
OriginalSheets INT NOT NULL
WeightPerSheet DECIMAL(10,2) NOT NULL
UnitCost DECIMAL(10,2) NULL
ReceivedDate DATETIME2 NOT NULL
Location NVARCHAR(100) NULL
Notes NVARCHAR(1000) NULL
MaterialTypeId INT NOT NULL FK
SteelGradeId INT NOT NULL FK
SupplierId INT NULL FK
CertificateDocumentId INT NULL FK
PurchaseOrderId INT NULL FK
PONumber NVARCHAR(50) NULL
ProjectId INT NULL FK
-- EN 1090 fields
MaterialBatch NVARCHAR(100) NULL
MillTestCertificateNumber NVARCHAR(100) NULL
CertificateType NVARCHAR(10) NULL
MaterialStandard NVARCHAR(50) NULL
ManufacturingRoute NVARCHAR(50) NULL
SurfaceCondition NVARCHAR(50) NULL
CountryOfOrigin NVARCHAR(100) NULL
TraceabilityNotes NVARCHAR(2000) NULL
RowVersion ROWVERSION NOT NULL
```

### SheetUsageLogs Table
```sql
SheetUsageLogId INT PRIMARY KEY
SheetInventoryId INT NOT NULL FK
SheetsUsed INT NOT NULL
AreaUsed DECIMAL(10,3) NULL
UsedDate DATETIME2 NOT NULL
Purpose NVARCHAR(200) NULL
UsedBy NVARCHAR(100) NULL
Notes NVARCHAR(1000) NULL
ProjectId INT NULL FK
RowVersion ROWVERSION NOT NULL
```

### SheetRemnantInventories Table
```sql
SheetRemnantInventoryId INT PRIMARY KEY
ParentSheetInventoryId INT NOT NULL FK
RemnantLotNumber NVARCHAR(50) NOT NULL UNIQUE
RemainingWidth DECIMAL(10,3) NOT NULL
RemainingLength DECIMAL(10,3) NOT NULL
RemnantSheets INT NOT NULL
CreatedDate DATETIME2 NOT NULL
Location NVARCHAR(100) NULL
Notes NVARCHAR(1000) NULL
IsAvailable BIT NOT NULL DEFAULT 1
SheetUsageLogId INT NULL FK
RowVersion ROWVERSION NOT NULL
```

### Updated PurchaseOrderLines
```sql
-- NEW COLUMN
MaterialInventoryType INT NOT NULL DEFAULT 1  -- 1=Profile, 2=Sheet

-- MODIFIED COLUMN
ProfileTypeId INT NULL  -- Changed from NOT NULL to NULL
```

### Updated PriceRequestLines
```sql
-- NEW COLUMN
MaterialInventoryType INT NOT NULL DEFAULT 1

-- MODIFIED COLUMN
ProfileTypeId INT NULL  -- Changed from NOT NULL to NULL
```

## Common Queries

### Get All Sheets
```csharp
var sheets = await _context.SheetInventories
    .Include(s => s.MaterialType)
    .Include(s => s.SteelGrade)
    .Include(s => s.Supplier)
    .Where(s => s.SheetsOnHand > 0)
    .OrderBy(s => s.LotNumber)
    .ToListAsync();
```

### Get Sheet with Usage History
```csharp
var sheet = await _context.SheetInventories
    .Include(s => s.SheetUsageLogs)
        .ThenInclude(u => u.Project)
    .Include(s => s.SheetRemnantInventories)
    .FirstOrDefaultAsync(s => s.SheetInventoryId == id);
```

### Get PO with Mixed Material Types
```csharp
var po = await _context.PurchaseOrders
    .Include(po => po.PurchaseOrderLines)
        .ThenInclude(l => l.MaterialType)
    .Include(po => po.PurchaseOrderLines)
        .ThenInclude(l => l.ProfileType)  // Null for sheets
    .FirstOrDefaultAsync(po => po.PurchaseOrderId == id);
```

### Receive Sheet from PO
```csharp
var poLine = await _context.PurchaseOrderLines
    .Include(l => l.PurchaseOrder)
    .FirstOrDefaultAsync(l => l.PurchaseOrderLineId == lineId);

if (poLine.MaterialInventoryType == MaterialInventoryType.Sheet)
{
    var sheet = new SheetInventory
    {
        LotNumber = await GenerateSheetLotNumberAsync(),
        Thickness = ParseThickness(poLine.Size),
        Width = ParseWidth(poLine.Size),
        Length = ParseLength(poLine.Size),
        SheetsOnHand = quantityReceived,
        OriginalSheets = quantityReceived,
        WeightPerSheet = weightPerSheet,
        MaterialTypeId = poLine.MaterialTypeId,
        SteelGradeId = poLine.SteelGradeId,
        PurchaseOrderId = poLine.PurchaseOrderId,
        ReceivedDate = DateTime.UtcNow
    };
    _context.SheetInventories.Add(sheet);
}
```

### Record Sheet Usage with Remnant
```csharp
var sheet = await _context.SheetInventories
    .FirstOrDefaultAsync(s => s.SheetInventoryId == id);

// Decrement inventory
sheet.SheetsOnHand -= sheetsUsed;

// Create usage log
var usage = new SheetUsageLog
{
    SheetInventoryId = id,
    SheetsUsed = sheetsUsed,
    AreaUsed = areaUsed,
    UsedDate = DateTime.UtcNow,
    ProjectId = projectId
};
_context.SheetUsageLogs.Add(usage);

// Create remnant if needed
if (hasRemnant)
{
    var remnant = new SheetRemnantInventory
    {
        ParentSheetInventoryId = id,
        RemnantLotNumber = $"{sheet.LotNumber}-{remainingWidth}x{remainingLength}",
        RemainingWidth = remainingWidth,
        RemainingLength = remainingLength,
        RemnantSheets = 1,
        SheetUsageLogId = usage.SheetUsageLogId
    };
    _context.SheetRemnantInventories.Add(remnant);
}

await _context.SaveChangesAsync();
```

## Validation Rules

### SheetInventory
- `LotNumber`: Required, max 10 chars, pattern `^[A-Z]{1,2}\d{1,3}$`
- `Thickness`: Required, > 0
- `Width`: Required, > 0
- `Length`: Required, > 0
- `SheetsOnHand`: Required, >= 0
- `OriginalSheets`: Required, >= 1
- `WeightPerSheet`: Required, > 0
- `MaterialTypeId`: Required
- `SteelGradeId`: Required

### PurchaseOrderLine / PriceRequestLine
- `MaterialInventoryType`: Required
- `ProfileTypeId`: Required if MaterialInventoryType = Profile, NULL if Sheet
- `Size`: Required, format depends on MaterialInventoryType
- `Length`: Required, > 0
- `Quantity`: Required, >= 1

## Frontend Conditional Logic

### Material Type Selector Pattern
```razor
<MudSelect T="MaterialInventoryType" 
           Label="Material Type" 
           @bind-Value="@line.MaterialInventoryType"
           Required="true">
    <MudSelectItem Value="MaterialInventoryType.Profile">
        <MudIcon Icon="@Icons.Material.Filled.ViewInAr" Size="Size.Small" /> Profile/Beam
    </MudSelectItem>
    <MudSelectItem Value="MaterialInventoryType.Sheet">
        <MudIcon Icon="@Icons.Material.Filled.RectangleOutlined" Size="Size.Small" /> Sheet/Plate
    </MudSelectItem>
</MudSelect>

@if (line.MaterialInventoryType == MaterialInventoryType.Profile)
{
    <!-- Profile-specific fields -->
    <MudSelect T="int?" Label="Profile Type" @bind-Value="@line.ProfileTypeId" Required="true">
        @foreach (var type in profileTypes)
        {
            <MudSelectItem Value="@type.ProfileTypeId">@type.Name</MudSelectItem>
        }
    </MudSelect>
    
    <MudTextField Label="Size" 
                  @bind-Value="@line.Size" 
                  HelperText="e.g., W12x26, L4x4x1/2"
                  Required="true" />
}
else if (line.MaterialInventoryType == MaterialInventoryType.Sheet)
{
    <!-- Sheet-specific fields -->
    <MudTextField Label="Size/Dimensions" 
                  @bind-Value="@line.Size" 
                  HelperText="e.g., 1200x2400x10mm, 4'x8'x0.5in"
                  Required="true" />
                  
    <!-- Optional: Dedicated dimension fields -->
    <MudNumericField T="decimal" Label="Thickness" @bind-Value="@sheet.Thickness" />
    <MudNumericField T="decimal" Label="Width" @bind-Value="@sheet.Width" />
    <MudNumericField T="decimal" Label="Length" @bind-Value="@sheet.Length" />
}
```

### Display Pattern in Tables
```razor
<MudTable Items="@poLines">
    <HeaderContent>
        <MudTh>Type</MudTh>
        <MudTh>Profile Type</MudTh>
        <MudTh>Size</MudTh>
        <MudTh>Quantity</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>
            @if (context.MaterialInventoryType == MaterialInventoryType.Profile)
            {
                <MudIcon Icon="@Icons.Material.Filled.ViewInAr" Size="Size.Small" Color="Color.Primary" />
                <span>Profile</span>
            }
            else
            {
                <MudIcon Icon="@Icons.Material.Filled.RectangleOutlined" Size="Size.Small" Color="Color.Secondary" />
                <span>Sheet</span>
            }
        </MudTd>
        <MudTd>
            @(context.ProfileType?.Name ?? "N/A")
        </MudTd>
        <MudTd>@context.Size</MudTd>
        <MudTd>@context.Quantity</MudTd>
    </RowTemplate>
</MudTable>
```

## Migration Commands

### Apply Migration
```bash
cd Manimp.Data
dotnet ef database update --context AppDbContext
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName --context AppDbContext
```

### Generate SQL Script (for production)
```bash
dotnet ef migrations script --context AppDbContext --output migration.sql
```

## Testing Scenarios

### Scenario 1: Mixed Material PO
1. Create PO with 3 lines:
   - Line 1: Profile (W12x26)
   - Line 2: Sheet (1200x2400x10mm)
   - Line 3: Profile (L4x4x1/2)
2. Receive all lines → Creates 2 ProfileInventory + 1 SheetInventory
3. Verify ProfileTypeId is NULL for sheet line

### Scenario 2: Sheet Usage with Remnant
1. Create sheet inventory (1200x2400mm, 10 sheets)
2. Record usage: 3 sheets used, 1 remnant (600x2400mm)
3. Verify:
   - SheetsOnHand = 7
   - SheetUsageLog created
   - SheetRemnantInventory created with parent link

### Scenario 3: RFQ to PO Conversion
1. Create RFQ with mixed lines (profiles + sheets)
2. Receive quotes from suppliers
3. Convert to PO → MaterialInventoryType preserved
4. Receive PO → Correct inventory type created

---

**Last Updated**: October 7, 2025  
**Related Docs**: SHEET-INVENTORY-IMPLEMENTATION.md, docs/implementation-status.md
