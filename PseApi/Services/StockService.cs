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

        public Task<Stock> GetStockByIdAsync(long id)
        {
            return _context.Stocks.FindAsync(id);
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
    }
}
