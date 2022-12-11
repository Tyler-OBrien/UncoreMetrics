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

    [HttpGet("scheduled")]
    public async Task<ActionResult<IResponse>> GetScheduled(CancellationToken token)
    {
        return Ok(new DataResponse<int?>(await _genericServersContext.Servers.Where(server => server.ServerDead == false)
            .CountAsync(token)));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IResponse>> GetOverdue(CancellationToken token)
    {
        return Ok(new DataResponse<int?>(await _genericServersContext.Servers.Where(server => server.NextCheck > DateTime.UtcNow && server.ServerDead == false)
            .CountAsync(token)));
    }
    [HttpGet("mostoverdue")]
    public async Task<ActionResult<IResponse>> OldestServerInQueue(CancellationToken token)
    {
        return Ok(new DataResponse<DateTime?>((await _genericServersContext.Servers.Where(server => server.ServerDead == false).OrderBy(server => server.NextCheck)
            .FirstOrDefaultAsync(token))?.NextCheck));
    }
    [HttpGet("mostoverdue/{appid}")]
    public async Task<ActionResult<IResponse>> OldestServerInQueue(ulong appid, CancellationToken token)
    {
        return Ok(new DataResponse<DateTime?>((await _genericServersContext.Servers.Where(server => server.ServerDead == false && server.AppID == appid).OrderBy(server => server.NextCheck)
            .FirstOrDefaultAsync(token))?.NextCheck));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IResponse>> GetServer(Guid id, CancellationToken token)
    {
        return Ok(new DataResponse<Server?>(await _genericServersContext.Servers.Where(server => server.ServerID == id)
            .FirstOrDefaultAsync(token)));
    }
    [HttpGet("uptime/{id}")]
    public async Task<ActionResult<IResponse>> GetServer(Guid id, [FromQuery] int? hours, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        return Ok(new DataResponse<double?>(await _clickHouseService.GetServerUptime(id.ToString(), hours.Value, token)));
    }

    [HttpGet("uptimedata/{id}")]
    public async Task<ActionResult<IResponse>> GetUptimeData(Guid id, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        if (hours > 336)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Please use uptimedata1d to query for data older then 14 days. Note that the granularity/min groupby of that data is 1 day. ", "too_old_data"));
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


    [HttpGet("uptimedataoverall")]
    public Task<ActionResult<IResponse>> GetUptimeDataOverallAppId([FromQuery] int? hours, [FromQuery] int? groupby,
        CancellationToken token) => GetUptimeDataOverallAppId(null, hours, groupby, token);

    [HttpGet("uptimedataoverall/{appid?}")]
    public async Task<ActionResult<IResponse>> GetUptimeDataOverallAppId(ulong? appid, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;

        if (groupby.HasValue == false)
        {
            if (hours / 0.5 > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {hours / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeDataOverall(appid, hours.Value, token)));

        }

        if (hours / groupby > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Max Results Generated can be 500. Your Query would have returned {hours / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeDataOverall(appid, hours.Value, groupby.Value, token)));
    }

    [HttpGet("uptimedata1d/{id}")]
    public async Task<ActionResult<IResponse>> GetUptimeData1d(Guid id, [FromQuery] int? days, [FromQuery] int? groupby, CancellationToken token)
    {
        if (days.HasValue == false)
            days = 30;

        if (groupby.HasValue == false)
        {
            if (days / 1 > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {days / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeData1d(id.ToString(), days.Value, token)));

        }

        if (days / groupby > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Max Results Generated can be 500. Your Query would have returned {days / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeData1d(id.ToString(), days.Value, groupby.Value, token)));
    }


    [HttpGet("playerdata/{id}")]
    public async Task<ActionResult<IResponse>> GetPlayersData(Guid id, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        if (hours > 336)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Please use playerdata1d to query for data older then 14 days. Note that the granularity/min groupby of that data is 1 day. ", "too_old_data"));
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

    [HttpGet("playerdataoverall")]
    public Task<ActionResult<IResponse>> GetPlayersDataOverall([FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token) => GetPlayersDataOveralAppId(null, hours, groupby, token);

    [HttpGet("playerdataoverall/{appid?}")]
    public async Task<ActionResult<IResponse>> GetPlayersDataOveralAppId(ulong? appid, [FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
    

        if (groupby.HasValue == false)
        {
            if (hours / 1 > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {hours / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHousePlayerData>>(
                await _clickHouseService.GetPlayerDataOverall(appid, hours.Value, token)));
        }
        if (hours / groupby > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {hours / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHousePlayerData>>(await _clickHouseService.GetPlayerDataOverall(appid, hours.Value, groupby.Value, token)));
    }

    [HttpGet("playerdata1d/{id}")]
    public async Task<ActionResult<IResponse>> GetPlayersData1d(Guid id, [FromQuery] int? days, [FromQuery] int? groupby, CancellationToken token)
    {
        if (days.HasValue == false)
            days = 30;
  

        if (groupby.HasValue == false)
        {
            if (days / 0.5 > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {days / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHousePlayerData>>(
                await _clickHouseService.GetPlayerData1d(id.ToString(), days.Value, token)));
        }
        if (days / groupby > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {days / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHousePlayerData>>(await _clickHouseService.GetPlayerData1d(id.ToString(), days.Value, groupby.Value, token)));
    }


    [HttpGet("search/{search}")]
    public async Task<ActionResult<IResponse>> GetServers(string search, CancellationToken token, [FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        IPAddress searchAddress;
        IPAddress.TryParse(search, out searchAddress);
        if (searchAddress == null)
            searchAddress = IPAddress.None;

            if (pageSize > 100)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        if (page <= 0)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Minimum Page is 0",
                "page_out_of_range"));
        if (includeDead == false)
            return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers
                .Where(server => server.SearchVector.Matches(search) || server.Address == searchAddress)
                .Where(server => server.ServerDead == includeDead).OrderByDescending(server => server.Players)
                .GetPaged(page, pageSize, token)));
        return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers
            .Where(server => server.SearchVector.Matches(search) || server.Address == searchAddress)
            .OrderByDescending(server => server.Players).GetPaged(page, pageSize, token)));
    }
}