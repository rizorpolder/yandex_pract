namespace Domain.Models.Event;

public class Event : IEquatable<Event>
{
	public Guid Id { get; private set; } = Guid.NewGuid();
	public string Title { get; private set; }
	public string Description { get; private set; }
	public DateTime StartAt { get; private set; }
	public DateTime EndAt { get; private set; }

	public int TotalSeats { get; private set; }
	public int AvailableSeats { get; private set; }

	public List<Booking.Booking> Bookings { get; set; }

	private Event()
	{
	}

	public Event(string title, string description, DateTime startAt, DateTime endAt, int totalSeats)
	{
		Title = title;
		Description = description;
		StartAt = startAt;
		EndAt = endAt;
		TotalSeats = totalSeats;
		AvailableSeats = totalSeats;
	}

	#region Fluent Methods

	public Event SetGuid(Guid guid)
	{
		Id = guid;
		return this;
	}

	public Event SetTitle(string title)
	{
		Title = title;
		return this;
	}

	public Event SetDescription(string description)
	{
		Description = description;
		return this;
	}

	public Event SetStartAt(DateTime startAt)
	{
		StartAt = startAt;
		return this;
	}

	public Event SetEndAt(DateTime endAt)
	{
		EndAt = endAt;
		return this;
	}

	#endregion

	// public Event(EventDto dto)
	// {
	// 	Title = dto.Title;
	// 	Description = dto.Description;
	// 	StartAt = dto.StartAt;
	// 	EndAt = dto.EndAt;
	// 	TotalSeats = dto.TotalSeats.Value;
	// 	AvailableSeats = TotalSeats;
	// }

	public void UpdateEvent(Event customEvent)
	{
		Title = customEvent.Title;
		Description = customEvent.Description;
		StartAt = customEvent.StartAt;
		EndAt = customEvent.EndAt;
		AvailableSeats = customEvent.AvailableSeats;
		TotalSeats = customEvent.TotalSeats;
	}

	public bool TryReserveSeats(int count = 1)
	{
		if (AvailableSeats - count < 0) return false;
		AvailableSeats -= count;
		return true;
	}

	public void ReleaseSeats(int count = 1)
	{
		AvailableSeats += count;
	}

	public bool Equals(Event? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return Id.Equals(other.Id) && Title == other.Title && Description == other.Description &&
		       StartAt.Equals(other.StartAt) && EndAt.Equals(other.EndAt);
	}
}