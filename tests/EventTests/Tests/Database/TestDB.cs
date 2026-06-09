using System.Diagnostics;
using System.Text.Json;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace TestProject.Tests.Database;

public class TestDB : IEventDataBase, IBookingDataBase
{
	private readonly List<Event> _events = new();
	private readonly Dictionary<Guid, Booking> _bookings = new();

	public IReadOnlyList<Event> GetAllEvents() => _events;

	public bool TryAddEvent(Event evt)
	{
		if (_events.Any(e => e.Id == evt.Id))
			return false;

		_events.Add(evt);
		return true;
	}

	public bool TryRemoveEvent(Event evt)
	{
		return _events.Remove(evt);
	}

	public (bool hasElement, Event? resultModel) GetEventById(Guid eventId)
	{
		var evt = _events.FirstOrDefault(e => e.Id == eventId);
		return (evt != null, evt);
	}

	public void Update(Event evt)
	{
		var (found, existing) = GetEventById(evt.Id);
		if (found)
			existing.UpdateEvent(evt);
	}

	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid id, Event newEvent)
	{
		var (found, existing) = GetEventById(id);
		if (!found)
			return (false, null);

		existing.UpdateEvent(newEvent);
		return (true, existing);
	}

	public void Enqueue(Booking booking)
	{
		_bookings[booking.Id] = booking;
	}

	public bool TryFindBooking(Guid bookingId, out Booking result)
	{
		return _bookings.TryGetValue(bookingId, out result);
	}

	public List<Booking> GetPending()
	{
		return _bookings.Values
			.Where(b => b.Status == BookingStatus.Pending)
			.ToList();
	}

	public void UpdateBooking(Booking booking)
	{
		_bookings[booking.Id] = booking;
	}
}