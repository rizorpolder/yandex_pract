using System.Text.Json;
using EventsService.Application.Services.Abstraction.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EventsService.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
	private IDatabase Db => redis.GetDatabase();

	public async Task<T?> GetAsync<T>(string key)
	{
		try
		{
			var value = await Db.StringGetAsync(key);
			return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Redis недоступен при чтении ключа {Key}, кэш пропущен", key);
			return default;
		}
	}

	public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
	{
		try
		{
			await Db.StringSetAsync(key, JsonSerializer.Serialize(value), ttl);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Redis недоступен при записи ключа {Key}, кэширование пропущено", key);
		}
	}

	public async Task RemoveAsync(string key)
	{
		try
		{
			await Db.KeyDeleteAsync(key);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Redis недоступен при удалении ключа {Key}", key);
		}
	}
}