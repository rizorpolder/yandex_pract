using Domain.Models.Event;
using yandex_pract.CustomEventService.Dto;

namespace Application.Services.Abstraction.Services;

public interface IEventService
{
	Task<bool> CreateEventAsync(Event customEvent);
	Task<bool> RemoveEvent(Event customEvent);
	public Task<bool> TryUpdateEvent(Event newEvent);

	public Task<PaginatedResultDto> GetEvents(string? title, DateTime? from, DateTime? to, int page = 1,
		int pageSize = 10);

	public Task<(bool hasElement, Event? resultModel)> GetEventById(Guid id);
}