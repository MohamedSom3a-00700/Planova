using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;

namespace Planova.UI.ViewModels.Cost;

public partial class BudgetRevisionViewModel : ObservableObject
{
    private readonly IBudgetRevisionRepository _repository;

    [ObservableProperty]
    private ObservableCollection<BudgetRevisionDto> _revisions = new();

    [ObservableProperty]
    private BudgetRevisionDto? _selectedRevision;

    [ObservableProperty]
    private string _newRevisionType = "Revised";

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private string? _newReason;

    public BudgetRevisionViewModel(IBudgetRevisionRepository repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    private async Task LoadAsync(Guid budgetId)
    {
        var revisions = await _repository.GetByBudgetIdAsync(budgetId);
        Revisions = new ObservableCollection<BudgetRevisionDto>(revisions.Select(r => new BudgetRevisionDto(
            r.Id, r.BudgetId, r.RevisionNumber, r.RevisionType.ToString(), r.Amount, r.Status.ToString(),
            r.Reason, r.ApprovedBy, r.ApprovedAt, r.CreatedAt, r.CreatedBy ?? string.Empty)));
    }

    [RelayCommand]
    private async Task CreateRevisionAsync(Guid budgetId)
    {
        var revision = new Planova.Cost.Domain.Entities.BudgetRevision
        {
            Id = Guid.NewGuid(),
            BudgetId = budgetId,
            RevisionNumber = await _repository.GetNextRevisionNumberAsync(budgetId),
            RevisionType = Enum.Parse<Planova.Cost.Domain.Enums.BudgetRevisionType>(NewRevisionType),
            Amount = NewAmount,
            Status = Planova.Cost.Domain.Enums.BudgetRevisionStatus.Pending,
            Reason = NewReason,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(revision);
        await LoadAsync(budgetId);
    }
}
