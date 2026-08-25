namespace EventsService.Application.Services.Options;

public class CacheOptions
{
	public int EventTtlSeconds { get; set; } = 300;
	public int TopEventsTtlSeconds { get; set; } = 300;
}