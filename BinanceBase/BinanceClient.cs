using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase
{

    public class BinanceClient
    {
        public const string API_URL = "https://api.binance.com/";
        public const bool TEST_MODE = false;//true;

        public void Ping()
        {
            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v1/ping", RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;

            var response = client.Execute(request);
        }

        public long GetServerTime()
        {
            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v1/time", RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;

            var response = client.Execute(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);

            return obj["serverTime"];
        }

        public DTO.ExchangeInfo GetExchangeInfo()
        {
            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v1/exchangeInfo", RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<DTO.ExchangeInfo>(response.Content);
        }

        public DTO.Order[] GetOpenOrders(string symbol = "")
        {
            var queryString = $"timestamp={GetServerTime()}";
            if (!string.IsNullOrEmpty(symbol)) queryString += $"&symbol={symbol}";

            var queryStringHashed = GetEncrypted(queryString);

            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v3/openOrders?" + queryString, RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MBX-APIKEY", Common.Config.Key);
            request.AddParameter("signature", queryStringHashed);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<DTO.Order[]>(response.Content);
        }

        public decimal GetPrice(string symbol)
        {
            var client = new RestClient(API_URL);

            var request = new RestRequest("api/v3/ticker/price", RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("symbol", symbol);

            var response = client.Execute(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);

            return obj["price"];
        }

        public string Buy(string symbol, double quantity, decimal price, decimal stopPrice)
        {
            return Order(symbol, "BUY", quantity, price, stopPrice);
        }

        public string Sell(string symbol, double quantity, decimal price, decimal stopPrice)
        {
            return Order(symbol, "SELL", quantity, price, stopPrice);
        }

        private string Order(string symbol, string side, double quantity, decimal price, decimal stopPrice)
        {
            var quantityStr = quantity.ToString().Replace(",", ".");
            var priceStr = price.ToString().Replace(",", ".");
            var stopPriceStr = stopPrice.ToString().Replace(",", ".");
            var id = Guid.NewGuid().ToString();

            var queryString = $"symbol={symbol}&timestamp={GetServerTime()}&side={side}&type=STOP_LOSS_LIMIT&timeInForce=GTC&quantity={quantityStr}&price={priceStr}&newClientOrderId={id}";
            queryString += $"&stopPrice={stopPriceStr}";

            var queryStringHashed = GetEncrypted(queryString);

            var client = new RestClient(API_URL);
            var request = new RestRequest((TEST_MODE ? "/api/v3/order/test?" : "/api/v3/order?") + queryString, RestSharp.Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MBX-APIKEY", Common.Config.Key);
            request.AddParameter("signature", queryStringHashed);

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Console.WriteLine($"{side} Order Success! Symbol :{symbol}, Price :{price}, Qty :{quantity}, Amount :{ (double)price * quantity } order client id = {id}"); dont need it
                Console.WriteLine($"{side} Order Success! Symbol :{symbol}, PRICE :{price}, Qty :{quantity}, Amount :{ (double)price * quantity }");
                return id;
            }
            else
            {
                Console.WriteLine($"{side} Order Failed! Symbol :{symbol}, Price :{price}, Qty :{quantity}, Amount :{ (double)price * quantity } " + response.Content);
                return string.Empty;
            }
        }

        public string Status(string symbol, string id)
        {
            var queryString = $"symbol={symbol}&timestamp={GetServerTime()}&origClientOrderId={id}";

            var queryStringHashed = GetEncrypted(queryString);

            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v3/order?" + queryString, RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MBX-APIKEY", Common.Config.Key);
            request.AddParameter("signature", queryStringHashed);

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic obj = JsonConvert.DeserializeObject(response.Content);
                var status = obj["status"];
                Console.WriteLine($"Get Order Status Success! Symbol :{symbol}, Status : {status}, Order Client Id : {id}");
                return status;
            }
            else
            {
                Console.WriteLine($"Status Failed! Symbol :{symbol}, Order Client Id : {id} " + response.Content);
                return string.Empty;
            }
        }

        public bool Cancel(string symbol, string id)
        {
            var queryString = $"symbol={symbol}&timestamp={GetServerTime()}&origClientOrderId={id}";

            var queryStringHashed = GetEncrypted(queryString);

            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v3/order?" + queryString, RestSharp.Method.DELETE);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MBX-APIKEY", Common.Config.Key);
            request.AddParameter("signature", queryStringHashed);

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Console.WriteLine($"Canceled! Symbol :{symbol}, order client id = {id}"); dont need it
                Console.WriteLine($"Canceled! Symbol :{symbol}");
                return true;
            }
            else
            {
                Console.WriteLine($"Cancel Failed! Symbol :{symbol}, order client id = {id}" + response.Content);
                return false;
            }
        }

        public DTO.Order[] GetAllOrders(string symbol)
        {
            var queryString = $"symbol={symbol}&timestamp={GetServerTime()}";
            var queryStringHashed = GetEncrypted(queryString);

            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v3/allOrders?" + queryString, RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MBX-APIKEY", Common.Config.Key);
            request.AddParameter("signature", queryStringHashed);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<DTO.Order[]>(response.Content);
        }

        public DTO.Trade[] GetTrades(string symbol)
        {
            var queryString = $"symbol={symbol}&timestamp={GetServerTime()}";
            var queryStringHashed = GetEncrypted(queryString);

            var client = new RestClient(API_URL);
            var request = new RestRequest("/api/v3/myTrades?" + queryString, RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MBX-APIKEY", Common.Config.Key);
            request.AddParameter("signature", queryStringHashed);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<DTO.Trade[]>(response.Content);
        }

        public List<DTO.Kline> GetKLines(string symbol, string klineIntervalTypeCode)
        {
            var client = new RestClient(API_URL);

            var request = new RestRequest("/api/v1/klines", RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("symbol", symbol);
            request.AddParameter("interval", klineIntervalTypeCode);

            var response = client.Execute(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);

            var klines = new List<DTO.Kline>();

            foreach (var item in obj)
            {
                var kline = new DTO.Kline()
                {
                    openTime = Common.LongToDate((long)((Newtonsoft.Json.Linq.JValue)item[0]).Value),
                    openPrice = (decimal)(Newtonsoft.Json.Linq.JValue)item[1],
                    HighPrice = (decimal)(Newtonsoft.Json.Linq.JValue)item[2],
                    lowPrice = (decimal)(Newtonsoft.Json.Linq.JValue)item[3],
                    closePrice = (decimal)(Newtonsoft.Json.Linq.JValue)item[4],
                    volume = (decimal)(Newtonsoft.Json.Linq.JValue)item[5],
                    closeTime = Common.LongToDate((long)((Newtonsoft.Json.Linq.JValue)item[6]).Value),
                    quoteAssetVolume = (decimal)(Newtonsoft.Json.Linq.JValue)item[7],
                    numberOfTrades = (int)(Newtonsoft.Json.Linq.JValue)item[8],
                    takerBuyBaseAssetVolume = (decimal)(Newtonsoft.Json.Linq.JValue)item[9],
                    takerBuyQuoteAssetVolume = (decimal)(Newtonsoft.Json.Linq.JValue)item[10],
                };
                klines.Add(kline);
            }

            return klines;
        }

        public DTO._24hr Get24hrData(string symbol)
        {
            var client = new RestClient(API_URL);

            var request = new RestRequest("/api/v1/ticker/24hr", RestSharp.Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("symbol", symbol);

            var response = client.Execute(request);
            return JsonConvert.DeserializeObject<DTO._24hr>(response.Content);
        }

        #region Helpers
        private string GetEncrypted(string data)
        {
            var key = Encoding.UTF8.GetBytes(Common.Config.Secret);
            var message = Encoding.UTF8.GetBytes(data);

            var hash = new System.Security.Cryptography.HMACSHA256(key);
            var hashedData = hash.ComputeHash(message);

            return HashEncode(hashedData);
        }

        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private static decimal ToDecimal(string data)
        {
            return decimal.Parse(data, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        private static double ToDouble(string data)
        {
            return double.Parse(data, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        #endregion
    }
}