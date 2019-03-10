﻿using Microsoft.Extensions.Logging;
using PseApi.Services;
using Quartz;
using System;
using System.Threading.Tasks;

namespace PseApi.Jobs
{
    public class UpdateTrades : IJob
    {
        private readonly ILogger<UpdateTrades> _logger;
        private readonly TradeService _tradeService;

        public UpdateTrades(ILogger<UpdateTrades> logger, TradeService tradeService)
        {
            _logger = logger;
            _tradeService = tradeService;
        }

        /// <summary>
        /// Gets new trades
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Started {Name} execution", nameof(UpdateTrades));

            if (await _tradeService.IsValidDate(DateTime.Now))
            {
                await _tradeService.GetTradesForDay(DateTime.Now);
            }

            _logger.LogInformation("Finished {Name} execution", nameof(UpdateTrades));
            await Task.CompletedTask;
        }
    }
}
