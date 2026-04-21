using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using yandex_pract.CustomException;

namespace yandex_pract.Middleware;

public class MyCustomMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<MyCustomMiddleware> _logger;

	public MyCustomMiddleware(RequestDelegate next, ILogger<MyCustomMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleException(context, ex);
		}
	}

	private async Task HandleException(HttpContext context, Exception ex)
	{
		_logger.LogError(ex, ex.Message);

		if (context.Response.HasStarted)
			return;

		var statusCode = StatusCodeMapping(ex);
		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/json";
		var error = new ProblemDetails()
		{
			Status = statusCode,
			Detail = ex.Message
		};
		await context.Response.WriteAsJsonAsync(error);
	}

	private int StatusCodeMapping(Exception ex)
		=> ex switch
		{
			ValidationException ve => StatusCodes.Status400BadRequest,
			NotFoundException nfe => StatusCodes.Status404NotFound,
			_ => StatusCodes.Status500InternalServerError,
		};
}