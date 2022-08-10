﻿using API.Models.Responses.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UncoreMetrics.Data;
using UncoreMetrics.Data.ClickHouse;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("v1/jobs")]
    [ApiController]
    public class ScrapeJobController : ControllerBase
    {

        private readonly ServersContext _genericServersContext;
        private readonly ILogger _logger;


        public ScrapeJobController(ServersContext serversContext,
            ILogger<GenericServerController> logger)
        {
            _genericServersContext = serversContext;
            _logger = logger;
        }


        // GET: api/<ScrapeJobController>
        [HttpGet]
        public async Task<ActionResult<IResponse>> Get()
        {
            return Ok(new DataResponse<List<ScrapeJob>>(await _genericServersContext.ScrapeJobs.ToListAsync()));
        }

        // GET api/<ScrapeJobController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IResponse>> Get(string id)
        {
            return Ok(new DataResponse<ScrapeJob?>(await _genericServersContext.ScrapeJobs.Where(job => job.InternalId == id ).FirstOrDefaultAsync()));
        }

    }
}