using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using yandex_pract.CustomEventService.Dto;

public class EventDateValidationTests
{
	private readonly HttpClient _client;

	public EventDateValidationTests()
	{
		var host = new HostBuilder()
			.ConfigureWebHost(webBuilder =>
			{
				webBuilder
					.UseTestServer()
					.UseStartup<ProgramAdapter>();
			})
			.Start();

		_client = host.GetTestClient();
	}

	[Fact]
	public async Task UpdateEvent_ShouldReturnBadRequest_WhenEndAtBeforeStartAt()
	{
		var dto = new EventDto
		{
			Title = "Test",
			Description = "Desc",
			StartAt = DateTime.UtcNow,
			EndAt = DateTime.UtcNow.AddHours(-1)
		};

		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");
		var response = await _client.PutAsJsonAsync($"/events/{id}", dto);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
}