using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auggie.Lib.Log;
using Auggie.Lib.Utils;

namespace UsefulFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            //CustomLoggerManager.GetCustomLogger("t");

            SensitiveWordSplit.LoadWords(new List<string>() { "日本", "日出", "日本人" });

            var str = SensitiveWordSplit.Proc("日日本人阿萨德日大厦日本请问日出看见了看见合力科技日日本人");
        }
    }
}
