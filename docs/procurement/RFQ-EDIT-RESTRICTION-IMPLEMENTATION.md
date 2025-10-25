# RFQ Edit Restriction - October 11, 2025

## Business Rule Implementation

Successfully implemented business rule: **RFQ editing is only allowed for Draft status**.

## Change Summary

### What Changed
Added status-based disabling logic to the Edit button in the Price Requests table.

### Business Justification
- **Audit Trail Protection**: Once an RFQ is sent to suppliers, it becomes part of the procurement record
- **Data Integrity**: Prevents modification of sent quotes that suppliers may have already responded to
- **Compliance**: Maintains accurate history for procurement auditing
- **Workflow Enforcement**: Clear status progression (Draft → Sent → Quoted → Completed)

## Technical Implementation

### File Modified
`Manimp.Web/Components/Pages/PriceRequests.razor` - Actions column in Price Requests table

### Code Change

**Before**:
```razor
<MudTooltip Text="Edit">
    <MudIconButton Icon="@Icons.Material.Filled.Edit"
                   Size="Size.Small"
                   Color="Color.Default"
                   OnClick="@(() => EditPriceRequest(context.Item))" />
</MudTooltip>
```

**After**:
```razor
<MudTooltip Text="@(context.Item.Status == "Draft" ? "Edit" : "Cannot edit after sending")">
    <MudIconButton Icon="@Icons.Material.Filled.Edit"
                   Size="Size.Small"
                   Color="Color.Default"
                   Disabled="@(context.Item.Status != "Draft")"
                   OnClick="@(() => EditPriceRequest(context.Item))" />
</MudTooltip>
```

### Key Features

1. **Disabled Property**: `Disabled="@(context.Item.Status != "Draft")"`
   - Evaluates to `true` when status is NOT "Draft"
   - Button becomes grayed out and non-clickable

2. **Dynamic Tooltip**: `Text="@(context.Item.Status == "Draft" ? "Edit" : "Cannot edit after sending")"`
   - Shows "Edit" for Draft status
   - Shows "Cannot edit after sending" for all other statuses

3. **Visual Feedback**:
   - Draft status: Normal button (clickable, default color)
   - Other statuses: Grayed out button (disabled, cursor not-allowed)

## Status Workflow

```
Draft ────────▶ Sent ────────▶ Quoted ────────▶ Completed
  ✅ Editable      ❌ Read-only    ❌ Read-only     ❌ Read-only
                   ▼
                Cancelled
              ❌ Read-only
```

### Status Descriptions

| Status | Edit Allowed | Description |
|--------|-------------|-------------|
| **Draft** | ✅ Yes | Initial state, can modify freely |
| **Sent** | ❌ No | Sent to suppliers, awaiting quotes |
| **Quoted** | ❌ No | Quotes received from suppliers |
| **Completed** | ❌ No | Quote accepted, converted to PO |
| **Cancelled** | ❌ No | RFQ cancelled |

## User Experience

### For Draft RFQs:
1. Edit button appears normal (full opacity, default color)
2. Hover shows "Edit" tooltip
3. Click opens EditPriceRequestDialog
4. User can modify all fields and line items

### For Sent/Quoted/Completed/Cancelled RFQs:
1. Edit button appears grayed out (reduced opacity)
2. Hover shows "Cannot edit after sending" tooltip
3. Click does nothing (button disabled)
4. Cursor changes to "not-allowed" icon on hover

## Testing Scenarios

### Scenario 1: Draft RFQ
```
Given: RFQ with Status = "Draft"
When: User views price requests table
Then: Edit button is enabled
And: Tooltip shows "Edit"
When: User clicks Edit button
Then: EditPriceRequestDialog opens with data
```

### Scenario 2: Sent RFQ
```
Given: RFQ with Status = "Sent"
When: User views price requests table
Then: Edit button is disabled (grayed out)
And: Tooltip shows "Cannot edit after sending"
When: User hovers over Edit button
Then: Cursor shows "not-allowed" icon
When: User clicks Edit button
Then: Nothing happens (button is disabled)
```

### Scenario 3: Status Transition
```
Given: RFQ with Status = "Draft"
And: Edit button is enabled
When: User clicks "Send to Suppliers" button
And: Status changes to "Sent"
Then: Edit button becomes disabled
And: Tooltip changes to "Cannot edit after sending"
```

## Build Status

✅ **Build: SUCCESS**
- 0 errors
- 43 warnings (pre-existing, unrelated)

## Benefits

1. **Data Protection**: Prevents accidental modification of sent RFQs
2. **Clear UX**: Visual feedback indicates when editing is not allowed
3. **Audit Compliance**: Maintains immutable procurement records after sending
4. **Error Prevention**: UI-level validation prevents invalid operations
5. **User Guidance**: Tooltip explains why button is disabled

## Related Documentation

- `EDIT-RFQ-DIALOG-IMPLEMENTATION.md` - Complete edit dialog implementation
- `RFQ-DIALOGS-IMPLEMENTATION.md` - All RFQ dialog components
- `RFQ-INLINE-EDITING-IMPLEMENTATION.md` - Inline editing pattern

## Future Enhancements (Optional)

1. **Admin Override**: Add role-based permission to edit sent RFQs (with audit log)
2. **Version History**: Track all changes to RFQs with timestamps
3. **Amendment Workflow**: Allow creating "Amendment" RFQs linked to original
4. **Status-based Warnings**: Show warning dialog if trying to edit near-sent RFQ
5. **Bulk Status Update**: Change multiple RFQ statuses at once

## Notes

- This is a **UI-level restriction** (button disabled)
- Backend validation should also enforce this rule (API level)
- Consider adding similar restrictions to Delete button (e.g., can't delete Completed RFQs)
- Status transitions should be validated (e.g., can't go from Completed back to Draft)

## Summary

✅ Edit button now respects RFQ status
✅ Only Draft RFQs can be edited
✅ Clear visual feedback for disabled state
✅ Informative tooltip explains restriction
✅ Maintains data integrity and audit trail
✅ Build successful, ready for testing!
