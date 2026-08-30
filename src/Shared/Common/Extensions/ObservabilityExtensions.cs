using Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace Common.Extensions;

public static class ObservabilityExtensions
{
	public static WebApplication UseObservability(this WebApplication app)
	{
		app.MapPrometheusScrapingEndpoint();
		return app;
	}

	public static void UseSerilog(this WebApplicationBuilder builder)
	{
		builder.Host.UseSerilog((ctx, cfg) =>
			cfg.ReadFrom.Configuration(ctx.Configuration)
				.WriteTo.Console(new CompactJsonFormatter()));
	}

	public static IServiceCollection AddObservability(
		this IServiceCollection services,
		IConfiguration configuration,
		string serviceName)
	{
		var otlpOptions = configuration.GetSection("Otlp").Get<OtlpOptions>()
		                  ?? throw new InvalidOperationException(
			                  "Секция конфигурации 'Otlp' не найдена или не заполнена.");

		if (string.IsNullOrWhiteSpace(otlpOptions.Endpoint))
			throw new InvalidOperationException("Не задан Otlp:Endpoint в конфигурации.");

		var otlpEndpoint = new Uri(otlpOptions.Endpoint);

		services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				serviceName: serviceName,
				serviceVersion: "1.0.0"))
			.WithTracing(tracing =>
			{
				tracing.AddAspNetCoreInstrumentation();
				tracing.AddHttpClientInstrumentation();
				tracing.AddEntityFrameworkCoreInstrumentation();
				tracing.AddOtlpExporter(exporterOptions =>
				{
					exporterOptions.Endpoint = otlpEndpoint;
					exporterOptions.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 2000;
					exporterOptions.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = 3000;
				});
			})
			.WithMetrics(metrics =>
			{
				metrics.AddAspNetCoreInstrumentation();
				metrics.AddRuntimeInstrumentation();
				metrics.AddPrometheusExporter();
			});

		return services;
	}
}