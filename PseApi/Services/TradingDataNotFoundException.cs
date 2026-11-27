using System;

namespace PseApi.Services
{
    /// <summary>
    /// The exchange publishes no trading data for the requested day: an exchange holiday, a day whose
    /// results are not published yet, or a day older than the exchange's archive.
    /// </summary>
    public class TradingDataNotFoundException : Exception
    {
        public TradingDataNotFoundException(DateTime day, Exception innerException)
            : base($"No trading data for {day:yyyy-MM-dd}", innerException)
        {
            Day = day;
        }

        public DateTime Day { get; }
    }
}
