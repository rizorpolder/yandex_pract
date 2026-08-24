using Application.Services.Abstraction.RequestResult;
using BookingService.Application.Services.Abstraction.Repositories;
using BookingService.Application.Services.Abstraction.Services;
using BookingService.Application.Services.Booking.Dto;
using BookingService.Application.Services.Mapping;
using BookingService.Domain.Exceptions;
using BookingService.Domain.Models.BookingModel;
using BookingService.Domain.Models.BookingModel.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Services.Booking;

public class BookingService(IBookingRepository bookingRepository,IOptions<BookingOptions> options)
	: IBookingService
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<Result<BookingDto>> CreateBookingAsync(Guid eventId, Guid userId)
	{
		await _semaphore.WaitAsync();
		try
		{
			var activeCount = await bookingRepository.GetActiveBookingsCountAsync(userId);
			if (activeCount >= options.Value.LimitPerUser)
				throw new BookingLimitReachedException(options.Value.LimitPerUser);
			
			var booking = new BookingModel(eventId, userId);
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
			return Result<BookingDto>.Failure("NotFound");
		}

		return Result<BookingDto>.Success(BookingMapper.ToDto(booking));
	}

	public async Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid userId)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null)
			return Result<bool>.Failure("NotFound");

		booking.Cancel();

		await bookingRepository.SaveChangesAsync();

		return Result<bool>.Success(true);
	}
}