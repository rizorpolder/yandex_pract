namespace EventsService.Application.Options;

public class RedisOptions
{
	public string Host { get; set; } = string.Empty;
	public int Port { get; set; } = 6379;
	public string Password { get; set; } = string.Empty;
	public int ConnectTimeoutMs { get; set; } = 5000;
	public int SyncTimeoutMs { get; set; } = 3000;
}