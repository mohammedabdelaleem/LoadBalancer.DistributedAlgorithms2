using LoadBalancer.Models;

namespace LoadBalancer.Services;

public interface IHealthService
{
	HealthStatus GetHealthStatus();
}