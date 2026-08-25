namespace Contracts.Events;

public record BookingConfirmed(Guid BookingId,
	Guid EventId,
	Guid UserId,
	int SeatsCount,
	DateTime ConfirmedAt);