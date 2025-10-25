# Quick Integration Guide - Procurement Quote Workflow UI

**Target Audience:** Next developer who needs to integrate the 3 new dialog components  
**Estimated Time:** 30-45 minutes  
**Prerequisites:** Backend API is complete and working, database migration applied

---

## What's Already Done ✅

### Backend (100% Complete)
- ✅ `PriceQuoteLine` entity created
- ✅ Database migration created (`AddPriceQuoteLineTracking`)
- ✅ 5 new service methods in `ProcurementService.cs`
- ✅ 4 new API endpoints in `ProcurementController.cs`
- ✅ Complete DTOs in `ProcurementWorkflowDTOs.cs`
- ✅ 4 HTTP service methods in `ProcurementHttpService.cs`

### Frontend (100% Built, Needs Integration)
- ✅ `SubmitQuoteDialog.razor` (330 lines) - Ready to use
- ✅ `QuoteReviewDialog.razor` (330 lines) - Ready to use
- ✅ `QuoteLinesTable.razor` (170 lines) - Ready to use
- ⏳ **Need to wire into Procurement.razor page**

---

## Integration Steps

### Step 1: Apply Database Migration (5 minutes)

```bash
cd /Users/matisspetersons/Documents/MANIMP/manimp-1/Manimp.Data
dotnet ef database update --context AppDbContext
```

**Verify:** Check database for `PriceQuoteLines` table with 14 columns.

---

### Step 2: Fix Pre-existing Error (10 minutes)

**Location:** `Manimp.Web/Components/Pages/Procurement.razor` line 388

**Current Error:**
```
error CS0246: The type or namespace name 'PurchaseOrderDialog' could not be found
```

**Options:**
1. **If PurchaseOrderDialog exists elsewhere:** Add correct using statement
2. **If it doesn't exist:** Comment out or remove the reference (it's not used by new components)
3. **Quick fix:** Replace with `PurchaseOrderLineDialog` if that's what was intended

---

### Step 3: Add Dialog Trigger Buttons (10 minutes)

**File:** `Manimp.Web/Components/Pages/Procurement.razor`

**Find the section where Price Requests are displayed in a table/list**

**Add button for submitting quotes:**
```razor
@* In the actions column for each Price Request with Status == "Sent" *@
<MudButton Color="Color.Success" 
           Size="Size.Small" 
           Variant="Variant.Filled"
           StartIcon="@Icons.Material.Filled.RequestQuote"
           OnClick="() => OpenSubmitQuoteDialog(priceRequest)">
    Submit Quote
</MudButton>
```

**Add button for reviewing quotes:**
```razor
@* In the actions column for each Price Quote with Status == "Received" *@
<MudButton Color="Color.Primary" 
           Size="Size.Small" 
           Variant="Variant.Filled"
           StartIcon="@Icons.Material.Filled.CompareArrows"
           OnClick="() => OpenQuoteReviewDialog(quote)">
    Review & Process
</MudButton>
```

---

### Step 4: Add Dialog Methods to Code Block (10 minutes)

**File:** `Manimp.Web/Components/Pages/Procurement.razor`

**Add to the @code block:**

```csharp
@code {
    // ... existing code ...
    
    private async Task OpenSubmitQuoteDialog(PriceRequest priceRequest)
    {
        // You need to determine the supplier ID
        // Options:
        // 1. If user is logged in as supplier, use their ID
        // 2. Prompt user to select supplier
        // 3. Get from priceRequest.SupplierId if it's a directed RFQ
        
        int supplierId = 1; // TODO: Get actual supplier ID
        
        var parameters = new DialogParameters
        {
            { "PriceRequest", priceRequest },
            { "SupplierId", supplierId }
        };
        
        var options = new DialogOptions 
        { 
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true
        };
        
        var dialog = await DialogService.ShowAsync<SubmitQuoteDialog>(
            "Submit Supplier Quote", 
            parameters,
            options);
        
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            var submittedQuote = result.Data as PriceQuote;
            Snackbar.Add($"Quote {submittedQuote?.QuoteNumber} submitted successfully", Severity.Success);
            await LoadData(); // Refresh your data
        }
    }
    
    private async Task OpenQuoteReviewDialog(PriceQuote quote)
    {
        var parameters = new DialogParameters
        {
            { "PriceQuote", quote }
        };
        
        var options = new DialogOptions 
        { 
            MaxWidth = MaxWidth.ExtraLarge,
            FullWidth = true,
            CloseButton = true
        };
        
        var dialog = await DialogService.ShowAsync<QuoteReviewDialog>(
            "Review and Process Quote",
            parameters,
            options);
        
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            var processResult = result.Data as ProcessQuoteResult;
            
            // Show success message
            var message = $"Purchase Order {processResult?.CreatedPO?.PONumber} created successfully";
            if (processResult?.NewPriceRequestForUnavailableItems != null)
            {
                message += $"\nNew RFQ {processResult.NewPriceRequestForUnavailableItems.RequestNumber} created for unavailable items";
            }
            
            Snackbar.Add(message, Severity.Success);
            await LoadData(); // Refresh your data
        }
    }
}
```

