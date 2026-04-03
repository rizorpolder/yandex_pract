using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public interface IEventService
{
	bool AddEvent(EventModel eventModel);
	bool RemoveEvent(EventModel eventModel);
	public bool TryUpdateEvent(Guid modelId, EventModel newEventModel);
	public IReadOnlyList<EventModel> GetEvents();
	public (bool hasElement, EventModel? resultModel) GetEventById(Guid id);
}