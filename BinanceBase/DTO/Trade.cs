using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase.DTO
{
    public class Trade
    {
        public string id { get; set; }
        public string orderId { get; set; }
        public decimal price { get; set; }
        public double qty { get; set; }
        public bool isBuyer { get; set; }
        public bool isMaker { get; set; }
        public bool isBestMatch { get; set; }

        [JsonConverter(typeof(BinanceTimeConverter))]
        public DateTime time { get; set; }
    }
}
