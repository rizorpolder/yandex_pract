using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.DbContext.Interfaces;

public interface IEventDataBase
{
	public Task<IReadOnlyList<Event>> GetAllEventsAsync();
	public Task<bool> TryAddEventAsync(Event customEvent);
	public Task<bool> TryRemoveEventAsync(Event customEvent);
	public Task<(bool hasElement, Event? eventResult)> TryUpdateEventAsync(Guid modelId, Event newEvent);

	public Task<(bool hasElement, Event? resultModel)> GetEventByIdAsync(Guid eventId);

	public Task<bool> UpdateAsync(Event evt);
}