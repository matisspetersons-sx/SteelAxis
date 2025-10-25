# Sheet Inventory Integration - Implementation Summary

**Date**: October 7, 2025  
**Status**: ✅ Complete - Models, Database Schema, Migration Created  
**Build Status**: ✅ 0 Errors, 67 Warnings (pre-existing)

## Overview

Added comprehensive sheet/plate inventory management as a separate entity from profiles, with full integration into RFQ (Price Requests) and PO (Purchase Orders) systems. Both material types can now be mixed in procurement workflows with proper type selection.

## What Was Added

### 1. New Domain Models (3 Core Entities)

#### `SheetInventory` (`Manimp.Shared/Models/SheetInventory.cs`)
- **Purpose**: Tracks sheet/plate inventory items (distinct from profile beams)
- **Key Fields**:
  - `Thickness`, `Width`, `Length` (dimensions instead of profile size)
  - `SheetsOnHand` / `OriginalSheets` (quantity tracking)
  - `WeightPerSheet` (vs. `WeightPerPiece` for profiles)
  - EN 1090 traceability (MaterialBatch, MillTestCertificateNumber, CertificateType)
  - Lot number with validation pattern: `^[A-Z]{1,2}\d{1,3}$`
- **Navigation**: Relationships to MaterialType, SteelGrade, Supplier, PurchaseOrder, Project, Document

#### `SheetUsageLog`
- **Purpose**: Tracks sheet usage with automatic inventory decrements
- **Key Fields**:
  - `SheetsUsed`, `AreaUsed` (area tracking for partial sheets)
  - `Purpose`, `UsedBy`, `UsedDate`
  - Project association for cost tracking
- **Remnant Generation**: Auto-creates remnants from leftover material

#### `SheetRemnantInventory`
- **Purpose**: Tracks remnant sheets created from usage
- **Key Fields**:
  - `RemainingWidth`, `RemainingLength`, `RemnantSheets`
  - `RemnantLotNumber` (format: `S1-1200x600` - parent lot + dimensions)
  - `IsAvailable` flag for reuse tracking
  - Lineage: `ParentSheetInventoryId`, `SheetUsageLogId`

### 2. Material Type Selection System

#### `MaterialInventoryType` Enum (`Manimp.Shared/Constants/MaterialInventoryType.cs`)
```csharp
public enum MaterialInventoryType
{
    Profile = 1,  // Beams, channels, angles
    Sheet = 2     // Plates, sheets
}
```

### 3. Updated Procurement Models

#### `PurchaseOrderLine` Changes
- **Added**: `MaterialInventoryType` (required enum field)
- **Changed**: `ProfileTypeId` → **nullable** `int?` (not needed for sheets)
- **Size field**: Now accepts both formats:
  - Profiles: "W12x26", "L4x4x1/2"
  - Sheets: "1200x2400x10mm", "4'x8'x0.5in"

#### `PriceRequestLine` Changes
- **Added**: `MaterialInventoryType` (required enum field)
- **Changed**: `ProfileTypeId` → **nullable** `int?`
- **Same dual format** for size specification

### 4. Navigation Property Updates

**Updated Models**:
- `MaterialType`: Added `SheetInventories` collection
- `SteelGrade`: Added `SheetInventories` collection
- `Supplier`: Added `SheetInventories` collection
- `Project`: Added `SheetInventories`, `SheetUsageLogs` collections
- `Document`: Added `SheetInventories` collection
- `PurchaseOrder`: Added `SheetInventories` collection

### 5. Database Configuration (`AppDbContext.cs`)

#### New DbSets Added
```csharp
public DbSet<SheetInventory> SheetInventories { get; set; }
public DbSet<SheetUsageLog> SheetUsageLogs { get; set; }
public DbSet<SheetRemnantInventory> SheetRemnantInventories { get; set; }
```

#### Entity Configurations
- **SheetInventory**: 
  - Decimal precision for Thickness/Width/Length (10,3)
  - Indexes on LotNumber, MaterialBatch, ReceivedDate, Location
  - Foreign keys: MaterialType, SteelGrade, Supplier, PurchaseOrder, Project, Document
- **SheetUsageLog**: 
  - Cascade delete with SheetInventory
  - Indexes on UsedDate, SheetInventoryId, Project
- **SheetRemnantInventory**: 
  - Unique constraint on RemnantLotNumber
  - Restrict delete on ParentSheetInventory (lineage preservation)

#### Updated Configurations
- **PurchaseOrderLine**: ProfileType relationship now `.IsRequired(false)`
- **PriceRequestLine**: ProfileType relationship now `.IsRequired(false)`

### 6. Data Transfer Objects (`Manimp.Shared/DTOs/SheetInventoryDTOs.cs`)

