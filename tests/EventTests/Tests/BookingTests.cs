using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.Services.BookingService;
using yandex_pract.Services.BookingService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class BookingTests
{
	private readonly IBookingService _service;
	private readonly IEventService _eventService;

	public BookingTests(TestDBFixture fixture)
	{
		_service = fixture.BookingService;
		_eventService = fixture.EventService;
	}


	[Fact]
	public void CreateSingleBookingTest()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");

		var bookingResult = _service.CreateBookingAsync(id).Result;
		Assert.True(bookingResult.result);
		Assert.NotNull(bookingResult.booking);
		Assert.Equal(id, bookingResult.booking.EventId);
	}

	[Fact]
	public void CreateSeveralBookingsTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10));
		var added = _eventService.AddEvent(evt);
		Assert.True(added);

		var booking1 = _service.CreateBookingAsync(evt.Id).Result;
		var booking2 = _service.CreateBookingAsync(evt.Id).Result;

		Assert.True(booking1.result);
		Assert.True(booking2.result);
		Assert.NotNull(booking1.booking);
		Assert.NotNull(booking2.booking);
		Assert.Equal(evt.Id, booking1.booking.EventId);
		Assert.Equal(evt.Id, booking2.booking.EventId);
		Assert.NotEqual(booking1.booking.Id, booking2.booking.Id);
	}

	[Fact]
	public void GetBookingByIdTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10));
		var added = _eventService.AddEvent(evt);
		Assert.True(added);
		var booking = _service.CreateBookingAsync(evt.Id).Result;
		Assert.True(booking.result);
		Assert.NotNull(booking.booking);

		var result = _service.GetBookingByIdAsync(booking.booking.Id).Result;
		Assert.True(result.haveBooking);
		Assert.NotNull(result.booking);
	}

	[Fact]
	public void CreateBookingWithWrongIdTest()
	{
		var booking = _service.CreateBookingAsync(Guid.NewGuid()).Result;
		Assert.False(booking.result);
		Assert.Null(booking.booking);
	}

	[Fact]
	public void CreateBookingForRemovedEventTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10));

		_eventService.AddEvent(evt);

		var removed = _eventService.RemoveEvent(evt);

		Assert.True(removed);

		var result = _service.CreateBookingAsync(evt.Id).Result;

		Assert.False(result.result);
		Assert.Null(result.booking);
	}

	[Fact]
	public void GetBookingWithBrokenIdTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10));
		var booking = new Booking(evt.Id);
		var result = _service.GetBookingByIdAsync(booking.Id).Result;
		Assert.False(result.haveBooking);
		Assert.Null(result.booking);
	}
}