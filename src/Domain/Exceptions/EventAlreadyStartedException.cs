namespace Domain.Exceptions;

public class EventAlreadyStartedException : Exception
{
	public override string Message => "Event already started";

	public EventAlreadyStartedException() : base()
	{
	}

	public EventAlreadyStartedException(string message) : base(message)
	{
	}

	public EventAlreadyStartedException(string message, Exception inner) : base(message, inner)
	{
	}
}