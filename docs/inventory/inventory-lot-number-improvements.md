# Inventory System Improvements - Implementation Summary

## Date: October 5, 2025

## Changes Implemented

### 1. **Profile Type Column Separation**
- **ProfileInventory Model** (`Manimp.Shared/Models/CoreInventory.cs`):
  - Added new `Dimensions` field (nullable string, max 50 chars) to store dimensions separately from profile type
  - Profile type is now derived from the `Size` field and displayed in UI only
  - Example: Size="W12x26" → ProfileType="W-Beam", Dimensions="12x26"

### 2. **Lot Number Schema Update**
- **New Schema**: 1 letter followed by max 3 digits (e.g., A1, A999)
- When first letter exhausts (A1-A999), second letter is added (AA1-AA999, AB1, etc.)
- **Validation**: Added regex pattern `^[A-Z]{1,2}\d{1,3}$` to `LotNumber` field
- **MaxLength**: Updated from 100 to 10 characters

### 3. **Remnant Lot Number Format**
- **New Format**: Original lot number + length (e.g., A1-3.5m, B15-1.2m)
- Simplified from complex `REM-LOT-2024-001-20240923-001` format
- Updated in `ProfileRemnantInventory` model (`Manimp.Shared/Models/Procurement.cs`)
- MaxLength updated from 100 to 50 characters

### 4. **Clickable Lot Numbers with Details Dialog**

#### **LotDetailsDialog Component** (NEW)
- Location: `Manimp.Web/Components/Dialogs/LotDetailsDialog.razor`
- **Features**:
  - **Lot Summary**: Displays lot number, profile type, dimensions, size, pieces (original/current/used), length, weight, location
  - **EN 1090 Traceability**: Shows material batch, certificate type, mill test certificate, material standard, country of origin, manufacturing route
  - **Usage History**: Table of all usage logs with date, pieces used, length used, project, purpose, used by
  - **Remnants List**: Table of all remnants created from this lot with lot numbers, lengths, pieces, status
  - **Related Projects**: Chip set of all projects that have used material from this lot
  - Includes mock data generation for demo purposes when API is unavailable

#### **Clickable Lot Numbers in Inventory Page**
- Main inventory table: Lot numbers are now clickable `MudButton` components
- Opens `LotDetailsDialog` when clicked
- Remnant inventory table: Remnant lot numbers are also clickable
- Color-coded: Primary blue for inventory lots, Warning orange for remnant lots

#### **Inventory Page Updates** (`Manimp.Web/Components/Pages/Inventory.razor`):
- Replaced `PropertyColumn` for lot number with `TemplateColumn` containing clickable button
- Added `OpenLotDetailsDialog()` method to show lot details
- Added `OpenRemnantDetailsDialog()` method to show remnant details
- Reordered columns: Lot Number | Profile Type | Dimensions | Size | ...

### 5. **HTTP Service Enhancement**
- **InventoryHttpService** (`Manimp.Web/Services/InventoryHttpService.cs`):
  - Added `GetRemnantsByOriginalInventoryIdAsync()` method to fetch remnants for a specific lot
  - Supports API endpoint: `GET /api/inventory/profiles/{id}/remnants`

### 6. **Mock Data Updates**
- Updated all mock inventory data to use new lot number format:
  - LOT-2024-001 → A1
  - LOT-2024-002 → A2
  - LOT-2024-003 → B15
  - LOT-2024-004 → C999
  - LOT-2024-005 → AA1
- Added `Dimensions` field to all mock inventory items
- Added `OriginalPieces` field to properly track usage
- Updated remnant mock data to use new format (A1-3.5m, A2-2.8m, B15-1.2m)

## Database Schema Changes Required

### Migration Needed:
```sql
-- Add Dimensions column to ProfileInventory
ALTER TABLE ProfileInventory 
ADD Dimensions NVARCHAR(50) NULL;

-- Update LotNumber column constraints
ALTER TABLE ProfileInventory 
ALTER COLUMN LotNumber NVARCHAR(10) NOT NULL;

-- Add check constraint for lot number format
ALTER TABLE ProfileInventory 
ADD CONSTRAINT CK_ProfileInventory_LotNumber_Format 
CHECK (LotNumber LIKE '[A-Z][0-9]%' OR LotNumber LIKE '[A-Z][A-Z][0-9]%');

-- Update RemnantLotNumber column length
ALTER TABLE ProfileRemnantInventory 
ALTER COLUMN RemnantLotNumber NVARCHAR(50) NOT NULL;

-- Add OriginalPieces if not exists (for tracking total received)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProfileInventory') AND name = 'OriginalPieces')
BEGIN
    ALTER TABLE ProfileInventory 
    ADD OriginalPieces INT NOT NULL DEFAULT 0;
    
    -- Set OriginalPieces to current PiecesOnHand for existing records
    UPDATE ProfileInventory 
    SET OriginalPieces = PiecesOnHand 
    WHERE OriginalPieces = 0;
END
```

