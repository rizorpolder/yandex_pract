namespace Domain.Exceptions;

public class PermissionException : Exception
{
	public override string Message => "Not allowed to perform this action";

	public PermissionException() : base()
	{
	}

	public PermissionException(string message) : base(message)
	{
	}

	public PermissionException(string message, Exception innerException) : base(message, innerException)
	{
	}
}