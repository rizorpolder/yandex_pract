using yandex_pract.Services;

namespace yandex_pract;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddScoped<IBuildingService, BuildingService>();
		builder.Services.AddSwaggerGen();
		builder.Services.AddControllers();

		var app = builder.Build();
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseAuthorization();
		app.MapControllers();

		app.Run();
	}
}