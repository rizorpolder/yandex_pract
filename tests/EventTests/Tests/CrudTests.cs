using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class CrudTests
{
	private readonly EventService _service;

	public CrudTests(TestDBFixture fixture)
	{
		_service = fixture.Service;
	}

	[Fact]
	public void CreateEventTest()
	{
		var evt = new Event(
			"testTitle",
			"testDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(10));

		var added = _service.AddEvent(evt);

		Assert.True(added);

		var result = _service.GetEventById(evt.Id);
		Assert.True(result.hasElement);
		Assert.NotNull(result.resultModel);
		Assert.Equal(evt.Title, result.resultModel!.Title);
		Assert.Equal(evt.Description, result.resultModel.Description);
	}

	[Fact]
	public void GetAllEventsTest()
	{
		var events = _service.GetEvents();

		Assert.NotNull(events);
		Assert.True(events.Count > 0);
	}

	[Fact]
	public void GetEventByID()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");

		var evt = _service.GetEventById(id);

		Assert.True(evt.hasElement);
		Assert.NotNull(evt.resultModel);
		Assert.Equal(id, evt.resultModel!.Id);
	}

	[Fact]
	public void GetEventByIncorrectID()
	{
		var id = Guid.NewGuid();

		var evt = _service.GetEventById(id);

		Assert.False(evt.hasElement);
		Assert.Null(evt.resultModel);
	}

	[Fact]
	public void UpdateEventTest()
	{
		var evt = new Event(
			"oldTitle",
			"oldDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(10));

		_service.AddEvent(evt);

		var updated = new Event(
			"newTitle",
			"newDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(20));

		var result = _service.TryUpdateEvent(evt.Id, updated);

		Assert.True(result.hasElement);
		Assert.NotNull(result.eventResult);
		Assert.Equal("newTitle", result.eventResult!.Title);
		Assert.Equal("newDescription", result.eventResult.Description);
	}

	[Fact]
	public void UpdateBrokenIDEventTest()
	{
		var evt = new Event(
			"title",
			"desc",
			DateTime.Now,
			DateTime.Now.AddSeconds(10));

		var result = _service.TryUpdateEvent(Guid.NewGuid(), evt);

		Assert.False(result.hasElement);
		Assert.Null(result.eventResult);
	}

	[Fact]
	public void DeleteEventTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10));

		_service.AddEvent(evt);

		var removed = _service.RemoveEvent(evt);

		Assert.True(removed);

		var result = _service.GetEventById(evt.Id);
		Assert.False(result.hasElement);
		Assert.Null(result.resultModel);
	}
}