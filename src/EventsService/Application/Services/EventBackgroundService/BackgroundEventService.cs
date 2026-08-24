using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Services.EventBackgroundService;

internal class BackgroundEventService(IServiceScopeFactory scopeFactory) : BackgroundService
{
	private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
	}
}