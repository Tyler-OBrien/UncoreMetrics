using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data;


namespace UncoreMetrics.API.Controllers;

[Route("v1/jobs")]
[ApiController]
[SwaggerResponse(400, Type = typeof(ErrorResponseDetails<object>), Description = "On request failure, an object will be returned indicating what part of your request is invalid. Optionally includes details object of type BadRequestObjectResult.")]
[SwaggerResponse(404, Type = typeof(ErrorResponse), Description = "On invalid route, an object will be returned indicating the invalid route.")]
[SwaggerResponse(500, Type = typeof(ErrorResponse), Description = "On an internal server error, an object will be returned indicating the server error.")]
[SwaggerResponse(405, Type = typeof(ErrorResponse), Description = "On an invalid request method,  an object will be returned indicating the wrong request method.")]
[SwaggerResponse(429, Type = typeof(ErrorResponse), Description = "On hitting a rate limit, a rate limit response will be returned.")]
public class ScrapeJobController : ControllerBase
{
    private readonly ServersContext _genericServersContext;
    private readonly ILogger _logger;


    public ScrapeJobController(ServersContext serversContext,
        ILogger<GenericServerController> logger)
    {
        _genericServersContext = serversContext;
        _logger = logger;
    }


    // GET: api/<ScrapeJobController>
    [HttpGet]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ScrapeJob>>), Description = "On success")]
    public async Task<ActionResult<IResponse>> Get(CancellationToken token)
    {
        return Ok(new DataResponse<List<ScrapeJob>>(await _genericServersContext.ScrapeJobs.AsNoTracking().ToListAsync(token)));
    }

    // GET api/<ScrapeJobController>/5
    [HttpGet("{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ScrapeJob>>), Description = "On success")]
    public async Task<ActionResult<IResponse>> Get(string id, CancellationToken token)
    {
        return Ok(new DataResponse<ScrapeJob?>(await _genericServersContext.ScrapeJobs
            .AsNoTracking().Where(job => job.InternalId == id).FirstOrDefaultAsync(token)));
    }
}