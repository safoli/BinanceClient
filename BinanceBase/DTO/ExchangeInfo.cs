using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase.DTO
{
    public class ExchangeInfo
    {
        public SymbolInfo[] symbols { get; set; }

        //public void RemoveUseless()
        //{
        //    symbols = symbols.Where((i) => i.symbol.EndsWith(Common.BaseSymbol)).ToArray();
        //}
    }

    public class SymbolInfo
    {
        public string symbol { get; set; }
        public SymbolFilter[] filters { get; set; }

        public override string ToString()
        {
            return symbol;
        }
    }

    public class SymbolFilter
    {
        public string filterType { get; set; }
        public decimal minPrice { get; set; }
        public decimal minQty { get; set; }
        public decimal minNotional { get; set; }
    }

}
