using BinanceBase;
using BinanceBase.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StopTrack
{
    public class StopTracker
    {

        BinanceClient Client;
        ExchangeInfo MarketExchangeInfo;
        
        public StopTracker()
        {
            SetupClient();
        }

        private void SetupClient()
        {
            Client = new BinanceClient();
            Client.Ping();
            MarketExchangeInfo = Client.GetExchangeInfo();
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    var orders = Client.GetOpenOrders();
                    orders.ToList().ForEach((order) => Track(order));

                    //Task.Delay(TimeSpan.FromSeconds(60));
                    //System.Threading.Thread.Sleep(30 * 1000);
                    System.Threading.Thread.Sleep(15 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try { SetupClient(); } catch (Exception) { } //try setup
                    Console.WriteLine("Hata sonrası izlemeye tekrar başla");
                }
            }
        }

        void Track(Order order)
        {
            try
            {
                if (order.side != "SELL" || order.type != "STOP_LOSS_LIMIT")
                {
                    return;
                }

                //read market rules
                var symbolFilters = MarketExchangeInfo.symbols.Where((i) => i.symbol == order.symbol).First().filters;
                var minQty = symbolFilters.Where((i) => i.filterType == "LOT_SIZE").First().minQty;
                var minPrice = symbolFilters.Where((i) => i.filterType == "PRICE_FILTER").First().minPrice;

                //low level hesapla
                var price = Client.GetPrice(order.symbol);
                var cost = GetCostByOrders(order.symbol);
                decimal stopTrackPercent = 0.07m;
                decimal profit = 0;

                if (cost != 0)
                    profit = (price - cost) / cost;

                if (profit > 0) //kar varsa
                {
                    if (profit < 0.05m)
                        stopTrackPercent = 0.06m;
                    else if (profit < 0.1m)
                        stopTrackPercent = 0.05m;
                    else if (profit < 0.2m)
                        stopTrackPercent = 0.04m;
                    else if (profit < 0.3m)
                        stopTrackPercent = 0.035m;
                    else if (profit < 0.5m)
                        stopTrackPercent = 0.03m;
                    else if (profit < 0.75m)
                        stopTrackPercent = 0.025m;
                    else
                        stopTrackPercent = 0.02m;
                }

                var lowlevelPrice = Common.FixPrice(price * (1 - stopTrackPercent), minPrice);
                Console.WriteLine($"{order.symbol} {Math.Round(profit * 100, 2)}% | price :{price} | curr stop :{order.stopPrice} |rate :{stopTrackPercent * 100}% stop :{lowlevelPrice} | {DateTime.Now}");

                //uygula
                if (order.stopPrice < lowlevelPrice)
                {
                    Client.Cancel(order.symbol, order.clientOrderId);
                    Client.Sell(order.symbol, order.origQty, Common.FixPrice(lowlevelPrice - (10 * minPrice), minPrice), lowlevelPrice); //stop un 10 birim altına satış gir
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        double GetCost(string symbol)
        {
            var trades = Client.GetTrades(symbol);

            var buyTrades = trades.Where((trade) => trade.isBuyer).ToList();
            var sellTrades = trades.Where((trade) => trade.isMaker).ToList();

            var sellSumQty = sellTrades.Sum((trade) => trade.qty);

            foreach (var buyTrade in buyTrades)
            {
                if (sellSumQty <= 0)
                    continue;

                if (buyTrade.qty <= sellSumQty)
                {
                    sellSumQty -= buyTrade.qty;
                    buyTrade.qty = 0;
                }
                else
                {
                    buyTrade.qty -= sellSumQty;
                    sellSumQty = 0;
                }
            }

            return 0;
        }

        decimal GetCostByOrders(string symbol)
        {
            var allOrders = Client.GetAllOrders(symbol);
            var filledOrders = allOrders.Where((x) => x.status == "FILLED").ToList();

            var buyOrders = filledOrders.Where((x) => x.side == "BUY").ToList();
            var sellOrders = filledOrders.Where((x) => x.side == "SELL").ToList();

            var sellSumQty = sellOrders.Sum((x) => x.origQty);

            //sell leri yedir
            foreach (var buyOrder in buyOrders)
            {
                if (sellSumQty <= 0)
                    continue;

                if (buyOrder.origQty <= sellSumQty)
                {
                    sellSumQty -= buyOrder.origQty;
                    buyOrder.origQty = 0;
                }
                else
                {
                    buyOrder.origQty -= sellSumQty;
                    sellSumQty = 0;
                }
            }

            //buy hesapla
            var sumQty = buyOrders.Sum((x) => x.origQty);
            var sumAmout = buyOrders.Sum((x) => (decimal)x.origQty * x.price);

            var symbolFilters = MarketExchangeInfo.symbols.Where((i) => i.symbol == symbol).First().filters;
            var minPrice = symbolFilters.Where((i) => i.filterType == "PRICE_FILTER").First().minPrice;

            return sumQty == 0 || sumAmout == 0 ? 0 : Common.FixPrice(sumAmout / (decimal)sumQty, minPrice);
        }

    }
}
