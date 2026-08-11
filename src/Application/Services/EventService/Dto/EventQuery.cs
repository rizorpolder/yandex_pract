namespace Application.Services.EventService.Dto;

public class EventQuery
{
	public string? Title { get; set; }
	public DateTime? From { get; set; }
	public DateTime? To { get; set; }

	public int Page { get; set; }
	public int PageSize { get; set; }
}