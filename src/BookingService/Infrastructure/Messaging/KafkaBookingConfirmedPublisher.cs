using System.Text.Json;
using BookingService.Application.Services.Abstraction.Broker;
using Confluent.Kafka;
using Contracts.Events;

namespace BookingService.Infrastructure.Messaging;

public class KafkaBookingConfirmedPublisher(IProducer<string, string> producer) : IBookingConfirmedPublisher
{
	public async Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default)
	{
		await producer.ProduceAsync(KafkaTopics.BookingConfirmed,
			new Message<string, string>
			{
				Key = message.EventId.ToString(),
				Value = JsonSerializer.Serialize(message)
			},
			cancellationToken);
	}
}