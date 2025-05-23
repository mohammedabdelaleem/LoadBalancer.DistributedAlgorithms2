namespace LoadBalancer.Services;

public class HealthCheckBackgroundService : BackgroundService
{
	private readonly IHealthCheckService _healthCheckService;
	private readonly ILogger<HealthCheckBackgroundService> _logger;

	public HealthCheckBackgroundService(
		IHealthCheckService healthCheckService,
		ILogger<HealthCheckBackgroundService> logger)
	{
		_healthCheckService = healthCheckService;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await _healthCheckService.PerformHealthChecksAsync();
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during health check cycle");
				await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
			}
		}
	}
}

