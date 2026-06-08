using System;
using System.Collections.Generic;
using System.Linq;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;
using yandex_pract.Filters;
using yandex_pract.MockDB;

namespace yandex_pract.CustomEventService;

public class EventService(IEventDataBase db, EventFilterService filterService) : IEventService
{
	public IReadOnlyList<Event> GetEvents() => db.GetAllEvents();

	public PaginatedResultDto GetEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
	{
		var query = new EventQuery
		{
			Title = title,
			From = from,
			To = to,
			Page = page,
			PageSize = pageSize
		};

		var events = db.GetAllEvents().AsQueryable();

		events = filterService.ApplyFilters(events, query);

		var totalCount = events.Count();

		events = filterService.ApplyPagination(events, query);

		var dtoList = events
			.Select(e => new EventDto(e))
			.ToList();

		return new PaginatedResultDto(dtoList, page, totalCount);
	}

	public bool CreateEventAsync(Event customEvent)
	{
		return db.TryAddEvent(customEvent);
	}

	public bool RemoveEvent(Event customEvent)
	{
		return db.TryRemoveEvent(customEvent);
	}

	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent)
	{
		return db.TryUpdateEvent(modelId, newEvent);
	}

	public (bool hasElement, Event? resultModel) GetEventById(Guid id)
	{
		return db.GetEventById(id);
	}

	public IReadOnlyList<Event> FilterEvents(
		string? title = null,
		DateTime? startDate = null,
		DateTime? endDate = null,
		int page = 1,
		int pageSize = 10)
	{
		var events = db.GetAllEvents().AsQueryable();

		if (!string.IsNullOrWhiteSpace(title))
			events = events.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

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