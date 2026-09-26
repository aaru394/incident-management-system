namespace IncidentManagement.Application.Common;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key) : base($"{entity} with id '{key}' was not found.")
    {
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message)
    {
    }
}
