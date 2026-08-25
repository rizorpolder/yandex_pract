namespace EventsService.Application.Services.Abstraction.Caching;

public interface ICacheService
{
	public Task<T?> GetAsync<T>(string key);
	public Task SetAsync<T>(string key, T value, TimeSpan ttl);
	public Task RemoveAsync(string key);
}