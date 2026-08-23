using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Interceptors;

public class DateTimeInterceptor : SaveChangesInterceptor
{
	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData, InterceptionResult<int> result)
	{
		Truncate(eventData.Context);
		return base.SavingChanges(eventData, result);
	}

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData, InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		Truncate(eventData.Context);
		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}

	private static void Truncate(Microsoft.EntityFrameworkCore.DbContext? context)
	{
		if (context is null) return;

		foreach (var entry in context.ChangeTracker.Entries())
		{
			if (entry.State is not (EntityState.Added or EntityState.Modified))
				continue;

			foreach (var property in entry.Properties)
			{
				switch (property.CurrentValue)
				{
					case DateTime dt:
						property.CurrentValue = TruncateToMicroseconds(dt);
						break;
				}
			}
		}
	}

	private static DateTime TruncateToMicroseconds(DateTime dt) =>
		new(dt.Ticks - dt.Ticks % 10, dt.Kind);
}