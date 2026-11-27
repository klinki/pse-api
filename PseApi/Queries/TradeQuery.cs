using System;
using System.ComponentModel.DataAnnotations;

namespace PseApi.Queries
{
    public class TradeQuery
    {
        public string BIC { get; set; }

        public string ISIN { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        /// <summary>
        /// Maximum number of trades to return, the oldest first.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? Limit { get; set; }

        /// <summary>
        /// True when at least one filter narrows the result down (limit alone does not count).
        /// </summary>
        public bool HasFilter() => BIC != null || ISIN != null || From.HasValue || To.HasValue;
    }
}
