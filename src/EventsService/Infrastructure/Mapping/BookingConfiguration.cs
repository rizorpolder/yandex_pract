using Domain.Models.Bookings;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mapping;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
	public void Configure(EntityTypeBuilder<Booking> builder)
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

		builder.HasOne(x => x.Event)
			.WithMany(x => x.Bookings)
			.HasForeignKey(x => x.EventId);

		builder.HasIndex(x => x.EventId)
			.HasDatabaseName("ix_bookings_eventid");

		builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
	}
}