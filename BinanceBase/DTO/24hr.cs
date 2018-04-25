using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase.DTO
{
    public class _24hr
    {
        public string symbol { get; set; }
        public decimal priceChange { get; set; }
        public decimal priceChangePercent { get; set; }
        public decimal weightedAvgPrice { get; set; }
        public decimal prevClosePrice { get; set; }
        public decimal lastPrice { get; set; }
        public decimal lastQty { get; set; }
        public decimal bidPrice { get; set; }
        public decimal askPrice { get; set; }
        public decimal openPrice { get; set; }
        public decimal highPrice { get; set; }
        public decimal lowPrice { get; set; }
        public decimal volume { get; set; }
        public decimal quoteVolume { get; set; }

        public int fristId { get; set; }   // First tradeId
        public int lastId { get; set; }    // Last tradeId
        public int count { get; set; }   // Trade count

        [JsonConverter(typeof(BinanceTimeConverter))]
        public DateTime openTime { get; set; }

        [JsonConverter(typeof(BinanceTimeConverter))]
        public DateTime closeTime { get; set; }
    }
}
