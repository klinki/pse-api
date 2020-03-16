using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PseApi.Jobs;
using PseApi.Scheduler;
using PseApi.Scheduler.Quartz;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Specialized;
using Serilog;

namespace PseApi.Configuration
{
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Adds Quartz to DI
        /// </summary>
        /// <param name="services"></param>
        public static void AddQuartz(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, QuartzJobFactory>();
            services.AddSingleton<QuartzLogger>();

            services.AddSingleton(provider =>
            {
                var properties = new NameValueCollection
                {
                    // json serialization is the one supported under .NET Core (binary isn't)
                    ["quartz.serializer.type"] = "json",

                    ["quartz.threadPool.threadCount"] = "2"
                };

                ISchedulerFactory schedulerFactory = new StdSchedulerFactory(properties);
                IScheduler scheduler = schedulerFactory.GetScheduler().Result;

                scheduler.JobFactory = provider.GetService<IJobFactory>();

                return scheduler;
            });

            services.RegisterJob<ScopedJobRunner>();
            services.RegisterJob<UpdateTrades>();
        }

        public static void RegisterJob<TJob>(this IServiceCollection services) where TJob : class, IJob
        {
            services.AddSingleton<TJob>();
        }

        public static void UseScheduler(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            QuartzLogger logger = app.ApplicationServices.GetService<QuartzLogger>();
            LogProvider.SetCurrentLogProvider(logger);

            IScheduler scheduler = app.ApplicationServices.GetRequiredService<IScheduler>();

            lifetime.ApplicationStarted.Register(() => scheduler.Start());
            lifetime.ApplicationStopping.Register(() => scheduler.Shutdown());

            app.ScheduleJob<UpdateTrades>(
                TriggerBuilder.Create()
                    .WithDailyTimeIntervalSchedule(interval => interval
                        .OnMondayThroughFriday()
                        .StartingDailyAt(new TimeOfDay(23, 50))
                    )
                    .StartNow()
            );
        }

        public static void ScheduleJob<TJob>(this IApplicationBuilder app, TriggerBuilder triggerBuilder) where TJob: class, IJob
        {
            IScheduler scheduler = app.ApplicationServices.GetRequiredService<IScheduler>();

            string jobName = typeof(TJob).FullName;

            var job = JobBuilder.Create<TJob>()
                .WithIdentity(jobName)
                .Build();

            triggerBuilder = triggerBuilder
                .ForJob(job)
                .WithIdentity($"{jobName}.trigger");

            var trigger = triggerBuilder.Build();

            // TODO: Fix this issue - currently probably only in tests
            if (scheduler.CheckExists(job.Key).Result)
            {
                Log.Logger.Warning("Job should not exist but it does. {@JobKey}", job.Key);
                scheduler.DeleteJob(job.Key).Wait();
            }
            
            scheduler.ScheduleJob(job, trigger).Wait();
        }

        public static void ScheduleJob<TJob>(this IApplicationBuilder app, TimeSpan interval) where TJob : class, IJob
        {
            var triggerBuilder = TriggerBuilder.Create();

#if DEBUG
            triggerBuilder = triggerBuilder.WithSimpleSchedule(builder => builder
                .WithInterval(new TimeSpan(0, 0, 10))
                .RepeatForever());
#else
            triggerBuilder = triggerBuilder.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(1, 0));
#endif

            var trigger = triggerBuilder
                .StartNow()
                .Build();

            app.ScheduleJob<TJob>(triggerBuilder);
        }
    }
}
