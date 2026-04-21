using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using yandex_pract.Cors;
using yandex_pract.CustomEventService;
using yandex_pract.Middleware;

namespace yandex_pract;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);


		//builder.Services.AddCors(options => { options.AddPolicy("CORS", CORSMiddleware.CreatePolicyBuilder()); });
		builder.Services.AddControllers();
		builder.Services.AddScoped<IEventService, EventService>();
		builder.Services.AddSwaggerGen();
		
		var app = builder.Build();
		app.UseMiddleware<MyCustomMiddleware>();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}


		app.UseHttpsRedirection();
		app.UseRouting();
		//app.UseCors("CORS");
		app.MapControllers();
		app.Run();
	}
}