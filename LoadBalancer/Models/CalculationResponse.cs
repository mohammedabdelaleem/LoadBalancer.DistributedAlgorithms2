namespace LoadBalancer.Models;

public class CalculationResponse
{
	public double Result { get; set; }
	public string RequestId { get; set; } = null!;
	public string WorkerNodeId { get; set; } = null!;
	public TimeSpan ProcessingTime { get; set; }
	public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
	public bool FromCache { get; set; }
}