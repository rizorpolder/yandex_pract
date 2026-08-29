using Application.Services.Abstraction.RequestResult;
using EventsService.Application.Services.Abstraction.Caching;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.Abstraction.Services;
using EventsService.Application.Services.Caching;
using EventsService.Application.Services.EventService.Dto;
using EventsService.Application.Services.Filters;
using EventsService.Application.Services.Mapping;
using EventsService.Application.Services.Options;
using Microsoft.Extensions.Options;

namespace EventsService.Application.Services.EventService;

public class EventService(
	IEventRepository eventRepository,
	EventFilterService filterService,
	ICacheService cache,
	IOptions<CacheOptions> cacheOptions)
	: IEventService
{
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

	public async Task<IReadOnlyList<EventDto>> GetTopEventsAsync()
	{
		var cached = await cache.GetAsync<List<EventDto>>(CacheKeys.TopEvents);
		if (cached is not null)
			return cached;

		var events = await eventRepository.GetTopEventsAsync(10);
		var dtos = events.Select(EventMapper.ToDto).ToList();

		await cache.SetAsync(CacheKeys.TopEvents, dtos, TimeSpan.FromSeconds(cacheOptions.Value.TopEventsTtlSeconds));

		return dtos;
	}

	public async Task<Result<EventDto>> GetEventById(Guid id)
	{
		var cached = await cache.GetAsync<EventDto>(CacheKeys.Event(id));
		if (cached is not null)
			return Result<EventDto>.Success(cached);

		var evt = await eventRepository.GetByIdAsync(id);
		if (evt is null)
			return Result<EventDto>.Failure("NotFound");

		var dto = EventMapper.ToDto(evt);
		await cache.SetAsync(CacheKeys.Event(id), dto, TimeSpan.FromSeconds(cacheOptions.Value.EventTtlSeconds));

		return Result<EventDto>.Success(dto);
	}

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

		await cache.RemoveAsync(CacheKeys.Event(eventDto.ID));
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

		await cache.RemoveAsync(CacheKeys.Event(id));
		return Result<EventDto>.Success(EventMapper.ToDto(evt));
	}

	public async Task<Result<bool>> DecreaseAvailableSeatsAsync(Guid eventId, int seatsCount)
	{
		var evt = await eventRepository.GetByIdAsync(eventId);
		if (evt is null)
			return Result<bool>.Failure("NotFound");

		if (!evt.TryReserveSeats(seatsCount))
			return Result<bool>.Failure("NoAvailableSeats");


		await eventRepository.SaveChangesAsync();
		await cache.RemoveAsync(CacheKeys.Event(eventId));

		return Result<bool>.Success(true);
	}
}