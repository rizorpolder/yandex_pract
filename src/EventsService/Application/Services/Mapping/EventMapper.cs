using Application.Services.EventService.Dto;
using Domain.Models.Events;

namespace Application.Services.Mapping;

internal static class EventMapper
{
	public static EventDto ToDto(Event evt)
	{
		return new EventDto
		{
			ID = evt.Id,
			Title = evt.Title,
			Description = evt.Description,
			StartAt = evt.StartAt,
			EndAt = evt.EndAt,
			TotalSeats = evt.TotalSeats,
			AvailableSeats = evt.AvailableSeats
		};
	}

	public static Event FromDto(EventDto dto)
	{
		return new Event(
			dto.Title,
			dto.Description,
			dto.StartAt,
			dto.EndAt,
			dto.TotalSeats
		);
	}
}