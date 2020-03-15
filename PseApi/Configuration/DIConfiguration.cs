using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PseApi.Services;

namespace PseApi.Configuration
{
    /// <summary>
    /// DI Container configuration class.
    /// </summary>
    public static class DIConfiguration
    {
        /// <summary>
        /// Extension method registering services to DI container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITradeService, TradeService>();
            services.AddScoped<StockService>();
            services.AddScoped<FinSharp.PragueStockExchange.PragueStockExchangeApiClient>();
            services.AddTransient<AppInfoService>();

            return services;
        }
    }
}
