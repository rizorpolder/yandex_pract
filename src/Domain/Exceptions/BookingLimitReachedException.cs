namespace Domain.Exceptions;

public class BookingLimitReachedException : Exception
{
	private int _limit;

	public override string Message => $"Booking limit has been reached ({_limit})";

	public BookingLimitReachedException(int limit)
		: base($"Booking limit has been reached ({limit})")
	{
		_limit = limit;
	}

	public BookingLimitReachedException(int limit, string message)
		: base(message)
	{
		_limit = limit;
	}

	public BookingLimitReachedException(int limit, string message, Exception innerException)
		: base(message, innerException)
	{
		_limit = limit;
	}
}