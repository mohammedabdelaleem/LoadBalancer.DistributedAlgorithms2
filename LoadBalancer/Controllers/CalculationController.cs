using LoadBalancer.Models;
using LoadBalancer.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoadBalancer.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CalculationController : ControllerBase
{
	private readonly ICalculationService _calculationService;
	private readonly ILogger<CalculationController> _logger;

	public CalculationController(
		ICalculationService calculationService,
		ILogger<CalculationController> logger)
	{
		_calculationService = calculationService;
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
			var response = await _calculationService.CalculateAsync(request);
			return Ok(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing calculation request {RequestId}", request.RequestId);
			return StatusCode(500, "Internal server error");
		}
	}
}
