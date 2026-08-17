using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.RequestResult;
using Application.Services.Abstraction.Services;
using Application.Services.BookingService.Dto;
using Application.Services.Mapping;
using Domain.Exceptions;
using Domain.Models.Bookings;
using Domain.Models.Bookings.Options;
using Domain.Models.Users;
using Microsoft.Extensions.Options;

namespace Application.Services.BookingService;

public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository,IOptions<BookingOptions> options)
	: IBookingService
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<Result<BookingDto>> CreateBookingAsync(Guid eventId, Guid userId)
	{
		await _semaphore.WaitAsync();
		try
		{
			var evt = await eventRepository.GetByIdAsync(eventId);
			if (evt == null)

				return Result<BookingDto>.Failure("Event not found");

			if (DateTime.UtcNow > evt.EndAt)
				throw new OutOfDateException();

			if (DateTime.UtcNow >= evt.StartAt)
				throw new EventAlreadyStartedException();

			var activeCount = await bookingRepository.GetActiveBookingsCountAsync(userId);
			if (activeCount >= options.Value.LimitPerUser)
				throw new BookingLimitReachedException(options.Value.LimitPerUser);

			if (!evt.TryReserveSeats())
				throw new NoAvailableSeatsException("No available seats");

			await eventRepository.SaveChangesAsync();
			var booking = new Booking(eventId, userId);
			try
			{
				await bookingRepository.AddBookingAsync(booking);
				await bookingRepository.SaveChangesAsync();
			}
			catch (Exception e)
			{
				return Result<BookingDto>.Failure("Booking not created" + e.Message);
			}

			return Result<BookingDto>.Success(BookingMapper.ToDto(booking));
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<Result<BookingDto>> GetBookingByIdAsync(Guid bookingId)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null)
		{
			return Result<BookingDto>.Failure("Booking not found");
		}

		return Result<BookingDto>.Success(BookingMapper.ToDto(booking));
	}

	public async Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid userId, UserRole role)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null)
			return Result<bool>.Failure("Booking not found");

		if (booking.UserId != userId && role != UserRole.Admin)
			return Result<bool>.Failure("Forbidden");

		var evt = await eventRepository.GetByIdAsync(booking.EventId);
		if (evt == null)
			return Result<bool>.Failure("Event not found");

		evt.ReleaseSeats();

		booking.Status = BookingStatus.Rejected;
		booking.ProcessedAt = DateTime.UtcNow;

		await eventRepository.SaveChangesAsync();
		await bookingRepository.SaveChangesAsync();

		return Result<bool>.Success(true);
	}
}