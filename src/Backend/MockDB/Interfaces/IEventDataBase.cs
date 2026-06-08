using System;
using System.Collections.Generic;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.MockDB;

public interface IEventDataBase
{
	public IReadOnlyList<Event> GetAllEvents();
	public bool TryAddEvent(Event customEvent);
	public bool TryRemoveEvent(Event customEvent);
	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent);

	public (bool hasElement, Event? resultModel) GetEventById(Guid eventId);

	void Update(Event evt);
}