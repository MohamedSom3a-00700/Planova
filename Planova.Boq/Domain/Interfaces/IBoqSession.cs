namespace Planova.Boq.Domain.Interfaces;

public interface IBoqSession
{
    Guid? CurrentBoqId { get; set; }
    Guid? CurrentProjectId { get; set; }

    event EventHandler<Guid>? BoqChanged;

    void SelectBoq(Guid boqId, Guid projectId);
}
