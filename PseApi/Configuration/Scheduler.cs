using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PseApi.Jobs;
using Quartz;

namespace PseApi.Configuration
{
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Adds Quartz to DI and schedules the application's jobs.
        /// Jobs are resolved from a new DI scope for every execution.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddQuartz(this IServiceCollection services, [NotNull] IConfiguration configuration)
        {
            services.AddQuartz(configuration.GetSection("quartz"), quartz =>
            {
                quartz.UseDefaultThreadPool(2);

                quartz.ScheduleJob<UpdateTrades>(
                    trigger => trigger
                        .WithIdentity($"{typeof(UpdateTrades).FullName}.trigger")
                        .WithDailyTimeIntervalSchedule(interval => interval
                            .OnMondayThroughFriday()
                            .StartingDailyAt(new TimeOnly(23, 50)))
                        .StartNow(),
                    job => job.WithIdentity(typeof(UpdateTrades).FullName));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }
}
