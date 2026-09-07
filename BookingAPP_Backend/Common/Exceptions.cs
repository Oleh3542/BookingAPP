namespace BookingAPP_Backend.Common;

// базовий клас доменних винятків API
public abstract class ApiException : Exception
{
    protected ApiException(string message) : base(message) { }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string resource, object id)
        : base($"{resource} з ідентифікатором '{id}' не знайдено.") { }
}

// дані запиту не пройшли валідацію
public class ValidationFailedException : ApiException
{
    public ValidationFailedException(string message) : base(message) { }
}

// конфлікт бізнес-логіки
public class BookingConflictException : ApiException
{
    public BookingConflictException(string message) : base(message) { }
}
