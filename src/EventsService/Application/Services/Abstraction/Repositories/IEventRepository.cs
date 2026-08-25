using EventsService.Domain.Models.Events;

namespace EventsService.Application.Services.Abstraction.Repositories;

public interface IEventRepository
{
	Task<Event?> GetByIdAsync(Guid id);
	Task<IReadOnlyList<Event>> GetAllEventsAsync();
	Task<List<Event>> GetTopEventsAsync(int count);
	Task AddAsync(Event evt);
	Task RemoveAsync(Event evt);
	Task SaveChangesAsync();

}