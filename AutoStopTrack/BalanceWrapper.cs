using BinanceBase.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoStopTrack
{
    class BalanceWrapper : Balance
    {

        public BalanceWrapper(Balance balance)
        {
            this.asset = balance.asset;
            this.free = balance.free;
            this.locked = balance.locked;
        }

        public decimal CurrentPrice { get; set; } //By BTC
        public decimal CostPrice { get; set; }
    }
}
