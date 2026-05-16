using System;

namespace yandex_pract.CustomException;

public class NotFoundException : Exception
{
	public NotFoundException() : base()
	{
	}

	public NotFoundException(string message) : base(message)
	{
	}

	public NotFoundException(string message, Exception inner) : base(message, inner)
	{
	}
}