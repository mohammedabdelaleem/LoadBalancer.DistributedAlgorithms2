using LoadBalancer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
	config.ReadFrom.Configuration(context.Configuration)
		  .Enrich.WithProperty("ServiceName", "LoadBalancer")
		  .Enrich.WithProperty("NodeId", Environment.MachineName));

// Define Polly retry policy
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
	Policy<HttpResponseMessage>
		.Handle<HttpRequestException>()
		.OrResult(msg => (int)msg.StatusCode >= 500)
		.WaitAndRetryAsync(
			3,
			retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
			onRetry: (outcome, timespan, retryCount, context) =>
			{
				Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "unknown error"}");
			});

// Define Polly circuit breaker policy
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
	Policy<HttpResponseMessage>
		.Handle<HttpRequestException>()
		.OrResult(msg => (int)msg.StatusCode >= 500)
		.CircuitBreakerAsync(
			3,
			TimeSpan.FromSeconds(30),
			onBreak: (outcome, breakDelay) =>
			{
				Console.WriteLine($"Circuit breaker opened for {breakDelay.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "unknown error"}");
			},
			onReset: () =>
			{
				Console.WriteLine("Circuit breaker reset.");
			});

// Determine if this instance is the load balancer
var isLoadBalancer = bool.TryParse(Environment.GetEnvironmentVariable("IS_LOAD_BALANCER"), out bool lb) && lb;

if (isLoadBalancer)
{
	// Load balancer specific services
	builder.Services.AddControllers();
	builder.Services.AddMemoryCache();
	builder.Services.AddHttpClient<ILoadBalancerService, LoadBalancerService>()
		.AddPolicyHandler(GetRetryPolicy())
		.AddPolicyHandler(GetCircuitBreakerPolicy());
	builder.Services.AddSingleton<ILoadBalancerService, LoadBalancerService>();
	builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
	builder.Services.AddHostedService<HealthCheckBackgroundService>();
}
else
{
	// Worker specific services
	builder.Services.AddControllers();
	builder.Services.AddMemoryCache();
	builder.Services.AddStackExchangeRedisCache(options =>
	{
		options.Configuration = builder.Configuration.GetConnectionString("Redis");
	});
	builder.Services.AddSingleton<ICalculationService, OptimizedCalculationService>();
	builder.Services.AddSingleton<IHealthService, HealthService>();
	builder.Services.AddHealthChecks();
}

// Shared services for both load balancer and worker
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddHostedService<MetricsBackgroundService>();

var app = builder.Build();

// Shared middleware
app.UseRouting(); // Ensure routing is enabled before mapping endpoints

if (isLoadBalancer)
{
	app.UseEndpoints(endpoints =>
	{
		endpoints.MapControllers();
		endpoints.MapMetrics("/metrics"); // Map the metrics endpoint
	});
}
else
{
	app.UseHealthChecks("/health");
	app.UseEndpoints(endpoints =>
	{
		endpoints.MapControllers();
		endpoints.MapMetrics("/metrics"); // Map the metrics endpoint for workers too
	});
}

app.Run();


//using LoadBalancer.Services;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.Caching.StackExchangeRedis;
//using Microsoft.Extensions.DependencyInjection;
//using Polly;
//using Prometheus;
//using Serilog;

//var builder = WebApplication.CreateBuilder(args);

//// Configure Serilog
//builder.Host.UseSerilog((context, config) =>
//	config.ReadFrom.Configuration(context.Configuration)
//		  .Enrich.WithProperty("ServiceName", "LoadBalancer")
//		  .Enrich.WithProperty("NodeId", Environment.MachineName));

//// Define Polly retry policy
//static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
//	Policy<HttpResponseMessage>
//		.Handle<HttpRequestException>()
//		.OrResult(msg => (int)msg.StatusCode >= 500)
//		.WaitAndRetryAsync(
//			3,
//			retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
//			onRetry: (outcome, timespan, retryCount, context) =>
//			{
//				Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "unknown error"}");
//			});

//// Define Polly circuit breaker policy
//static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
//	Policy<HttpResponseMessage>
//		.Handle<HttpRequestException>()
//		.OrResult(msg => (int)msg.StatusCode >= 500)
//		.CircuitBreakerAsync(
//			3,
//			TimeSpan.FromSeconds(30),
//			onBreak: (outcome, breakDelay) =>
//			{
//				Console.WriteLine($"Circuit breaker opened for {breakDelay.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "unknown error"}");
//			},
//			onReset: () =>
//			{
//				Console.WriteLine("Circuit breaker reset.");
//			});

//// Determine if this instance is the load balancer
//var isLoadBalancer = bool.TryParse(Environment.GetEnvironmentVariable("IS_LOAD_BALANCER"), out bool lb) && lb;

//if (isLoadBalancer)
//{
//	// Load balancer specific services
//	builder.Services.AddControllers();
//	builder.Services.AddMemoryCache();
//	builder.Services.AddHttpClient<ILoadBalancerService, LoadBalancerService>()
//		.AddPolicyHandler(GetRetryPolicy())
//		.AddPolicyHandler(GetCircuitBreakerPolicy());
//	builder.Services.AddSingleton<ILoadBalancerService, LoadBalancerService>();
//	builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
//	builder.Services.AddHostedService<HealthCheckBackgroundService>();
//}
//else
//{
//	// Worker specific services
//	builder.Services.AddControllers();
//	builder.Services.AddMemoryCache();
//	builder.Services.AddStackExchangeRedisCache(options =>
//	{
//		options.Configuration = builder.Configuration.GetConnectionString("Redis");
//	});
//	builder.Services.AddSingleton<ICalculationService, OptimizedCalculationService>();
//	builder.Services.AddSingleton<IHealthService, HealthService>();
//	builder.Services.AddHealthChecks();
//}

//// Shared services for both load balancer and worker
//builder.Services.AddSingleton<IMetricsService, MetricsService>();
//builder.Services.AddHostedService<MetricsBackgroundService>();

//var app = builder.Build();

//// Shared middleware
//app.MapMetrics("/metrics"); // Replaced UseMetricServer with MapMetrics

//if (isLoadBalancer)
//{
//	app.UseRouting();
//	app.MapControllers();
//}
//else
//{
//	app.UseHealthChecks("/health");
//	app.UseRouting();
//	app.MapControllers();
//}

//app.Run();