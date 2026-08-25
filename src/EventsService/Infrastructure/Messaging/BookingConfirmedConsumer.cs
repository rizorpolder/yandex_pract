using System.Text.Json;
using Confluent.Kafka;
using Contracts.Events;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.Abstraction.Services;
using EventsService.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventsService.Infrastructure.Messaging;

public class BookingConfirmedConsumer(
	IServiceScopeFactory scopeFactory,
	IOptions<KafkaOptions> kafkaOptions,
	ILogger<BookingConfirmedConsumer> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var consumer = BuildConsumer();
		consumer.Subscribe(KafkaTopics.BookingConfirmed);

		while (!stoppingToken.IsCancellationRequested)
		{
			ConsumeResult<string, string>? consumeResult;
			try
			{
				consumeResult = consumer.Consume(stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (ConsumeException ex)
			{
				logger.LogError(ex, "Ошибка при чтении сообщения из топика {Topic}", KafkaTopics.BookingConfirmed);
				continue;
			}

			try
			{
				await HandleMessageAsync(consumeResult!.Message.Value, stoppingToken);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Ошибка при обработке BookingConfirmed: {Payload}", consumeResult.Message.Value);
			}

			consumer.Commit(consumeResult);
		}

		consumer.Close();
	}

	private async Task HandleMessageAsync(string payload, CancellationToken cancellationToken)
	{
		var message = JsonSerializer.Deserialize<BookingConfirmed>(payload)
		              ?? throw new InvalidOperationException("Не удалось десериализовать BookingConfirmed");

		using var scope = scopeFactory.CreateScope();
		var processedMessages = scope.ServiceProvider.GetRequiredService<IProcessedMessageRepository>();

		if (await processedMessages.IsProcessedAsync(message.BookingId))
		{
			logger.LogInformation("BookingId {BookingId} уже обработан, пропускаем повторную доставку",
				message.BookingId);
			return;
		}

		var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
		var result = await eventService.DecreaseAvailableSeatsAsync(message.EventId, message.SeatsCount);

		if (!result.IsSuccess)
		{
			logger.LogWarning("Не удалось уменьшить места для события {EventId}, бронь {BookingId}: {Error}",
				message.EventId,
				message.BookingId,
				result.ErrorMessage);
			return;
		}

		try
		{
			await processedMessages.MarkAsProcessedAsync(message.BookingId);
		}
		catch (DbUpdateException)
		{
			logger.LogWarning("Race condition: BookingId {BookingId} уже был помечен как обработанный параллельно",
				message.BookingId);
		}
	}

	private IConsumer<string, string> BuildConsumer()
	{
		var config = new ConsumerConfig
		{
			BootstrapServers = kafkaOptions.Value.BootstrapServers,
			GroupId = "events-service-booking-confirmed-group",
			AutoOffsetReset = AutoOffsetReset.Earliest,
			EnableAutoCommit = false
		};

		return new ConsumerBuilder<string, string>(config).Build();
	}
}