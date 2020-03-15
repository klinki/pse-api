using Microsoft.EntityFrameworkCore;
using PseApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinSharp.PragueStockExchange;
using Microsoft.Extensions.Logging;

namespace PseApi.Services
{
    public interface ITradeService
    {
        Task<bool> IsValidDate(DateTime date);
        Task<IEnumerable<Trade>> GetTradesForDay(DateTime day);
    }
    
    public class TradeService : ITradeService
    {
        private readonly PseContext _context;
        private readonly PragueStockExchangeApiClient _pseClient;
        private readonly ILogger<TradeService> _logger;

        public TradeService(PseContext context, PragueStockExchangeApiClient pseClient, ILogger<TradeService> logger)
        {
            _context = context;
            _pseClient = pseClient;
            _logger = logger;
        }

        public async Task<bool> IsValidDate(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || date > DateTime.Now)
            {
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<Trade>> GetTradesForDay(DateTime day)
        {
            day = new DateTime(day.Year, day.Month, day.Day);

            Dataset dataset = await _context.Datasets.AsNoTracking().SingleOrDefaultAsync(row => row.Day == day);
            IEnumerable<Trade> trades;

            if (dataset == null)
            {
                _logger.LogInformation("Data for date: {Date} not downloaded yet, downloading...", day);

                IEnumerable<PragueStockExchangeCsvRow> csvData = await _pseClient.GetData(day);

                _logger.LogInformation("Downloaded: {Count} records", csvData.Count());

                int countNullValues = csvData.Where(csvTrade => csvTrade == null).Count();

                if (countNullValues > 0)
                {
                    _logger.LogWarning("Dataset contains {Count} null values", countNullValues);
                }

                trades = csvData.Where(csvTrade => csvTrade != null).Select(csvTrade => new Trade
                {
                    BIC = csvTrade.BIC,
                    Change = csvTrade.Change,
                    Close = csvTrade.Close,
                    Date = csvTrade.Date,
                    DayMax = csvTrade.DayMax,
                    DayMin = csvTrade.DayMin,
                    ISIN = csvTrade.ISIN,
                    LastTrade = csvTrade.LastTrade,
                    LotSize = csvTrade.LotSize,
                    MarketCode = csvTrade.MarketCode,
                    MarketGroup = csvTrade.MarketGroup,
                    YearMin = csvTrade.YearMin,
                    YearMax = csvTrade.YearMax,
                    Mode = csvTrade.Mode,
                    Name = csvTrade.Name,
                    Open = csvTrade.Open,
                    Previous = csvTrade.Previous,
                    TradedAmount = csvTrade.TradedAmount,
                    Volume = csvTrade.Volume
                });

                dataset = new Dataset
                {
                    Day = day
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.Trades.AddRangeAsync(trades);
                        await _context.Datasets.AddAsync(dataset);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception thrown when saving data to database");
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                trades = await _context.Trades
                    .AsNoTracking()
                    .Where(trade => trade.Date == day)
                    .ToListAsync();
            }

            return trades;
        }
    }
}
