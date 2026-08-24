using BookingService.Domain.Models.BookingModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Mapping;

public class BookingConfiguration : IEntityTypeConfiguration<BookingModel>
{
	public void Configure(EntityTypeBuilder<BookingModel> builder)
	{
		builder.ToTable("bookings");

		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();
		builder.Property(x => x.EventId)
			.HasColumnName("event_id");

		builder.Property(x => x.Status)
			.HasColumnName("status")
			.HasConversion<string>();
		
		builder.HasIndex(x => x.EventId)
			.HasDatabaseName("ix_bookings_eventid");
	}
}