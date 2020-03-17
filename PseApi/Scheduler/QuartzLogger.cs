using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;
using System;

using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using QuartzLogLevel = Quartz.Logging.LogLevel;


namespace PseApi.Scheduler
{
    public class QuartzLogger : ILogProvider
    {
        private readonly ILogger<IScheduler> _logger;

        public QuartzLogger(ILogger<IScheduler> logger)
        {
            _logger = logger;
        }

        protected MSLogLevel GetMicrosoftLogLevel(QuartzLogLevel quartzLogLevel)
        {
            return quartzLogLevel switch
            {
                QuartzLogLevel.Info => MSLogLevel.Information,
                QuartzLogLevel.Debug => MSLogLevel.Debug,
                QuartzLogLevel.Error => MSLogLevel.Error,
                QuartzLogLevel.Fatal => MSLogLevel.Critical,
                QuartzLogLevel.Trace => MSLogLevel.Trace,
                QuartzLogLevel.Warn => MSLogLevel.Warning,
                _ => MSLogLevel.Information
            };
        }

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null)
                {
                    _logger.Log(GetMicrosoftLogLevel(level), exception, func(), parameters);
                }

                return true;
            };
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }
    }
}
