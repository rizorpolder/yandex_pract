namespace Contracts.Events;

public record SeatReservationResult
{
	public Guid BookingId;
	public Guid EventId;
	public bool Success;
	public string? FailReason;
	public DateTime ProcessedAt;
}