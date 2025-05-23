using LoadBalancer.Models;

namespace LoadBalancer.Services;

public interface ILoadBalancerService
{
	Task<CalculationResponse> ProcessRequestAsync(CalculationRequest request);
}
