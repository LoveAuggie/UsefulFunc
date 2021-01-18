using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auggie.Lib.Http
{
    public class HttpHelper
    {
        public static string Get(string url)
        {
            var request = new HttpRequest() { Address = url, Method = HttpMethod.GET };
            return request.GetString();
        }

        public static string Post(string url, string body)
        {
            var request = new HttpRequest() { Address = url, RequestBodyString = body, Method = HttpMethod.POST };
            return request.GetString();
        }

        public static string Put(string url, string body)
        {
            var request = new HttpRequest() { Address = url, RequestBodyString = body, Method = HttpMethod.PUT };
            return request.GetString();
        }

        public static string Delete(string url)
        {
            var request = new HttpRequest() { Address = url, Method = HttpMethod.DELETE };
            return request.GetString();
        }
    }
}
