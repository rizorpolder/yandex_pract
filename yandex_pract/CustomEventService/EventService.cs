using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public class EventService : IEventService
{
	private static readonly List<Event> _evenst = new();

	public IReadOnlyList<Event> GetEvents() => _evenst;

	public bool AddEvent(Event customEvent)
	{
		if (_evenst.Contains(customEvent))
			return false;
		_evenst.Add(customEvent);
		return true;
	}

	public bool RemoveEvent(Event customEvent)
	{
		if (!_evenst.Contains(customEvent))
			return false;
		_evenst.Remove(customEvent);
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

	public (bool hasElement, Event? resultModel) GetEventById(Guid id)
	{
		var result = _evenst.FirstOrDefault(x => x.Id.Equals(id));
		return (result != null, result);
	}
}