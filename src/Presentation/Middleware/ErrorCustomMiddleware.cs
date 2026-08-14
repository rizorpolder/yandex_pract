using System.ComponentModel.DataAnnotations;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Presentation.Middleware;

public class ErrorCustomMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ErrorCustomMiddleware> _logger;

	public ErrorCustomMiddleware(RequestDelegate next, ILogger<ErrorCustomMiddleware> logger)
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
			NoAvailableSeatsException nse => StatusCodes.Status409Conflict,
			_ => StatusCodes.Status500InternalServerError,
		};
}