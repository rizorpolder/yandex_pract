using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class CrudTests
{
	private readonly IEventService _service;

	public CrudTests(TestDbFixture fixture)
	{
		_service = fixture.EventService;
	}

	[Fact]
	public async Task CreateEventTest()
	{
		var evt = new Event(
			"testTitle",
			"testDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(10), 3);

		var added = await _service.CreateEventAsync(evt);

		Assert.True(added);

		var result = await _service.GetEventById(evt.Id);
		Assert.True(result.hasElement);
		Assert.NotNull(result.resultModel);
		Assert.Equal(evt.Title, result.resultModel!.Title);
		Assert.Equal(evt.Description, result.resultModel.Description);
	}

	[Fact]
	public async Task GetAllEventsTest()
	{
		var evt1 = new Event("A", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		var evt2 = new Event("B", "desc", DateTime.Now, DateTime.Now.AddMinutes(2), 5);

		await _service.CreateEventAsync(evt1);
		await _service.CreateEventAsync(evt2);

		var result = await _service.GetEvents(null, null, null, 1, 10);


		Assert.NotNull(result);
		Assert.Equal(2, result.Data.Count);
		Assert.Contains(result.Data, e => e.Title == "A");
		Assert.Contains(result.Data, e => e.Title == "B");
	}

	[Fact]
	public async Task GetEventByID()
	{
		var testEvt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await _service.CreateEventAsync(testEvt);
		Assert.True(added);

		var evt = await _service.GetEventById(testEvt.Id);

		Assert.True(evt.hasElement);
		Assert.NotNull(evt.resultModel);
		Assert.Equal(testEvt.Id, evt.resultModel!.Id);
	}

	[Fact]
	public async Task GetEventByIncorrectID()
	{
		var id = Guid.NewGuid();

		var evt = await _service.GetEventById(id);

		Assert.False(evt.hasElement);
		Assert.Null(evt.resultModel);
	}

	[Fact]
	public async Task UpdateEventTest()
	{
		var evt = new Event(
			"oldTitle",
			"oldDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(10), 3);

		await _service.CreateEventAsync(evt);

		var updated = new Event(
			"newTitle",
			"newDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(20), 3);

		var result = await _service.TryUpdateEvent(evt.Id, updated);

		Assert.True(result.hasElement);
		Assert.NotNull(result.eventResult);
		Assert.Equal("newTitle", result.eventResult!.Title);
		Assert.Equal("newDescription", result.eventResult.Description);
	}

	[Fact]
	public async Task UpdateBrokenIDEventTest()
	{
		var evt = new Event(
			"title",
			"desc",
			DateTime.Now,
			DateTime.Now.AddSeconds(10), 3);

		var result = await _service.TryUpdateEvent(Guid.NewGuid(), evt);

		Assert.False(result.hasElement);
		Assert.Null(result.eventResult);
	}

	[Fact]
	public async Task DeleteEventTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		await _service.CreateEventAsync(evt);

		var removed = await _service.RemoveEvent(evt);

		Assert.True(removed);

		var result = await _service.GetEventById(evt.Id);
		Assert.False(result.hasElement);
		Assert.Null(result.resultModel);
	}
}