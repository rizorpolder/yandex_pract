using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService;

public interface IEventService
{
	bool AddEvent(EventModel eventModel);
	bool RemoveEvent(EventModel eventModel);
	bool TryUpdateEvent(EventModel eventModel);
}