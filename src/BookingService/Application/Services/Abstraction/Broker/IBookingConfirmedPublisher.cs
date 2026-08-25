using Contracts.Events;

namespace BookingService.Application.Services.Abstraction.Broker;

public interface IBookingConfirmedPublisher
{
	Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default);

}