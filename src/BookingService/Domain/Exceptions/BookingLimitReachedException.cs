namespace BookingService.Domain.Exceptions;

public sealed class BookingLimitReachedException(int limit) : Exception($"Booking limit has been reached ({limit})")
{
	public int Limit { get; } = limit;
}