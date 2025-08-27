namespace Domain.Exceptions
{
    public class NotFound : Exception
    {
        public NotFound(string entity, object key)
        : base($"{entity} with key '{key}' was not found.") { }
    }
}
