using LoadBalancer.Models;

namespace LoadBalancer.Services;
public interface ICalculationService
{
	Task<CalculationResponse> CalculateAsync(CalculationRequest request);
}