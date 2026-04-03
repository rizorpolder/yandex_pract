using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public interface IEventService
{
	bool AddEvent(Event customEvent);
	bool RemoveEvent(Event customEvent);
	public  (bool hasElement, Event? eventResult) TryUpdateEvent(Guid modelId, Event newEvent);
	public IReadOnlyList<Event> GetEvents();
	public (bool hasElement, Event? resultModel) GetEventById(Guid id);
}