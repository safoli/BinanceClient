using BinanceBase;
using BinanceBase.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoStopTrack
{
    class AutoStopTracker
    {

        BinanceClient Client;
        ExchangeInfo MarketExchangeInfo;

        decimal MIN_BTC_LIMIT = 0.001m;

        public AutoStopTracker()
        {
            SetupClient();
        }

        private void SetupClient()
        {
            Client = new BinanceClient();
            Client.Ping();
            MarketExchangeInfo = Client.GetExchangeInfo();
        }

        private MarketMinRule GetRule(string symbol)
        {
            var rule = new MarketMinRule() { Symbol = symbol };
            var symbolFilters = MarketExchangeInfo.symbols.Where((i) => i.symbol == symbol).First().filters;
            rule.MinQty = symbolFilters.Where((i) => i.filterType == "LOT_SIZE").First().minQty;
            rule.MinPrice = symbolFilters.Where((i) => i.filterType == "PRICE_FILTER").First().minPrice;

            return rule;
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    BalanceTrack();
                    System.Threading.Thread.Sleep(2 * 1000);

                    OrderTrack();
                    System.Threading.Thread.Sleep(10 * 1000);
                }
                catch (Exception ex)
                {
                    Common.Logger.Write(ex.Message);
                    try { SetupClient(); } catch (Exception) { } //try setup
                    Common.Logger.Write("Hata sonrası izlemeye tekrar başla");
                }
            }
        }

        #region Balance Track

        void BalanceTrack()
        {
            var balances = GetBalance();

            if (balances.Count == 0)
            {
                Common.Logger.Write($"işlem yapacak bakiye yok. {DateTime.Now}");
            }
            else
            {
                balances.ForEach((balance) =>
                {
                    decimal profit;
                    var symbol = balance.asset + "BTC";
                    var rule = GetRule(symbol);
                    var qty = Common.FixQty(balance.free, rule.MinQty);

                    if (balance.CostPrice == 0)
                    {
                        Common.Logger.Write($"{balance.asset} | Balance :{balance.free * balance.CurrentPrice}BTC | DIKKAT : cost = 0, atlanıyor. | {DateTime.Now}");
                        return;
                    }
                    else
                    {
                        profit = (balance.CurrentPrice - balance.CostPrice) / balance.CostPrice;
                        Common.Logger.Write($"{balance.asset} | {Math.Round(profit * 100, 2)}% | cost :{balance.CostPrice}BTC | price :{balance.CurrentPrice}BTC | qty :{qty} | {DateTime.Now}");
                    }

                    if (profit < 0.07m) //en az %7 kar yoksa emir girme
                        return;

                    //emri gir
                    var stopTrackPercent = GetStopPercent(profit);
                    var lowlevelPrice = Common.FixPrice(balance.CurrentPrice * (1 - stopTrackPercent), rule.MinPrice);

                    Common.Logger.Write($"{symbol} {Math.Round(profit * 100, 2)}% | price :{balance.CurrentPrice} |rate :{stopTrackPercent * 100}% | stop :{lowlevelPrice} | {DateTime.Now}");

                    Client.Sell(symbol, (double)qty, Common.FixPrice(lowlevelPrice - (10 * rule.MinPrice), rule.MinPrice), lowlevelPrice); //stop un 10 birim altına satış gir
                });
            }
        }

        private List<BalanceWrapper> GetBalance()
        {
            var balances = Client.GetBalance();
            balances.RemoveAll((x) => x.free == 0 || new string[] { "BTC", "USDT", "BNB" }.Contains(x.asset));

            var balanceWrapperList = new List<BalanceWrapper>();

            foreach (var balance in balances)
            {
                var balanceWrapper = new BalanceWrapper(balance);
                balanceWrapper.CurrentPrice = Client.GetPrice(balance.asset + "BTC");

                var amount = balance.free * balanceWrapper.CurrentPrice;

                if (amount >= MIN_BTC_LIMIT)
                {
                    balanceWrapper.CostPrice = GetCostByOrders(balance.asset + "BTC");
                    balanceWrapperList.Add(balanceWrapper);
                }
            }

            return balanceWrapperList;
        }

        #endregion

        #region Order Track

        void OrderTrack()
        {
            var orders = Client.GetOpenOrders();
            orders.ToList().ForEach((order) => Track(order));
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
                var rule = GetRule(order.symbol);

                //low level hesapla
                var price = Client.GetPrice(order.symbol);
                var cost = GetCostByOrders(order.symbol);
                var profit = (price - cost) / cost;
                var stopTrackPercent = GetStopPercent(profit);

                var lowlevelPrice = Common.FixPrice(price * (1 - stopTrackPercent), rule.MinPrice);
                Common.Logger.Write($"{order.symbol} {Math.Round(profit * 100, 2)}% | price :{price} | curr stop :{order.stopPrice} | rate :{stopTrackPercent * 100}% | stop :{lowlevelPrice} | {DateTime.Now}");

                //uygula
                if (order.stopPrice < lowlevelPrice)
                {
                    Client.Cancel(order.symbol, order.clientOrderId);
                    Client.Sell(order.symbol, order.origQty, Common.FixPrice(lowlevelPrice - (10 * rule.MinPrice), rule.MinPrice), lowlevelPrice); //stop un 10 birim altına satış gir
                }
            }
            catch (Exception ex)
            {

                Common.Logger.Write(ex.Message);
            }
        }

        #endregion

        private decimal GetStopPercent(decimal profit)
        {
            decimal stopTrackPercent = 0.05m;

            if (profit < 0.1m)
                stopTrackPercent = 0.045m;
            else if (profit < 0.125m)
                stopTrackPercent = 0.04m;
            else if (profit < 0.15m)
                stopTrackPercent = 0.035m;
            else if (profit < 0.2m)
                stopTrackPercent = 0.03m;
            else if (profit < 0.3m)
                stopTrackPercent = 0.027m;
            else if (profit < 0.5m)
                stopTrackPercent = 0.025m;
            else if (profit < 0.75m)
                stopTrackPercent = 0.022m;
            else
                stopTrackPercent = 0.02m;

            return stopTrackPercent;
        }

        [Obsolete()]
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

            var rule = GetRule(symbol);

            return sumQty == 0 || sumAmout == 0 ? 0 : Common.FixPrice(sumAmout / (decimal)sumQty, rule.MinPrice);
        }

    }

    class MarketMinRule
    {
        public string Symbol { get; set; }
        public decimal MinQty { get; set; }
        public decimal MinPrice { get; set; }
    }
}
