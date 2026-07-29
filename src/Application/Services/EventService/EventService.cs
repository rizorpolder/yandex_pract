using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Domain.Models.Event;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.Filters;

namespace yandex_pract.CustomEventService;

public class EventService(IEventRepository eventRepository, EventFilterService filterService) : IEventService
{
	public async Task<IReadOnlyList<Event>> GetEvents() => await eventRepository.GetAllEventsAsync();

	public async Task<PaginatedResultDto> GetEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
	{
		var query = new EventQuery
		{
			Title = title,
			From = from,
			To = to,
			Page = page,
			PageSize = pageSize
		};

		var events = (await eventRepository.GetAllEventsAsync()).AsQueryable();

		events = filterService.ApplyFilters(events, query);

		var totalCount = events.Count();

		events = filterService.ApplyPagination(events, query);

		var dtoList = events
			.Select(e => new EventDto(e))
			.ToList();

		return new PaginatedResultDto(dtoList, page, totalCount);
	}

	public async Task<bool> CreateEventAsync(Event customEvent)
	{
		return await eventRepository.TryAddEventAsync(customEvent);
	}

	public async Task<bool> RemoveEvent(Event customEvent)
	{
		return await eventRepository.TryRemoveEventAsync(customEvent);
	}

	public async Task<bool> TryUpdateEvent(Event newEvent)
	{
		return await eventRepository.UpdateAsync(newEvent);
	}

	public async Task<(bool hasElement, Event? resultModel)> GetEventById(Guid id)
	{
		return await eventRepository.GetEventByIdAsync(id);
	}

	public async Task<IReadOnlyList<Event>> FilterEvents(
		string? title = null,
		DateTime? startDate = null,
		DateTime? endDate = null,
		int page = 1,
		int pageSize = 10)
	{
		IQueryable<Event> events = (await eventRepository.GetAllEventsAsync()).AsQueryable();

		if (!string.IsNullOrWhiteSpace(title))
			events = events.Where(e => e.Title.Contains(title));

		if (startDate.HasValue)
			events = events.Where(e => e.StartAt >= startDate.Value);

		if (endDate.HasValue)
			events = events.Where(e => e.EndAt <= endDate.Value);

		return events
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();
	}
}