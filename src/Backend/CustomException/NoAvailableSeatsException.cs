using System;

namespace yandex_pract.CustomException;

public class NoAvailableSeatsException : Exception
{
	public override string Message => "No available seats for this event";

	public NoAvailableSeatsException() : base()
	{
	}

	public NoAvailableSeatsException(string message) : base(message)
	{
	}

	public NoAvailableSeatsException(string message, Exception inner) : base(message, inner)
	{
	}
}