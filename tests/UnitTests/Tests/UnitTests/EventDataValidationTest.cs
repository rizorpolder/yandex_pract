// using System.Net;
// using System.Net.Http.Json;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.VisualStudio.TestPlatform.TestHost;
// using yandex_pract.CustomEventService.Dto;
//
// public class EventDateValidationTests
// {
// 	private readonly HttpClient _client;
//
// 	public EventDateValidationTests()
// 	{
// 		var factory = new WebApplicationFactory<Program>();
// 		_client = factory.CreateClient();
// 	}
//
// 	[Fact]
// 	public async Task UpdateEvent_ShouldReturnBadRequest_WhenEndAtBeforeStartAt()
// 	{
// 		var dto = new EventDto
// 		{
// 			Title = "Test",
// 			Description = "Desc",
// 			StartAt = DateTime.UtcNow,
// 			EndAt = DateTime.UtcNow.AddHours(-1)
// 		};
//
// 		var id = Guid.NewGuid();
//
// 		var response = await _client.PutAsJsonAsync($"/events/{id}", dto);
//
// 		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
// 	}
// }