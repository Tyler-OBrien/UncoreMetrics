using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using UncoreMetrics.API.Models.DTOs;
using UncoreMetrics.API.Models.Responses.API;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;

namespace UncoreMetrics.API.Controllers;

[ApiController]
    [Route("v1/serverdata")]
    [SwaggerResponse(400, Type = typeof(ErrorResponseDetails<object>), Description = "On request failure, an object will be returned indicating what part of your request is invalid. Optionally includes details object of type BadRequestObjectResult.")]
    [SwaggerResponse(404, Type = typeof(ErrorResponse), Description = "On invalid route, an object will be returned indicating the invalid route.")]
    [SwaggerResponse(500, Type = typeof(ErrorResponse), Description = "On an internal server error, an object will be returned indicating the server error.")]
    [SwaggerResponse(405, Type = typeof(ErrorResponse), Description = "On an invalid request method,  an object will be returned indicating the wrong request method.")]
    [SwaggerResponse(429, Type = typeof(ErrorResponse), Description = "On hitting a rate limit, a rate limit response will be returned.")]
    public class SpecificGameDataController : ControllerBase
    {

        private readonly ServersContext _genericServersContext;
        private readonly ILogger _logger;


        public SpecificGameDataController(ServersContext serversContext,
            ILogger<GenericServerController> logger)
        {
            _genericServersContext = serversContext;
            _logger = logger;
        }

        [HttpGet("{id:guid}/{appid:long}")]
        [SwaggerResponse(200, Type = typeof(DataResponse<FullServerDTO?>), Description = "On success, the API will respond with a full server object. If no server is found, the data will be null.")]
        public async Task<ActionResult<IResponse>> GetServer(Guid id, ulong appid, CancellationToken token)
        {
            object? specificGameResponse = null;

            switch (appid)
            {
            case 251570:
                specificGameResponse = await
                    _genericServersContext.SevenDaysToDieServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;
            case 346110:
                specificGameResponse = await
                    _genericServersContext.ArkServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;
            case 108600:
                specificGameResponse = await
                    _genericServersContext.ProjectZomboidServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 107410:
                specificGameResponse = await
                    _genericServersContext.Arma3Servers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 221100:
                specificGameResponse = await
                    _genericServersContext.DayZServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 686810:
                specificGameResponse = await
                    _genericServersContext.HellLetLooseServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 736220:
                specificGameResponse = await
                    _genericServersContext.PostScriptumServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 252490:
                specificGameResponse = await
                    _genericServersContext.RustServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 304930:
                specificGameResponse = await
                    _genericServersContext.UnturnedServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;

            case 393380:
                specificGameResponse = await
                    _genericServersContext.SquadServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;
            case 1604030:
                specificGameResponse = await
                    _genericServersContext.VRisingServers.AsNoTracking().FirstOrDefaultAsync(server => server.ServerID == id,
                        token);
                break;
            default:
                return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest,
                    $"Cannot find the game type specified.",
                    "invalid_game"));
        }

            if (specificGameResponse == null)
            {
                return BadRequest(new ErrorResponse(HttpStatusCode.NotFound,
                    $"Cannot find the server .",
                    "not_found_server"));

            }

            var getExtraProperties = GetExtraProperties(specificGameResponse);
            return Ok(new DataResponse<Dictionary<string, object>>(getExtraProperties));

        }
        // Hacky way for now to just return the raw extra metadata
        static Dictionary<string, object> GetExtraProperties(object derivedObject)
        {
            Type baseType = typeof(Server);
            Type derivedType = derivedObject.GetType();

            // Get the properties that are declared only in the derived class
            PropertyInfo[] derivedProperties = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Dictionary<string, object> extraProperties = new Dictionary<string, object>();

            // Add the properties to the dictionary
            foreach (var property in derivedProperties)
            {
                if (baseType.GetProperty(property.Name) == null) // Ensure the property is not in the base class
                {
                    extraProperties.Add(property.Name, property.GetValue(derivedObject));
                }
            }

            return extraProperties;
        }

}
