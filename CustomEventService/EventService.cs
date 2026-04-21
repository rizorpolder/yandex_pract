using System;
using System.Collections.Generic;
using System.Linq;
using yandex_pract.CustomEventService.Models;
using yandex_pract.MockDB;

namespace yandex_pract.CustomEventService;

public class EventService : IEventService
{
	private readonly ICustomDataBase _db;

	public EventService(ICustomDataBase db)
	{
		_db = db;
	}

	public IReadOnlyList<Event> GetEvents() => _db.GetAllEvents();

	public IReadOnlyList<Event> GetEvents(string? title, DateTime? from, DateTime? to)
	{
		return _db.GetFilteredEvents(title, from, to);
	}

	public bool AddEvent(Event customEvent)
	{
		return _db.TryAddEvent(customEvent);
	}

	public bool RemoveEvent(Event customEvent)
	{
		return _db.TryRemoveEvent(customEvent);
	}

	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent)
	{
		return _db.TryUpdateEvent(modelId, newEvent);
	}

	public (bool hasElement, Event? resultModel) GetEventById(Guid id)
	{
		return _db.GetEventById(id);
	}
}