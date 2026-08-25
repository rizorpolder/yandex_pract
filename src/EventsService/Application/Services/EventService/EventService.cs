using Application.Services.Abstraction.RequestResult;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.Abstraction.Services;
using EventsService.Application.Services.EventService.Dto;
using EventsService.Application.Services.Filters;
using EventsService.Application.Services.Mapping;
using EventsService.Domain.Models.Events;

namespace EventsService.Application.Services.EventService;

public class EventService(IEventRepository eventRepository, EventFilterService filterService) : IEventService
{
	public async Task<IReadOnlyList<Event>> GetEvents() => await eventRepository.GetAllEventsAsync();

	public async Task<Result<EventDto>> CreateEventAsync(EventDto eventDto)
	{
		var evt = EventMapper.FromDto(eventDto);
		try
		{
			await eventRepository.AddAsync(evt);
			await eventRepository.SaveChangesAsync();
		}
		catch (Exception e)
		{
			return Result<EventDto>.Failure(e.Message);
		}

		return Result<EventDto>.Success(EventMapper.ToDto(evt));
	}

	public async Task<Result<EventDto>> RemoveEvent(EventDto eventDto)
	{
		var evt = await eventRepository.GetByIdAsync(eventDto.ID);
		if (evt == null)
			return Result<EventDto>.Failure("NotFound");

		try
		{
			await eventRepository.RemoveAsync(evt);
			await eventRepository.SaveChangesAsync();
		}
		catch (Exception e)
		{
			return Result<EventDto>.Failure(e.Message);
		}

		return Result<EventDto>.Success(EventMapper.ToDto(evt));
	}

	public async Task<Result<EventDto>> UpdateEventAsync(Guid id, EventDto dto)
	{
		var evt = await eventRepository.GetByIdAsync(id);
		if (evt == null)
			return Result<EventDto>.Failure("NotFound");
		evt.UpdateEvent(EventMapper.FromDto(dto));
		try
		{
			await eventRepository.SaveChangesAsync();
		}
		catch (Exception e)
		{
			return Result<EventDto>.Failure("Database update failed: " + e.Message);
		}

		return Result<EventDto>.Success(EventMapper.ToDto(evt));
	}

	public async Task<PaginatedResultDto> GetEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
	{
		var query = new EventQuery
		{
			Title = title,
			From = from,
			To = to,
			Page = page,
			PageSize = pageSize
		};

		var events = (await eventRepository.GetAllEventsAsync()).AsQueryable();

		events = filterService.ApplyFilters(events, query);

		var totalCount = events.Count();

		events = filterService.ApplyPagination(events, query);

		var dtoList = events
			.Select(e => EventMapper.ToDto(e))
			.ToList();

		return new PaginatedResultDto(dtoList, page, totalCount);
	}

	public async Task<Result<EventDto>> GetEventById(Guid id)
	{
		var evt = await eventRepository.GetByIdAsync(id);
		if (evt == null)
			return Result<EventDto>.Failure("NotFound");
		return Result<EventDto>.Success(EventMapper.ToDto(evt));
	}

	public async Task<Result<bool>> DecreaseAvailableSeatsAsync(Guid eventId, int seatsCount)
	{
		var evt = await eventRepository.GetByIdAsync(eventId);
		if (evt is null)
			return Result<bool>.Failure("EventNotFound");
		
		if(!evt.TryReserveSeats(seatsCount))
			return Result<bool>.Failure("NoAvailableSeats");

		await eventRepository.SaveChangesAsync();

		return Result<bool>.Success(true);
	}
}