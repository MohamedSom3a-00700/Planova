using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Application.Dto;

public record ClassificationDto(
    Guid Id,
    Guid? ParentId,
    string Code,
    string Name,
    string? Description,
    ClassificationScope Scope,
    Guid? ProjectId,
    List<ClassificationDto> Children
);
