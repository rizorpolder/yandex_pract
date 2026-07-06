using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;
using yandex_pract.Filters;
using yandex_pract.MockDB;

namespace yandex_pract.CustomEventService;

public class EventService(AppDbContext dbContext, EventFilterService filterService) : IEventService
{
	public IReadOnlyList<Event> GetEvents() => dbContext.Events.AsNoTracking().ToList();

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

		var events = dbContext.Events.AsNoTracking();

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
		dbContext.Add(customEvent);
		return dbContext.SaveChanges() > 0;
	}

	public bool RemoveEvent(Event customEvent)
	{
		dbContext.Remove(customEvent);
		return dbContext.SaveChanges() > 0;
	}

	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent)
	{
		var existing = dbContext.Events.FirstOrDefault(e => e.Id == modelId);

		if (existing is null)
			return (false, null);

		existing.UpdateEvent(newEvent);

		dbContext.SaveChanges();

		return (true, existing);
	}

	public (bool hasElement, Event? resultModel) GetEventById(Guid id)
	{
		var existing = dbContext.Events.FirstOrDefault(e => e.Id == id);
		return (existing is not null, existing);
	}

	public IReadOnlyList<Event> FilterEvents(
		string? title = null,
		DateTime? startDate = null,
		DateTime? endDate = null,
		int page = 1,
		int pageSize = 10)
	{
		IQueryable<Event> events = dbContext.Events.AsNoTracking();

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