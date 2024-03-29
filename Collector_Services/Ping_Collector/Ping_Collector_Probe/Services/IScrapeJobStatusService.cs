﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ping_Collector_Probe.Services
{
    public interface IScrapeJobStatusService
    {
        public Task StartRun(int totalCount, string runType, CancellationToken token = default);

        public Task EndRun(string runType, CancellationToken token = default);

        public Task UpdateStatus(int progress, int totalDone, int totalCount, string runType,
            CancellationToken token = default);
    }
}
