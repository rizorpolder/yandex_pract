using System;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public interface IEventService
{
	bool AddEvent(Event customEvent);
	bool RemoveEvent(Event customEvent);
	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent);
	public PaginatedResultDto GetEvents(string? title, DateTime? from, DateTime? to, int page = 1, int pageSize = 10);
	public (bool hasElement, Event? resultModel) GetEventById(Guid id);
}