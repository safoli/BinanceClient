using BinanceBase;
using BinanceBase.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceVolumeTrack
{
    class Tracker
    {

        BinanceClient Client;
        ExchangeInfo MarketExchangeInfo;

        public Tracker()
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
                    Track();
                    System.Threading.Thread.Sleep(30 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try { SetupClient(); } catch (Exception) { } //try setup
                    Console.WriteLine("Hata sonrası izlemeye tekrar başla");
                }
            }
        }

        void Track()
        {
            var symbol = "NEOBTC";
            var _24hr = Client.Get24hrData(symbol);

            var klines = Client.GetKLines(symbol, KlineIntervalTypes.OneMinute);

            //TODO :/

        }
    }
}
