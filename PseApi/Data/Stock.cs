using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PseApi.Data
{
    public class Stock
    {
        public long Id { get; set; }

        public string BIC { get; set; }

        public string Name { get; set; }

        public string ISIN { get; set; }
    }
}
