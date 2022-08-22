using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UncoreMetrics.API.Models.PagedResults;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.ClickHouse.Models;

namespace UncoreMetrics.API.Controllers;

[ApiController]
[Route("v1/servers")]
public class GenericServerController : ControllerBase
{
    private readonly IClickHouseService _clickHouseService;

    private readonly ServersContext _genericServersContext;
    private readonly ILogger _logger;


    public GenericServerController(ServersContext serversContext, IClickHouseService clickHouse,
        ILogger<GenericServerController> logger)
    {
        _genericServersContext = serversContext;
        _clickHouseService = clickHouse;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IResponse>> GetServers(CancellationToken token, [FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (pageSize > 100)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        if (page <= 0)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Minimum Page is 0",
                "page_out_of_range"));
        if (includeDead == false)
            return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers
                .OrderByDescending(server => server.Players)
                .Where(server => server.ServerDead == includeDead).GetPaged(page, pageSize, token)));
        return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers
            .OrderByDescending(server => server.Players).GetPaged(page, pageSize, token)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IResponse>> GetServer(Guid id, CancellationToken token)
    {
        return Ok(new DataResponse<Server?>(await _genericServersContext.Servers.Where(server => server.ServerID == id)
            .FirstOrDefaultAsync(token)));
    }
    [HttpGet("uptime/{id}")]
    public async Task<ActionResult<IResponse>> GetServer(Guid id, [FromQuery] int hours, CancellationToken token)
    {
        return Ok(new DataResponse<double?>(await _clickHouseService.GetServerUptime(id.ToString(), hours, token)));
    }


    [HttpGet("players/{id}")]
    public async Task<ActionResult<IResponse>> GetPlayersCountGroupBy(Guid id, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        if (groupby.HasValue == false)
            return Ok(new DataResponse<List<ClickHousePlayerData>>(await _clickHouseService.GetPlayerCountPer30Minutes(id.ToString(), hours.Value, token)));

        return Ok(new DataResponse<List<ClickHousePlayerData>>(await _clickHouseService.GetPlayerCount(id.ToString(), hours.Value, groupby.Value, token)));
    }


    [HttpGet("search/{search}")]
    public async Task<ActionResult<IResponse>> GetServers(string search, CancellationToken token, [FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (pageSize > 100)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        if (page <= 0)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Minimum Page is 0",
                "page_out_of_range"));
        if (includeDead == false)
            return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers
                .Where(server => server.SearchVector.Matches(search))
                .Where(server => server.ServerDead == includeDead).OrderByDescending(server => server.Players)
                .GetPaged(page, pageSize, token)));
        return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers
            .Where(server => server.SearchVector.Matches(search) || server.IpAddress == search)
            .OrderByDescending(server => server.Players).GetPaged(page, pageSize, token)));
    }
}