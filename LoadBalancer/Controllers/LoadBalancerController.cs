using LoadBalancer.Models;
using LoadBalancer.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoadBalancer.Controllers;
[ApiController]
[Route("api/[controller]")]
public class LoadBalancerController : ControllerBase
{
	private readonly ILoadBalancerService _loadBalancerService;
	private readonly ILogger<LoadBalancerController> _logger;

	public LoadBalancerController(
		ILoadBalancerService loadBalancerService,
		ILogger<LoadBalancerController> logger)
	{
		_loadBalancerService = loadBalancerService;
		_logger = logger;
	}

	[HttpGet("cal")]
	public async Task<ActionResult<CalculationResponse>> Calculate([FromQuery] double n)
	{
		if (n <= 0)
		{
			return BadRequest("Parameter 'n' must be greater than 0");
		}

		var request = new CalculationRequest { N = n };

		try
		{
			var response = await _loadBalancerService.ProcessRequestAsync(request);
			return Ok(response);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogError(ex, "No healthy workers available for request {RequestId}", request.RequestId);
			return StatusCode(StatusCodes.Status500InternalServerError, new { message= "No healthy worker nodes available" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing load balanced request {RequestId}", request.RequestId);
			return StatusCode(500, "Internal server error");
		}
	}
}
