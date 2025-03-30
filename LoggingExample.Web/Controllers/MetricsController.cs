using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace LoggingExample.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private static readonly Counter SampleCounter = Metrics
        .CreateCounter("sample_counter", "An example counter");

    [HttpGet("increment")]
    public IActionResult IncrementCounter()
    {
        SampleCounter.Inc();
        return Ok(new { message = "Counter incremented" });
    }
}