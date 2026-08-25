using EventsService.Domain.Models.Events;

namespace EventsService.Application.Services.Abstraction.Repositories;

public interface IEventRepository
{
	public Task<IReadOnlyList<Event>> GetAllEventsAsync();
	Task<Event?> GetByIdAsync(Guid id);

	public Task AddAsync(Event customEvent);
	public Task RemoveAsync(Event customEvent);

	public Task SaveChangesAsync();
}