Created 7 DTOs:
1. `CreateSheetInventoryDto` - Add new sheet inventory
2. `UpdateSheetInventoryDto` - Modify existing sheets
3. `CreateSheetUsageDto` - Record sheet usage
4. `CreateSheetRemnantDto` - Create remnants manually
5. `CreatePurchaseOrderLineDto` - Updated with MaterialInventoryType
6. `CreatePriceRequestLineDto` - Updated with MaterialInventoryType

### 7. Database Migration

**Migration**: `AddSheetInventoryAndMaterialType`  
**Location**: `Manimp.Data/Migrations/`  
**Status**: Created, ready to apply

**Tables Created**:
- `SheetInventories`
- `SheetUsageLogs`
- `SheetRemnantInventories`

**Tables Modified**:
- `PurchaseOrderLines` - Added `MaterialInventoryType`, made `ProfileTypeId` nullable
- `PriceRequestLines` - Added `MaterialInventoryType`, made `ProfileTypeId` nullable

## Architecture Design Decisions

### 1. Separate Entities (Not Unified)
**Why**: Profiles and sheets have fundamentally different:
- **Dimensions**: Profiles use Size + Length, sheets use Thickness × Width × Length
- **Unit tracking**: Pieces vs. Sheets
- **Weight calculation**: Per piece vs. per sheet
- **Remnant handling**: Linear (length) vs. area (width × length)

**Benefits**:
- Type-safe queries (no need to check material type in every query)
- Cleaner domain model (no unused fields)
- Easier validation (sheet-specific vs. profile-specific rules)

### 2. Unified Procurement (RFQ + PO)
**Why**: Both material types flow through the same procurement process
- Same suppliers provide both profiles and sheets
- Purchase orders can mix material types
- Price requests compare across material types

**Implementation**:
- `MaterialInventoryType` enum distinguishes at line level
- `ProfileTypeId` nullable (NULL for sheets, required for profiles)
- Size field flexible format handles both types

### 3. Nullable ProfileTypeId Pattern
**Instead of**: Discriminator column or table-per-type inheritance  
**Rationale**: 
- Simple schema change (one nullable column)
- Backward compatible (existing profile lines unaffected)
- Easy to query: `WHERE MaterialInventoryType = 1` (Profile) or `= 2` (Sheet)
- Frontend validation: Show ProfileType dropdown only if MaterialInventoryType = Profile

## Integration Points

### RFQ (Price Request) Workflow
1. User selects **Material Type** (Profile or Sheet) for each line
2. If Profile: ProfileType dropdown appears + Size field
3. If Sheet: ProfileType hidden, Size field accepts dimensions like "1200x2400x10mm"
4. Request sent to suppliers with mixed material types
5. Quotes received and compared across types

### PO (Purchase Order) Workflow
1. Can create PO from RFQ (inherits MaterialInventoryType)
2. Can create manual PO with mixed material types
3. Upon receiving:
   - Profile lines → Create `ProfileInventory` records
   - Sheet lines → Create `SheetInventory` records
4. Automatic remnant generation on usage

## Frontend Implementation Needed

### Components to Create (7 new pages/components)

1. **`SheetInventory.razor`** - Main sheet inventory grid
   - Display: Thickness, Width, Length, Sheets On Hand, Weight, Location
   - Actions: Add, Edit, View Details, Usage Log
   - Filters: By material type, steel grade, supplier, date range

2. **`SheetInventoryDialog.razor`** - Add/Edit sheets
   - Input: All sheet fields (thickness, width, length, weight, etc.)
   - EN 1090 traceability section
   - Validation: Dimensions > 0, lot number pattern

3. **`SheetUsage.razor`** - Record sheet usage
   - Select sheet from inventory
   - Input: Sheets used, area used (optional), project, purpose
   - Auto-decrement sheets on hand
   - Remnant creation option

4. **`SheetRemnants.razor`** - View sheet remnants
   - Display: Parent lot, remaining dimensions, availability
   - Actions: Mark as used, update location, delete

5. **Update `PurchaseOrderDialog.razor`**:
   - Add Material Type selector (Profile/Sheet) for each line
   - Conditionally show ProfileType dropdown (only if Material Type = Profile)
   - Update Size field label based on material type
   - Validate: ProfileTypeId required only if MaterialInventoryType = Profile

6. **Update `PriceRequestDialog.razor`**:
   - Same Material Type selector pattern
   - Size field helper text: "e.g., W12x26 for profiles, 1200x2400x10mm for sheets"

7. **Update `Procurement.razor` & `PriceRequests.razor`**:
   - Display Material Type column in line items tables
   - Show appropriate icon (🏗️ for profiles, 📐 for sheets)

### MudBlazor Components to Use

