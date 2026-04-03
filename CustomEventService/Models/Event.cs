using yandex_pract.CustomEventService.Dto;

namespace yandex_pract.CustomEventService.Models;

public class Event
{
	public Guid Id = Guid.NewGuid();
	public string Title;
	public string Description;

	public DateTime StartTime;
	public DateTime EndTime;

	public Event(EventDto dto)
	{
		Title = dto.Title;
		Description = dto.Description;
		StartTime = dto.StartTime;
		EndTime = dto.EndTime;
	}

	public void UpdateEvent(Event customEvent)
	{
		Title = customEvent.Title;
		Description = customEvent.Description;
		StartTime = customEvent.StartTime;
		EndTime = customEvent.EndTime;
	}
}