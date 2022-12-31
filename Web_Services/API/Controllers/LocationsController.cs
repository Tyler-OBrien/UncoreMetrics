using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using UncoreMetrics.API.Models.DTOs;
using UncoreMetrics.API.Models.PagedResults;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;

namespace UncoreMetrics.API.Controllers;


[Route("v1/locations")]
[ApiController]
[SwaggerResponse(400, Type = typeof(ErrorResponseDetails<object>), Description = "On request failure, an object will be returned indicating what part of your request is invalid. Optionally includes details object of type BadRequestObjectResult.")]
[SwaggerResponse(404, Type = typeof(ErrorResponse), Description = "On invalid route, an object will be returned indicating the invalid route.")]
[SwaggerResponse(500, Type = typeof(ErrorResponse), Description = "On an internal server error, an object will be returned indicating the server error.")]
[SwaggerResponse(405, Type = typeof(ErrorResponse), Description = "On an invalid request method,  an object will be returned indicating the wrong request method.")]
[SwaggerResponse(429, Type = typeof(ErrorResponse), Description = "On hitting a rate limit, a rate limit response will be returned.")]
public class LocationsController : ControllerBase
{
    private readonly ServersContext _genericServersContext;
    private readonly ILogger _logger;


    public LocationsController(ServersContext serversContext,
        ILogger<GenericServerController> logger)
    {
        _genericServersContext = serversContext;
        _logger = logger;
    }


    // GET: api/<ScrapeJobController>
    [HttpGet]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<Location>>), Description = "On success")]

    public async Task<ActionResult<IResponse>> Get(CancellationToken token)
    {
        return Ok(new DataResponse<List<Location>>(await _genericServersContext.Locations.AsNoTracking().ToListAsync(token)));
    }

    // GET api/<ScrapeJobController>/5
    [HttpGet("{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<Location?>), Description = "On success")]
    public async Task<ActionResult<IResponse>> Get(int id, CancellationToken token)
    {
        return Ok(new DataResponse<Location?>(await _genericServersContext.Locations
            .AsNoTracking().Where(job => job.LocationID == id).FirstOrDefaultAsync(token)));
    }
}