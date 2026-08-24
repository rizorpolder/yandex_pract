using System.Text.Json;
using BookingService.Application.Services.Abstraction.Services;
using BookingService.Infrastructure.Options;
using Confluent.Kafka;
using Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Messaging;

public class SeatsReservationResultConsumer(IServiceScopeFactory scopeFactory, IOptions<KafkaOptions> kafkaOptions)
	: BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig()
		{
			BootstrapServers = kafkaOptions.Value.BootstrapServers,
			GroupId = "booking-service-group",
			AutoOffsetReset = AutoOffsetReset.Earliest,
			EnableAutoCommit = false
		}).Build();

		consumer.Subscribe("seats-reservation-result");
		while (!stoppingToken.IsCancellationRequested)
		{
			var cr = consumer.Consume(stoppingToken);
			var result = JsonSerializer.Deserialize<SeatReservationResult>(cr.Message.Value)!;

			using var scope = scopeFactory.CreateScope();
			var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
			await bookingService.ApplyReservationResultAsync(result.BookingId, result.Success, result.FailReason);
			consumer.Commit(cr);
		}
	}
}