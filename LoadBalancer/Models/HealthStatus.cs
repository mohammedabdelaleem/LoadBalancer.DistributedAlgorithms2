namespace LoadBalancer.Models;

public class HealthStatus
{
	public string NodeId { get; set; }
	public bool IsHealthy { get; set; }
	public double CpuUsage { get; set; }
	public double MemoryUsage { get; set; }
	public int ActiveRequests { get; set; }
	public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}