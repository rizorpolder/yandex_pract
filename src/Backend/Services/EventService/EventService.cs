using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Filters;

namespace yandex_pract.CustomEventService;

public class EventService(IEventDataBase eventDataBase, EventFilterService filterService) : IEventService
{
	public async Task<IReadOnlyList<Event>> GetEvents() => await eventDataBase.GetAllEventsAsync();

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

		var events = (await eventDataBase.GetAllEventsAsync()).AsQueryable();

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
		return await eventDataBase.TryAddEventAsync(customEvent);
	}

	public async Task<bool> RemoveEvent(Event customEvent)
	{
		return await eventDataBase.TryRemoveEventAsync(customEvent);
	}

	public async Task<bool> TryUpdateEvent(Event newEvent)
	{
		return await eventDataBase.UpdateAsync(newEvent);
	}

	public async Task<(bool hasElement, Event? resultModel)> GetEventById(Guid id)
	{
		return await eventDataBase.GetEventByIdAsync(id);
	}

	public async Task<IReadOnlyList<Event>> FilterEvents(
		string? title = null,
		DateTime? startDate = null,
		DateTime? endDate = null,
		int page = 1,
		int pageSize = 10)
	{
		IQueryable<Event> events = (await eventDataBase.GetAllEventsAsync()).AsQueryable();

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