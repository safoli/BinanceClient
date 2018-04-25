using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase.DTO
{
    
    public class Kline
    {

        public DateTime openTime { get; set; }
        public decimal openPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal lowPrice { get; set; }
        public decimal closePrice { get; set; }
        public decimal volume { get; set; }
        public DateTime closeTime { get; set; }
        public decimal quoteAssetVolume { get; set; }
        public int numberOfTrades { get; set; }
        public decimal takerBuyBaseAssetVolume { get; set; }
        public decimal takerBuyQuoteAssetVolume { get; set; }

        public override string ToString()
        {
            return $"diff :{openPrice - closePrice} vol :{volume}";
        }
    }
}
