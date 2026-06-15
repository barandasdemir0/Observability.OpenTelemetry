using Observability.OpenTelemetry.Metrics.WebAPI;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .WithMetrics(options =>
    {
        options.AddMeter("metric.meter.api");
        options.ConfigureResource(resource =>
        {
            resource.AddService("Metric.API", serviceVersion: "1.0.0");
        });

        options.AddPrometheusExporter();
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/add-order-count", () => 
{
    OpenTelemetryMetric.OrderCounter.Add(new Random().Next(1, 10));

    return "is done.";
});

app.Run();
