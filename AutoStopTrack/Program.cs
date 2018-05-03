using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoStopTrack
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var stopper = new AutoStopTracker();
            stopper.Run();

            Console.ReadLine();
        }
    }
}
