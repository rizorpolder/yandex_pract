using Application.Services.Abstraction.RequestResult;
using Application.Services.EventService.Dto;

namespace Application.Services.Abstraction.Services;

public interface IEventService
{
	Task<Result<EventDto>> CreateEventAsync(EventDto eventDto);
	Task<Result<EventDto>> RemoveEvent(EventDto eventDto);
	public Task<Result<EventDto>> UpdateEventAsync(Guid id, EventDto dto);

	public Task<PaginatedResultDto> GetEvents(string? title, DateTime? from, DateTime? to, int page = 1,
		int pageSize = 10);

	public Task<Result<EventDto>> GetEventById(Guid id);
}