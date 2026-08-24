using System.Text.Json;
using BookingService.Application.Services.Abstraction.Broker;
using Confluent.Kafka;
using Contracts.Events;

namespace BookingService.Infrastructure.Messaging;

public class KafkaBookingRequestPublisher(IProducer<string, string> producer) : IBookingRequestPublisher
{
	private const string Topic = "booking-requested";

	public async Task PublishAsync(BookingRequested message, CancellationToken token = default)
	{
		await producer.ProduceAsync(Topic, new Message<string, string>
		{
			Key = message.ToString(),
			Value = JsonSerializer.Serialize(message)
		}, token);
	}
}