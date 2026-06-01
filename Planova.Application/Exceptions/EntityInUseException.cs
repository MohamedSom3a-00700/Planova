namespace Planova.Application.Exceptions;

public class EntityInUseException : ApplicationException
{
    public EntityInUseException(string entityName, string childEntity)
        : base($"Cannot delete {entityName}. It has linked {childEntity} records.")
    {
    }
}
