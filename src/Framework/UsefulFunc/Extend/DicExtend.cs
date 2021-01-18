using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auggie.Lib.Extend
{
    public static class DicExtend
    {
        public static string Read(this Dictionary<string, string> dic, string key)
        {
            if (dic.ContainsKey(key))
                return dic[key];
            return "";
        }
    }
}
