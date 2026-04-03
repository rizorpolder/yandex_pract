using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public class EventService : IEventService
{
	private static readonly List<EventModel> _evenst = new();

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

	public bool TryUpdateEvent(EventModel eventModel)
	{
		var result = _evenst.FirstOrDefault(x => x.ID.Equals(eventModel.ID));
		if (result == null) return false;

		result.UpdateEvent(eventModel);
		return true;
	}
}