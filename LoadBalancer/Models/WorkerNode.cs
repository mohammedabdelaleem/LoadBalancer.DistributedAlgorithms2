namespace LoadBalancer.Models;
public class WorkerNode
{
	public required string Id { get; set; }
	public required string BaseUrl { get; set; }

	public int Weight { get; set; } = 1;
	public bool IsHealthy { get; set; } = true;
	public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
	public int ActiveRequests { get; set; }
}