```razor
<!-- Material Type Selector -->
<MudSelect T="MaterialInventoryType" 
           Label="Material Type" 
           @bind-Value="line.MaterialInventoryType"
           Required="true">
    <MudSelectItem Value="MaterialInventoryType.Profile">Profile</MudSelectItem>
    <MudSelectItem Value="MaterialInventoryType.Sheet">Sheet/Plate</MudSelectItem>
</MudSelect>

<!-- Conditional Profile Type (only show if Profile selected) -->
@if (line.MaterialInventoryType == MaterialInventoryType.Profile)
{
    <MudSelect T="int?" 
               Label="Profile Type" 
               @bind-Value="line.ProfileTypeId"
               Required="true">
        @foreach (var type in profileTypes)
        {
            <MudSelectItem Value="type.ProfileTypeId">@type.Name</MudSelectItem>
        }
    </MudSelect>
}

<!-- Sheet Dimensions (only show if Sheet selected) -->
@if (line.MaterialInventoryType == MaterialInventoryType.Sheet)
{
    <MudNumericField T="decimal" 
                     Label="Thickness" 
                     @bind-Value="sheet.Thickness" 
                     Required="true" 
                     Min="0.001M" />
    <MudNumericField T="decimal" 
                     Label="Width" 
                     @bind-Value="sheet.Width" 
                     Required="true" 
                     Min="0.001M" />
    <MudNumericField T="decimal" 
                     Label="Length" 
                     @bind-Value="sheet.Length" 
                     Required="true" 
                     Min="0.001M" />
}
```

## Backend Services Needed

### `ISheetInventoryService` Interface
```csharp
Task<SheetInventory> CreateSheetAsync(CreateSheetInventoryDto dto);
Task<SheetInventory> UpdateSheetAsync(UpdateSheetInventoryDto dto);
Task<SheetInventory?> GetSheetByIdAsync(int id);
Task<List<SheetInventory>> GetAllSheetsAsync();
Task<bool> DeleteSheetAsync(int id);
Task<SheetUsageLog> RecordSheetUsageAsync(CreateSheetUsageDto dto);
Task<List<SheetRemnantInventory>> GetSheetRemnantsAsync(int parentSheetId);
Task<string> GenerateSheetLotNumberAsync(); // S1, S2, S3...
```

### `SheetInventoryService` Implementation
**Location**: `Manimp.Services/Implementation/SheetInventoryService.cs`  
**Pattern**: Follow `InventoryService.cs` structure (line 1-600 for reference)

**Key Methods**:
- Auto-generate lot numbers (S-prefix convention vs. A-prefix for profiles)
- Calculate weight from thickness × width × length × density
- Remnant auto-creation when usage leaves partial sheets
- EN 1090 traceability validation

### Update `ProcurementService.cs`
**Lines to Modify**: 410-440 (receiving logic), 1075-1105 (inventory creation)

```csharp
// In ReceivePurchaseOrderLineAsync method:
if (line.MaterialInventoryType == MaterialInventoryType.Profile)
{
    // Existing profile inventory creation
    inventory = new ProfileInventory { ... };
}
else if (line.MaterialInventoryType == MaterialInventoryType.Sheet)
{
    // New sheet inventory creation
    var sheetInventory = new SheetInventory 
    { 
        LotNumber = lotNumber,
        Thickness = ParseThickness(line.Size),
        Width = ParseWidth(line.Size),
        Length = ParseLength(line.Size),
        ...
    };
    _context.SheetInventories.Add(sheetInventory);
}
```

## API Controllers Needed

### `SheetInventoryController`
**Location**: `Manimp.Api/Controllers/SheetInventoryController.cs`  
**Feature Gate**: `[RequireFeature(FeatureKeys.CoreInventory)]`

```csharp
[HttpGet] GetAllSheets()
[HttpGet("{id}")] GetSheetById(int id)
[HttpPost] CreateSheet(CreateSheetInventoryDto dto)
[HttpPut] UpdateSheet(UpdateSheetInventoryDto dto)
[HttpDelete("{id}")] DeleteSheet(int id)
[HttpPost("usage")] RecordUsage(CreateSheetUsageDto dto)
[HttpGet("{id}/remnants")] GetSheetRemnants(int id)
```

## Database Migration Instructions

### Apply Migration (Development)
```bash
cd Manimp.Data
dotnet ef database update --context AppDbContext
```

### Seed Data (Optional)
Add to `Data/DbInitializer.cs`:
```csharp
// Sample sheet inventory
var sampleSheets = new[]
{
    new SheetInventory 
    { 
        LotNumber = "S1", 
        Thickness = 10, 
        Width = 1200, 
        Length = 2400, 
        SheetsOnHand = 50,
        OriginalSheets = 50,
        WeightPerSheet = 225.6M,
        MaterialTypeId = plateTypeId,
        SteelGradeId = a36GradeId,
        ReceivedDate = DateTime.UtcNow.AddDays(-30)
    },
    // ... more samples
};
context.SheetInventories.AddRange(sampleSheets);
```

