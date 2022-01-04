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
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace PseApi.Configuration
{
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Adds Quartz to DI
        /// </summary>
        /// <param name="services"></param>
        public static void AddQuartz(this IServiceCollection services, [NotNull] IConfiguration configuration)
        {
            services.AddSingleton<IJobFactory, QuartzJobFactory>();

            services.Configure<QuartzOptions>(configuration.GetSection("quartz"));
            services.Configure<QuartzOptions>(options =>
            {
                options["quartz.serializer.type"] = "json";
                options["quartz.threadPool.threadCount"] = "2";
            });

            services.AddQuartz(configurator =>
            {
                configurator.UseJobFactory<QuartzJobFactory>();
            });
            services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            services.AddSingleton<IScheduler>(sp =>
            {
                var schedulerFactory = sp.GetRequiredService<ISchedulerFactory>();
                var scheduler = schedulerFactory.GetScheduler().Result;

                return scheduler;
            });
            
            services.RegisterJob<ScopedJobRunner>();
            services.RegisterJob<UpdateTrades>();
        }

        public static void RegisterJob<TJob>(this IServiceCollection services) where TJob : class, IJob
        {
            services.AddScoped<TJob>();
        }

        public static void UseScheduler(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            var scheduler = app.ApplicationServices.GetRequiredService<IScheduler>();

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
            var scheduler = app.ApplicationServices.GetRequiredService<IScheduler>();

            var jobName = typeof(TJob).FullName;

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
