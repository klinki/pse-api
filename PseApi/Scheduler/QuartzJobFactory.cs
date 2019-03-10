using Quartz;
using Quartz.Spi;
using System;

namespace PseApi.Scheduler.Quartz
{
    public class QuartzJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public QuartzJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;

            var runner = (ScopedJobRunner)_serviceProvider.GetService(typeof(ScopedJobRunner));
            runner.SetType(jobDetail.JobType);

            return runner;
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
