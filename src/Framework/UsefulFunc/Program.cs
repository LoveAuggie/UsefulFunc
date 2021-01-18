using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auggie.Lib.Log;

namespace UsefulFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomLoggerManager.GetCustomLogger("t");
        }
    }
}
