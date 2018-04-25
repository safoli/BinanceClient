using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase.DTO
{
    public class Order
    {
        public string symbol { get; set; }
        public string orderId { get; set; }
        public string clientOrderId { get; set; }
        public decimal price { get; set; }
        public decimal stopPrice { get; set; }
        public string status { get; set; }
        public double origQty { get; set; }
        public string side { get; set; }
        public string type { get; set; }

        [JsonConverter(typeof(BinanceTimeConverter))]
        public DateTime time { get; set; }
    }
}