## Testing Checklist

### Model Tests
- [ ] SheetInventory validation (thickness/width/length > 0)
- [ ] Lot number pattern validation
- [ ] Remnant lot number generation (S1-1200x600 format)
- [ ] MaterialInventoryType enum serialization

### Database Tests
- [ ] Create sheet inventory record
- [ ] Update sheets on hand
- [ ] Record sheet usage (auto-decrement)
- [ ] Create sheet remnant with lineage
- [ ] Query sheets by material type, steel grade, supplier
- [ ] Foreign key constraints (MaterialType, SteelGrade, Supplier)

### Procurement Integration Tests
- [ ] Create PO with mixed material types (profiles + sheets)
- [ ] Receive PO line with MaterialInventoryType = Sheet → Creates SheetInventory
- [ ] Receive PO line with MaterialInventoryType = Profile → Creates ProfileInventory
- [ ] Create RFQ with sheet lines (ProfileTypeId = null)
- [ ] Convert RFQ to PO preserves MaterialInventoryType

### UI Tests
- [ ] Material Type selector shows Profile/Sheet options
- [ ] ProfileType dropdown hidden when Sheet selected
- [ ] Sheet dimensions fields appear when Sheet selected
- [ ] PO line validation: ProfileTypeId required only for Profiles
- [ ] Sheet inventory grid displays correctly
- [ ] Sheet usage dialog decrements inventory

## Feature Gating

- **Tier 1 (Basic)**: Core sheet inventory management
- **Tier 2 (Professional)**: Sheet procurement + remnants (requires PO feature)
- **Tier 3 (Enterprise)**: Sheet sourcing in RFQs (requires Price Request feature)

No changes needed to `FeatureKeys` - uses existing:
- `FeatureKeys.CoreInventory` (Tier 1)
- `FeatureKeys.Procurement` (Tier 2)
- `FeatureKeys.Sourcing` (Tier 3)

## Next Steps (Priority Order)

1. **Apply Migration** - `dotnet ef database update` in Manimp.Data
2. **Create SheetInventoryService** - Business logic implementation
3. **Create SheetInventoryController** - REST API endpoints
4. **Update ProcurementService** - Handle sheet receiving logic
5. **Create Blazor Components** - 4 new pages (Inventory, Usage, Remnants, Dialog)
6. **Update PO/RFQ Dialogs** - Add Material Type selector
7. **Add HTTP Services** - `SheetInventoryHttpService.cs` in Manimp.Web/Services
8. **Update Navigation** - Add Sheet Inventory menu item
9. **Seed Demo Data** - Sample sheets for testing
10. **Documentation** - Update README with sheet inventory features

## Files Created/Modified Summary

### New Files (3)
- `Manimp.Shared/Models/SheetInventory.cs` (375 lines)
- `Manimp.Shared/Constants/MaterialInventoryType.cs` (13 lines)
- `Manimp.Shared/DTOs/SheetInventoryDTOs.cs` (275 lines)

### Modified Files (8)
- `Manimp.Shared/Models/Procurement.cs` - Added MaterialInventoryType to PurchaseOrderLine
- `Manimp.Shared/Models/Sourcing.cs` - Added MaterialInventoryType to PriceRequestLine
- `Manimp.Shared/Models/LookupTables.cs` - Added SheetInventories navigation
- `Manimp.Shared/Models/SupportingEntities.cs` - Added sheet navigation properties
- `Manimp.Data/AppDbContext.cs` - Added 3 DbSets + entity configurations
- `Manimp.Services/Implementation/ProcurementService.cs` - Fixed ProfileTypeId nullable
- Migration file: `AddSheetInventoryAndMaterialType.cs`

### Total Impact
- **8 C# files** modified
- **3 new domain models** (Sheet, SheetUsage, SheetRemnant)
- **3 new database tables** (+ 2 modified)
- **1 new enum** (MaterialInventoryType)
- **663 lines of code** added

## Benefits

✅ **Complete Material Coverage**: Handles both structural profiles AND sheet/plate inventory  
✅ **Type-Safe Design**: Separate entities prevent data integrity issues  
✅ **Unified Procurement**: Single workflow for all material types  
✅ **EN 1090 Compliant**: Full traceability for both profiles and sheets  
✅ **Remnant Tracking**: Reduces waste for both material types  
✅ **Flexible Procurement**: Mix profiles and sheets in same PO/RFQ  
✅ **Backward Compatible**: Existing profile inventory unaffected  

---

**Implementation Status**: Database schema complete ✅ | Backend services pending ⏳ | Frontend UI pending ⏳  
**Estimated Frontend Work**: 16-20 hours (4 new pages, 2 dialog updates, services, testing)
