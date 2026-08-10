using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.RequestResult;
using Application.Services.Abstraction.Services;
using Application.Services.BookingService.Dto;
using Application.Services.Mapping;
using Domain.Exceptions;
using Domain.Models.Booking;

namespace Application.Services.BookingService;

public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
	: IBookingService
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<Result<BookingDto>> CreateBookingAsync(Guid eventId)
	{
		await _semaphore.WaitAsync();
		try
		{
			var evt = await eventRepository.GetByIdAsync(eventId);
			if (evt == null)

				return Result<BookingDto>.Failure("Event not found");

			if (!evt.TryReserveSeats())
				throw new NoAvailableSeatsException("No available seats");

			await eventRepository.SaveChangesAsync();
			var booking = new Booking(eventId);
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

	// public async Task<List<Booking>> DequeuePendingAsync()
	// {
	// 	await _semaphore.WaitAsync();
	// 	try
	// 	{
	// 		var pending = await bookingRepository.GetPendingAsync();
	//
	// 		foreach (var booking in pending)
	// 		{
	// 			booking.Status = BookingStatus.Processing;
	// 			await bookingRepository.UpdateBookingAsync(booking);
	// 		}
	//
	// 		return pending;
	// 	}
	// 	finally
	// 	{
	// 		_semaphore.Release();
	// 	}
}