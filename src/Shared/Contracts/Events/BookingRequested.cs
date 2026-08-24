namespace Contracts.Events;

public record BookingRequested(Guid BookingId, Guid EventId, DateTime UtcNow)
{
	public Guid BookingId;
	public Guid EventId;
	public DateTime RequestedAt;
}