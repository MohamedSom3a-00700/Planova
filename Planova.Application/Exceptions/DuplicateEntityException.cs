namespace Planova.Application.Exceptions;

public class DuplicateEntityException : ApplicationException
{
    public DuplicateEntityException(string entityName, string field, string value)
        : base($"A {entityName} with {field} '{value}' already exists.")
    {
    }
}
