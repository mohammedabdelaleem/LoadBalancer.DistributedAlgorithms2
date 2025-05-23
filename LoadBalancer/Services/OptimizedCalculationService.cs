namespace LoadBalancer.Services;

using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using LoadBalancer.Models;
using System.Diagnostics;

public class OptimizedCalculationService : ICalculationService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<OptimizedCalculationService> _logger;
	private readonly string _nodeId;
	private readonly SemaphoreSlim _semaphore;
	private static readonly ConcurrentDictionary<string, double> _localCache = new();

	public OptimizedCalculationService(
		IDistributedCache cache,
		ILogger<OptimizedCalculationService> logger)
	{
		_cache = cache;
		_logger = logger;
		_nodeId = Environment.MachineName;
		_semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
	}

	public async Task<CalculationResponse> CalculateAsync(CalculationRequest request)
	{
		var stopwatch = Stopwatch.StartNew();
		var cacheKey = $"calc_{request.N}";

		_logger.LogInformation("Processing calculation request {RequestId} for N={N}",
			request.RequestId, request.N);

		// Check local cache first
		if (_localCache.TryGetValue(cacheKey, out var cachedResult))
		{
			_logger.LogInformation("Result found in local cache for N={N}", request.N);
			return new CalculationResponse
			{
				Result = cachedResult,
				RequestId = request.RequestId,
				WorkerNodeId = _nodeId,
				ProcessingTime = stopwatch.Elapsed,
				FromCache = true
			};
		}

		// Check distributed cache
		var cacheValue = await _cache.GetStringAsync(cacheKey);
		if (!string.IsNullOrEmpty(cacheValue) && double.TryParse(cacheValue, out var distributedCachedResult))
		{
			_localCache.TryAdd(cacheKey, distributedCachedResult);
			_logger.LogInformation("Result found in distributed cache for N={N}", request.N);

			return new CalculationResponse
			{
				Result = distributedCachedResult,
				RequestId = request.RequestId,
				WorkerNodeId = _nodeId,
				ProcessingTime = stopwatch.Elapsed,
				FromCache = true
			};
		}

		// Perform calculation with resource limiting
		await _semaphore.WaitAsync();
		try
		{
			var result = await CalculateHOptimizedAsync(request.N);

			// Cache the result
			_localCache.TryAdd(cacheKey, result);
			await _cache.SetStringAsync(cacheKey, result.ToString(),
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
				});

			_logger.LogInformation("Calculation completed for request {RequestId} in {ElapsedMs}ms",
				request.RequestId, stopwatch.ElapsedMilliseconds);

			return new CalculationResponse
			{
				Result = result,
				RequestId = request.RequestId,
				WorkerNodeId = _nodeId,
				ProcessingTime = stopwatch.Elapsed,
				FromCache = false
			};
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task<double> CalculateHOptimizedAsync(double n)
	{
		var iterations = (long)(n * 1_000_000);
		var numThreads = Environment.ProcessorCount;
		var chunkSize = iterations / numThreads;

		var tasks = new Task<double>[numThreads];

		for (int t = 0; t < numThreads; t++)
		{
			var start = t * chunkSize;
			var end = (t == numThreads - 1) ? iterations : (t + 1) * chunkSize;

			tasks[t] = Task.Run(() => CalculateChunk(start, end));
		}

		var results = await Task.WhenAll(tasks);
		return results.Sum();
	}

	private static double CalculateChunk(long start, long end)
	{
		double sum = 0;
		for (long i = start; i < end; i++)
		{
			if (i == 0) continue; // Avoid Math.Log(1) = 0 division
			sum += (Math.Sqrt(i) * Math.Sin(i)) / Math.Log(i + 1);
		}
		return sum;
	}
}