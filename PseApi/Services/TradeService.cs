using Microsoft.EntityFrameworkCore;
using PseApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

            if (await IsDownloaded(day))
            {
                return await LoadTradesForDay(day);
            }

            // One download at a time: FinSharp saves the results file to a fixed temp path, so parallel
            // downloads of the same day fail, and the second one would only find the day already stored
            await DownloadLock.WaitAsync();
            try
            {
                return await DownloadTradesForDay(day);
            }
            finally
            {
                DownloadLock.Release();
            }
        }

        private static readonly SemaphoreSlim DownloadLock = new(1, 1);

        private Task<bool> IsDownloaded(DateTime day) =>
            _context.Datasets.AsNoTracking().AnyAsync(row => row.Day == day);

        private async Task<IEnumerable<Trade>> DownloadTradesForDay(DateTime day)
        {
            Dataset dataset = await _context.Datasets.AsNoTracking()
                .SingleOrDefaultAsync(row => row.Day == day);
            IEnumerable<Trade> trades;

            if (dataset == null)
            {
                _logger.LogInformation("Data for date: {Date} not downloaded yet, downloading...", day);

                List<PragueStockExchangeCsvRow> csvData;
                try
                {
                    csvData = (await _pseClient.GetData(day)).ToList();
                }
                // FinSharp's DataNotFoundException is internal; only its base class is public
                catch (FinSharp.Api.InvestmentApiException e) when (e.GetType().Name == "DataNotFoundException")
                {
                    throw new TradingDataNotFoundException(day, e);
                }

                _logger.LogInformation("Downloaded: {Count} records", csvData.Count());

                int countNullValues = csvData.Count(csvTrade => csvTrade == null);

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
                }).ToList();

                dataset = new Dataset
                {
                    Day = day
                };

                try
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    await _context.Trades.AddRangeAsync(trades);
                    await _context.Datasets.AddAsync(dataset);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException e) when (e.IsUniqueViolation())
                {
                    // Another request (or the nightly job) stored the same day in the meantime
                    _logger.LogInformation("Data for date: {Date} was stored concurrently, loading it from the database", day);
                    _context.ChangeTracker.Clear();
                    return await LoadTradesForDay(day);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception thrown when saving data to database");
                    throw;
                }

                return trades.OrderBy(trade => trade.ISIN, StringComparer.Ordinal).ToList();
            }

            return await LoadTradesForDay(day);
        }

        private async Task<IEnumerable<Trade>> LoadTradesForDay(DateTime day) =>
            await _context.Trades
                .AsNoTracking()
                .Where(trade => trade.Date == day)
                .OrderBy(trade => trade.ISIN)
                .ToListAsync();
    }
}
