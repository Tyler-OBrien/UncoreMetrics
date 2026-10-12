using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data.GameData;

namespace UncoreMetrics.API.Controllers;

[ApiController]
[Route("v1/games")]
[SwaggerResponse(404, Type = typeof(ErrorResponse), Description = "On invalid route, an object will be returned indicating the invalid route.")]
[SwaggerResponse(500, Type = typeof(ErrorResponse), Description = "On an internal server error, an object will be returned indicating the server error.")]
public class GamesController : ControllerBase
{
    [HttpGet]
    [SwaggerResponse(200, Type = typeof(DataResponse<IReadOnlyList<GameInformation>>), Description = "The games Uncore Metrics collects data for")]
    public ActionResult<IResponse> GetGames()
    {
        return Ok(new DataResponse<IReadOnlyList<GameInformation>>(StaticGameInfo.Games));
    }
}
