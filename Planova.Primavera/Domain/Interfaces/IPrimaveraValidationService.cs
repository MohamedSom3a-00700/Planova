using Planova.Primavera.Application.Dto;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraValidationService
{
    Task<List<PrimaveraValidationIssueDto>> ValidateAsync(int projectId, CancellationToken ct = default);
    Task<DcmaAssessmentResultDto> AssessDcma14PointAsync(int projectId, CancellationToken ct = default);
}
