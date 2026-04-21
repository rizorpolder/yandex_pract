using System;
using System.Collections.Generic;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public interface IEventService
{
	bool AddEvent(Event customEvent);
	bool RemoveEvent(Event customEvent);
	public  (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent);
	// public IReadOnlyList<Event> GetEvents();
	public IReadOnlyList<Event> GetEvents(string? title, DateTime? from, DateTime? to);
	public (bool hasElement, Event? resultModel) GetEventById(Guid id);
}