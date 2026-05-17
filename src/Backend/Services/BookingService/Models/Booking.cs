using System;

namespace yandex_pract.Services.BookingService.Modesl;

public class Booking
{
	public Guid Id;
	public Guid EventId;
	public BookingStatus Status;
	public DateTime CreatedAt;
	public DateTime ProceedAt;

	public Booking(Guid eventId)
	{
		Id = Guid.NewGuid();
		EventId = eventId;
		CreatedAt = DateTime.UtcNow;
		Status = BookingStatus.Pending;	
	}
}