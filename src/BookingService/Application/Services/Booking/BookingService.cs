using Application.Services.Abstraction.RequestResult;
using BookingService.Application.Services.Abstraction.Broker;
using BookingService.Application.Services.Abstraction.Repositories;
using BookingService.Application.Services.Abstraction.Services;
using BookingService.Application.Services.Booking.Dto;
using BookingService.Application.Services.Mapping;
using BookingService.Domain.Exceptions;
using BookingService.Domain.Models.BookingModel;
using BookingService.Domain.Models.BookingModel.Options;
using Common.Models;
using Contracts.Events;
using Microsoft.Extensions.Options;

namespace BookingService.Application.Services.Booking;

public class BookingService(
	IBookingRepository bookingRepository,
	IOptions<BookingOptions> options)
	: IBookingService
{
	public async Task ApplyReservationResultAsync(Guid bookingId, bool success, string? failureReason)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null)
			return;

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

	public async Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid userId, UserRole role)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null)
			return Result<bool>.Failure("NotFound");

		if (booking.UserId != userId && role != UserRole.Admin)
			return Result<bool>.Failure("Forbidden");

		booking.Cancel();
		await bookingRepository.SaveChangesAsync();

		return Result<bool>.Success(true);
	}

	public async Task ConfirmBookingAsync(Guid bookingId)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null) return;

		booking.Confirm();
		await bookingRepository.SaveChangesAsync();
	}

	public async Task RejectBookingAsync(Guid bookingId)
	{
		var booking = await bookingRepository.GetBookingAsync(bookingId);
		if (booking is null) return;

		booking.Reject();
		await bookingRepository.SaveChangesAsync();
	}
}