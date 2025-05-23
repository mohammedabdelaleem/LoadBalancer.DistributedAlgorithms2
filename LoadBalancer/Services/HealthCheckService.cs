using LoadBalancer.Models;
using System.Collections.Concurrent;

namespace LoadBalancer.Services;

public class HealthCheckService : IHealthCheckService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<HealthCheckService> _logger;
	private readonly ConcurrentDictionary<string, WorkerNode> _nodeStatuses = new();

	private readonly List<WorkerNode> _workerNodes = new()
	{
		new WorkerNode { Id = "worker1", BaseUrl = "http://worker1:5002" },
		new WorkerNode { Id = "worker2", BaseUrl = "http://worker2:5003" },
		new WorkerNode { Id = "worker3", BaseUrl = "http://worker3:5004" }
	};

	public HealthCheckService(IHttpClientFactory httpClientFactory, ILogger<HealthCheckService> logger)
	{
		_httpClient = httpClientFactory.CreateClient();
		_httpClient.Timeout = TimeSpan.FromSeconds(5);
		_logger = logger;

		// Initialize node statuses
		foreach (var node in _workerNodes)
		{
			_nodeStatuses[node.Id] = node;
		}
	}

	public bool IsNodeHealthy(string nodeId)
	{
		return _nodeStatuses.TryGetValue(nodeId, out var node) && node.IsHealthy;
	}

	public void MarkNodeUnhealthy(string nodeId)
	{
		if (_nodeStatuses.TryGetValue(nodeId, out var node))
		{
			node.IsHealthy = false;
			_logger.LogWarning("Marked node {NodeId} as unhealthy", nodeId);
		}
	}

	public async Task PerformHealthChecksAsync()
	{
		var tasks = _workerNodes.Select(CheckNodeHealthAsync);
		await Task.WhenAll(tasks);
	}

	private async Task CheckNodeHealthAsync(WorkerNode node)
	{
		try
		{
			var response = await _httpClient.GetAsync($"{node.BaseUrl}/health");
			var isHealthy = response.IsSuccessStatusCode;

			if (_nodeStatuses.TryGetValue(node.Id, out var status))
			{
				status.IsHealthy = isHealthy;
				status.LastHealthCheck = DateTime.UtcNow;
			}

			if (isHealthy)
			{
				_logger.LogDebug("Health check passed for {NodeId}", node.Id);
			}
			else
			{
				_logger.LogWarning("Health check failed for {NodeId}: {StatusCode}",
					node.Id, response.StatusCode);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Health check error for {NodeId}", node.Id);
			if (_nodeStatuses.TryGetValue(node.Id, out var status))
			{
				status.IsHealthy = false;
				status.LastHealthCheck = DateTime.UtcNow;
			}
		}
	}
}
