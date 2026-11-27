using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PseApi.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PseApi.Services
{
    public class StockService
    {
        private readonly PseContext _context;
        private readonly ILogger<StockService> _logger;

        public StockService(PseContext context, ILogger<StockService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Stock> GetStockByIdAsync(long id)
        {
            return await _context.Stocks.FindAsync(id);
        }

        public Task<Stock> GetStockByBicAsync(string bic)
        {
            return _context.Stocks.Where(stock => stock.BIC == bic)
                .SingleOrDefaultAsync();
        }

        public Task<Stock> GetStockByIsinAsync(string isin)
        {
            return _context.Stocks.Where(stock => stock.ISIN == isin)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Makes sure a stock with the given ISIN is known, creating it from its trades if needed.
        /// </summary>
        /// <returns>False when neither a stock nor any trade with that ISIN exists.</returns>
        public async Task<bool> EnsureStockForIsinAsync(string isin)
        {
            if (await _context.Stocks.AnyAsync(stock => stock.ISIN == isin))
            {
                return true;
            }

            var trade = await _context.Trades
                .AsNoTracking()
                .Where(row => row.ISIN == isin)
                .OrderByDescending(row => row.Date)
                .FirstOrDefaultAsync();

            if (trade == null)
            {
                return false;
            }

            // Trades of stocks without a BIC have an empty one; stocks store those as NULL, which the
            // unique index allows any number of times
            var bic = string.IsNullOrEmpty(trade.BIC) ? null : trade.BIC;

            if (bic != null && await _context.Stocks.AnyAsync(stock => stock.BIC == bic))
            {
                // The BIC still belongs to another stock (e.g. the ISIN changed); serve the trades without a stock
                _logger.LogDebug("Stock {ISIN} not created, BIC {BIC} belongs to another stock", isin, bic);
                return true;
            }

            var stock = new Stock
            {
                Name = trade.Name,
                BIC = bic,
                ISIN = trade.ISIN,
            };

            _context.Stocks.Add(stock);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.IsUniqueViolation())
            {
                // A concurrent request created the same stock; the trades exist either way
                _context.Entry(stock).State = EntityState.Detached;
                _logger.LogWarning("Stock {ISIN} ({BIC}) was not created: {Reason}", isin, bic, e.InnerException?.Message);
            }

            return true;
        }
    }
}
