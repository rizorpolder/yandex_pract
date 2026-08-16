using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.RequestResult;
using Application.Services.Abstraction.Services;
using Application.Services.BookingService.Dto;
using Application.Services.Mapping;
using Domain.Exceptions;
using Domain.Models.Bookings;
using Domain.Models.Users;

namespace Application.Services.BookingService;

public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
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
				return Result<BookingDto>.Failure("Event already started");

			var activeCount = await bookingRepository.GetActiveBookingsCountAsync(userId);
			if (activeCount >= 10)
				return Result<BookingDto>.Failure("User has reached booking limit");

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

		if (role != UserRole.Admin && booking.UserId != userId)
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