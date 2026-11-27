using Microsoft.Extensions.Logging;
using PseApi.Services;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PseApi.Jobs
{
    public class UpdateTrades : IJob
    {
        private readonly ILogger<UpdateTrades> _logger;
        private readonly ITradeService _tradeService;

        public UpdateTrades(ILogger<UpdateTrades> logger, ITradeService tradeService)
        {
            _logger = logger;
            _tradeService = tradeService;
        }

        /// <summary>
        /// Gets new trades
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started {Name} execution", nameof(UpdateTrades));

            if (await _tradeService.IsValidDate(DateTime.Today))
            {
                try
                {
                    await _tradeService.GetTradesForDay(DateTime.Today);
                }
                catch (TradingDataNotFoundException)
                {
                    // Exchange holiday: PSE publishes no results file for the day
                    _logger.LogInformation("No trading data for {Date}", DateTime.Today);
                }
            }

            _logger.LogInformation("Finished {Name} execution", nameof(UpdateTrades));
        }
    }
}
