namespace LoadBalancer.Models;

public class CalculationRequest
{
	public double N { get; set; }
	public string RequestId { get; set; } = Guid.NewGuid().ToString();
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

