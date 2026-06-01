namespace Planova.Application.Exceptions;

public class EntityNotFoundException : ApplicationException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with id {id} was not found.")
    {
    }
}
