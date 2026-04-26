using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService.Controllers;

[ApiController] [Route("events")]
public class EventsController : ControllerBase
{
	private readonly IEventService _eventService;

	public EventsController(IEventService eventService)
	{
		_eventService = eventService;
	}

	[HttpGet]
	public ActionResult<PaginatedResultDto> GetEvents(string? title, DateTime? from, DateTime? to,  int page = 1, int pageSize = 10)
	{
		var result = _eventService.GetEvents(title, from, to, page, pageSize);
		return new OkObjectResult(result);
	}

	[HttpGet("{id:guid}")]
	public ActionResult<EventDto> GetEventById(Guid id)
	{
		var result = _eventService.GetEventById(id);

		if (result.hasElement)
		{
			return new OkObjectResult(new EventDto(result.resultModel));
		}


		return NotFound();
	}

	[HttpPost]
	public ActionResult<EventDto> CreateNewEvent([FromBody] EventDto newEvent)
	{
		if (!TryValidateModel(newEvent))
			return BadRequest();

		var model = new Event(newEvent);

		if (_eventService.AddEvent(model))
			return new OkObjectResult(new EventDto(model)) {StatusCode = StatusCodes.Status201Created};
		
		return BadRequest();
	}

	[HttpPut("{id:guid}")]
	public ActionResult<EventDto> UpdateEventById(Guid id, [FromBody] EventDto eventDto)
	{
		if (!TryValidateModel(eventDto))
			return BadRequest();

		var newModel = new Event(eventDto);

		var updateResult = _eventService.TryUpdateEvent(id, newModel);
		if (!updateResult.hasElement)
			return NotFound();

		return new OkObjectResult(new EventDto(updateResult.eventResult));
	}

	[HttpDelete("{id:guid}")]
	public IActionResult DeleteEventById(Guid id)
	{
		var model = _eventService.GetEventById(id);
		if (!model.hasElement)
			return NotFound();

		var isSuccess = _eventService.RemoveEvent(model.resultModel);
		if (isSuccess)
			return Ok();

		return BadRequest();
	}
}