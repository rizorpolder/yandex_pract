namespace BookingService.Domain.Models.BookingModel;

public class BookingModel
{
	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime ProcessedAt { get; private set; }
	public Guid UserId { get; private set; }

	private BookingModel()
	{
	}

	public BookingModel(Guid eventId, Guid userId)
	{
		Id = Guid.NewGuid();
		UserId = userId;
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
		if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
			return;

		Status = BookingStatus.Cancelled;
		ProcessedAt = DateTime.UtcNow;
	}
}