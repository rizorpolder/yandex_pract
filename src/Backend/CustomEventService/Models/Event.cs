using System;
using yandex_pract.CustomEventService.Dto;

namespace yandex_pract.CustomEventService.Models;

public class Event
{
	public Guid Id = Guid.NewGuid();
	public string Title;
	public string Description;

	public DateTime StartAt;
	public DateTime EndAt;

	public Event(string title, string description, DateTime startAt, DateTime endAt)
	{
		Title = title;
		Description = description;
		StartAt = startAt;
		EndAt = endAt;
	}

	public Event(EventDto dto)
	{
		Title = dto.Title;
		Description = dto.Description;
		StartAt = dto.StartAt;
		EndAt = dto.EndAt;
	}

	public void UpdateEvent(Event customEvent)
	{
		Title = customEvent.Title;
		Description = customEvent.Description;
		StartAt = customEvent.StartAt;
		EndAt = customEvent.EndAt;
	}
}