using Application.Services.EventService.Dto;
using Domain.Models.Events;

namespace Application.Services.Filters;

public class EventFilterService
{
	public IQueryable<Event> ApplyFilters(IQueryable<Event> events, EventQuery query)
	{
		if (!string.IsNullOrWhiteSpace(query.Title))
			events = events.Where(e =>
				e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));

		if (query.From.HasValue)
			events = events.Where(e => e.StartAt >= query.From.Value);

		if (query.To.HasValue)
			events = events.Where(e => e.EndAt <= query.To.Value);

		return events;
	}

	public IQueryable<Event> ApplyPagination(IQueryable<Event> events, EventQuery query)
	{
		return events
			.Skip((query.Page - 1) * query.PageSize)
			.Take(query.PageSize);
	}
}