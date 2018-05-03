using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase
{
    public class Config
    {
        public const string CONFIG_FILENAME = "config.json";

        public string Key { get; set; }
        public string Secret { get; set; }

    }

    public sealed class Common
    {

        private static Config _Config;
        public static Config Config
        {
            get
            {
                if (_Config is null)
                {
                    if (!System.IO.File.Exists(Config.CONFIG_FILENAME))
                    {
                        System.IO.File.WriteAllText(Config.CONFIG_FILENAME, JsonConvert.SerializeObject(new Config()));
                        throw new ArgumentException($"{Config.CONFIG_FILENAME} file empty!");
                    }
                    else
                        _Config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText(Config.CONFIG_FILENAME));
                }

                if (string.IsNullOrEmpty(_Config.Key))
                    throw new ArgumentException($" {Config.CONFIG_FILENAME}, Key is null!");

                if (string.IsNullOrEmpty(_Config.Secret))
                    throw new ArgumentException($" {Config.CONFIG_FILENAME}, Secret is null!");

                return _Config;
            }
        }

        public static decimal FixQty(decimal qty, decimal minQty)
        {
            //if ((Int32)qty < 1) //1 den küçük ise bu fiyatı büyük bir altcoin dir.
            //    qty = Math.Round(qty, 8);
            //else
            //    qty = Math.Floor(qty);

            //seçili symbol için izin verilen min qty girilmelidir.
            var depth = BitConverter.GetBytes(decimal.GetBits(Normalize(minQty))[3])[2];

            if (depth == 0)
                return qty;

            qty = Math.Round(qty, depth);
            return qty;
        }

        public static decimal FixPrice(decimal price, decimal minPrice)
        {
            var depth = BitConverter.GetBytes(decimal.GetBits(Normalize(minPrice))[3])[2];
            return Math.Round(price, depth);
        }

        public static decimal Normalize(decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public static DateTime LongToDate(long value)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime;
        }

    }

    public class BinanceTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is long)
            {
                //return DateTimeOffset.FromUnixTimeMilliseconds((long)reader.Value).LocalDateTime;
                return Common.LongToDate((long)reader.Value);
            }

            return null;
        }
    }


    public class KlineIntervalTypes
    {
        public const string OneMinute = "1m";
        public const string ThreeMinutes = "3m";
        public const string FiveMinutes = "5m";
        public const string FifteenMinutes = "15m";
        public const string ThirtyMinute = "30m";

        public const string OneHour = "1h";
        public const string TwoHours = "2h";
        public const string FourHours = "4h";
        public const string SixHours = "6h";
        public const string EightHours = "8h";
        public const string TwelveHours = "12h";

        public const string OneDay = "1d";
        public const string ThreeDays = "3d";

        public const string OneWeek = "1w";

        public const string OneMonth = "1M";
    }
}
