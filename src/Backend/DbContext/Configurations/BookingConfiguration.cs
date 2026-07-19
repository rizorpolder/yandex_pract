using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.DbContext.DataAccess.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
	public void Configure(EntityTypeBuilder<Booking> builder)
	{
		builder.ToTable("bookings");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id).ValueGeneratedNever();
		builder.Property(x => x.Status).HasConversion<string>();
		builder.HasOne(x => x.Event)
			.WithMany(x => x.Bookings)
			.HasForeignKey(x => x.EventId);
	}
}