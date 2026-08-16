namespace Domain.Exceptions;

public class OutOfDateException : Exception
{
	public override string Message => "Out of date exception";

	public OutOfDateException() : base()
	{
	}

	public OutOfDateException(string message) : base(message)
	{
	}

	public OutOfDateException(string message, Exception innerException) : base(message, innerException)
	{
	}
}