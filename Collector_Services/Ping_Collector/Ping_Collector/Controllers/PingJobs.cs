using Microsoft.AspNetCore.Mvc;
using System.Net;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Ping_Collector.Models;
using Ping_Collector.Responses;
using UncoreMetrics.Data.ClickHouse.Models;
using UncoreMetrics.Data.ClickHouse;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector.Controllers
{
    [ApiController]
    [Route("v1/PingJobs")]
    public class PingJobsController : ControllerBase
    {

        private readonly ServersContext _genericServersContext;
        private readonly ILogger _logger;


        public PingJobsController(ServersContext serversContext,
            ILogger<PingJobsController> logger)
        {
            _genericServersContext = serversContext;
            _logger = logger;
        }


        [HttpGet("getpingjobs/{id}")]
        public async Task<ActionResult<IResponse>> GetPingJobsForServer(int id, CancellationToken token)
        {

            var getPingJobsForLocation = await _genericServersContext.Servers.AsNoTracking().IgnoreAutoIncludes()
                .Where(server => server.ServerDead == false && server.ServerPings.Any(ping => ping.LocationID == id && ping.NextCheck < DateTime.UtcNow) || server.ServerPings.Any(ping => ping.LocationID == id) == false).OrderBy(server => server.NextCheck)
                .Include(ping => ping.ServerPings).Select(server => new MiniServerDTO() { Address = server.Address, ServerId = server.ServerID, ServerPings = server.ServerPings}).Take(50_000).ToListAsync(token);

            return Ok(new DataResponse<List<MiniServerDTO>>(getPingJobsForLocation));
        }
        [HttpPost("RegisterLocation")]
        public async Task<ActionResult<IResponse>> SubmitLocation([FromBody] Location location, CancellationToken token)
        {
            var tryGetLocation = await _genericServersContext.Locations.AsNoTracking().IgnoreAutoIncludes()
                .FirstOrDefaultAsync(loc => loc.LocationID == location.LocationID, token);

            if (tryGetLocation == null)
            {
                await _genericServersContext.Locations.AddAsync(location, token);
                await _genericServersContext.SaveChangesAsync(token);
            }
            return Ok(new DataResponse<Location>(location));
        }

        [HttpPost("PingJobScrapeStatusUpdate")]
        public async Task<ActionResult<IResponse>> SubmitScrapeJobUpdate([FromBody] ScrapeJob job, CancellationToken token)
        {

            var tryFind = await _genericServersContext.ScrapeJobs.FirstOrDefaultAsync(currentJob => currentJob.InternalId == job.InternalId, token);
            if (tryFind != null)
            {
                tryFind.Copy(job);
                await _genericServersContext.SaveChangesAsync(token);
            }
            else
            {
                await _genericServersContext.ScrapeJobs.AddAsync(job, token);
                await _genericServersContext.SaveChangesAsync(token);
            }
            return Ok(new GenericDataResponse(HttpStatusCode.OK, "Updated"));

        }


        [HttpPost("SubmitPingJobs")]
        public async Task<ActionResult<IResponse>> SubmitPostJobs([FromBody] PingJobCompleteDTO data, CancellationToken token)
        {
            using var transaction = await _genericServersContext.Database.BeginTransactionAsync(token);

            await _genericServersContext.BulkInsertOrUpdateAsync(data.CompletedPings, cancellationToken: token);
            await transaction.CommitAsync(token);
            return Ok(new GenericDataResponse(HttpStatusCode.OK, "Successfully Ingested, yum."));
        }
    }
}