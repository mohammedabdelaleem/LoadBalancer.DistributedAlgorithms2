using LoadBalancer.Models;

namespace LoadBalancer.Services;


public class HealthService : IHealthService
{
	public HealthStatus GetHealthStatus()
	{
		// Simulate health status (replace with actual system metrics)
		return new HealthStatus
		{
			NodeId = Environment.MachineName,
			IsHealthy = true,
			CpuUsage = new Random().NextDouble() * 100,
			MemoryUsage = new Random().NextDouble() * 100,
			ActiveRequests = 0,
			LastChecked = DateTime.UtcNow
		};
	}
}