namespace EventsService.Domain.Models;

public class ProcessedMessage
{
	public Guid BookingId { get; private set; }
	public DateTime ProcessedAt { get; private set; }

	private ProcessedMessage()
	{
	}

	public ProcessedMessage(Guid bookingId)
	{
		BookingId = bookingId;
		ProcessedAt = DateTime.UtcNow;
	}
}