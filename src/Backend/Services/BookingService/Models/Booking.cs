using System;

namespace yandex_pract.Services.BookingService.Models;

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

	public void Confirm()
	{
		Status = BookingStatus.Confirmed;
		ProceedAt = DateTime.UtcNow;
	}

	public void Reject()
	{
		Status = BookingStatus.Rejected;
	}
}