## UI/UX Improvements

### Inventory Table Layout:
```
Lot Number  | Profile Type | Dimensions | Size      | Pieces | Length | Weight | ...
(Clickable) | (Chip)       |            |           |        |        |        |
A1          | W-Beam       | 12x26      | W12x26    | 20     | 12.0m  | 38.5kg |
A2          | Angle        | 4x4x1/2    | L4x4x1/2  | 50     | 6.0m   | 12.8kg |
```

### Lot Details Dialog Sections:
1. **Lot Information** (MudPaper, Primary color)
2. **EN 1090 Traceability** (MudPaper, Success color) - conditionally shown
3. **Usage History** (MudPaper, Info color) - MudTable with usage logs
4. **Remnants from This Lot** (MudPaper, Warning color) - MudTable with remnants
5. **Related Projects** (MudPaper, Primary color) - MudChipSet with project IDs

## Testing Checklist

- [x] Build succeeds with 0 errors
- [ ] Create new material with new lot number format (A1, B123, AA1)
- [ ] Verify lot number validation rejects invalid formats (A, 123, AAA1, A1234)
- [ ] Click lot number in inventory table → opens dialog with full details
- [ ] Verify usage history displayed correctly in dialog
- [ ] Verify remnants displayed correctly in dialog
- [ ] Create remnant → verify new format (e.g., A1-3.5m)
- [ ] Click remnant lot number → shows details
- [ ] Verify dimensions column shows separately from profile type
- [ ] Test with empty/null dimensions value

## Files Modified

1. `Manimp.Shared/Models/CoreInventory.cs` - Added Dimensions, updated LotNumber validation
2. `Manimp.Shared/Models/Procurement.cs` - Updated RemnantLotNumber format
3. `Manimp.Web/Components/Dialogs/LotDetailsDialog.razor` - NEW comprehensive details dialog
4. `Manimp.Web/Components/Pages/Inventory.razor` - Clickable lot numbers, updated columns, mock data
5. `Manimp.Web/Services/InventoryHttpService.cs` - Added GetRemnantsByOriginalInventoryIdAsync

## Next Steps

1. **Database Migration**: Create and run EF Core migration for schema changes
2. **API Implementation**: Implement `GetRemnantsByOriginalInventoryIdAsync` in backend controller
3. **Validation**: Add UI validation for lot number format in AddMaterialDialog
4. **Auto-generation**: Implement automatic lot number generation (next available in sequence)
5. **Remnant Dialog**: Consider creating a dedicated RemnantDetailsDialog (currently uses alert)
6. **Print/Export**: Add ability to print or export lot details from dialog
7. **QR Codes**: Generate QR codes for lot numbers for scanning

## Design Decisions

### Why separate Dimensions from ProfileType?
- Profile type (W-Beam, Angle, etc.) is a categorical classification
- Dimensions (12x26, 4x4x1/2, etc.) are specific measurements
- Separation allows:
  - Better filtering by profile type
  - Clear visual distinction in UI
  - Easier analytics (e.g., "how many W-Beams do we have?")

### Why simplify remnant lot numbers?
- Old format: `REM-LOT-2024-001-20240923-001` (32 chars, hard to read)
- New format: `A1-3.5m` (7 chars, immediately clear)
- Benefits:
  - Instantly shows parent lot (A1)
  - Shows remnant length (3.5m)
  - Easier to communicate verbally
  - Fits better in mobile/compact UIs

### Why limit lot numbers to 10 characters?
- Current max: 2 letters + 3 digits = 5 chars (AA999)
- Theoretical max: 26 * 999 + 26 * 26 * 999 = 701,974 unique lots
- With 10 char limit, room for future expansion (e.g., AAA1 pattern)
- Still compact for labels, QR codes, and verbal communication
