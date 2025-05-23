namespace LoadBalancer.Services;

public interface IMetricsService
{
	void IncrementRequestCounter(string nodeId, bool fromCache);
	void RecordRequestLatency(string nodeId, double milliseconds);
	void UpdateResourceUsage(string nodeId, double cpuUsage, double memoryUsage);
}