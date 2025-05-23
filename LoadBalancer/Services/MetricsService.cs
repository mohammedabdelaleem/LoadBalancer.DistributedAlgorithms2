using Prometheus;

namespace LoadBalancer.Services;

public class MetricsService : IMetricsService
{
	private readonly Counter _requestCounter;
	private readonly Histogram _requestLatency;
	private readonly Gauge _cpuUsage;
	private readonly Gauge _memoryUsage;

	public MetricsService()
	{
		_requestCounter = Metrics.CreateCounter(
			"calculation_requests_total",
			"Total calculation requests processed",
			new CounterConfiguration { LabelNames = new[] { "node_id", "from_cache" } });

		_requestLatency = Metrics.CreateHistogram(
			"calculation_request_latency_ms",
			"Request processing latency in milliseconds",
			new HistogramConfiguration { LabelNames = new[] { "node_id" } });

		_cpuUsage = Metrics.CreateGauge(
			"node_cpu_usage",
			"CPU usage percentage",
			new GaugeConfiguration { LabelNames = new[] { "node_id" } });

		_memoryUsage = Metrics.CreateGauge(
			"node_memory_usage",
			"Memory usage percentage",
			new GaugeConfiguration { LabelNames = new[] { "node_id" } });
	}

	public void IncrementRequestCounter(string nodeId, bool fromCache)
	{
		_requestCounter.WithLabels(nodeId, fromCache.ToString()).Inc();
	}

	public void RecordRequestLatency(string nodeId, double milliseconds)
	{
		_requestLatency.WithLabels(nodeId).Observe(milliseconds);
	}

	public void UpdateResourceUsage(string nodeId, double cpuUsage, double memoryUsage)
	{
		_cpuUsage.WithLabels(nodeId).Set(cpuUsage);
		_memoryUsage.WithLabels(nodeId).Set(memoryUsage);
	}
}