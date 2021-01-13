using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auggie.Lib.Extend
{
    public static class DateTimeExtend
    {
        /// <summary>
        /// 时间转时间戳
        /// </summary>
        /// <param name="sec">true: 秒(10位) false: 毫秒(13位)</param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime dt, bool sec = true)
        {
            DateTimeOffset dof = new DateTimeOffset(dt);
            if (sec)
                return dof.ToUnixTimeSeconds();
            else
                return dof.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 时间戳转本地时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long ts)
        {
            var str = ts.ToString();
            if (str.Length == 10)
                return DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime().DateTime;
            else if (str.Length == 13)
                return DateTimeOffset.FromUnixTimeMilliseconds(ts).ToLocalTime().DateTime;
            else
                throw new Exception("时间戳格式错误！");
        }
    }
}
