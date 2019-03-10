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
        ILogger<IScheduler> _logger;

        public QuartzLogger(ILogger<IScheduler> logger)
        {
            _logger = logger;
        }

        protected MSLogLevel GetMicrosoftLogLevel(QuartzLogLevel quartzLogLevel)
        {
            switch (quartzLogLevel)
            {
                default:
                case QuartzLogLevel.Info:
                    return MSLogLevel.Information;
                case QuartzLogLevel.Debug:
                    return MSLogLevel.Debug;
                case QuartzLogLevel.Error:
                    return MSLogLevel.Error;
                case QuartzLogLevel.Fatal:
                    return MSLogLevel.Critical;
                case QuartzLogLevel.Trace:
                    return MSLogLevel.Trace;
                case QuartzLogLevel.Warn:
                    return MSLogLevel.Warning;
            }
        }

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null)
                {
                    _logger.Log(GetMicrosoftLogLevel(level), func(), parameters);
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
