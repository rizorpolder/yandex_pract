using System;
using System.Collections.Generic;
using System.Linq;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.MockDB;

public class MockDB : ICustomDataBase
{
	private const int INITIAL_CAPACITY = 100;
	private List<Event> _events;

	public MockDB()
	{
		_events = new List<Event>();
		GenerateSomeEvents();
	}

	private void GenerateSomeEvents()
	{
		var rnd = new Random();
		var description = $"description_";

		for (int i = 0; i < INITIAL_CAPACITY; i++)
		{
			var now = DateTime.Now + TimeSpan.FromSeconds(rnd.Next(0, 128));
			var end = now.AddSeconds(rnd.Next(0, 128));
			var titleIdx = rnd.Next(0, INITIAL_CAPACITY);
			var title = $"event_name_{titleIdx}";
			var entity = new Event(title: $"{title}", description: $"{description}{i}", now, end);
			_events.Add(entity);
		}
	}

	public IReadOnlyList<Event> GetAllEvents()
	{
		return _events;
	}
	
	public bool TryAddEvent(Event customEvent)
	{
		if (_events.Contains(customEvent))
			return false;
		_events.Add(customEvent);
		return true;
	}

	public bool TryRemoveEvent(Event customEvent)
	{
		if (!_events.Contains(customEvent))
			return false;
		_events.Remove(customEvent);
		return true;
	}

	public (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent)
	{
		var modelResult = GetEventById(modelId);
		if (!modelResult.hasElement)
			return (hasElement: false, eventResult: null);

		modelResult.resultModel?.UpdateEvent(newEvent);
		return (hasElement: true, eventResult: modelResult.resultModel);
	}

	public (bool hasElement, Event? resultModel) GetEventById(Guid eventId)
	{
		var result = _events.FirstOrDefault(x => x.Id.Equals(eventId));
		return (result != null, result);
	}
}