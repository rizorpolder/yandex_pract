namespace Domain.Models.Bookings;

public class Booking
{
	public Guid Id;
	public Guid EventId;
	public BookingStatus Status;
	public DateTime CreatedAt;
	public DateTime ProcessedAt;
	public Guid UserId;
	public Events.Event Event { get; set; }

	private Booking()
	{
	}

	public Booking(Guid eventId, Guid userID)
	{
		Id = Guid.NewGuid();
		UserId = userID;
		EventId = eventId;
		CreatedAt = DateTime.UtcNow;
		Status = BookingStatus.Pending;
	}

	public void Confirm()
	{
		Status = BookingStatus.Confirmed;
		ProcessedAt = DateTime.UtcNow;
	}

	public void Reject()
	{
		Status = BookingStatus.Rejected;
		ProcessedAt = DateTime.UtcNow;
	}

	public void Cancel()
	{
		if (!BookingStatus.OnProcess.HasFlag(Status))
			return;

		Status = BookingStatus.Cancelled;
		ProcessedAt = DateTime.UtcNow;
	}
}