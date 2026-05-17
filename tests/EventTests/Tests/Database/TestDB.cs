using System.Diagnostics;
using System.Text.Json;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace TestProject.Tests.Database;

public class TestDB : IEventDataBase, IBookingDataBase
{
	private List<Event> _events = new List<Event>();
	private Dictionary<Guid, Booking> _bookings = new Dictionary<Guid, Booking>();

	public TestDB()
	{
		LoadData();
	}

	private void LoadData()
	{
		var projectRoot = Path.GetFullPath(
			Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
		);

		var fullPath = Path.Combine(projectRoot, "Tests", "Database", "MockDB.json");

		if (!File.Exists(fullPath))
		{
			Debug.WriteLine("MockDB.json not found");
			return;
		}

		var file = File.ReadAllText(fullPath);
		List<EventDto>? eventsDto = JsonSerializer.Deserialize<List<EventDto>>(file);
		if (eventsDto == null)
		{
			Debug.WriteLine("MockDB.json is broken");
			return;
		}

		foreach (var dto in eventsDto)
		{
			var evt = new Event(dto)
			{
				Id = dto.ID
			};

			_events.Add(evt);
		}
	}

	public IReadOnlyList<Event> GetAllEvents()
	{
		return _events;
	}

	public bool TryAddEvent(Event customEvent)
	{
		if (_events.Contains(customEvent))
			return false;
		_events.Add(customEvent);
		return true;
	}

	public bool TryRemoveEvent(Event customEvent)
	{
		if (!_events.Contains(customEvent))
			return false;
		_events.Remove(customEvent);
		return true;
	}

	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent)
	{
		var modelResult = GetEventById(modelId);
		if (!modelResult.hasElement)
			return (hasElement: false, eventResult: null);

		modelResult.resultModel?.UpdateEvent(newEvent);
		return (hasElement: true, eventResult: modelResult.resultModel);
	}

	public (bool hasElement, Event? resultModel) GetEventById(Guid eventId)
	{
		var result = _events.FirstOrDefault(x => x.Id.Equals(eventId));
		return (result != null, result);
	}


	public Booking Dequeue()
	{
		return null;
	}

	public void Enqueue(Booking booking)
	{
		_bookings.TryAdd(booking.Id, booking);
	}

	public bool TryFindBooking(Guid bookingId, out Booking result)
	{
		return _bookings.TryGetValue(bookingId, out result);
	}
}