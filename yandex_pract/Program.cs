using yandex_pract.Cors;
using yandex_pract.CustomEventService;

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