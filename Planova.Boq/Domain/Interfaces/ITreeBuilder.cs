using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Entities;

namespace Planova.Boq.Domain.Interfaces;

public interface ITreeBuilder
{
    IReadOnlyList<BoqItem> BuildTree(IReadOnlyList<ImportRow> rows, TreeBuildStrategy strategy);
    TreeBuildStrategy DetectStrategy(IReadOnlyList<ImportRow> rows);
}
