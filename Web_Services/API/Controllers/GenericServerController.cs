using System.Net;
using API.Models.PagedResults;
using API.Models.Responses.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;

namespace API.Controllers;

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
    public async Task<ActionResult<IResponse>> GetServers([FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (pageSize > 100)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        if (includeDead == false)
            return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers.OrderByDescending(server => server.Players)
                .Where(server => server.ServerDead == includeDead).GetPaged(page, pageSize)));
        return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers.OrderByDescending(server => server.Players).GetPaged(page, pageSize)));
    }
    [HttpGet("search/{search}")]
    public async Task<ActionResult<IResponse>> GetServers(string search, [FromQuery] bool? includeDead = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (pageSize > 100)
            return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "Max Page Size is 100",
                "page_size_out_of_range"));
        if (includeDead == false)
            return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers.Where(server => server.SearchVector.Matches(search))
                .Where(server => server.ServerDead == includeDead).OrderByDescending(server => server.Players).GetPaged(page, pageSize)));
        return Ok(new DataResponse<PagedResult<Server>>(await _genericServersContext.Servers.Where(server => server.SearchVector.Matches(search) || server.IpAddress == search).OrderByDescending(server => server.Players).GetPaged(page, pageSize)));
    }

}