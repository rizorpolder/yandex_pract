using System;
using System.Collections.Generic;
using System.Linq;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;
using yandex_pract.MockDB;

namespace yandex_pract.CustomEventService;

public class EventService(ICustomDataBase db) : IEventService
{
	public IReadOnlyList<Event> GetEvents() => db.GetAllEvents();

	public PaginatedResultDto GetEvents(string? title, DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
	{
		IEnumerable<Event> result = GetEvents();

		if (title != null)
		{
			result = result.Where(x => x.Title.Equals(title));
		}

		//TODO в ТЗ не указано что нужно Валидировать переданные параметры. 

		if (from.HasValue)
		{
			result = result.Where(x => x.StartAt > from);
		}

		if (to.HasValue)
		{
			result = result.Where(x => x.EndAt > to);
		}

		return GetEvents(result, page, pageSize);
	}

	private PaginatedResultDto GetEvents(IEnumerable<Event> source, int page = 1, int pageSize = 10)
	{
		var filteredDto = source
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(eventItem => new EventDto(eventItem))
			.ToList();

		return new PaginatedResultDto(filteredDto, page, pageSize);
	}

	public bool AddEvent(Event customEvent)
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
}