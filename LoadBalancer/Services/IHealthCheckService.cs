namespace LoadBalancer.Services;

public interface IHealthCheckService
{
	bool IsNodeHealthy(string nodeId);
	void MarkNodeUnhealthy(string nodeId);
	Task PerformHealthChecksAsync();
}

