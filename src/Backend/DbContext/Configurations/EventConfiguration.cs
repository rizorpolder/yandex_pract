using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.DbContext.DataAccess.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
	public void Configure(EntityTypeBuilder<Event> builder)
	{
		builder.ToTable("events");
		builder.HasKey(e => e.Id);
		builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
		builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
		builder.Property(e => e.Description).HasMaxLength(255).IsRequired(false);
		builder.Property(e => e.AvailableSeats).HasColumnName("available_seats").IsRequired();
		builder.Property(e => e.TotalSeats).HasColumnName("total_seats").IsRequired();
		builder.Property(e => e.StartAt).HasColumnName("start_at")
			.HasColumnType("timestamp with time zone")
			.IsRequired();
		builder.Property(e => e.EndAt).HasColumnName("end_at")
			.HasColumnType("timestamp with time zone")
			.IsRequired();
		builder.HasMany(e => e.Bookings)
			.WithOne(b => b.Event)
			.HasForeignKey(b => b.EventId);

		builder.ToTable(t => t.HasCheckConstraint(
			"ck_events_available_seats",
			"available_seats <= total_seats"));

		builder.ToTable(t => t.HasCheckConstraint(
			"ck_events_time_range",
			"end_at > start_at"));
	}
}