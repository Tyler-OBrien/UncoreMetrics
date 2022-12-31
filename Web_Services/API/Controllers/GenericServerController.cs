using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations.Rules;
using Swashbuckle.AspNetCore.Annotations;
using UncoreMetrics.API.Models.DTOs;
using UncoreMetrics.API.Models.PagedResults;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data.ClickHouse.Models;
using static NodaTime.TimeZones.TzdbZone1970Location;

namespace UncoreMetrics.API.Controllers;

[ApiController]
[Route("v1/servers")]
[SwaggerResponse(400, Type = typeof(ErrorResponseDetails<object>), Description = "On request failure, an object will be returned indicating what part of your request is invalid. Optionally includes details object of type BadRequestObjectResult.")]
[SwaggerResponse(404, Type = typeof(ErrorResponse), Description = "On invalid route, an object will be returned indicating the invalid route.")]
[SwaggerResponse(500, Type = typeof(ErrorResponse), Description = "On an internal server error, an object will be returned indicating the server error.")]
[SwaggerResponse(405, Type = typeof(ErrorResponse), Description = "On an invalid request method,  an object will be returned indicating the wrong request method.")]
[SwaggerResponse(429, Type = typeof(ErrorResponse), Description = "On hitting a rate limit, a rate limit response will be returned.")]
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
    [SwaggerResponse(200, Type = typeof(DataResponse<PagedResult<ServerSearchResultDTO>>), Description = "On success, the API will respond with a server list item  object. If no servers are found, the list will be empty..")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max page side or min. page, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetServers(CancellationToken token,
        [FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (pageSize > 100)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        }

        if (page <= 0)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Minimum Page is 0",
                "page_out_of_range"));
        }

        if (includeDead == false)
        {
            return Ok(new DataResponse<PagedResult<ServerSearchResultDTO>>(await _genericServersContext.Servers
                .AsNoTracking()
                .OrderByDescending(server => server.Players)
                .Where(server => server.ServerDead == includeDead).Select(server => new ServerSearchResultDTO
                {
                    ServerID = server.ServerID,
                    Name = server.Name,
                    Game = server.Game,
                    Map = server.Map,
                    AppID = server.AppID,
                    Players = server.Players,
                    MaxPlayers = server.MaxPlayers,
                    ISP = server.ISP,
                    Country = server.Country
                }).GetPaged(page, pageSize, token)));
        }

        return Ok(new DataResponse<PagedResult<ServerSearchResultDTO>>(await _genericServersContext.Servers
            .AsNoTracking()
            .OrderByDescending(server => server.Players).Select(server => new ServerSearchResultDTO
            {
                ServerID = server.ServerID,
                Name = server.Name,
                Game = server.Game,
                Map = server.Map,
                AppID = server.AppID,
                Players = server.Players,
                MaxPlayers = server.MaxPlayers,
                ISP = server.ISP,
                Country = server.Country
            }).GetPaged(page, pageSize, token)));
    }


    [HttpGet("{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<FullServerDTO?>), Description = "On success, the API will respond with a full server object. If no server is found, the data will be null.")]
    public async Task<ActionResult<IResponse>> GetServer(Guid id, CancellationToken token)
    {
        return Ok(new DataResponse<FullServerDTO?>(await _genericServersContext.Servers.AsNoTracking()
            .Select(server => new FullServerDTO
            {
                ServerID = server.ServerID,
                Name = server.Name,
                Game = server.Game,
                Map = server.Map,
                AppID = server.AppID,
                IpAddress = server.Address.ToString(),
                Port = server.Port,
                QueryPort = server.QueryPort,
                Players = server.Players,
                MaxPlayers = server.MaxPlayers,
                ASN = server.ASN,
                ISP = server.ISP,
                Latitude = server.Latitude,
                Longitude = server.Longitude,
                Country = server.Country,
                Continent = server.Continent.ToString(),
                Timezone = server.Timezone,
                IsOnline = server.IsOnline,
                ServerDead = server.ServerDead,
                LastCheck = server.LastCheck,
                NextCheck = server.NextCheck,
                FailedChecks = server.FailedChecks,
                FoundAt = server.FoundAt,
                ServerPings = server.ServerPings
            }).Where(server => server.ServerID == id)
            .FirstOrDefaultAsync(token)));
    }
    [HttpGet("uptime/{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<double>), Description = "On success")]
    public async Task<ActionResult<IResponse>> GetServer(Guid id, [FromQuery] int? hours, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 24;
        return Ok(new DataResponse<double?>(await _clickHouseService.GetServerUptime(id.ToString(), hours.Value, token)));
    }

    [HttpGet("uptimedataraw/{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHouseRawUptimeData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetUptimeDataRaw(Guid id, [FromQuery] int? hours, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 6;

        // Eventually there should be actual date range selectors
        if ((hours * 60)  > 1000)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Max Results Generated can be 1000. Your Query would have returned {hours * 60}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHouseRawUptimeData>>(await _clickHouseService.GetUptimeDataRaw(id.ToString(), hours.Value, token)));
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


    [HttpGet("uptimedataoverall")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHouseUptimeData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]

    public Task<ActionResult<IResponse>> GetUptimeDataOverallAppId([FromQuery] int? hours, [FromQuery] int? groupby,
        CancellationToken token) => GetUptimeDataOverallAppId(null, hours, groupby, token);

    [HttpGet("uptimedataoverall/{appid?}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHouseUptimeData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
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
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHouseUptimeData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetUptimeData1d(Guid id, [FromQuery] int? days, [FromQuery] int? groupby, CancellationToken token)
    {
        if (days.HasValue == false)
            days = 30;

        if (groupby.HasValue == false)
        {
            if ((days / 1) > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {days / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeData1d(id.ToString(), days.Value, token)));

        }

        if ((days / groupby) > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                $"Max Results Generated can be 500. Your Query would have returned {days / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHouseUptimeData>>(await _clickHouseService.GetUptimeData1d(id.ToString(), days.Value, groupby.Value, token)));
    }


    [HttpGet("playerdataraw/{id}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHouseRawPlayerData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetPlayersDataRaw(Guid id, [FromQuery] int? hours, CancellationToken token)
    {
        if (hours.HasValue == false)
            hours = 6;
 
        if (hours * 60 > 1000)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 1000. Your Query would have returned {hours * 60}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHouseRawPlayerData>>(await _clickHouseService.GetPlayerDataRaw(id.ToString(), hours.Value, token)));
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

    [HttpGet("playerdataoverall")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHousePlayerData>>), Description = "On success")]

    public Task<ActionResult<IResponse>> GetPlayersDataOverall([FromQuery] int? hours, [FromQuery] int? groupby, CancellationToken token) => GetPlayersDataOveralAppId(null, hours, groupby, token);

    [HttpGet("playerdataoverall/{appid?}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHousePlayerData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
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
    [SwaggerResponse(200, Type = typeof(DataResponse<List<ClickHousePlayerData>>), Description = "On success")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse), Description = "If you exceed the max results, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetPlayersData1d(Guid id, [FromQuery] int? days, [FromQuery] int? groupby, CancellationToken token)
    {
        if (days.HasValue == false)
            days = 30;
  

        if (groupby.HasValue == false)
        {
            if ((days / 1) > 500)
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {days / 0.5}",
                    "too_many_results"));
            return Ok(new DataResponse<List<ClickHousePlayerData>>(
                await _clickHouseService.GetPlayerData1d(id.ToString(), days.Value, token)));
        }
        if ((days / groupby) > 500)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, $"Max Results Generated can be 500. Your Query would have returned {days / groupby}",
                "too_many_results"));

        return Ok(new DataResponse<List<ClickHousePlayerData>>(await _clickHouseService.GetPlayerData1d(id.ToString(), days.Value, groupby.Value, token)));
    }


    [HttpGet("search/{search}")]
    [SwaggerResponse(200, Type = typeof(DataResponse<PagedResult<ServerSearchResultDTO>>),
        Description =
            "On success, the API will respond with a server list item  object. If no servers are found, the list will be empty..")]
    [SwaggerResponse(400, Type = typeof(ErrorResponse),
        Description = "If you exceed the max page side or min. page, an error will be returned.")]
    public async Task<ActionResult<IResponse>> GetServers(string search, CancellationToken token,
        [FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        IPAddress searchAddress;
        IPAddress.TryParse(search, out searchAddress);
        if (searchAddress == null)
        {
            searchAddress = IPAddress.None;
        }

        if (pageSize > 100)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        }

        if (page <= 0)
        {
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Minimum Page is 0",
                "page_out_of_range"));
        }

        if (includeDead == false)
        {
            return Ok(new DataResponse<PagedResult<ServerSearchResultDTO>>(await _genericServersContext.Servers.AsNoTracking()
                .Where(server => server.SearchVector.Matches(search) || server.Address == searchAddress)
                .Where(server => server.ServerDead == includeDead).OrderByDescending(server => server.Players).Select(
                    server => new ServerSearchResultDTO
                    {
                        ServerID = server.ServerID,
                        Name = server.Name,
                        Game = server.Game,
                        Map = server.Map,
                        AppID = server.AppID,
                        Players = server.Players,
                        MaxPlayers = server.MaxPlayers,
                        ISP = server.ISP,
                        Country = server.Country
                    })
                .GetPaged(page, pageSize, token)));
        }

        return Ok(new DataResponse<PagedResult<ServerSearchResultDTO>>(await _genericServersContext.Servers.AsNoTracking()
            .Where(server => server.SearchVector.Matches(search) || server.Address == searchAddress)
            .OrderByDescending(server => server.Players).Select(server => new ServerSearchResultDTO
            {
                ServerID = server.ServerID,
                Name = server.Name,
                Game = server.Game,
                Map = server.Map,
                AppID = server.AppID,
                Players = server.Players,
                MaxPlayers = server.MaxPlayers,
                ISP = server.ISP,
                Country = server.Country
            }).GetPaged(page, pageSize, token)));
    }
}