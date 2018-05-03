using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBase
{
    class DefaultLogger : ILogger
    {
        public void Write(string text)
        {
            Console.WriteLine(text);
        }
    }
}
