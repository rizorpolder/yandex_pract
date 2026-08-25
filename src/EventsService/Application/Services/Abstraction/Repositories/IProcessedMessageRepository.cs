namespace EventsService.Application.Services.Abstraction.Repositories;

public interface IProcessedMessageRepository
{
	Task<bool> IsProcessedAsync(Guid bookingId);
	Task MarkAsProcessedAsync(Guid bookingId);
}