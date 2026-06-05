using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class BoqSession : IBoqSession
{
    public Guid? CurrentBoqId { get; set; }
    public Guid? CurrentProjectId { get; set; }

    public event EventHandler<Guid>? BoqChanged;

    public void SelectBoq(Guid boqId, Guid projectId)
    {
        CurrentBoqId = boqId;
        CurrentProjectId = projectId;
        BoqChanged?.Invoke(this, boqId);
    }
}
