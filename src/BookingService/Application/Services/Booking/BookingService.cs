using Application.Services.Abstraction.RequestResult;
using BookingService.Application.Services.Abstraction.Broker;
using BookingService.Application.Services.Abstraction.Repositories;
using BookingService.Application.Services.Abstraction.Services;
using BookingService.Application.Services.Booking.Dto;
using BookingService.Application.Services.Mapping;
using BookingService.Domain.Exceptions;
using BookingService.Domain.Models.BookingModel;
using BookingService.Domain.Models.BookingModel.Options;
using Contracts.Events;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Services.Booking;

public class BookingService(
	IBookingRepository bookingRepository,
	IBookingRequestPublisher publisher,
	IOptions<BookingOptions> options)
	: IBookingService
{
	public async Task ApplyReservationResultAsync(Guid bookingId, bool success, string? failureReason)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null)
			return; // возможно, стоит залогировать — сообщение пришло на несуществующую бронь

		if (success)
			booking.Confirm();
		else
			booking.Reject();

		await bookingRepository.SaveChangesAsync();
	}

	public async Task<Result<BookingDto>> CreateBookingAsync(Guid eventId, Guid userId)
	{
		var activeCount = await bookingRepository.GetActiveBookingsCountAsync(userId);
		if (activeCount >= options.Value.LimitPerUser)
			throw new BookingLimitReachedException(options.Value.LimitPerUser);

		var booking = new BookingModel(eventId, userId);
		await bookingRepository.AddBookingAsync(booking);
		await bookingRepository.SaveChangesAsync();
		await publisher.PublishAsync(new BookingRequested(booking.Id, eventId, DateTime.UtcNow));
		return Result<BookingDto>.Success(BookingMapper.ToDto(booking));
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