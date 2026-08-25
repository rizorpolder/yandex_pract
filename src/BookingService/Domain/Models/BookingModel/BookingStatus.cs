namespace BookingService.Domain.Models.BookingModel;

public enum BookingStatus
{
	Pending = 1 << 1,
	Confirmed = 1 << 2,
	Rejected = 1 << 3,
	Cancelled = 1 << 4,
}