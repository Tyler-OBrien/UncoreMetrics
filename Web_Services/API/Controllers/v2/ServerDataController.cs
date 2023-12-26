using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using UncoreMetrics.API.Models.DTOs;
using UncoreMetrics.API.Models.PagedResults;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.API.Controllers;


[ApiController]
// V2 has magic adaptive endpoints which select the right dataset magically and accept a date range.
[Route("v2/servers")]
[SwaggerResponse(400, Type = typeof(ErrorResponseDetails<object>), Description = "On request failure, an object will be returned indicating what part of your request is invalid. Optionally includes details object of type BadRequestObjectResult.")]
[SwaggerResponse(404, Type = typeof(ErrorResponse), Description = "On invalid route, an object will be returned indicating the invalid route.")]
[SwaggerResponse(500, Type = typeof(ErrorResponse), Description = "On an internal server error, an object will be returned indicating the server error.")]
[SwaggerResponse(405, Type = typeof(ErrorResponse), Description = "On an invalid request method,  an object will be returned indicating the wrong request method.")]
[SwaggerResponse(429, Type = typeof(ErrorResponse), Description = "On hitting a rate limit, a rate limit response will be returned.")]
public class ServerDataController : ControllerBase
{
    private readonly IClickHouseService _clickHouseService;

    private readonly ServersContext _genericServersContext;
    private readonly ILogger _logger;


    public ServerDataController(ServersContext serversContext, IClickHouseService clickHouse,
        ILogger<GenericServerController> logger)
    {
        _genericServersContext = serversContext;
        _clickHouseService = clickHouse;
        _logger = logger;
    }



    [HttpGet("uptimedata/{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHouseUptimeData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetUptimeData(Guid id, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        if (groupby >= 24)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Please use uptimedata1d to group data larger then 24 hours. Note that the granularity/min groupby of that data is 1 day. ", "too_old_data"));
        }

        if (groupby.HasValue == false)
        {
            if (hours / 0.5 > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {hours / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeData(id.ToString(), hours.Value, token)));

        }

        if (hours / groupby > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Max Results Generated can be 500. Your Query would have returned {hours / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeData(id.ToString(), hours.Value, groupby.Value, token)));
    }



    [HttpGet("playerdata/{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHousePlayerData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetPlayersData(Guid id, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        if (groupby >= 24)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Please use playerdata1d to group data larger then 24 hours. Note that the granularity/min groupby of that data is 1 day. ", "too_old_data"));
        }

        if (groupby.HasValue == false)
        {
            if (hours / 0.5 > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {hours / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHousePlayerData>>(
                await _clickHouseService.GetPlayerData(id.ToString(), hours.Value, token)));
        }
        if (hours / groupby > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {hours / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHousePlayerData>>(await _clickHouseService.GetPlayerData(id.ToString(), hours.Value, groupby.Value, token)));
    }



}