using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Domain.Models.Events;
using EventsService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Repositories;

public class EfEventRepository(AppDbContext dbContext) : IEventRepository
{
	public async Task<IReadOnlyList<Event>> GetAllEventsAsync()
	{
		return await dbContext.Events.AsNoTracking().ToListAsync();
	}

	public Task<Event?> GetByIdAsync(Guid id)
	{
		return dbContext.Events.FirstOrDefaultAsync(x => x.Id.Equals(id));
	}

	public async Task AddAsync(Event customEvent)
	{
		await dbContext.Events.AddAsync(customEvent);
	}

	public Task RemoveAsync(Event customEvent)
	{
		dbContext.Events.Remove(customEvent);
		return Task.CompletedTask;
	}

	public Task SaveChangesAsync()
	{
		return dbContext.SaveChangesAsync();
	}
}