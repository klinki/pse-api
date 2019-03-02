using System;

namespace PseApi.Queries
{
    public class TradeQuery
    {
        public string BIC { get; set; }

        public string ISIN { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public int? Limit { get; set; }
    }
}
