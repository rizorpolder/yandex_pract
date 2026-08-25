using System.Reflection;
using BookingService.Domain.Models.BookingModel;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Contexts;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
	public DbSet<BookingModel> Bookings => Set<BookingModel>();

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(Assembly
			.GetExecutingAssembly()); // получить все настройки из Configurations текущей сборки
		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}