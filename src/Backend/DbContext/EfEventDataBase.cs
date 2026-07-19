using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext.Interfaces;

namespace yandex_pract.DbContext;

public class EfEventDataBase : IEventDataBase
{
	private readonly AppDbContext _dbContext;

	public EfEventDataBase(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<IReadOnlyList<Event>> GetAllEventsAsync()
	{
		return await _dbContext.Events.AsNoTracking().ToListAsync();
	}

	public async Task<bool> TryAddEventAsync(Event customEvent)
	{
		_dbContext.Events.Add(customEvent);
		return await _dbContext.SaveChangesAsync() > 0;
	}

	public async Task<bool> TryRemoveEventAsync(Event customEvent)
	{
		var existing = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == customEvent.Id);
		if (existing is null) return false;

		_dbContext.Events.Remove(existing);
		return await _dbContext.SaveChangesAsync() > 0;
	}

	public async Task<(bool hasElement, Event? eventResult)> TryUpdateEventAsync(Guid modelId, Event newEvent)
	{
		var existing = await _dbContext.Events
			.FirstOrDefaultAsync(e => e.Id == modelId);

		if (existing is null)
			return (false, null);

		existing.UpdateEvent(newEvent);

		await _dbContext.SaveChangesAsync();

		return (true, existing);
	}

	public async Task<(bool hasElement, Event? resultModel)> GetEventByIdAsync(Guid eventId)
	{
		var element = await _dbContext.Events
			.AsNoTracking()
			.FirstOrDefaultAsync(e => e.Id == eventId);

		return (element != null, element);
	}

	public async Task UpdateAsync(Event evt)
	{
		_dbContext.Events.Update(evt);
		await _dbContext.SaveChangesAsync();
	}
}