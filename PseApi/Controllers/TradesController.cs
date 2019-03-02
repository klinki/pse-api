using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PseApi.Data;
using PseApi.Queries;
using PseApi.Services;

namespace PseApi.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class TradesController : ControllerBase
    {
        private readonly PseContext _context;
        private readonly TradeService _tradeService;
        private readonly ILogger<TradesController> _logger;

        public TradesController(PseContext context, TradeService tradeService, ILogger<TradesController> logger)
        {
            _context = context;
            _tradeService = tradeService;
            _logger = logger;
        }

        // GET api/values
        [HttpGet("day/{day}")]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesForDay([FromRoute] DateTime day)
        {
            IEnumerable<Trade> result = await _tradeService.GetTradesForDay(day);

            return Ok(result);
        }

        // GET api/values
        [HttpGet("{stock}")]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTrades([FromRoute] string stock)
        {
            IEnumerable<Trade> result = await _context.Trades
                .AsNoTracking()
                .Where(row => row.BIC == stock)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/values
        [HttpGet("isin/{isin}")]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesByIsin([FromRoute] string isin)
        {
            IEnumerable<Trade> result = await _context.Trades
                .AsNoTracking()
                .Where(row => row.ISIN == isin)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesByQuery([FromQuery] TradeQuery queryParams)
        {
            IQueryable<Trade> tradeQuery = _context.Trades.AsNoTracking();

            if (queryParams.From.HasValue)
            {
                tradeQuery = tradeQuery.Where(row => row.Date >= queryParams.From.Value);
            }

            if (queryParams.To.HasValue)
            {
                tradeQuery = tradeQuery.Where(row => row.Date <= queryParams.To.Value);
            }

            if (queryParams.BIC != null)
            {
                tradeQuery = tradeQuery.Where(row => row.BIC == queryParams.BIC);
            }

            if (queryParams.ISIN != null)
            {
                tradeQuery = tradeQuery.Where(row => row.ISIN == queryParams.ISIN);
            }

            if (queryParams.Limit.HasValue)
            {
                tradeQuery = tradeQuery.Take(queryParams.Limit.Value);
            }

            IEnumerable<Trade> result = await tradeQuery
                .OrderBy(row => row.Date)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("compute")]
        public async Task<ActionResult> ComputeEverything()
        {
            DateTime starting = DateTime.Now;
            DateTime final = new DateTime(2013, 1, 1);
            int currentStep = 0;
            int stepsBeforeSleep = 10;

            for (DateTime date = starting; date >= final; date = date.AddDays(-1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                try
                {
                    await this.GetTradesForDay(date);
                    currentStep++;

                    if (currentStep % stepsBeforeSleep == 0)
                    {
                        // Thread.Sleep(3 * 1000);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Error processing date: {Date}", date.ToShortDateString());
                    _logger.LogError(e, e.Message);
                }
            }

            return Ok();
        }
    }
}
