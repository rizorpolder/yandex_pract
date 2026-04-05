using yandex_pract.CustomEventService.Dto;

namespace yandex_pract.CustomEventService.Models;

public class Event
{
	public Guid Id = Guid.NewGuid();
	public string Title;
	public string Description;

	public DateTime StartAt;
	public DateTime EndAd;

	public Event(EventDto dto)
	{
		Title = dto.Title;
		Description = dto.Description;
		StartAt = dto.StartAt;
		EndAd = dto.EndAt;
	}

	public void UpdateEvent(Event customEvent)
	{
		Title = customEvent.Title;
		Description = customEvent.Description;
		StartAt = customEvent.StartAt;
		EndAd = customEvent.EndAd;
	}
}