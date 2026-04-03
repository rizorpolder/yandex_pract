using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public class EventService : IEventService
{
	private static readonly List<EventModel> _evenst = new();

	public IReadOnlyList<EventModel> GetEvents() => _evenst;

	public bool AddEvent(EventModel eventModel)
	{
		if (_evenst.Contains(eventModel))
			return false;
		_evenst.Add(eventModel);
		return true;
	}

	public bool RemoveEvent(EventModel eventModel)
	{
		if (!_evenst.Contains(eventModel))
			return false;
		_evenst.Remove(eventModel);
		return true;
	}

	public bool TryUpdateEvent(Guid modelId, EventModel newEventModel)
	{
		var modelResult = GetEventById(modelId);
		if(!modelResult.hasElement)
			return false;
		
		modelResult.resultModel?.UpdateEvent(newEventModel);
		return true;
	}

	public (bool hasElement, EventModel? resultModel) GetEventById(Guid id)
	{
		var result = _evenst.FirstOrDefault(x => x.ID.Equals(id));
		return (result != null, result);
	}
}