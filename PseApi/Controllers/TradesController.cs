using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
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
        private readonly StockService _stockService;
        private readonly ILogger<TradesController> _logger;

        public TradesController(PseContext context, TradeService tradeService, ILogger<TradesController> logger, StockService stockService)
        {
            _context = context;
            _tradeService = tradeService;
            _logger = logger;
            _stockService = stockService;
        }

        // GET api/values
        [HttpGet("day/{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesForDay([FromRoute] DateTime day)
        {
            if (!await _tradeService.IsValidDate(day))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date",
                    Detail = $"'{day}' is not a valid trading day."
                });
            }

            IEnumerable<Trade> result = await _tradeService.GetTradesForDay(day);

            return Ok(result);
        }

        // GET api/values
        [HttpGet("{stock}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTrades([FromRoute] string stock, [FromQuery] TradeQuery queryParams)
        {
            Stock stockObject = await _stockService.GetStockByBicAsync(stock);

            if (stockObject == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Stock not found",
                    Detail = $"Stock with BIC {stock} was not found"
                });
            }

            IQueryable<Trade> tradeQuery = BuildQuery(queryParams);

            IEnumerable<Trade> result = await tradeQuery
                .Where(row => row.BIC == stock)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/values
        [HttpGet("isin/{isin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesByIsin([FromRoute] string isin, [FromQuery] TradeQuery queryParams)
        {
            Stock stockObject = await _stockService.GetStockByIsinAsync(isin);

            if (stockObject == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Stock not found",
                    Detail = $"Stock with ISIN {isin} was not found"
                });
            }

            IQueryable<Trade> tradeQuery = BuildQuery(queryParams);

            IEnumerable<Trade> result = await tradeQuery
                .Where(row => row.ISIN == isin)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesByQuery([FromQuery] TradeQuery queryParams)
        {
            IQueryable<Trade> tradeQuery = BuildQuery(queryParams);

            IEnumerable<Trade> result = await tradeQuery
                .OrderBy(row => row.Date)
                .ToListAsync();

            return Ok(result);
        }

        protected IQueryable<Trade> BuildQuery(TradeQuery queryParams)
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

            return tradeQuery;
        }
    }
}
