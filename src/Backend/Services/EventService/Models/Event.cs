using System;
using yandex_pract.CustomEventService.Dto;

namespace yandex_pract.CustomEventService.Models;

public class Event : IEquatable<Event>
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

	#region Fluent Methods

	public Event SetGuid(Guid guid)
	{
		Id = guid;
		return this;
	}

	public Event SetTitle(string title)
	{
		Title = title;
		return this;
	}

	public Event SetDescription(string description)
	{
		Description = description;
		return this;
	}

	public Event SetStartAt(DateTime startAt)
	{
		StartAt = startAt;
		return this;
	}

	public Event SetEndAt(DateTime endAt)
	{
		EndAt = endAt;
		return this;
	}

	#endregion

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

	public bool Equals(Event? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return Id.Equals(other.Id) && Title == other.Title && Description == other.Description &&
		       StartAt.Equals(other.StartAt) && EndAt.Equals(other.EndAt);
	}
	
}