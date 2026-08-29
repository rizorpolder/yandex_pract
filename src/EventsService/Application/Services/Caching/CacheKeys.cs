namespace EventsService.Application.Services.Caching;

public static class CacheKeys
{
	public static string Event(Guid id) => $"event:{id}";
	public const string TopEvents = "events:top10";
}