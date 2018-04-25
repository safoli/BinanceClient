using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PriceVolumeTrack
{
    class Program
    {
        static void Main(string[] args)
        {
            var tracker = new Tracker();
            tracker.Run();

            Console.ReadLine();
        }
    }
}
