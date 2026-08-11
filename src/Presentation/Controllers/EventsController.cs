using Application.Services.Abstraction.Services;
using Application.Services.EventService.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
	private readonly IEventService _eventService;

	public EventsController(IEventService eventService)
	{
		_eventService = eventService;
	}

	[HttpGet(Name = nameof(GetEvents))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginatedResultDto>> GetEvents(string? title,
		DateTime? from,
		DateTime? to,
		int page = 1,
		int pageSize = 10)
	{
		var result = await _eventService.GetEvents(title, from, to, page, pageSize);
		return new OkObjectResult(result);
	}

	[HttpGet("{id:guid}", Name = nameof(GetEventById))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<EventDto>> GetEventById(Guid id)
	{
		var result = await _eventService.GetEventById(id);

		if (!result.IsSuccess)
			return NotFound(new { Message = result.ErrorMessage });

		return new OkObjectResult(result.Value);
	}

	[HttpPost(Name = nameof(CreateNewEvent))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<EventDto>> CreateNewEvent([FromBody] EventDto eventDto)
	{
		if (!TryValidateModel(eventDto))
			return BadRequest();

		var result = await _eventService.CreateEventAsync(eventDto);

		if (!result.IsSuccess)
			return BadRequest(new { Message = result.ErrorMessage });

		return new OkObjectResult(result.Value) { StatusCode = StatusCodes.Status201Created };
	}

	[HttpPut("{id:guid}", Name = nameof(UpdateEventById))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<EventDto>> UpdateEventById(Guid id, [FromBody] EventDto eventDto)
	{
		if (!TryValidateModel(eventDto))
			return BadRequest();
		var result = await _eventService.UpdateEventAsync(id, eventDto);
		if (!result.IsSuccess)
			return NotFound(new { Message = result.ErrorMessage });

		return new OkObjectResult(result.Value);
	}

	[HttpDelete("{id:guid}", Name = nameof(DeleteEventById))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> DeleteEventById(Guid id)
	{
		var getEvtResult = await _eventService.GetEventById(id);
		if (!getEvtResult.IsSuccess)
			return NotFound(new { message = getEvtResult.ErrorMessage });

		var removeEvtResult = await _eventService.RemoveEvent(getEvtResult.Value);
		if (!removeEvtResult.IsSuccess)
			return BadRequest(new { message = removeEvtResult.ErrorMessage });

		return Ok();
	}
}