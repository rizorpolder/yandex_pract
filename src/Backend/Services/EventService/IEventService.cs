using System;
using System.Threading.Tasks;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public interface IEventService
{
	Task<bool> CreateEventAsync(Event customEvent);
	Task<bool> RemoveEvent(Event customEvent);
	public Task<(bool hasElement, Event? eventResult)> TryUpdateEvent(Guid modelId, Event newEvent);

	public Task<PaginatedResultDto> GetEvents(string? title, DateTime? from, DateTime? to, int page = 1,
		int pageSize = 10);

	public Task<(bool hasElement, Event? resultModel)> GetEventById(Guid id);
}