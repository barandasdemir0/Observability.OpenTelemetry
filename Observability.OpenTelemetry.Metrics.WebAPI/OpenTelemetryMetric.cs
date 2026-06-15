using System.Diagnostics.Metrics;

namespace Observability.OpenTelemetry.Metrics.WebAPI;

public static class OpenTelemetryMetric
{
    public static readonly Meter meter = new("metric.meter.api");
    public static Counter<int> OrderCounter = meter.CreateCounter<int>("order_count_event");
}
