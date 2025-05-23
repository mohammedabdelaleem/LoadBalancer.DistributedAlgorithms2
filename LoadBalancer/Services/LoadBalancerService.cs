namespace LoadBalancer.Services;

using LoadBalancer.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Concurrent;

public class LoadBalancerService : ILoadBalancerService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<LoadBalancerService> _logger;
	private readonly IHealthCheckService _healthCheckService;
	private int _currentIndex = 0;

	private readonly List<WorkerNode> _workerNodes = new()
	{
		new WorkerNode { Id = "worker1", BaseUrl = "http://worker1:5002", Weight = 1 },
		new WorkerNode { Id = "worker2", BaseUrl = "http://worker2:5003", Weight = 1 },
		new WorkerNode { Id = "worker3", BaseUrl = "http://worker3:5004", Weight = 1 }
	};

	public LoadBalancerService(
	HttpClient httpClient,
		ILogger<LoadBalancerService> logger,
		IHealthCheckService healthCheckService)
	{
		_httpClient = httpClient;
		_logger = logger;
		_healthCheckService = healthCheckService;
	}

	public async Task<CalculationResponse> ProcessRequestAsync(CalculationRequest request)
	{
		var availableNodes = _workerNodes.Where(n => _healthCheckService.IsNodeHealthy(n.Id)).ToList();

		if (!availableNodes.Any())
		{
			throw new InvalidOperationException("No healthy worker nodes available");
		}

		var selectedNode = SelectNode(availableNodes);
		var url = $"{selectedNode.BaseUrl}/api/calculation/cal?n={request.N}";

		_logger.LogInformation("Routing request {RequestId} to {NodeId}",
			request.RequestId, selectedNode.Id);

		try
		{
			var response = await _httpClient.GetFromJsonAsync<CalculationResponse>(url);

			_logger.LogInformation("Request {RequestId} completed successfully on {NodeId}",
				request.RequestId, selectedNode.Id);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing request {RequestId} on {NodeId}",
				request.RequestId, selectedNode.Id);

			_healthCheckService.MarkNodeUnhealthy(selectedNode.Id);
			throw;
		}
	}

	private WorkerNode SelectNode(List<WorkerNode> availableNodes)
	{
		// Weighted round-robin selection
		var totalWeight = availableNodes.Sum(n => n.Weight);
		var weightedNodes = new List<WorkerNode>();

		foreach (var node in availableNodes)
		{
			for (int i = 0; i < node.Weight; i++)
			{
				weightedNodes.Add(node);
			}
		}

		var index = Interlocked.Increment(ref _currentIndex) % weightedNodes.Count;
		return weightedNodes[index];
	}
}