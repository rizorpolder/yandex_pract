using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext.Interfaces;

namespace yandex_pract.DbContext;

public class EfEventRepository : IEventRepository
{
	private readonly AppDbContext _dbContext;

	public EfEventRepository(AppDbContext dbContext)
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
		var result = await _dbContext.SaveChangesAsync() > 0;
		_dbContext.Entry(customEvent).State = EntityState.Detached;
		return result;
	}

	public async Task<bool> TryRemoveEventAsync(Event customEvent)
	{
		var existing = await _dbContext.Events.FindAsync(customEvent.Id);
		if (existing is null) return false;

		_dbContext.Events.Remove(existing);
		return await _dbContext.SaveChangesAsync() > 0;
	}

	public async Task<(bool hasElement, Event? eventResult)> TryUpdateEventAsync(Guid modelId, Event newEvent)
	{
		var existing = await _dbContext.Events.FindAsync(modelId);

		if (existing is null)
			return (false, null);

		existing.UpdateEvent(newEvent);

		bool isUpdated = await _dbContext.SaveChangesAsync() > 0;
		_dbContext.Entry(existing).State = EntityState.Detached;

		return (true, existing);
	}

	public async Task<(bool hasElement, Event? resultModel)> GetEventByIdAsync(Guid eventId)
	{
		var element = await _dbContext.Events
			.AsNoTracking()
			.FirstOrDefaultAsync(e => e.Id == eventId);

		return (element != null, element);
	}

	public async Task<bool> UpdateAsync(Event evt)
	{
		bool isUpdated = false;

		var existing = await _dbContext.Events.FindAsync(evt.Id);
		if (existing is null) return isUpdated;


		_dbContext.Entry(existing).CurrentValues.SetValues(evt);
		_dbContext.Update(existing);
		isUpdated = await _dbContext.SaveChangesAsync() > 0;
		_dbContext.Entry(existing).State = EntityState.Detached;
		return isUpdated;
	}
}