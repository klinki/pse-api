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
    public class TradeService
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

        public async Task<IEnumerable<Trade>> GetTradesForDay(DateTime day)
        {
            Dataset dataset = await _context.Datasets.AsNoTracking().SingleOrDefaultAsync(row => row.Day == day);
            IEnumerable<Trade> trades;

            if (dataset == null)
            {
                IEnumerable<PragueStockExchangeCsvRow> csvData = await _pseClient.GetData(day);

                if (csvData.Where(csvTrade => csvTrade == null).Count() > 0)
                {
                    _logger.LogError("Data null for date: {Date}", day);
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
