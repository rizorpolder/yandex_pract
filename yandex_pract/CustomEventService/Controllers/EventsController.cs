using Microsoft.AspNetCore.Mvc;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService.Controllers;

[ApiController] [Route("api/events")]
public class EventsController : ControllerBase
{
	private readonly IEventService _eventService;

	public EventsController(IEventService eventService)
	{
		_eventService = eventService;
	}

	[HttpGet("api/events")]
	public ActionResult<IReadOnlyList<EventModel>> GetAddEvents()
	{
		var events = _eventService.GetEvents();

		if (events.Count > 0)
			return new OkObjectResult(events);

		return NoContent();
	}

	[HttpGet("api/events/{id}")]
	public IActionResult GetEventById(Guid id)
	{
		var result = _eventService.GetEventById(id);

		if (result.hasElement)
			return new OkObjectResult(result);

		return NotFound();
	}

	[HttpPost("api/events")]
	public ActionResult<EventModel> CreateNewEvent([FromBody] EventModelDto newEventModel)
	{
		if (!TryValidateModel(newEventModel))
			return BadRequest();

		var model = new EventModel(newEventModel);

		if (_eventService.AddEvent(model))
			return new OkObjectResult(model);

		return BadRequest();
	}

	[HttpPut("api/events/{id}")]
	public ActionResult<EventModel> UpdateEventById(Guid id, [FromBody] EventModelDto eventModel)
	{
		if (!TryValidateModel(eventModel))
			return BadRequest();

		var model = _eventService.GetEventById(id);

		if (!model.hasElement)
			return NoContent();

		var isUpdated = _eventService.TryUpdateEvent(id, model.resultModel);

		if (isUpdated)
			return new OkObjectResult(model.resultModel);

		return BadRequest();
	}

	[HttpDelete("api/events/{id}")]
	public IActionResult DeleteEventById(Guid id)
	{
		var model = _eventService.GetEventById(id);
		if (!model.hasElement)
			return NoContent();

		var isSuccess = _eventService.RemoveEvent(model.resultModel);
		if (isSuccess)
			return Ok();

		return BadRequest();
	}
}