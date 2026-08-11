using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation;
using Presentation.Middleware;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddPresentation(builder.Configuration);

		var app = builder.Build();

		app.UsePresentation();
		app.MapPresentationEndpoints();

		app.Run();
	}
}