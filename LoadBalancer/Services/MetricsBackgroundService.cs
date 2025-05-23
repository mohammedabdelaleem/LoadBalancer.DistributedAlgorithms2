namespace LoadBalancer.Services;


public class MetricsBackgroundService : BackgroundService
{
	private readonly IMetricsService _metricsService;
	private readonly ILogger<MetricsBackgroundService> _logger;

	public MetricsBackgroundService(IMetricsService metricsService, ILogger<MetricsBackgroundService> logger)
	{
		_metricsService = metricsService;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				// Simulate resource usage (replace with actual system metrics)
				var cpuUsage = new Random().NextDouble() * 100;
				var memoryUsage = new Random().NextDouble() * 100;
				_metricsService.UpdateResourceUsage(Environment.MachineName, cpuUsage, memoryUsage);
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating metrics");
			}
		}
	}
}