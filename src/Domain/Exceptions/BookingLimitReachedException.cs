namespace Domain.Exceptions;

public class BookingLimitReachedException : Exception
{
	public override string Message => "Booking limit has been reached";

	public BookingLimitReachedException() : base()
	{
	}

	public BookingLimitReachedException(string message) : base(message)
	{
	}

	public BookingLimitReachedException(string message, Exception innerException) : base(message, innerException)
	{
	}
}