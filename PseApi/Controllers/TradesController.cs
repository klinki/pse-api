using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly ITradeService _tradeService;
        private readonly StockService _stockService;
        private readonly ILogger<TradesController> _logger;

        public TradesController(PseContext context, ITradeService tradeService, ILogger<TradesController> logger, StockService stockService)
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            try
            {
                return Ok(await _tradeService.GetTradesForDay(day));
            }
            catch (TradingDataNotFoundException)
            {
                // Holidays, days not published yet and days older than the PSE archive
                return NotFound(new ProblemDetails
                {
                    Title = "No trading data",
                    Detail = $"The exchange has no trading data for {day:yyyy-MM-dd}."
                });
            }
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

            IEnumerable<Trade> result = await BuildQuery(queryParams, row => row.BIC == stock)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/values
        [HttpGet("isin/{isin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesByIsin([FromRoute] string isin, [FromQuery] TradeQuery queryParams)
        {
            if (!await _stockService.EnsureStockForIsinAsync(isin))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Stock not found",
                    Detail = $"Stock with ISIN {isin} was not found"
                });
            }

            IEnumerable<Trade> result = await BuildQuery(queryParams, row => row.ISIN == isin)
                .ToListAsync();

            return Ok(result);
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetTradesByQuery([FromQuery] TradeQuery queryParams)
        {
            if (!queryParams.HasFilter())
            {
                // Without a filter this would return the whole table (about a million rows)
                return BadRequest(new ProblemDetails
                {
                    Title = "Missing filter",
                    Detail = "Specify at least one of bic, isin, from or to."
                });
            }

            IEnumerable<Trade> result = await BuildQuery(queryParams)
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// Applies the query filters, orders the trades by date (oldest first) and then applies the limit.
        /// </summary>
        protected IQueryable<Trade> BuildQuery(TradeQuery queryParams, Expression<Func<Trade, bool>> filter = null)
        {
            IQueryable<Trade> tradeQuery = _context.Trades.AsNoTracking();

            if (filter != null)
            {
                tradeQuery = tradeQuery.Where(filter);
            }

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

            // Order before limiting, otherwise the limit picks arbitrary rows
            tradeQuery = tradeQuery
                .OrderBy(row => row.Date)
                .ThenBy(row => row.ISIN);

            if (queryParams.Limit.HasValue)
            {
                tradeQuery = tradeQuery.Take(queryParams.Limit.Value);
            }

            return tradeQuery;
        }
    }
}