---

### Step 5: Test the Integration (10 minutes)

**Test Scenario 1: Submit a Quote**
1. Navigate to Procurement page
2. Find an RFQ with status "Sent"
3. Click "Submit Quote" button
4. Fill in quote details:
   - Quote number: `Q-2025-TEST-001`
   - Mark some items as Available with pricing
   - Mark some items as Unavailable with reason
5. Click Submit
6. Verify quote appears in quotes list

**Test Scenario 2: Process a Quote**
1. Find the quote you just submitted
2. Click "Review & Process" button
3. Review the quote lines
4. Select which lines to accept (checkboxes)
5. Set expected delivery date
6. Click "Create Purchase Order"
7. Verify:
   - Success message shows PO number
   - Success message shows new RFQ number (if any unavailable items)
   - PO appears in PO list
   - New RFQ appears in RFQ list (if applicable)

---

## Troubleshooting

### Issue: "SupplierId not found"
**Solution:** You need to determine the supplier ID. Options:
- If the current user is a supplier, get their company's supplier ID
- Add a supplier dropdown to the Procurement page
- Pass the supplier ID from the RFQ if it's a directed request

### Issue: "DialogService is null"
**Solution:** Make sure you have this injection in your page:
```csharp
@inject IDialogService DialogService
```

### Issue: "Snackbar is null"
**Solution:** Make sure you have this injection:
```csharp
@inject ISnackbar Snackbar
```

### Issue: Dialog doesn't open
**Solution:** Check browser console for errors. Most likely:
- Missing parameter
- Wrong parameter type
- Dialog component not found (check using statement)

### Issue: Data doesn't refresh after dialog closes
**Solution:** Make sure you call your data loading method:
```csharp
await LoadData(); // or whatever your method is called
await LoadPriceRequests();
await LoadQuotes();
```

---

## Optional Enhancements

### Display Quote Lines Table
To show quote lines in a details section:

```razor
@if (selectedQuote != null)
{
    <MudPaper Class="pa-4 mt-4">
        <MudText Typo="Typo.h6" Class="mb-3">Quote Lines</MudText>
        <QuoteLinesTable QuoteLines="@selectedQuote.PriceQuoteLines?.ToList()" 
                         ShowNotes="true" 
                         Dense="true" />
    </MudPaper>
}
```

### Add Loading States
```razor
@if (_isLoadingQuotes)
{
    <MudProgressLinear Indeterminate="true" />
}
```

### Add Confirmation Dialogs
Before processing a quote:
```csharp
bool? confirm = await DialogService.ShowMessageBox(
    "Confirm",
    "This will create a Purchase Order. Continue?",
    yesText: "Create PO",
    cancelText: "Cancel");

if (confirm == true)
{
    // Proceed with processing
}
```

---

## Expected Results After Integration

- ✅ Users can click "Submit Quote" on sent RFQs
- ✅ Dialog opens with all RFQ line items
- ✅ Users can mark items as available/unavailable
- ✅ Quote totals calculate automatically
- ✅ Quote submits to backend successfully
- ✅ Users can click "Review & Process" on received quotes
- ✅ Dialog shows all quote lines with availability
- ✅ Users can select which lines to accept
- ✅ System creates PO for accepted lines
- ✅ System auto-creates new RFQ for unavailable items
- ✅ Success messages show PO number and new RFQ number
- ✅ Data refreshes automatically

---

## Files Modified (During Integration)

1. `Manimp.Web/Components/Pages/Procurement.razor`
   - Add 2 button elements
   - Add 2 methods to @code block
   - Fix PurchaseOrderDialog reference

That's it! The dialogs handle everything else.

---

## Next Steps After Integration

1. **Test End-to-End Workflow**
   - Create RFQ → Submit Quote → Review → Create PO
   - Verify new RFQ created for unavailable items
   - Test material receiving

2. **Enhance Material Receiving Dialog** (Optional)
   - Add EN 1090 certificate fields
   - Show auto-generated lot number
   - Link to `ReceiveMaterialsAndCreateInventoryAsync` method

3. **Add Quote Comparison View** (Future)
   - Side-by-side comparison of multiple quotes
   - Highlight best prices
   - Help users make decisions

---

## Support Documentation

- **Backend API**: `/PROCUREMENT-QUOTE-WORKFLOW-IMPLEMENTATION.md`
- **UI Components**: `/PROCUREMENT-UI-IMPLEMENTATION-SUMMARY.md`
- **Complete Summary**: `/PROCUREMENT-WORKFLOW-COMPLETE-SUMMARY.md`
- **Implementation Status**: `/docs/implementation-status.md`

---

**Estimated Total Integration Time: 30-45 minutes**

**Questions?** Check the comprehensive documentation files or the component source code - all components are fully commented.
