using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Domain.Models;
using EventsService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Repositories;

public class EfProcessedMessageRepository(AppDbContext context) : IProcessedMessageRepository
{
	public async Task<bool> IsProcessedAsync(Guid bookingId)
	{
		return await context.ProcessedMessages.AnyAsync(x => x.BookingId == bookingId);
	}

	public async Task MarkAsProcessedAsync(Guid bookingId)
	{
		context.ProcessedMessages.Add(new ProcessedMessage(bookingId));
		await context.SaveChangesAsync();
	}
}