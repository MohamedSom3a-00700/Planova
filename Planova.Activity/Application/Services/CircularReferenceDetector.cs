using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Application.Services;

public class CircularReferenceDetector
{
    public CircularReferenceCheckResult Detect(
        Guid predecessorId,
        Guid successorId,
        Func<Guid, Task<List<Guid>>> getPredecessorIdsAsync)
    {
        if (predecessorId == successorId)
        {
            return new CircularReferenceCheckResult
            {
                HasCycle = true,
                CycleActivities = [predecessorId],
                Message = "Cannot create a relationship between the same activity (self-reference)."
            };
        }

        var visited = new HashSet<Guid>();
        var recursionStack = new Stack<Guid>();
        var parentMap = new Dictionary<Guid, Guid>();

        recursionStack.Push(successorId);
        parentMap[successorId] = predecessorId;

        while (recursionStack.Count > 0)
        {
            var current = recursionStack.Pop();

            if (!visited.Add(current))
                continue;

            var predecessors = Task.Run(async () => await getPredecessorIdsAsync(current)).GetAwaiter().GetResult();

            foreach (var pred in predecessors)
            {
                if (pred == predecessorId)
                {
                    var cycle = new List<Guid> { predecessorId, current };
                    return new CircularReferenceCheckResult
                    {
                        HasCycle = true,
                        CycleActivities = cycle,
                        Message = $"Adding this relationship would create a circular reference: {string.Join(" -> ", cycle)}"
                    };
                }

                if (!visited.Contains(pred))
                {
                    recursionStack.Push(pred);
                    parentMap[pred] = current;
                }
            }
        }

        return new CircularReferenceCheckResult
        {
            HasCycle = false,
            CycleActivities = [],
            Message = null
        };
    }
}
