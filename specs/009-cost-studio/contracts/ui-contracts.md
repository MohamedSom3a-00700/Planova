# UI Contracts — Cost Studio

## ViewModel Contracts

Each ViewModel follows the CommunityToolkit.Mvvm pattern with `ObservableObject`, `RelayCommand`, and `ObservableProperty` attributes.

### ICostStudioViewModel (Shell Integration)

```csharp
public interface ICostStudioViewModel
{
    string DisplayName { get; }                // "Cost Studio"
    string IconKey { get; }                    // Resource dictionary key for icon
    ICommand NavigateToTabCommand { get; }     // Navigation within Cost Studio
}
```

### ViewModel Navigation Structure

```
CostStudioViewModel (shell)
├── CostBreakdownViewModel
├── DirectCostManagerViewModel
├── BudgetViewModel
│   └── BudgetRevisionViewModel (dialog)
├── ActualCostViewModel
├── CashFlowViewModel
├── EvmViewModel
├── CostAiViewModel
└── CostReportViewModel
```

### DataGridView Contracts

Cost Studio uses the following column contracts for data grid views:

#### DirectCostsGrid

| Column | Display | Type | Editable |
|--------|---------|------|----------|
| Category | Category | String (enum) | Yes |
| Description | Description | String | Yes |
| Quantity | Qty | Decimal | Yes |
| UnitOfMeasure | UOM | String | Yes |
| UnitRate | Rate | Decimal | Yes |
| TotalAmount | Total | Decimal (computed) | No |
| Scope | Scope | String (enum) | Yes |

#### BudgetRevisionsGrid

| Column | Display | Type | Editable |
|--------|---------|------|----------|
| RevisionNumber | # | Integer | No |
| RevisionType | Type | String (enum) | No |
| Amount | Amount | Decimal | Yes (Pending only) |
| Status | Status | String | No |
| Reason | Reason | String | Yes (Pending only) |
| ApprovedBy | Approved By | String | No |
| ApprovedAt | Approved At | DateTime | No |

#### ActualCostsGrid

| Column | Display | Type | Editable |
|--------|---------|------|----------|
| ActivityCode | Code | String | No |
| ActivityName | Activity | String | No |
| Amount | Actual Cost | Decimal | Yes |
| Currency | Currency | String | Yes |
| Source | Source | String | No |
| Variance | Variance | Decimal (computed) | No |
| VariancePercent | Var % | Decimal (computed) | No |

#### CashFlowGrid

| Column | Display | Type | Editable |
|--------|---------|------|----------|
| PeriodStart | Period | DateTime | No |
| PlannedCost | Planned | Decimal | No |
| ActualCost | Actual | Decimal | No |
| CumulativePlanned | Cum. Planned | Decimal | No |
| CumulativeActual | Cum. Actual | Decimal | No |

#### EvmMetricsGrid

| Metric | Display | Format | Color Condition |
|--------|---------|--------|-----------------|
| PlannedValue (PV) | PV | Currency | — |
| EarnedValue (EV) | EV | Currency | — |
| ActualCost (AC) | AC | Currency | — |
| CostVariance (CV) | CV | Currency | Negative = Red |
| ScheduleVariance (SV) | SV | Currency | Negative = Red |
| CPI | CPI | Decimal (2) | <0.8=Red, 0.8-0.99=Amber, >=1.0=Green |
| SPI | SPI | Decimal (2) | <0.8=Red, 0.8-0.99=Amber, >=1.0=Green |
| EAC | EAC | Currency | — |
| ETC | ETC | Currency | — |
| VAC | VAC | Currency | Negative = Red |

## Navigation

Cost Studio is registered in `ShellViewModel` as a navigation target with the key `"CostStudio"`. The shell invokes `INavigationService.NavigateTo("CostStudio")` when the user clicks the Cost Studio navigation rail item.
