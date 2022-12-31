using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ping_Collector_Probe.Models;
using UncoreMetrics.Data;
using UncoreMetrics.Data.GameData;

namespace Ping_Collector_Probe.Services
{
    public interface IPingCollectorAPI
    {
        Task<bool> RegisterLocation(Location location);

        Task<List<MiniServerDTO>> GetPingJobs(int locationId);

        Task<bool> SubmitScrapeJobUpdate(ScrapeJob newJobData);

        Task<bool> SubmitPingJobs(PingJobCompleteDTO Data);
    }
}
