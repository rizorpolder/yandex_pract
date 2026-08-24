using Contracts.Events;

namespace BookingService.Application.Services.Abstraction.Broker;

public interface IBookingRequestPublisher
{
	Task PublishAsync(BookingRequested requested, CancellationToken token = default);
}