using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Contracts.Events;
using EventsService.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventsService.Infrastructure.Messaging;

public class KafkaTopicInitializer(
	IOptions<KafkaOptions> kafkaOptions,
	ILogger<KafkaTopicInitializer> logger) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var adminClient = new AdminClientBuilder(new AdminClientConfig
		{
			BootstrapServers = kafkaOptions.Value.BootstrapServers
		}).Build();

		try
		{
			await adminClient.CreateTopicsAsync(new[]
			{
				new TopicSpecification
				{
					Name = KafkaTopics.BookingConfirmed,
					NumPartitions = 3,
					ReplicationFactor = 1
				}
			});

			logger.LogInformation("Топик {Topic} создан", KafkaTopics.BookingConfirmed);
		}
		catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
		{
			logger.LogInformation("Топик {Topic} уже существует", KafkaTopics.BookingConfirmed);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Не удалось создать топик {Topic}, продолжаем запуск", KafkaTopics.BookingConfirmed);
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}