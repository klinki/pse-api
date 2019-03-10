using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace PseApi.Scheduler
{
    /// <summary>
    /// ScopedJobRunner executes job within a new scope.
    /// </summary>
    public class ScopedJobRunner : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public Type Type { get; private set; }

        public ScopedJobRunner(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void SetType(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Safely executes job within a new scope.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ScopedJobRunner>>();
                var job = (IJob)scope.ServiceProvider.GetRequiredService(this.Type);

                logger.LogInformation("Started {Name} execution", nameof(ScopedJobRunner));

                await job.Execute(context);

                logger.LogInformation("Finished {Name} execution", nameof(ScopedJobRunner));
            }

            await Task.CompletedTask;
        }
    }
}
